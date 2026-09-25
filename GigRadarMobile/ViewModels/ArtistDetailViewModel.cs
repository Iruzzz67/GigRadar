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

        [ObservableProperty]
        public partial Artist? Artist { get; set; }
        [ObservableProperty]
        public partial ObservableCollection<CommunityPost> CommunityPosts { get; set; } = new();
        [ObservableProperty]
        public partial ObservableCollection<MerchItem> Merchandises { get; set; } = new();
        [ObservableProperty]
        public partial string NewCommunityPost { get; set; } = "";
        [ObservableProperty]
        public partial bool HasCommunityPosts { get; set; }
        [ObservableProperty]
        public partial bool HasMerchandises { get; set; }
        [ObservableProperty]
        public partial string PlaybackStatus { get; set; } = "Tap untuk preview lagu";
        [ObservableProperty]
        public partial ObservableCollection<ArtistGig> UpcomingGigs { get; set; } = new();
        [ObservableProperty]
        public partial bool HasUpcomingGigs { get; set; }
        [ObservableProperty]
        public partial bool HasMembers { get; set; }
        [ObservableProperty]
        public partial bool HasPosts { get; set; }

        // Komentar (Bab 32): state panel komentar post yang terbuka
        [ObservableProperty]
        public partial bool IsCommentsOpen { get; set; }
        [ObservableProperty]
        public partial ObservableCollection<PostCommentItem> Comments { get; set; } = new();
        [ObservableProperty]
        public partial bool HasComments { get; set; }
        [ObservableProperty]
        public partial string NewComment { get; set; } = "";
        [ObservableProperty]
        public partial bool IsSendingComment { get; set; }
        private int _openPostId;

        // ── Follow artist (§10) ───────────────────────────
        [ObservableProperty]
        public partial bool IsFollowing { get; set; }
        [ObservableProperty]
        public partial bool IsFollowBusy { get; set; }

        public string FollowButtonText => IsFollowing ? "Mengikuti" : "Ikuti";

        /// <summary>Jumlah follower versi VM — ikut berubah saat tombol follow dipakai tanpa reload halaman.</summary>
        public int FollowersCount => Artist?.FollowersCount ?? 0;

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

            OnPropertyChanged(nameof(FollowersCount));

            HasMembers = value?.Members is { Count: > 0 };
            HasPosts = value?.Posts is { Count: > 0 };

            if (value != null)
            {
                _ = LoadUpcomingGigsAsync(value.ArtistId);
                _ = LoadFollowStateAsync(value.ArtistId);
                _ = LoadCommunityAndShopAsync(value.ArtistId);
            }
        }

        partial void OnIsFollowingChanged(bool value)
        {
            OnPropertyChanged(nameof(FollowButtonText));
        }

        /// <summary>
        /// Status follow diambil dari server (bukan disimpan lokal) supaya tombol
        /// tidak pernah menampilkan "Ikuti" padahal user sudah mengikuti artist ini.
        /// </summary>
        private async Task LoadFollowStateAsync(int artistId)
        {
            try
            {
                _api.SetAuthToken(_auth.GetToken());
                var follows = await _api.GetFollowsAsync();
                IsFollowing = follows.Any(f => f.ArtistId == artistId);
            }
            catch { /* status follow bukan konten utama — biarkan default bila gagal */ }
        }

        private async Task LoadCommunityAndShopAsync(int artistId)
        {
            try
            {
                // Community & merch keduanya [Authorize] di API — set token dulu supaya
                // aman saat halaman ini dibuka tepat setelah cold start (bukan via login).
                _api.SetAuthToken(_auth.GetToken());

                var posts = await _api.GetCommunityPostsAsync(artistId);
                CommunityPosts = new ObservableCollection<CommunityPost>(posts);
                HasCommunityPosts = CommunityPosts.Count > 0;

                var merchs = await _api.GetArtistMerchandiseAsync(artistId);
                Merchandises = new ObservableCollection<MerchItem>(merchs);
                HasMerchandises = Merchandises.Count > 0;
            }
            catch { }
        }

        [RelayCommand]
        private async Task CreateCommunityPostAsync()
        {
            if (Artist == null || string.IsNullOrWhiteSpace(NewCommunityPost)) return;

            IsFollowBusy = true; // reusing loading state for brevity
            try
            {
                var (success, msg) = await _api.CreateCommunityPostAsync(Artist.ArtistId, NewCommunityPost, "");
                if (success)
                {
                    NewCommunityPost = "";
                    await LoadCommunityAndShopAsync(Artist.ArtistId);
                }
                else
                {
                    await Alerts.ShowAsync("Gagal", msg);
                }
            }
            finally
            {
                IsFollowBusy = false;
            }
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
        private async Task OpenSpotifyAsync()
        {
            if (string.IsNullOrWhiteSpace(Artist?.SpotifyUrl))
            {
                await Alerts.ShowAsync("Spotify", "Artist ini belum menambahkan link Spotify.");
                return;
            }

            await Launcher.OpenAsync(Artist.SpotifyUrl);
        }

        [RelayCommand]
        private async Task GoBackAsync()
        {
            await Shell.Current.GoToAsync("..");
        }

        /// <summary>Ikuti / berhenti mengikuti artist (§10). State akhir selalu dari server.</summary>
        [RelayCommand]
        private async Task ToggleFollowAsync()
        {
            if (Artist == null || IsFollowBusy) return;

            try
            {
                IsFollowBusy = true;
                _api.SetAuthToken(_auth.GetToken());

                var (success, message) = IsFollowing
                    ? await _api.UnfollowArtistAsync(Artist.ArtistId)
                    : await _api.FollowArtistAsync(Artist.ArtistId);

                if (!success)
                {
                    await Alerts.ShowAsync("Gagal", message);
                    return;
                }

                IsFollowing = !IsFollowing;

                // Angka di layar langsung ikut berubah (server tetap sumber kebenaran
                // saat halaman dimuat ulang).
                Artist.FollowersCount = Math.Max(0, Artist.FollowersCount + (IsFollowing ? 1 : -1));
                OnPropertyChanged(nameof(FollowersCount));
                OnPropertyChanged(nameof(FollowersLabel));
            }
            catch (Exception ex)
            {
                await Alerts.ShowAsync("Error", ex.Message);
            }
            finally
            {
                IsFollowBusy = false;
            }
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
