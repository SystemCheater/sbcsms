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
        public TargetDevice TargetDevice { get; set; } = new TargetDevice();

        public ObservableCollection<TelicEvent> Events { get; set; } = new ObservableCollection<TelicEvent>();
    }
}