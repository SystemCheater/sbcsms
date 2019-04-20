using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.OS;
using Android.Provider;
using Android.Widget;
using System;
using System.Collections.Generic;
using Android.Support.V4.App;
using TaskStackBuilder = Android.Support.V4.App.TaskStackBuilder;
using Android;

namespace sbcsms.Droid
{
    [Activity(Label = "sbcsms", Icon = "@mipmap/icon", Theme = "@style/MainTheme", MainLauncher = true, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation)]
    public class MainActivity : global::Xamarin.Forms.Platform.Android.FormsAppCompatActivity
    {
        internal static readonly int NOTIFICATION_ID = 1000;
        internal static readonly string CHANNEL_ID = "eventmsg_notification";
        internal static readonly string EVENT_TYPE = "count";

        private SbcEventData sbcData = new SbcEventData();

        protected override void OnCreate(Bundle savedInstanceState)
        {
            TabLayoutResource = Resource.Layout.Tabbar;
            ToolbarResource = Resource.Layout.Toolbar;
            base.OnCreate(savedInstanceState);

            CreateNotificationChannel();

            global::Xamarin.Forms.Forms.Init(this, savedInstanceState);
            App app = new App(sbcData);

            LoadApplication(app);

            ActivityCompat.RequestPermissions(this, new[] { Manifest.Permission.ReceiveSms, Manifest.Permission.ReadSms }, 0);
            ReadSms(sbcData);

            var smsReceiver = new SMSReceiver();
            smsReceiver.TargetDevice = sbcData.TargetDevice;
            smsReceiver.TelicEventReceived += SmsReceiverTelicEventReceived;
           
            RegisterReceiver(smsReceiver, new IntentFilter(Telephony.Sms.Intents.SmsReceivedAction));
        }

        private void SmsReceiverTelicEventReceived(object sender, TelicMessageReceivedEventArgs e)
        {
            sbcData.AddEvent(e.EventMessage);
            ShowEventMessageNotification(this, e.EventMessage);
        }

        void ShowEventMessageNotification(object sender, TelicEvent eventArgs)
        {
            // Pass the current button press count value to the next activity:
            var valuesForActivity = new Bundle();
            valuesForActivity.PutInt(EVENT_TYPE, (int)eventArgs.EventType);

            // When the user clicks the notification, SecondActivity will start up.
           var resultIntent = new Intent(this, typeof(MainActivity));
           
            // Pass some values to SecondActivity:
            resultIntent.PutExtras(valuesForActivity);

            // Construct a back stack for cross-task navigation:
            var stackBuilder = TaskStackBuilder.Create(this);
            stackBuilder.AddParentStack(this);
            stackBuilder.AddNextIntent(resultIntent);

            // Create the PendingIntent with the back stack:
            var resultPendingIntent = stackBuilder.GetPendingIntent(0, (int)PendingIntentFlags.UpdateCurrent);

            string contentTitle;
            string contentText;
            string gpsInformation = $"{eventArgs.PositionType} N{eventArgs.Latitude} E{eventArgs.Longitude} {eventArgs.Altitude}m {eventArgs.Speed} km/h {eventArgs.Course}°";
            if (eventArgs.EventType != EventType.NoEvent)
            {
                contentText = contentText = $"{eventArgs.EventTime}: {gpsInformation}";
                contentTitle = eventArgs.EventType.ToString();
            }
            else
            {
                contentText = gpsInformation;
                contentTitle = "Position Response";
            }

            // Build the notification:
            var builder = new NotificationCompat.Builder(this)
                          .SetAutoCancel(true) // Dismiss the notification from the notification area when the user clicks on it
                          .SetContentIntent(resultPendingIntent) // Start up this activity when the user clicks the intent.
                          .SetContentTitle(contentTitle) // Display the count in the Content Info
                          .SetSmallIcon(Resource.Drawable.ic_map_truck) //ic_stat_button_click) // This is the icon to display
                          .SetContentText(contentText); // the message to display.

            // Finally, publish the notification:
            var notificationManager = NotificationManagerCompat.From(this);
            notificationManager.Notify(NOTIFICATION_ID, builder.Build());
        }

        public void ReadSms(SbcEventData sbcData)
        {
            if (string.IsNullOrWhiteSpace(sbcData.TargetDevice.Phonenumber))
            {
                Toast.MakeText(this, "Make sure you have entered your device's phonennumber.", ToastLength.Long);
                return;
            }

            var inboxUri = Android.Net.Uri.Parse("content://sms/inbox");
            var smsColumns = new string[] { "_id", "address", "person", "date", "body", "type" };
            // $"address={sbcData.Phonenumber}"
            Android.Database.ICursor c;
            try
            {
                c = Application.Context.ContentResolver.Query(inboxUri, smsColumns, null, null, null);
            }
            catch
            {
                Toast.MakeText(this, "Could not read SMS. Make sure you gave the permission to this app.", ToastLength.Long);
                return;
            }

            if (!c.MoveToFirst())
            {
                return;
            }

            var readEvents = new List<TelicEvent>();
            do
            {
                if (!c.GetString(1).EndsWith(sbcData.TargetDevice.Phonenumber))
                    continue;

                // _id: 17 thread_id: 2 address: 6505551212 person: date: 1554416201177 date_sent: 1554423400000 protocol: 0 
                // read: 0 status: -1 type: 1 reply_path_present: 0 subject: 
                // body: Oreo's a slam dunk!kljkjkljlkj service_center: locked:0 sub_id:1 error_code:0 
                // creator:com.google.android.apps.messaging seen:1

                try
                {
                    var msgData = TelicEventMessageParser.Parse(c.GetString(4));
                    DateTime unixzero = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
                    msgData.ReceiveTime = unixzero.AddMilliseconds(c.GetLong(3)).ToLocalTime();
                    readEvents.Add(msgData);
                }
                catch (InvalidOperationException)
                {
                    // message is not a telic event message.
                }
            }
            while (c.MoveToNext());

            sbcData.AddEvent(readEvents);
        }

        bool IsSamePhonenumber(string phone1, string phone2)
        {
            var normalizedPhone1 = phone1.TrimStart('+', '0').Replace("(", "").Replace(")", "");
            var normalizedPhone2 = phone2.TrimStart('+', '0').Replace("(", "").Replace(")", "");

            return normalizedPhone1 == normalizedPhone2;
        }

        void CreateNotificationChannel()
        {
            if (Build.VERSION.SdkInt <= BuildVersionCodes.M)
            {
                // Notification channels are new in API 26 (and not a part of the
                // support library). There is no need to create a notification
                // channel on older versions of Android.
                return;
            }

            //var name = Resources.GetString(Resource.String.channel_name);
            //var description = GetString(Resource.String.channel_description);
            //var channel = new NotificationChannel(CHANNEL_ID, name, NotificationImportance.Default)
            //{
            //    Description = description
            //};

            //var notificationManager = (NotificationManager)GetSystemService(NotificationService);
            //notificationManager.CreateNotificationChannel(channel);
        }
    }
}