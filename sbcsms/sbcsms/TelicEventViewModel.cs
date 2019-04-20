using System;
using Xamarin.Forms;

namespace sbcsms
{
    public class TelicEventViewModel : TelicEvent
    {
        public TelicEvent TelicEvent { get; private set; }

        public TelicEventViewModel(TelicEvent telicEvent)
        {
            this.TelicEvent = telicEvent;
            this.Altitude = telicEvent.Altitude;
            this.AnalogInput = telicEvent.AnalogInput;
            this.Course = telicEvent.Course;
            this.DigitalInputStatus = telicEvent.DigitalInputStatus;
            this.DigitalInputStatus = telicEvent.DigitalOutputStatus;
            this.EventInfo = telicEvent.EventInfo;
            this.EventText = telicEvent.EventText;
            this.EventTime = telicEvent.EventTime;
            this.EventType = telicEvent.EventType;
            this.ExternalPowerSupply = telicEvent.ExternalPowerSupply;
            this.GpsTime = telicEvent.GpsTime;
            this.GsmRssiEvent = telicEvent.GsmRssiEvent;
            this.GsmRssiSmsSent = telicEvent.GsmRssiSmsSent;
            this.InternalBattery = telicEvent.InternalBattery;
            this.Latitude = telicEvent.Latitude;
            this.Longitude = telicEvent.Longitude;
            this.MCCMNC = telicEvent.MCCMNC;
            this.Mileage = telicEvent.Mileage;
            this.PositionType = telicEvent.PositionType;
            this.ReceiveTime = telicEvent.ReceiveTime;
            this.Satellites = telicEvent.Satellites;
            this.Speed = telicEvent.Speed;
            this.StationarySeconds = telicEvent.StationarySeconds;
        }

        public double MileageKm { get { return TelicEvent.Mileage / 1000.0; } }

        public string TimePassedSinceEvent
        {
            get
            {
                var timePassed = DateTime.Now - EventTime;
                if (timePassed < TimeSpan.FromSeconds(60))
                {
                    return "now";
                }

                if (timePassed < TimeSpan.FromMinutes(60))
                {
                    return $"{ timePassed.TotalMinutes.ToString("#")} min";
                }

                if (timePassed < TimeSpan.FromHours(24))
                {
                    return $"{ timePassed.TotalHours.ToString("#")} h";
                }

                return $"{ timePassed.TotalDays.ToString("#") } days";
            }
        }

        public string StationaryTimeWithUnit
        {
            get
            {
                var stationaryTime = TimeSpan.FromSeconds(StationarySeconds);
                return stationaryTime.ToString("c");
            }
        }

        public Color GsmColor
        {
            get
            {
                var difference = EventTime.ToUniversalTime() - ReceiveTime.ToUniversalTime();
                if (difference.Duration() <= TimeSpan.FromMinutes(2))
                {
                    return Color.Green;
                }

                return Color.Red;
            }
        }

        public Color GpsColor
        {
            get
            {
                var difference = EventTime.ToUniversalTime() - GpsTime.ToUniversalTime();
                if (difference.Duration() <= TimeSpan.FromMinutes(2))
                {
                    return Color.Green;
                }

                return Color.Red;
            }
        }
    }
}