using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using GigRadarMobile.Helpers;
using GigRadarMobile.Models;
using GigRadarMobile.Services;
using GigRadarMobile.Views;

namespace GigRadarMobile.ViewModels
{
    [QueryProperty(nameof(Artist), "Artist")]
    public partial class ArtistDetailViewModel : ObservableObject
    {
        private readonly ApiService _api;
        private readonly AuthService _auth;
        private bool _navigating;

        [ObservableProperty] private Artist? _artist;
        [ObservableProperty] private string _playbackStatus = "Tap untuk preview lagu";
        [ObservableProperty] private ObservableCollection<ArtistGig> _upcomingGigs = new();
        [ObservableProperty] private bool _hasUpcomingGigs;
        [ObservableProperty] private bool _hasMembers;
        [ObservableProperty] private bool _hasPosts;

        // Komentar (Bab 32): state panel komentar post yang terbuka
        [ObservableProperty] private bool _isCommentsOpen;
        [ObservableProperty] private ObservableCollection<PostCommentItem> _comments = new();
        [ObservableProperty] private bool _hasComments;
        [ObservableProperty] private string _newComment = "";
        [ObservableProperty] private bool _isSendingComment;
        private int _openPostId;

        public string HeroImageUrl =>
            !string.IsNullOrWhiteSpace(Artist?.CoverUrl) ? Artist.CoverUrl : (Artist?.PhotoUrl ?? "");

        public string FollowersLabel => $"{FormatCount(Artist?.FollowersCount ?? 0)} pengikut";
        public string TracksLabel => $"{Artist?.Tracks.Count ?? 0} track";
        public string AlbumsLabel => $"{Artist?.Albums.Count ?? 0} album";
        public string LocationLabel =>
            string.IsNullOrWhiteSpace(Artist?.City) ? Artist?.Genre ?? "" : Artist.City;

        partial void OnArtistChanged(Artist? value)
        {
            OnPropertyChanged(nameof(HeroImageUrl));
            OnPropertyChanged(nameof(FollowersLabel));
            OnPropertyChanged(nameof(TracksLabel));
            OnPropertyChanged(nameof(AlbumsLabel));
            OnPropertyChanged(nameof(LocationLabel));

            HasMembers = value?.Members is { Count: > 0 };
            HasPosts = value?.Posts is { Count: > 0 };

            if (value != null)
                _ = LoadUpcomingGigsAsync(value.ArtistId);
        }

        public ArtistDetailViewModel(ApiService api, AuthService auth)
        {
            _api = api;
            _auth = auth;
        }

        private async Task LoadUpcomingGigsAsync(int artistId)
        {
            try
            {
                _api.SetAuthToken(_auth.GetToken());
                var gigs = await _api.GetArtistEventsAsync(artistId);
                var upcoming = gigs.Where(g => g.IsUpcoming).OrderBy(g => g.StartDate).ToList();
                UpcomingGigs = new ObservableCollection<ArtistGig>(upcoming);
                HasUpcomingGigs = UpcomingGigs.Count > 0;
            }
            catch
            {
                HasUpcomingGigs = false;
            }
        }

        [RelayCommand]
        private async Task PlayTrackAsync(AudioTrack? track)
        {
            if (track == null || string.IsNullOrEmpty(track.AudioUrl))
            {
                PlaybackStatus = "Preview belum tersedia";
                return;
            }

            PlaybackStatus = $"Memutar: {track.Title}";
            await Launcher.OpenAsync(track.AudioUrl);
        }

        [RelayCommand]
        private async Task GoBackAsync()
        {
            await Shell.Current.GoToAsync("..");
        }

        // ── Like & komentar post (Bab 32, 48) ────────────

        /// <summary>Like/unlike post (toggle — Bab 48).</summary>
        [RelayCommand]
        private async Task ToggleLikeAsync(ArtistPost? post)
        {
            if (post == null) return;
            try
            {
                _api.SetAuthToken(_auth.GetToken());
                var (success, _, count) = await _api.TogglePostLikeAsync(post.PostId);
                if (success)
                {
                    post.LikesCount = count;
                    OnPropertyChanged(nameof(Artist.Posts));
                }
            }
            catch (Exception ex)
            {
                await Alerts.ShowAsync("Error", ex.Message);
            }
        }

        /// <summary>Buka/tutup panel komentar sebuah post.</summary>
        [RelayCommand]
        private async Task ToggleCommentsAsync(ArtistPost? post)
        {
            if (post == null) return;

            if (IsCommentsOpen && _openPostId == post.PostId)
            {
                IsCommentsOpen = false;
                return;
            }

            try
            {
                _api.SetAuthToken(_auth.GetToken());
                var comments = await _api.GetPostCommentsAsync(post.PostId);
                Comments = new ObservableCollection<PostCommentItem>(comments);
                HasComments = Comments.Count > 0;
                _openPostId = post.PostId;
                IsCommentsOpen = true;
            }
            catch (Exception ex)
            {
                await Alerts.ShowAsync("Error", ex.Message);
            }
        }

        /// <summary>Kirim komentar untuk post yang panelnya terbuka.</summary>
        [RelayCommand]
        private async Task SendCommentAsync()
        {
            var content = NewComment?.Trim() ?? "";
            if (content.Length == 0 || _openPostId == 0) return;

            try
            {
                IsSendingComment = true;
                _api.SetAuthToken(_auth.GetToken());
                var (success, message) = await _api.AddPostCommentAsync(_openPostId, content);
                if (!success)
                {
                    await Alerts.ShowAsync("Gagal", message);
                    return;
                }

                NewComment = "";
                var comments = await _api.GetPostCommentsAsync(_openPostId);
                Comments = new ObservableCollection<PostCommentItem>(comments);
                HasComments = Comments.Count > 0;

                // Update counter komentar pada post yang terbuka.
                var post = Artist?.Posts.FirstOrDefault(p => p.PostId == _openPostId);
                if (post != null)
                {
                    post.CommentsCount = comments.Count;
                    OnPropertyChanged(nameof(Artist.Posts));
                }
            }
            catch (Exception ex)
            {
                await Alerts.ShowAsync("Error", ex.Message);
            }
            finally
            {
                IsSendingComment = false;
            }
        }

        /// <summary>
        /// Buka detail event penuh dari baris gig — data lengkap (venue, genre, line-up)
        /// diambil dari API supaya halaman detail tetap akurat.
        /// </summary>
        [RelayCommand]
        private async Task GoToGigAsync(ArtistGig? gig)
        {
            if (gig == null || _navigating) return;
            _navigating = true;

            try
            {
                _api.SetAuthToken(_auth.GetToken());
                var fullEvent = await _api.GetEventAsync(gig.EventId);

                if (fullEvent == null)
                {
                    await Alerts.ShowAsync("Info", "Detail event tidak ditemukan.");
                    return;
                }

                await Shell.Current.GoToAsync(nameof(EventDetailPage),
                    new Dictionary<string, object> { { "Event", fullEvent } });
            }
            catch (Exception ex)
            {
                await Alerts.ShowAsync("Error", $"Gagal membuka detail event: {ex.Message}");
            }
            finally
            {
                _navigating = false;
            }
        }

        private static string FormatCount(int count)
            => count >= 1000 ? $"{count / 1000f:0.#}rb" : count.ToString();
    }
}