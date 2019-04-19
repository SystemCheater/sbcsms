using Plugin.Messaging;
using System;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using Xamarin.Forms;

namespace sbcsms
{
    public partial class MainPage : ContentPage
    {
        public SbcEventData SbcData { get; set; }

        public ObservableCollection<TelicEventViewModel> Events { get; set; } = new ObservableCollection<TelicEventViewModel>();

        public MainPage(SbcEventData sbcData)
        {
            SbcData = sbcData;
            InitializeComponent();
            if (Application.Current.Properties.ContainsKey("Phonenumber"))
                sbcData.TargetDevice.Phonenumber = Application.Current.Properties["Phonenumber"].ToString();
            if (Application.Current.Properties.ContainsKey("IMEI"))
                sbcData.TargetDevice.Imei = Application.Current.Properties["IMEI"].ToString();

            BindingContext = this;
            foreach (var e in SbcData.Events.OrderByDescending(e => e.EventTime))
            {
                Events.Add(new TelicEventViewModel(e));
            }

            SbcData.Events.CollectionChanged += EventsCollectionChanged;
        }

        private void OnChange(object sender, EventArgs e)
        {

        }

        private void EventsCollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            foreach (var newItem in e.NewItems)
            {
                var newEvent = newItem as TelicEvent;
                if (newEvent == null)
                {
                    continue;
                }

                Events.Insert(0, new TelicEventViewModel(newEvent));
            }
        }

        private void ListView_ItemTapped(object sender, ItemTappedEventArgs e)
        {
            Navigation.PushModalAsync(new EventInfoPage((TelicEventViewModel)e.Item));
        }

        private void RequestPositonButton_Clicked(object sender, EventArgs e)
        {
            var smsMessenger = CrossMessaging.Current.SmsMessenger;
            if (smsMessenger.CanSendSms)
            {
                var smsText = $"0041{SbcData.TargetDevice.ShortImei}";
                var phonenumber = SbcData.TargetDevice.Phonenumber;
                smsMessenger.SendSms(phonenumber, smsText);
                positionRequested = true;
                OnPropertyChanged(nameof(PositionRequestStatus));
            }
        }

        public string PositionRequestStatus
        {
            get
            {
                if (positionRequested)
                {
                    return "Position requested. Be patient.";
                }

                return "Request position from device";
            }
        }

        private bool positionRequested;

        private void Button_Clicked(object sender, EventArgs e)
        {
            var clickedEvent = sender as Button;
            if (clickedEvent == null)
                return;
            var telicEvent = clickedEvent.BindingContext as TelicEventViewModel;
            if (telicEvent == null)
                return;

            string mapsLabel = telicEvent.EventType.ToString();
            string url = String.Format("http://maps.google.com/maps?q={0},{1}+({2})", telicEvent.Latitude.ToString("#.#######", new CultureInfo("en-US")), telicEvent.Longitude.ToString("#.#######", new CultureInfo("en-US")), mapsLabel);
            Device.OpenUri(new Uri(url));
        }

        private void TargetDeviceInfoChanged(object sender, TextChangedEventArgs e)
        {
            if (!string.IsNullOrWhiteSpace(SbcData.TargetDevice.Phonenumber))
                Application.Current.Properties["Phonenumber"] = SbcData.TargetDevice.Phonenumber;
            if (!string.IsNullOrWhiteSpace(SbcData.TargetDevice.Imei))
                Application.Current.Properties["IMEI"] = SbcData.TargetDevice.Imei;
        }
    }
}
