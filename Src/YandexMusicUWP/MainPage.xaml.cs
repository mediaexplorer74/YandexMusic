
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
            //TODO
            await _api.User.AuthorizeAsync(default,default);
        }

        private async void SearchButton_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(SearchBox.Text)) return;

            try
            {
                //TODO
                // Perform search
                var searchResponse = await _api.Search.TrackAsync(default,SearchBox.Text);
                _tracks = default;//searchResponse.Result.Tracks.Items.ToList();

                TracksList.ItemsSource = _tracks.Select(t => new
                {
                    t.Title,
                    t.Artists,
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
                // Get track link
                //TODO
                var downloadInfo = /*await*/ _api.Track.GetDownloadInfoAsync(default,_selectedTrack.Id.ToString());
                var storage = /*await*/ _api.Track.GetFileLinkAsync(/*downloadInfo.Result.First()*/default, _selectedTrack.Id);

                // Play track
                var player = BackgroundMediaPlayer.Current;
                player.SetUriSource(new Uri(storage.Result));
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
