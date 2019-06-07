using Android.Content;
using Android.Provider;
using Android.Widget;
using System;

namespace sbcsms.Droid
{
    public class SMSReceiver : BroadcastReceiver
    {
        public event EventHandler<TelicMessageReceivedEventArgs> TelicEventReceived;

        public TargetDevice TargetDevice { get; set; }

        public override void OnReceive(Context context, Intent intent)
        {
            if (TargetDevice == null || string.IsNullOrWhiteSpace(TargetDevice.Phonenumber))
                return;

            if (intent.Action.Equals(Telephony.Sms.Intents.SmsReceivedAction))
            {
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
                            RaiseMessageReceivedEvent(telicEvent);
                            var timePast = DateTime.Now - telicEvent.EventTime;
                            Toast.MakeText(context, $"Event {telicEvent.EventType} ({timePast.TotalMinutes.ToString("#")} min) at {telicEvent.EventTime}", ToastLength.Long).Show();
                        }
                        catch (InvalidOperationException)
                        {
                            // ignore messages that are not telic event messages.
                        }
                    }
                }
            }
        }

        protected void RaiseMessageReceivedEvent(TelicEvent telicEvent)
        {
            TelicEventReceived?.Invoke(this, new TelicMessageReceivedEventArgs(telicEvent));
        }
    }
}
