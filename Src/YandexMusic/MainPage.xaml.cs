
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Windows.Media.Playback;
using Windows.Storage;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;
using Yandex.Music.Api;
using Yandex.Music.Api.Models.Account;
using Yandex.Music.Api.Models.Track;

namespace YandexMusicUWP
{
    public sealed partial class MainPage : Page
    {
        private string _authToken;
        private readonly YandexMusicApi _api = new YandexMusicApi(); 
        private List<YTrack> _tracks = new List<YTrack>();
        private YTrack _selectedTrack;


        public MainPage()
        {
            InitializeComponent();
            LoadToken();//InitializeApi();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            LoadToken();
            InitializeApi();
        }

        private void LoadToken()
        {
            var localSettings = ApplicationData.Current.LocalSettings;
            if (localSettings.Values.ContainsKey("YandexMusicToken"))
            {
                _authToken = localSettings.Values["YandexMusicToken"].ToString();
            }
        }


        private async void InitializeApi()
        {
            // Пытаемся авторизоваться по полной 
            try
            {
                Yandex.Music.Api.Common.AuthStorage storage = new Yandex.Music.Api.Common.AuthStorage()
                {
                    IsAuthorized = true,
                    Token = _authToken,
                };
                await _api.User.AuthorizeAsync(storage, default);
            }
            catch (Exception ex)
            { // Проблемы. Значит, guest account
                Debug.WriteLine("[ex] InitializeApi - User.Authorize error: " + ex.Message);

                // Попытка авторизоваться как гость
                //await _api.User.AuthorizeAsync();
            }
        }


        private void SettingsButton_Click(object sender, RoutedEventArgs e)
        {
            Frame.Navigate(typeof(LoginPage));
        }


        private async void SearchButton_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(SearchBox.Text)) return;

            try
            {
                Yandex.Music.Api.Common.AuthStorage storage = new Yandex.Music.Api.Common.AuthStorage()
                {
                    IsAuthorized = true,
                    Token = _authToken,
                };

                // Perform search
                Yandex.Music.Api.Models.Common.YResponse<Yandex.Music.Api.Models.Search.YSearch> searchResponse
                    = await _api.Search.TrackAsync(storage, SearchBox.Text);

                // Convert YSearchResult<YSearchTrackModel> to List<YTrack>
                _tracks = searchResponse.Result.Tracks.Results
                    .Select(track => new YTrack
                    {
                        Id = track.Id,
                        Title = track.Title,
                        Artists = track.Artists,
                        Albums = default,//track.Albums,
                        DurationMs = track.DurationMs,
                        CoverUri = track.CoverUri,
                        // Map other properties as needed
                    })
                    .ToList();

                TracksList.ItemsSource = _tracks.Select(t => new
                {
                    t.Title,
                    Artists = string.Join(", ", t.Artists.Select(a => a.Name)),
                    t.Id
                });
            }
            catch (Exception ex)
            {
                // Handle errors
                Debug.WriteLine("[ex] Search error: " + ex.Message);
            }
        }

        private void TracksList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (TracksList.SelectedIndex == -1) return;

            _selectedTrack = _tracks[TracksList.SelectedIndex];
            PlayButton.IsEnabled = true;
        }

     
        private async void PlayButton_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedTrack == null) return;

            try
            {
                Yandex.Music.Api.Common.AuthStorage authstorage = new Yandex.Music.Api.Common.AuthStorage()
                {
                    IsAuthorized = true,
                    Token = _authToken,
                };

                // Get track download metadata
                var downloadInfoResponse = await _api.Track.GetMetadataForDownloadAsync(authstorage, _selectedTrack.Id.ToString());
                var downloadInfo = downloadInfoResponse.Result.FirstOrDefault();

                if (downloadInfo == null)
                {
                    Debug.WriteLine("[ex] Play error: No download info available for the selected track.");
                    return;
                }

                // Get file link
                var fileLink = await _api.Track.GetFileLinkAsync(authstorage, _selectedTrack.Id);

                // Play track
                var player = BackgroundMediaPlayer.Current;
                player.SetUriSource(new Uri(fileLink));
                player.Play();
            }
            catch (Exception ex)
            {
                // Handle playback errors
                Debug.WriteLine("[ex] Play error: " + ex.Message);
            }
        }
    }
}
