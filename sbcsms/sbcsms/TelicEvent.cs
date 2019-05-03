using System;

namespace sbcsms
{
    public class TelicEvent
    {
        public DateTime EventTime { get; set; }

        public EventType EventType { get; set; }

        public DateTime ReceiveTime { get; set; }

        public DateTime GpsTime { get; set; }

        public float Speed { get; set; }    // [km/h]

        public float Course { get; set; }   // [°]

        public string EventText { get; set; }
        public float Latitude { get; set; }
        public float Longitude { get; set; }
        public PositionType PositionType { get; set; }
        public byte GsmRssiEvent { get; set; }
        public byte GsmRssiSmsSent { get; set; }
        public uint StationarySeconds { get; set; }
        public float AnalogInput { get; set; }
        public float InternalBattery { get; set; }
        public float ExternalPowerSupply { get; set; }
        public string DigitalOutputStatus { get; set; }
        public string DigitalInputStatus { get; set; }
        public uint MCCMNC { get; set; }
        public uint Mileage { get; set; }
        public int Altitude { get; set; }
        public int Satellites { get; set; }
        public uint EventInfo { get; set; }

        public override string ToString()
        {
            return $"{EventTime.ToLocalTime()}: {EventText}";
        }
    }
}
