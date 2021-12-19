// --------------------------------------------------------------------------------------------------------------------
// <copyright file="SmsReceiver.cs" company="KUKA Deutschland GmbH">
//   Copyright (c) KUKA Deutschland GmbH 2006 - 2021
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

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

      #endregion
   }
}