using Xamarin.Forms;

namespace RoRuCalendarN
{
    public partial class App : Application
    {
        public App()
        {
            InitializeComponent();
            MainPage = new NavigationPage(new MainPage());
            ((NavigationPage)MainPage).BarBackgroundColor = Color.FromHex("#E87400");
        }
        protected override void OnStart() { }
        protected override void OnSleep() { }
        protected override void OnResume() { }
    }
}
