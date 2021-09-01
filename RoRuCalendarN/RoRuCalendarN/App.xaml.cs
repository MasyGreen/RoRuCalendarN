using Plugin.Settings;
using Xamarin.Essentials;
using Xamarin.Forms;

namespace RoRuCalendarN
{
    public partial class App : Application
    {
        public static string SettingsUserName;
        public static string SettingsUserPassword;

        public App()
        {
            InitializeComponent();
            LoadSettings();
            MainPage = new NavigationPage(new MainPage());
            ((NavigationPage)MainPage).BarBackgroundColor = Color.FromHex("#E87400");
        }
        protected override void OnStart() { }
        protected override void OnSleep() { }
        protected override void OnResume() { }

        private void LoadSettings()
        {
            SettingsUserName = Preferences.Get("SettingsUserName", string.Empty);
            //App.Current.Properties.Add("SettingsUserName", SettingsUserName);

            SettingsUserName = Preferences.Get("SettingsUserPassword", string.Empty);
            //App.Current.Properties.Add("SettingsUserPassword", SettingsUserPassword);
        }
    }
}
