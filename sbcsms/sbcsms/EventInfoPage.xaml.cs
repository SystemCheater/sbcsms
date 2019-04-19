using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace sbcsms
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class EventInfoPage : TabbedPage
    {
        public TelicEventViewModel EventInfo { get; set; }

        public EventInfoPage(TelicEventViewModel item)
        {
            EventInfo = item;

            InitializeComponent();
            BindingContext = this;
        }
    }
}