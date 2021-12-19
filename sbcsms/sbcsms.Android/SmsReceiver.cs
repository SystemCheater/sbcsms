// --------------------------------------------------------------------------------------------------------------------
// <copyright file="SmsReceiver.cs" company="KUKA Deutschland GmbH">
//   Copyright (c) KUKA Deutschland GmbH 2006 - 2021
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

using Android.Content.Res;
using Android.OS;
using Android.Support.V4.App;

namespace sbcsms.Droid
{
   using System;

   using Android.App;
   using Android.Content;
   using Android.Provider;
   using Android.Widget;

   [BroadcastReceiver(Enabled = true, Label = "SMS Receiver")]
   [IntentFilter(new[] { "android.provider.Telephony.SMS_RECEIVED" })]
   public class SMSReceiver : BroadcastReceiver
   {
      #region Public Events

      public event EventHandler<TelicMessageReceivedEventArgs> TelicEventReceived;

      #endregion

      #region Public Properties

      public TargetDevice TargetDevice { get; set; }

      #endregion

      #region Public Methods and Operators

      public override void OnReceive(Context context, Intent intent)
      {
         if (TargetDevice == null || string.IsNullOrWhiteSpace(TargetDevice.Phonenumber))
            return;
         if (!intent.Action.Equals(Telephony.Sms.Intents.SmsReceivedAction))
            return;

         var messages = Telephony.Sms.Intents.GetMessagesFromIntent(intent);
         for (var i = 0; i < messages.Length; i++)
         {
            // keep it simple: if no phonenumber is given we try to parse any incoming message...
            if (messages[i].OriginatingAddress.EndsWith(TargetDevice.Phonenumber))
            {
               try
               {
                  var telicEvent = TelicEventMessageParser.Parse(messages[i].MessageBody);

                  DateTime unixzero = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
                  telicEvent.ReceiveTime = unixzero.AddMilliseconds(messages[i].TimestampMillis).ToLocalTime();
                  
                  ShowEventMessageNotification(context, telicEvent);

                        //showNotification(context, "short", false);
                        RaiseMessageReceivedEvent(telicEvent);
                  var timePast = DateTime.Now - telicEvent.EventTime;
                  Toast.MakeText(context, $"Event {telicEvent.EventType} ({timePast.TotalMinutes.ToString("#")} min) at {telicEvent.EventTime}",
                     ToastLength.Long).Show();
               }
               catch (InvalidOperationException)
               {
                  // ignore messages that are not telic event messages.
               }
            }
         }
      }

      #endregion

      #region Methods

      protected void RaiseMessageReceivedEvent(TelicEvent telicEvent)
      {
           TelicEventReceived?.Invoke(this, new TelicMessageReceivedEventArgs(telicEvent));
      }
      
      internal static readonly string EVENT_TYPE = "count";


      private void showNotification(Context context, string text, bool showIconOnly)
      {
          var _mNotificationManager = (NotificationManager)context.GetSystemService(Context.NotificationService);


            // This is who should be launched if the user selects our notification.
            Intent contentIntent = new Intent();

          // choose the ticker text
          String tickerText = "ticket text";

          Notification n = new Notification(Resource.Drawable.ic_map_truck, text);

          PendingIntent appIntent = PendingIntent.GetActivity(context, 0, contentIntent, 0);

          n.SetLatestEventInfo(context, "1", "2", appIntent);

          _mNotificationManager.Notify(NOTIFICATION_ID, n);
      }

      internal static readonly int NOTIFICATION_ID = 1000;

        void ShowEventMessageNotification(Context  context, TelicEvent eventArgs)
        {
            // Pass the current button press count value to the next activity:
            var valuesForActivity = new Bundle();
            valuesForActivity.PutInt(EVENT_TYPE, (int)eventArgs.EventType);

            // When the user clicks the notification, SecondActivity will start up.
            var resultIntent = new Intent(context, typeof(MainActivity));

            // Pass some values to SecondActivity:
            resultIntent.PutExtras(valuesForActivity);

            // Construct a back stack for cross-task navigation:
            var stackBuilder = TaskStackBuilder.Create(context);
            //stackBuilder.AddParentStack(context);
            stackBuilder.AddNextIntent(resultIntent);

            // Create the PendingIntent with the back stack:
            var resultPendingIntent = stackBuilder.GetPendingIntent(0, PendingIntentFlags.UpdateCurrent);

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
            var builder = new NotificationCompat.Builder(context)
                          .SetAutoCancel(true) // Dismiss the notification from the notification area when the user clicks on it
                          .SetContentIntent(resultPendingIntent) // Start up this activity when the user clicks the intent.
                          .SetContentTitle(contentTitle) // Display the count in the Content Info
                          .SetSmallIcon(Resource.Drawable.ic_map_truck) //ic_stat_button_click) // This is the icon to display
                          .SetContentText(contentText); // the message to display.

            // Finally, publish the notification:
            var notificationManager = NotificationManagerCompat.From(context);
            notificationManager.Notify(NOTIFICATION_ID, builder.Build());
        }
        #endregion
    }
}