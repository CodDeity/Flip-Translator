using System.Windows;
using System.Windows.Input;
using System.Timers;
using System.Linq.Expressions;
using Flip.Translation;
using Flip.Translation.Api;
using System.Windows.Controls;
using System.Windows.Media;
using System.Transactions;
using Flip.Settings;
using System.Net.NetworkInformation;
using System.Runtime.InteropServices;
using System.Windows.Media.Animation;

namespace Flip
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private List<char> filtered = new List<char>()
        {
            ':',
            '}',
            '\'',
            '\"',
            '{',
            '[',
            ']',
            '@',
            '$',
            '#'
        };
        public MainWindow()
        {
            InitializeComponent();
            GlobalSettings.LoadSettings();
            GlobalSettings.OnSettingsChanged += GlobalSettings_OnSettingsChanged;
            KeyIntercept.OnKeyPressed += KeyIntercept_OnKeyPressed;
            KeyIntercept.StartIntercept(1, 162);
            ClipIntercept.OnClipboardDataChange += ClipIntercept_OnClipboardDataChange;
            if (GlobalSettings.Settings.ClipInterceptOnStart)
            {
                if(GlobalSettings.Settings.FilterInvalidChar)
                    ClipIntercept.StartClipIntercept(filtered);
                else
                    ClipIntercept.StartClipIntercept(new List<char>());
                ClipInterceptOn.Visibility = Visibility.Collapsed;
                ClipInterceptOff.Visibility = Visibility.Visible;
            }
            else
                ClipInterceptOn.Visibility = Visibility.Visible;
                ClipInterceptOff.Visibility = Visibility.Collapsed;

            if (!GlobalSettings.Settings.AlertNetChange)
                StopNetworkConnectivityObserving();
            else
                StartNetworkConnectivityObserving();
            if (GlobalSettings.Settings.StoreSingleWord)
            {
                try
                {
                    OfflineTranslations.Load();
                }
                catch
                {
                    ShowNotifyMessage("Couldn't load offline translations", true, 1000);
                }
            }
            this.Activated += (s, e) => this.Activate();
            this.Activated += (s, e) => TextBox.Focus();

            foreach ( var language in Languages.LanguageList)
            {
                FromLanguage.Items.Add( language.Key );
                ToLanguage.Items.Add(language.Key );
            }
            FromLanguage.SelectedItem = GlobalSettings.Settings.FromLanguage;
            ToLanguage.SelectedItem = GlobalSettings.Settings.ToLanguage;
        }
        bool Observing = false;
        CancellationTokenSource ConnectionCheckerCancellationToken = new CancellationTokenSource();
        bool Connected = true;
        [DllImport("wininet.dll")]
        public extern static bool InternetGetConnectedState(out int Description, int ReservedValue);
        private bool checkConnection()
        {
            bool returnValue = false;
            try
            {
                int Desc;
                returnValue = InternetGetConnectedState(out Desc, 0);
            }
            catch
            {
                returnValue = false;
            }
            return returnValue;
        }
        private void StartNetworkConnectivityObserving()
        {
            if (!Observing)
            {
                ConnectionCheckerCancellationToken = new CancellationTokenSource();
                Task.Run(() =>
                {
                    while (!ConnectionCheckerCancellationToken.IsCancellationRequested)
                    {
                        Connected = checkConnection();
                        UpdateUINetwork();
                        Task.Delay(3000);
                    }
                });
                Observing = true;
            }
        }
        private void UpdateUINetwork()
        {
            this.Dispatcher.Invoke(() =>
            {
                if (!Connected)
                {
                    NoInternet.Visibility = Visibility.Visible;
                }
                else
                    NoInternet.Visibility = Visibility.Collapsed;
            });
        }
        private void StopNetworkConnectivityObserving()
        {
            ConnectionCheckerCancellationToken.Cancel();
        }
        private void GlobalSettings_OnSettingsChanged(object? sender, GlobalSettings e)
        {
            if (!e.AlertNetChange)
                StopNetworkConnectivityObserving();
            else
                StartNetworkConnectivityObserving();
        }
        private void ClipIntercept_OnClipboardDataChange(object? sender, string e)
        {
            this.Dispatcher.Invoke(() =>
            {
                RemoveOtherTranslations();
                if(FromLanguage.SelectedItem != null && ToLanguage.SelectedItem != null)
                {
                    DisplayTranslate(e, true);
                }
                if(e.Length == 0)
                {
                    mainTranslation.Content = "";
                    RemoveOtherTranslations();
                }
            });
        }

        private void KeyIntercept_OnKeyPressed(object? sender, KeyInterceptEventArgs e)
        {
            if (this.WindowState == WindowState.Minimized)
            {
                this.WindowState = WindowState.Normal;
                this.Focus();
                this.TextBox.Focus();
            }
            else
            {
                this.TextBox.Text = "";
                this.TextBox_TextChanged(null, null);
                this.WindowState = WindowState.Minimized;
            }
        }

        private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                this.DragMove();
            }
        }

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                this.WindowState = WindowState.Minimized;
            }
        }
        private void AddOtherTranslations(List<string> translations)
        {
            RemoveOtherTranslations();
            StackPanel CurrentPanel = new StackPanel() { 
                Orientation = Orientation.Horizontal,
                HorizontalAlignment = HorizontalAlignment.Right
            };
            for (int i = 1; i < translations.Count; i++)
            {
                string translation = translations[i];
                CurrentPanel.Children.Add(new Label()
                {
                    Content = translation,
                    Foreground = new SolidColorBrush(Colors.White),
                    FontSize = 15,
                    FontWeight = FontWeights.SemiBold,
                    HorizontalContentAlignment = HorizontalAlignment.Right,
                    Opacity = 0.6
                });
                if(i % 5 == 0)
                {
                    OtherTranslations.Children.Add(CurrentPanel);
                    CurrentPanel = new StackPanel()
                    {
                        Orientation = Orientation.Horizontal,
                        HorizontalAlignment = HorizontalAlignment.Right
                    };
                    this.Height += 10;
                }
            }
            if (!OtherTranslations.Children.Contains(CurrentPanel))
            {
                OtherTranslations.Children.Add(CurrentPanel);
                this.Height += 10;
            }
        }
        private void RemoveOtherTranslations()
        {
            OtherTranslations.Children.Clear();
            OtherTranslations.UpdateLayout();
            this.Height = 210;
        }
        private async void TextBox_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            
            if (this.TextBox.Text.Length != 0 && FromLanguage.SelectedItem != null && ToLanguage.SelectedItem != null && this.TextBox.Text.Last() == ' ')
            {
                DisplayTranslate(this.TextBox.Text);
            }
            if (this.TextBox.Text.Length == 0)
            {
                mainTranslation.Content = "";
                RemoveOtherTranslations();
            }
        }
        private async void DisplayTranslate(string text,bool fromClipboard =false)
        {
            if (Connected)
            {
                try
                {
                    string from = Languages.LanguageList[FromLanguage.SelectedItem.ToString()];
                    string to = Languages.LanguageList[ToLanguage.SelectedItem.ToString()];
                    if (fromClipboard)
                        this.TextBox.Text = $"{text} ";
                    TranslationModel? translationModel = await Translator.Translate(text, from, to, fromClipboard);
                    if (translationModel != null && translationModel.Success)
                    {
                        if (TextBox.Text == text)
                        {
                            mainTranslation.Content = translationModel.Translations[0];
                            if (translationModel.Translations.Count > 1)
                                AddOtherTranslations(translationModel.Translations);
                            if (translationModel.Translations.Count == 1)
                            {
                                RemoveOtherTranslations();
                            }
                        }
                    }
                    else
                    {
                        mainTranslation.Content = "";
                        RemoveOtherTranslations();
                        ShowNotifyMessage("an error Occurred", true, 1500);
                    }
                }
                catch
                {
                    mainTranslation.Content = "";
                    RemoveOtherTranslations();
                    ShowNotifyMessage("an error Occurred", true, 1500);
                }
            }
            else
            {
                mainTranslation.Content = "";
                RemoveOtherTranslations();
                ShowNotifyMessage("No internet connection", true, 1500);
            }
        }

        bool isNotifyShowing = false;
        private async void ShowNotifyMessage(string message, bool warn = false, int duration = 1000)
        {
            if (!isNotifyShowing && alertbox != null)
            {
                isNotifyShowing = true;
                if (warn)
                    infoicon.Visibility = Visibility.Collapsed;
                this.alertText.Text = message;
                alertbox.Visibility = Visibility.Visible;
                this.alertbox.Focus();
                alertbox.BeginAnimation(UIElement.OpacityProperty, new DoubleAnimation(0.9, TimeSpan.FromMilliseconds(200)));
                await Task.Delay(duration);
                alertbox.BeginAnimation(UIElement.OpacityProperty, new DoubleAnimation(0, TimeSpan.FromMilliseconds(200)));
                alertbox.Visibility = Visibility.Collapsed;
                infoicon.Visibility = Visibility.Visible;
                isNotifyShowing = false;
                this.Focus();
                this.TextBox.Focus();
            }
        }
        private void Arrow_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            string from = FromLanguage.SelectedItem.ToString();
            string to = ToLanguage.SelectedItem.ToString();
            FromLanguage.SelectedItem = to;
            ToLanguage.SelectedItem = from;
        }
        private void SettingsMenuItem_Click(object sender, RoutedEventArgs e)
        {
            SettingsWindow sn = new SettingsWindow();
            sn.Show();
        }

        private void ClipInterceptMenuItem_Click(object sender, RoutedEventArgs e)
        {
            if (ClipIntercept.Intercepting)
            {
                ClipIntercept.StopClipIntercept();
                ClipInterceptOn.Visibility = Visibility.Visible;
                ClipInterceptOff.Visibility = Visibility.Collapsed;
            }
            else
            {
                if (GlobalSettings.Settings.FilterInvalidChar)
                    ClipIntercept.StartClipIntercept(filtered);
                else
                    ClipIntercept.StartClipIntercept(new List<char>());
                ClipInterceptOn.Visibility = Visibility.Collapsed;
                ClipInterceptOff.Visibility = Visibility.Visible;
            }
        }

        private void AboutContextMenu_Click(object sender, RoutedEventArgs e)
        {
            About aboutWindow = new About();
            aboutWindow.ShowDialog();
        }

        private void FromLanguage_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ToLanguage.SelectedItem != null)
            {
                if (FromLanguage.SelectedItem.ToString() == ToLanguage.SelectedItem.ToString())
                {
                    ToLanguage.SelectedItem = e.RemovedItems[0];
                }
                if (FromLanguage.SelectedItem != null && FromLanguage.SelectedItem != ToLanguage.SelectedItem)
                {
                    GlobalSettings.Settings.FromLanguage = FromLanguage.SelectedItem.ToString();
                    GlobalSettings.SaveSettings(GlobalSettings.Settings);
                }
                DisplayTranslate(TextBox.Text);
                TextBox.Focus();
            }
        }

        private void ToLanguage_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (FromLanguage.SelectedItem != null)
            {
                if (FromLanguage.SelectedItem.ToString() == ToLanguage.SelectedItem.ToString())
                {
                    FromLanguage.SelectedItem = e.RemovedItems[0];
                }
                if (FromLanguage.SelectedItem != null && FromLanguage.SelectedItem != ToLanguage.SelectedItem)
                {
                    GlobalSettings.Settings.ToLanguage = ToLanguage.SelectedItem.ToString();
                    GlobalSettings.SaveSettings(GlobalSettings.Settings);
                }
                DisplayTranslate(TextBox.Text);
                TextBox.Focus();
            }
        }
    }
}