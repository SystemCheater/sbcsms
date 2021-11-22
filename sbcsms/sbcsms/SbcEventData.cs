using System;
using System.Collections.Generic;

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