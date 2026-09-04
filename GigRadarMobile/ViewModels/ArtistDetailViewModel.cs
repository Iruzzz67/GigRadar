using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using GigRadarMobile.Models;

namespace GigRadarMobile.ViewModels
{
    [QueryProperty(nameof(Artist), "Artist")]
    public partial class ArtistDetailViewModel : ObservableObject
    {
        [ObservableProperty] private Artist? _artist;
        [ObservableProperty] private string _playbackStatus = "Tap to play preview";

        [RelayCommand]
        private async Task PlayTrackAsync(AudioTrack? track)
        {
            if (track == null || string.IsNullOrEmpty(track.AudioUrl))
            {
                PlaybackStatus = "No audio available";
                return;
            }

            PlaybackStatus = $"Playing: {track.Title}";
            await Launcher.OpenAsync(track.AudioUrl);
        }
    }
}
