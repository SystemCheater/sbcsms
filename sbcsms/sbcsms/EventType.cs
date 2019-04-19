namespace sbcsms
{
    public enum EventType : byte
    {
        NoEvent = 0,
        PowerOn = 1,
        Emergency = 2,
        PositionLockAlarm =3,
        AlarmTracking = 4,
        PowerOff = 5,
        CourseChange = 6,
        GeofenceAreaEnter = 7,
        GeofenceAreaExit = 8,
        GpsFixLost = 9,
        RoutineMessage=10,
        IgnitionOn = 11,
        IgnitionOff = 12,
        Input2On = 13,
        Input2Off = 14,
        ChargerConnected = 15,
        ChargerDisconnected = 16,
        AnalogInOn = 17,
        AnalogInOff = 18,
        Reserved19 =19,
        OutputChanged = 20,
        BatteryVoltageOk =21,
        BatteryLowVoltage =22,
        ExternalVoltageOk = 23,
        ExternalLowVoltage = 24,
        Motion = 25,
        Stationary = 26,
        Reserved27 = 27,
        AED=28,
        ForceGprs = 29,
        IncomingCall = 30,
        Reserved31 = 31,
        GprsHeartbeat = 32
    }
}