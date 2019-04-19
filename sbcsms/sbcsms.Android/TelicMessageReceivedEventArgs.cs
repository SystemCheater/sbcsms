using System;

namespace sbcsms.Droid
{
    public class TelicMessageReceivedEventArgs : EventArgs
    {
        public TelicEvent EventMessage { get; set; }

        public TelicMessageReceivedEventArgs(TelicEvent telicEvent)
        {
            this.EventMessage = telicEvent;
        }
    }
}