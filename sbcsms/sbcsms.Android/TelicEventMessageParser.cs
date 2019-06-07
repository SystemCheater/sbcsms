using System;
using System.Globalization;
using System.Text.RegularExpressions;

namespace sbcsms.Droid
{
    public class TelicEventMessageParser
    {
        public static TelicEvent Parse(string msg)
        {
            var parts = msg.Split(new char[] { ',' }, StringSplitOptions.None);
            if (parts.Length < 10)
            {
                throw new InvalidOperationException("Not a telic event message.");
            }

            var isEventMessage = new Regex("^..[23][01].*,.*").IsMatch(msg);
            var isPositionRequestResponse = new Regex("^..44.*,.*").IsMatch(msg);
            if (!isEventMessage && !isPositionRequestResponse)
            {
                throw new InvalidOperationException("Not a telic event message.");
            }

            var telicEvent = new TelicEvent() { EventText = parts[0] };
            if (isEventMessage)
            {
                var headerAndImeiCount = GetHeaderAndImeiCount(parts);
                var eventType = parts[0].Remove(0, headerAndImeiCount);
                if (byte.TryParse(eventType, out byte b))
                {
                    telicEvent.EventType = (EventType)b;
                }

                telicEvent.EventTime = ParseDateTime(parts[1]);
                if (uint.TryParse(parts[2], out uint eventInfo))
                {
                    telicEvent.EventInfo = eventInfo;
                }
            }
            else
            {
                telicEvent.EventType = EventType.NoEvent;
                // Hack: event time is set to gps time to have somehow meaningful sorting (better would be receive time)
                telicEvent.EventTime = ParseDateTime(parts[3]);
                telicEvent.EventInfo = 0;
            }

            telicEvent.GpsTime = ParseDateTime(parts[3]);

            if (ParsePosition(parts[4], parts[5], out float longitude, out float latitude))
            {
                telicEvent.Longitude = longitude;
                telicEvent.Latitude = latitude;
            }

            if (byte.TryParse(parts[6], out byte pt))
            {
                telicEvent.PositionType = (PositionType)pt;
            }

            if (int.TryParse(parts[7], out int speed))
            {
                telicEvent.Speed = speed;
            }

            if (int.TryParse(parts[8], out int course))
            {
                telicEvent.Course = course;
            }

            if (int.TryParse(parts[9], out int satellites))
            {
                telicEvent.Satellites = satellites;
            }

            if (int.TryParse(parts[10], out int hdop))
            {
                telicEvent.Hdop = hdop;
            }

            if (int.TryParse(parts[11], out int vdop))
            {
                telicEvent.Vdop = vdop;
            }

            if (int.TryParse(parts[12], out int altitude))
            {
                telicEvent.Altitude = altitude;
            }

            if (uint.TryParse(parts[13], out uint mileage))
            {
                telicEvent.Mileage = mileage;
            }

            if (uint.TryParse(parts[14], out uint mccmnc))
            {
                telicEvent.MCCMNC = mccmnc;
            }

            telicEvent.DigitalInputStatus = parts[15];
            telicEvent.DigitalOutputStatus = parts[16];
            telicEvent.ExternalPowerSupply = ParseExternalVoltage(parts[17]);
            telicEvent.InternalBattery = ParseBatteryVoltage(parts[18]);
            telicEvent.AnalogInput = ParseExternalVoltage(parts[19]);

            if (uint.TryParse(parts[21], out uint stationarySeconds))
            {
                telicEvent.StationarySeconds = stationarySeconds;
            }

            if (uint.TryParse(parts[22], out uint positionAccuracy))
            {
                telicEvent.PositionAccuracy = positionAccuracy;
            }

            if (byte.TryParse(parts[23], out byte gsmRssiSmsSent))
            {
                telicEvent.GsmRssiSmsSent = gsmRssiSmsSent;
            }

            if (byte.TryParse(parts[24], out byte gsmRssiEvent))
            {
                telicEvent.GsmRssiEvent = gsmRssiEvent;
            }

            return telicEvent;
        }

        private static float ParseExternalVoltage(string externalVoltageString)
        {
            if (!uint.TryParse(externalVoltageString, out var externalPowerSupply))
                return 0.0f;

            float voltage;
            if (externalVoltageString.Length <= 3)
            {
                voltage = ConvertExternalVoltage(externalPowerSupply);
            }
            else
            {
                voltage = ConvertMilliVolts(externalPowerSupply);
            }

            return voltage;
        }

        private static float ParseBatteryVoltage(string batteryVoltageString)
        {
            if (!uint.TryParse(batteryVoltageString, out var batteryVoltageAsInteger))
                return 0.0f;

            float voltage;
            if (batteryVoltageString.Length <= 3)
            {
                voltage = ConvertBatteryVoltage(batteryVoltageAsInteger);
            }
            else
            {
                voltage = ConvertMilliVolts(batteryVoltageAsInteger);
            }

            return voltage;
        }

        private static float ConvertMilliVolts(uint externalPowerSupply)
        {
            return externalPowerSupply / 1000.0f;
        }

        private static int GetHeaderAndImeiCount(string[] parts)
        {
            bool isShortImei = parts[0].Length <= 4 + 6;
            int headerAndImeiCount;
            if (isShortImei)
            {
                headerAndImeiCount = 4 + 6;
            }
            else
            {
                headerAndImeiCount = 4 + 15;
            }

            return headerAndImeiCount;
        }

        protected static float ConvertBatteryVoltage(uint analogInput)
        {
            return (float)(analogInput * 19.8) / 1000;
        }

        /// <summary>
        /// Converts the raw ADC ouput of Telic devices' to Volts. 
        /// This algorithm is valid for circuits/ADCs that are designed for 40 V max voltage (e. g. for supply voltage).
        /// </summary>
        /// <param name="analogInput">The analog input.</param>
        /// <returns></returns>
        protected static float ConvertExternalVoltage(uint analogInput)
        {
            if (analogInput == 0)
                return 0.0f;

            return (float)(6 + analogInput * 0.128);
        }

        private static bool ParsePosition(string lng, string lat, out float longitude, out float latitude)
        {
            longitude = 0.0f;
            latitude = 0.0f;

            float f;
            bool success = float.TryParse(lng, out f);
            if (success)
            {
                if (lng.Length >= 9)
                {
                    longitude = f / 1000000;
                }
                else
                {
                    longitude = f / 10000;
                }
            }

            success &= float.TryParse(lat, out f);
            if (success)
            {
                if (lat.Length >= 8)
                {
                    latitude = f / 1000000;
                }
                else
                {
                    latitude = f / 10000;
                }
            }

            return success;
        }

        public static DateTime FromUnixTime(long unixTime)
        {
            DateTime unixzero = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
            DateTime timestamp = unixzero.AddSeconds(unixTime);
            return timestamp;
        }

        private static DateTime ParseDateTime(string timeToParse)
        {
            DateTime time;
            DateTime.TryParseExact(timeToParse, "ddMMyyHHmmss", CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal, out time);
            return time;
        }
    }
}