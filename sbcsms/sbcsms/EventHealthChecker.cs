using System;
using System.Collections.Generic;
using System.Text;

namespace sbcsms
{
    public static class EventHealthChecker
    {
        public static bool IsGpsPositionUpToDate(this TelicEvent telicEvent)
        {
            var difference = telicEvent.EventTime - telicEvent.GpsTime;
            if (difference <= TimeSpan.FromSeconds(15))
                return true;

            return false;
        }

    }
}
