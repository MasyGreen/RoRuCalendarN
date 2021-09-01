using System;
using Xamarin.Essentials;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace RoRuCalendarN
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class SettingsPage : ContentPage
    {
        public SettingsPage()
        {
            InitializeComponent();
            Entry localswitch0 = (Entry)FindByName("LabelSettingsUserName");
            localswitch0.Text = Preferences.Get("SettingsUserName", string.Empty);

            Entry localswitch1 = (Entry)FindByName("LabelSettingsUserPassword");
            localswitch1.Text = Preferences.Get("SettingsUserPassword", string.Empty);
        }

        /// <summary>
        /// Сохранить
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        async private void OnClickSave(object sender, EventArgs e)
        {
            Entry localswitch0 = (Entry)FindByName("LabelSettingsUserName");
            Preferences.Set("SettingsUserName", localswitch0.Text);

            Entry localswitch1 = (Entry)FindByName("LabelSettingsUserPassword");
            Preferences.Set("SettingsUserPassword", localswitch1.Text);

            base.OnBackButtonPressed();
        }

        /// <summary>
        /// Выход без сохранение
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        async private void OnClickExit(object sender, EventArgs e)
        {
            base.OnBackButtonPressed();
        }

    }
}