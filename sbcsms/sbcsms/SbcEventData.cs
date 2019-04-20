using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;

namespace sbcsms
{
    public class SbcEventData
    {
        public event EventHandler EventsUpdated;

        public TargetDevice TargetDevice { get; set; } = new TargetDevice();

        public List<TelicEvent> Events { get; private set; } = new List<TelicEvent>();

        public void AddEvent(TelicEvent telicEvent)
        {
            Events.Add(telicEvent);
            OnEventsUpdated();
        }

        public void AddEvent(IEnumerable<TelicEvent> events)
        {
            Events.AddRange(events);
            OnEventsUpdated();
        }

        protected void OnEventsUpdated()
        {
            EventsUpdated?.Invoke(this, new EventArgs());
        }
    }
}