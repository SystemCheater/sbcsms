using System;
using System.Collections.Generic;
using System.Text;

namespace sbcsms
{
    public class TargetDevice
    {
        public string Phonenumber { get; set; } = string.Empty;

        public string ShortImei
        {
            get
            {
                if (Imei.Length < 6)
                    return Imei;

                return Imei.Substring(Imei.Length - 6);
            }
        }

        public string Imei { get; set; } = string.Empty;
    }
}
