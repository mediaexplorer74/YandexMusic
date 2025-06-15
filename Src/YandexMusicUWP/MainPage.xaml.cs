
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Windows.Media.Playback;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Yandex.Music.Api;
using Yandex.Music.Api.Models.Track;

namespace YandexMusicUWP
{
    public sealed partial class MainPage : Page
    {
        readonly string AccessToken = ""; // Replace with your actual access token
        private readonly YandexMusicApi _api = new YandexMusicApi(); 
        private List<YTrack> _tracks = new List<YTrack>();
        private YTrack _selectedTrack;

        public MainPage()
        {
            InitializeComponent();
            InitializeApi();
        }

        private async void InitializeApi()
        {
            // Authenticate guest account
            try
            {
                Yandex.Music.Api.Common.AuthStorage storage = new Yandex.Music.Api.Common.AuthStorage()
                {
                    IsAuthorized = true,
                    Token = AccessToken,
                };
                await _api.User.AuthorizeAsync(storage, default);
            }
            catch (Exception ex)
            {
                Debug.WriteLine("[ex] InitializeApi - User.Authorize error: " + ex.Message);
            }
        }

        private async void SearchButton_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(SearchBox.Text)) return;

            try
            {
                Yandex.Music.Api.Common.AuthStorage storage = new Yandex.Music.Api.Common.AuthStorage()
                {
                    IsAuthorized = true,
                    Token = AccessToken,
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
                    Token = AccessToken,
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
