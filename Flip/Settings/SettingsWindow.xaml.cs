using Flip.Translation;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace Flip.Settings
{
    /// <summary>
    /// Interaction logic for SettingsWindow.xaml
    /// </summary>
    public partial class SettingsWindow : Window
    {
        public SettingsWindow()
        {
            InitializeComponent();
            this.ProxyHost.Text = GlobalSettings.Settings.ProxyHost;
            this.ProxyPort.Text = GlobalSettings.Settings.ProxyPort.ToString();
            this.AlertConnection.IsChecked = GlobalSettings.Settings.AlertNetChange;
            this.storeSingleWord.IsChecked = GlobalSettings.Settings.StoreSingleWord;
            this.storeClipboardData.IsChecked = GlobalSettings.Settings.StoreClipData;
            this.TryUseOffline.IsChecked = GlobalSettings.Settings.TryOfflineUse;
            this.ClipInterceptOnStart.IsChecked = GlobalSettings.Settings.ClipInterceptOnStart;
            this.FilterInvalidChar.IsChecked = GlobalSettings.Settings.FilterInvalidChar;
            var Api = Enum.GetValues<TranslateApi>();
            foreach (var api in Api)
            {
                if (api == TranslateApi.OfflineDataBase)
                    continue;
                TranslationApi.Items.Add(api);
                if(api == GlobalSettings.Settings.TranslateApi)
                    TranslationApi.SelectedItem = api;
            }
        }
        private T ConvertEnum<T>(object value) where T : Enum
        {
            return (T)Enum.Parse(typeof(T), value.ToString());
        }
        private void SaveSettings_Click(object sender, RoutedEventArgs e)
        {
            GlobalSettings PrevSettings = GlobalSettings.Settings;
            try
            {
                if (this.ProxyHost.Text != GlobalSettings.Settings.ProxyHost)
                    GlobalSettings.Settings.ProxyHost = this.ProxyHost.Text;
                if ((int.Parse(this.ProxyPort.Text) != GlobalSettings.Settings.ProxyPort) && int.Parse(this.ProxyPort.Text) >= 0)
                    GlobalSettings.Settings.ProxyPort = int.Parse(this.ProxyPort.Text);
                if (this.AlertConnection.IsChecked.Value != GlobalSettings.Settings.AlertNetChange)
                    GlobalSettings.Settings.AlertNetChange = this.AlertConnection.IsChecked.Value;
                if (this.storeSingleWord.IsChecked.Value != GlobalSettings.Settings.StoreSingleWord)
                    GlobalSettings.Settings.StoreSingleWord = this.storeSingleWord.IsChecked.Value;
                if (this.storeClipboardData.IsChecked.Value != GlobalSettings.Settings.StoreClipData)
                    GlobalSettings.Settings.StoreClipData = this.storeClipboardData.IsChecked.Value;
                if ((ConvertEnum<TranslateApi>(this.TranslationApi.SelectedItem)) != GlobalSettings.Settings.TranslateApi)
                    GlobalSettings.Settings.TranslateApi = ConvertEnum<TranslateApi>(this.TranslationApi.SelectedItem);
                if (this.TryUseOffline.IsChecked.Value != GlobalSettings.Settings.TryOfflineUse)
                    GlobalSettings.Settings.TryOfflineUse = this.TryUseOffline.IsChecked.Value;
                if (this.ClipInterceptOnStart.IsChecked.Value != GlobalSettings.Settings.ClipInterceptOnStart)
                    GlobalSettings.Settings.ClipInterceptOnStart = this.ClipInterceptOnStart.IsChecked.Value;
                if (this.FilterInvalidChar.IsChecked.Value != GlobalSettings.Settings.FilterInvalidChar)
                    GlobalSettings.Settings.FilterInvalidChar = this.FilterInvalidChar.IsChecked.Value;
                GlobalSettings.SaveSettings(GlobalSettings.Settings);
                MessageBox.Show("Settings saved.", "Info", MessageBoxButton.OK, MessageBoxImage.Information);
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Unable to change settings:\n{ex.Message}\n\n --All Settings Reverted to their Last value--", "Possible Error", MessageBoxButton.OK, MessageBoxImage.Error);
                GlobalSettings.SaveSettings(PrevSettings);
            }
        }

        private void OpenSavedData_Click(object sender, RoutedEventArgs e)
        {
            //~
        }

        private void ResetSavedData_Click(object sender, RoutedEventArgs e)
        {
            //~
        }
    }
}
