using System.Diagnostics;
using Windows.Storage;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace YandexMusicUWP
{
    public sealed partial class LoginPage : Page
    {
        public LoginPage()
        {
            InitializeComponent();
            LoadSavedToken();
        }

        private void LoadSavedToken()
        {
            var localSettings = ApplicationData.Current.LocalSettings;
            if (localSettings.Values.ContainsKey("YandexMusicToken"))
            {
                TokenBox.Text = localSettings.Values["YandexMusicToken"].ToString();
            }
        }

        private void SaveToken_Click(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrWhiteSpace(TokenBox.Text))
            {
                var localSettings = ApplicationData.Current.LocalSettings;
                localSettings.Values["YandexMusicToken"] = TokenBox.Text;

                // Возвращаемся на главную страницу
                if (Frame.CanGoBack) Frame.GoBack();
                else Frame.Navigate(typeof(MainPage));
            }
            else
            {
                // вывод предупреждения, если токен пустой
                Debug.WriteLine("[warn] Нет токена. Будет применяться Guest Access (гостевой доступ)");
            }
        }
    }
}