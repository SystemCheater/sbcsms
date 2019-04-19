using Android.App;
using Android.OS;
using Android.Widget;

namespace sbcsms.Droid
{
    [Activity(Label = "Second Activity")]
    public class SecondActivity : Activity
    {
        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

            int eventTypeAsInt = Intent.Extras.GetInt(MainActivity.EVENT_TYPE, -1);
            if (eventTypeAsInt <= 0)
            {
                return;
            }

            // make more robust!
            var eventType = (EventType)eventTypeAsInt;

            // Display the count sent from the first activity:
            SetContentView(Resource.Layout.Second);
            var txtView = FindViewById<TextView>(Resource.Id.textView1);
            txtView.Text = $"You received {eventType}.";
        }
    }
}