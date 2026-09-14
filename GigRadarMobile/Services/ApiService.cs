using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using GigRadarMobile.Models;

namespace GigRadarMobile.Services
{
    public class ApiService
    {
        private readonly HttpClient _http;
        private readonly JsonSerializerOptions _jsonOptions;

        public ApiService(HttpClient httpClient)
        {
            _http = httpClient;
            _http.BaseAddress = new Uri(ApiConfiguration.BaseUrl);
            _http.Timeout = TimeSpan.FromSeconds(30);
            _jsonOptions = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
        }

        public void SetAuthToken(string? token)
        {
            _http.DefaultRequestHeaders.Authorization =
                string.IsNullOrWhiteSpace(token)
                    ? null
                    : new AuthenticationHeaderValue("Bearer", token);
        }

        /// <summary>Pesan ramah untuk exception jaringan yang umum (tanpa bahasa Inggris teknis).</summary>
        private static string FriendlyNetworkMessage(Exception ex)
        {
            if (ex is HttpRequestException || ex.InnerException is HttpRequestException)
                return "Tidak bisa terhubung ke server. Pastikan API berjalan lalu coba lagi.";
            if (ex is TaskCanceledException)
                return "Koneksi timeout. Periksa jaringan lalu coba lagi.";
            return ex.Message;
        }

        /// <summary>
        /// GET yang melempar exception saat jaringan gagal atau server merespons error —
        /// sehingga ViewModel bisa membedakan "memang kosong" dari "gagal memuat"
        /// (error state inline + retry di tiap halaman).
        /// </summary>
        private async Task<T?> GetAsync<T>(string url)
        {
            HttpResponseMessage response;
            try
            {
                response = await _http.GetAsync(url);
            }
            catch (Exception ex)
            {
                throw new HttpRequestException(FriendlyNetworkMessage(ex), ex);
            }

            if (!response.IsSuccessStatusCode)
                throw new HttpRequestException($"Server merespons {(int)response.StatusCode}");

            var json = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<T>(json, _jsonOptions);
        }

        /// <summary>
        /// POST: melempar exception saat jaringan gagal atau server error (5xx).
        /// Untuk 4xx, body tetap dikembalikan agar pesan validasi dari server
        /// (mis. "Email sudah terdaftar") bisa dibaca oleh pemanggil.
        /// </summary>
        private async Task<T?> PostAsync<T>(string url, object data)
        {
            HttpResponseMessage response;
            try
            {
                response = await _http.PostAsJsonAsync(url, data, _jsonOptions);
            }
            catch (Exception ex)
            {
                throw new HttpRequestException(FriendlyNetworkMessage(ex), ex);
            }

            if ((int)response.StatusCode >= 500)
                throw new HttpRequestException($"Server sedang bermasalah ({(int)response.StatusCode}). Coba lagi nanti.");

            var json = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<T>(json, _jsonOptions);
        }

        /// <summary>
        /// PUT: melempar exception saat jaringan gagal atau server error (5xx).
        /// Untuk 4xx, body tetap dikembalikan agar pemanggil bisa menilai hasilnya.
        /// </summary>
        private async Task<T?> PutAsync<T>(string url, object data)
        {
            HttpResponseMessage response;
            try
            {
                response = await _http.PutAsJsonAsync(url, data, _jsonOptions);
            }
            catch (Exception ex)
            {
                throw new HttpRequestException(FriendlyNetworkMessage(ex), ex);
            }

            if ((int)response.StatusCode >= 500)
                throw new HttpRequestException($"Server sedang bermasalah ({(int)response.StatusCode}). Coba lagi nanti.");

            var json = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<T>(json, _jsonOptions);
        }

        // ── Auth ──────────────────────────────────────────

        public async Task<(bool Success, string Message, string? Token, User? User)> LoginAsync(string email, string password)
        {
            var result = await PostAsync<LoginResponse>("/api/auth/login", new { email, password });
            if (result?.Token == null) return (false, result?.Message ?? "Login failed", null, null);
            SetAuthToken(result.Token);
            return (true, result.Message, result.Token, result.User);
        }

        /// <summary>Registrasi publik — role tetap "User"; Artist/EO masuk status Pending menunggu verifikasi admin (§25).</summary>
        public async Task<(bool Success, string Message, string? Token, User? User)> RegisterAsync(string name, string email, string password, string? requestedRole = null)
        {
            var result = await PostAsync<RegisterResponse>("/api/auth/register", new { name, email, password, requestedRole });
            if (result?.Token == null) return (false, result?.Message ?? "Register failed", null, null);
            SetAuthToken(result.Token);
            return (true, result.Message, result.Token, result.User);
        }

        // ── Events ────────────────────────────────────────

        public async Task<List<GigEvent>> GetEventsAsync()
        {
            return await GetAsync<List<GigEvent>>("/api/events") ?? new();
        }

        public async Task<GigEvent?> GetEventAsync(int id)
        {
            return await GetAsync<GigEvent>($"/api/events/{id}");
        }

        public async Task<List<GigEvent>> GetNearbyEventsAsync(double lat, double lng, double radius = 50)
        {
            return await GetAsync<List<GigEvent>>($"/api/events/nearby?lat={lat}&lng={lng}&radius={radius}") ?? new();
        }

        public async Task<List<GigEvent>> GetTonightEventsAsync()
        {
            return await GetAsync<List<GigEvent>>("/api/events/tonight") ?? new();
        }

        public async Task<List<GigEvent>> GetWeekendEventsAsync()
        {
            return await GetAsync<List<GigEvent>>("/api/events/weekend") ?? new();
        }

        public async Task<List<GigEvent>> GetRecommendedEventsAsync()
        {
            return await GetAsync<List<GigEvent>>("/api/events/recommended") ?? new();
        }

        // ── Artists ───────────────────────────────────────

        public async Task<List<Artist>> GetArtistsAsync()
        {
            return await GetAsync<List<Artist>>("/api/artists") ?? new();
        }

        public async Task<Artist?> GetArtistAsync(int id)
        {
            return await GetAsync<Artist>($"/api/artists/{id}");
        }

        public async Task<List<AudioTrack>> GetArtistTracksAsync(int artistId)
        {
            return await GetAsync<List<AudioTrack>>($"/api/artists/{artistId}/tracks") ?? new();
        }

        public async Task<List<ArtistAlbum>> GetArtistAlbumsAsync(int artistId)
        {
            return await GetAsync<List<ArtistAlbum>>($"/api/artists/{artistId}/albums") ?? new();
        }

        public async Task<List<ArtistPost>> GetArtistPostsAsync(int artistId)
        {
            return await GetAsync<List<ArtistPost>>($"/api/artists/{artistId}/posts") ?? new();
        }

        public async Task<List<ArtistJourneyItem>> GetArtistJourneyAsync(int artistId)
        {
            return await GetAsync<List<ArtistJourneyItem>>($"/api/artists/{artistId}/journey") ?? new();
        }

        public async Task<List<ArtistGig>> GetArtistEventsAsync(int artistId)
        {
            return await GetAsync<List<ArtistGig>>($"/api/artists/{artistId}/events") ?? new();
        }

        // ── Artist: manajemen (role Artist) ───────────────

        public async Task<ArtistProfile?> GetMyArtistProfileAsync()
        {
            return await GetAsync<ArtistProfile>("/api/artist/me");
        }

        public async Task<(bool Success, string Message)> UpdateMyArtistProfileAsync(
            string? name, string? bio, string? genre, string? city,
            string? photoUrl, string? coverUrl, string? socialLinks)
        {
            var response = await _http.PutAsJsonAsync("/api/artist/me", new
            {
                name, bio, genre, city, photoUrl, coverUrl, socialLinks
            }, _jsonOptions);
            return await ParseResultAsync(response);
        }

        public async Task<ArtistDashboard?> GetArtistDashboardAsync()
        {
            return await GetAsync<ArtistDashboard>("/api/artist/me/dashboard");
        }

        public async Task<(bool Success, string Message)> CreateTrackAsync(
            string title, string audioUrl, string coverUrl, string genre, int durationSeconds, DateTime? releaseDate)
        {
            var response = await _http.PostAsJsonAsync("/api/artist/tracks", new
            {
                title, audioUrl, coverUrl, genre, durationSeconds, releaseDate
            }, _jsonOptions);
            return await ParseResultAsync(response);
        }

        public async Task<(bool Success, string Message)> UpdateTrackAsync(
            int trackId, string title, string audioUrl, string coverUrl, string genre, int durationSeconds, DateTime? releaseDate)
        {
            var response = await _http.PutAsJsonAsync($"/api/artist/tracks/{trackId}", new
            {
                title, audioUrl, coverUrl, genre, durationSeconds, releaseDate
            }, _jsonOptions);
            return await ParseResultAsync(response);
        }

        public async Task<(bool Success, string Message)> DeleteTrackAsync(int trackId)
        {
            var response = await _http.DeleteAsync($"/api/artist/tracks/{trackId}");
            return await ParseResultAsync(response);
        }

        public async Task<(bool Success, string Message)> CreateAlbumAsync(
            string title, string coverUrl, string description, DateTime? releaseDate)
        {
            var response = await _http.PostAsJsonAsync("/api/artist/albums", new
            {
                title, coverUrl, description, releaseDate
            }, _jsonOptions);
            return await ParseResultAsync(response);
        }

        public async Task<(bool Success, string Message)> UpdateAlbumAsync(
            int albumId, string title, string coverUrl, string description, DateTime? releaseDate)
        {
            var response = await _http.PutAsJsonAsync($"/api/artist/albums/{albumId}", new
            {
                title, coverUrl, description, releaseDate
            }, _jsonOptions);
            return await ParseResultAsync(response);
        }

        public async Task<(bool Success, string Message)> DeleteAlbumAsync(int albumId)
        {
            var response = await _http.DeleteAsync($"/api/artist/albums/{albumId}");
            return await ParseResultAsync(response);
        }

        public async Task<(bool Success, string Message)> CreatePostAsync(
            string title, string content, string imageUrl, bool isPublished)
        {
            var response = await _http.PostAsJsonAsync("/api/artist/posts", new
            {
                title, content, imageUrl, isPublished
            }, _jsonOptions);
            return await ParseResultAsync(response);
        }

        public async Task<(bool Success, string Message)> UpdatePostAsync(
            int postId, string title, string content, string imageUrl, bool isPublished)
        {
            var response = await _http.PutAsJsonAsync($"/api/artist/posts/{postId}", new
            {
                title, content, imageUrl, isPublished
            }, _jsonOptions);
            return await ParseResultAsync(response);
        }

        public async Task<(bool Success, string Message)> DeletePostAsync(int postId)
        {
            var response = await _http.DeleteAsync($"/api/artist/posts/{postId}");
            return await ParseResultAsync(response);
        }

        public async Task<(bool Success, string Message)> CreateJourneyAsync(
            string title, string description, string imageUrl, string category, DateTime? date)
        {
            var response = await _http.PostAsJsonAsync("/api/artist/journey", new
            {
                title, description, imageUrl, category, date
            }, _jsonOptions);
            return await ParseResultAsync(response);
        }

        public async Task<(bool Success, string Message)> UpdateJourneyAsync(
            int journeyId, string title, string description, string imageUrl, string category, DateTime? date)
        {
            var response = await _http.PutAsJsonAsync($"/api/artist/journey/{journeyId}", new
            {
                title, description, imageUrl, category, date
            }, _jsonOptions);
            return await ParseResultAsync(response);
        }

        public async Task<(bool Success, string Message)> DeleteJourneyAsync(int journeyId)
        {
            var response = await _http.DeleteAsync($"/api/artist/journey/{journeyId}");
            return await ParseResultAsync(response);
        }

        public async Task<(bool Success, string Message)> CreateMemberAsync(string name, string role, string photoUrl, DateTime? joinedAt)
        {
            var response = await _http.PostAsJsonAsync("/api/artist/members", new
            {
                name, role, photoUrl, joinedAt
            }, _jsonOptions);
            return await ParseResultAsync(response);
        }

        public async Task<(bool Success, string Message)> DeleteMemberAsync(int memberId)
        {
            var response = await _http.DeleteAsync($"/api/artist/members/{memberId}");
            return await ParseResultAsync(response);
        }

        // ── Follow artist (§30) ───────────────────────────

        public async Task<(bool Success, string Message)> FollowArtistAsync(int artistId)
        {
            var response = await _http.PostAsJsonAsync($"/api/users/follows/{artistId}", new { }, _jsonOptions);
            return await ParseResultAsync(response);
        }

        public async Task<(bool Success, string Message)> UnfollowArtistAsync(int artistId)
        {
            var response = await _http.DeleteAsync($"/api/users/follows/{artistId}");
            return await ParseResultAsync(response);
        }

        // ── Tickets ───────────────────────────────────────

        public async Task<List<Ticket>> GetMyTicketsAsync()
        {
            return await GetAsync<List<Ticket>>("/api/tickets") ?? new();
        }

        public async Task<List<EventTicketType>> GetEventTicketTypesAsync(int eventId)
        {
            return await GetAsync<List<EventTicketType>>($"/api/tickets/event/{eventId}/types") ?? new();
        }

        /// <summary>
        /// Membeli tiket dengan data diri pembeli. Mengembalikan status sukses,
        /// pesan (termasuk pesan validasi dari server), dan tiket bila berhasil.
        /// </summary>
        public async Task<(bool Success, string Message, Ticket? Ticket)> PurchaseTicketAsync(
            int eventId, int ticketTypeId,
            string fullName, string phone, string email, DateTime? dateOfBirth)
        {
            var response = await _http.PostAsJsonAsync("/api/tickets", new
            {
                eventId,
                ticketTypeId,
                fullName,
                phone,
                email,
                dateOfBirth
            }, _jsonOptions);

            var json = await response.Content.ReadAsStringAsync();
            var envelope = JsonSerializer.Deserialize<PurchaseEnvelope>(json, _jsonOptions);

            if (response.IsSuccessStatusCode && envelope?.Ticket != null)
                return (true, envelope.Message ?? "Tiket berhasil dibeli", envelope.Ticket);

            return (false, envelope?.Message ?? "Gagal membeli tiket", null);
        }

        // ── Admin / EO: kelola tipe tiket ────────────────

        /// <summary>Event yang dikelola pengguna (EO: event miliknya, Admin: semua).</summary>
        public async Task<List<GigEvent>> GetManagedEventsAsync()
        {
            return await GetAsync<List<GigEvent>>("/api/events/managed") ?? new();
        }

        public async Task<(bool Success, string Message)> CreateTicketTypeAsync(
            int eventId, string name, string description, decimal price, int stock, int sortOrder)
        {
            var response = await _http.PostAsJsonAsync($"/api/tickets/event/{eventId}/types", new
            {
                name, description, price, stock, sortOrder
            }, _jsonOptions);
            return await ParseResultAsync(response);
        }

        public async Task<(bool Success, string Message)> UpdateTicketTypeAsync(
            int typeId, string name, string description, decimal price, int stock, int sortOrder)
        {
            var response = await _http.PutAsJsonAsync($"/api/tickets/types/{typeId}", new
            {
                name, description, price, stock, sortOrder
            }, _jsonOptions);
            return await ParseResultAsync(response);
        }

        public async Task<(bool Success, string Message)> DeleteTicketTypeAsync(int typeId)
        {
            var response = await _http.DeleteAsync($"/api/tickets/types/{typeId}");
            return await ParseResultAsync(response);
        }

        private async Task<(bool Success, string Message)> ParseResultAsync(HttpResponseMessage response)
        {
            var json = await response.Content.ReadAsStringAsync();
            var envelope = JsonSerializer.Deserialize<ResultEnvelope>(json, _jsonOptions);
            if (response.IsSuccessStatusCode)
                return (true, envelope?.Message ?? "Berhasil");
            return (false, envelope?.Message ?? "Gagal, coba lagi");
        }

        /// <summary>Ubah status event (Published/Draft/SoldOut/Completed) — EO/Admin.</summary>
        public async Task<(bool Success, string Message)> UpdateEventStatusAsync(int eventId, string status)
        {
            var response = await _http.PutAsJsonAsync($"/api/events/{eventId}/status", new { status }, _jsonOptions);
            return await ParseResultAsync(response);
        }

        /// <summary>Hapus event beserta relasinya (tiket, favorit, line-up) — EO pemilik / Admin.</summary>
        public async Task<(bool Success, string Message)> DeleteEventAsync(int eventId)
        {
            var response = await _http.DeleteAsync($"/api/events/{eventId}");
            return await ParseResultAsync(response);
        }

        // ── Users ─────────────────────────────────────────

        public async Task<User?> GetProfileAsync()
        {
            return await GetAsync<User>("/api/users/me");
        }

        /// <summary>Daftar seluruh user — khusus role Admin (konsol Admin, GET /api/users).</summary>
        public async Task<List<User>> GetUsersAsync()
        {
            return await GetAsync<List<User>>("/api/users") ?? new();
        }

        public async Task<User?> UpdateProfileAsync(string? name, string? city, string? photoUrl = null)
        {
            return await PutAsync<User>("/api/users/me", new { name, city, photoUrl });
        }

        /// <summary>Ringkasan dashboard untuk profil EO (statistik agregat + per event).</summary>
        public async Task<EoDashboard?> GetEoDashboardAsync()
        {
            return await GetAsync<EoDashboard>("/api/events/managed/summary");
        }

        public async Task<bool> UpdatePreferencesAsync(List<int> genreIds)
        {
            var response = await _http.PostAsJsonAsync("/api/users/preferences", genreIds, _jsonOptions);
            return response.IsSuccessStatusCode;
        }

        // ── Role requests (Admin) ───────────────────────

        /// <summary>Daftar permohonan role — khusus Admin (GET /api/users/role-requests).</summary>
        public async Task<List<RoleRequestItem>> GetRoleRequestsAsync(string status = "Pending")
        {
            return await GetAsync<List<RoleRequestItem>>($"/api/users/role-requests?status={status}") ?? new();
        }

        /// <summary>Admin menyetujui permohonan role (POST /api/users/role-requests/{id}/approve).</summary>
        public async Task<(bool Success, string Message)> ApproveRoleRequestAsync(int requestId)
        {
            return await PostRoleRequestActionAsync($"/api/users/role-requests/{requestId}/approve");
        }

        /// <summary>Admin menolak permohonan role (POST /api/users/role-requests/{id}/reject).</summary>
        public async Task<(bool Success, string Message)> RejectRoleRequestAsync(int requestId)
        {
            return await PostRoleRequestActionAsync($"/api/users/role-requests/{requestId}/reject");
        }

        private async Task<(bool Success, string Message)> PostRoleRequestActionAsync(string url)
        {
            var response = await _http.PostAsync(url, content: null);
            var json = await response.Content.ReadAsStringAsync();
            if (!response.IsSuccessStatusCode)
            {
                var err = JsonSerializer.Deserialize<ApiErrorMessage>(json, _jsonOptions);
                return (false, err?.Message ?? "Gagal memproses permohonan");
            }
            var ok = JsonSerializer.Deserialize<ApiErrorMessage>(json, _jsonOptions);
            return (true, ok?.Message ?? "Berhasil");
        }

        // ── Genres ────────────────────────────────────────

        public async Task<List<Genre>> GetGenresAsync()
        {
            return await GetAsync<List<Genre>>("/api/genres") ?? new();
        }

        // ── Venues ────────────────────────────────────────

        /// <summary>Daftar venue — dipakai form pembuatan event (pemilihan tempat).</summary>
        public async Task<List<Venue>> GetVenuesAsync()
        {
            return await GetAsync<List<Venue>>("/api/venues") ?? new();
        }

        /// <summary>Tambah venue baru (EO/Admin) untuk event di tempat yang belum terdaftar.</summary>
        public async Task<(bool Success, string Message, Venue? Venue)> CreateVenueAsync(
            string name, string city, string address, int capacity)
        {
            var response = await _http.PostAsJsonAsync("/api/venues", new
            {
                name,
                city,
                address,
                capacity
            }, _jsonOptions);

            var json = await response.Content.ReadAsStringAsync();
            var envelope = JsonSerializer.Deserialize<VenueEnvelope>(json, _jsonOptions);

            if (response.IsSuccessStatusCode && envelope?.Venue != null)
                return (true, envelope.Message ?? "Venue berhasil ditambahkan", envelope.Venue);

            return (false, envelope?.Message ?? "Gagal menambahkan venue", null);
        }

        // ── EO/Admin: buat event baru ────────────────────

        /// <summary>Buat event baru. Event otomatis tercatat sebagai milik EO/Admin yang login.</summary>
        public async Task<(bool Success, string Message, int EventId)> CreateEventAsync(CreateEventData data)
        {
            // Nama properti dikirim camelCase agar cocok dengan model server.
            var response = await _http.PostAsJsonAsync("/api/events", data,
                new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });

            var json = await response.Content.ReadAsStringAsync();
            var envelope = JsonSerializer.Deserialize<EventCreateEnvelope>(json, _jsonOptions);

            if (response.IsSuccessStatusCode)
                return (true, envelope?.Message ?? "Event berhasil dibuat", envelope?.Id ?? 0);

            return (false, envelope?.Message ?? "Gagal membuat event", 0);
        }

        // ── Favorites ─────────────────────────────────────

        /// <summary>Toggle favorit — mengembalikan state akhir dari server (saved = true berarti tersimpan).</summary>
        public async Task<bool> ToggleFavoriteAsync(int eventId)
        {
            var envelope = await PostAsync<FavoriteToggleEnvelope>($"/api/users/favorites/{eventId}", new { });
            return envelope?.Saved ?? false;
        }

        private class FavoriteToggleEnvelope
        {
            public string? Message { get; set; }
            public bool Saved { get; set; }
        }

        /// <summary>Daftar event yang disimpan user login (GET /api/users/favorites).</summary>
        public async Task<List<FavoriteItem>> GetFavoritesAsync()
        {
            return await GetAsync<List<FavoriteItem>>("/api/users/favorites") ?? new();
        }

        private class PurchaseEnvelope
        {
            public string Message { get; set; } = "";
            public Ticket? Ticket { get; set; }
        }

        private class VenueEnvelope
        {
            public string Message { get; set; } = "";
            public Venue? Venue { get; set; }
        }

        private class EventCreateEnvelope
        {
            public string Message { get; set; } = "";
            public int Id { get; set; }
        }

        private class ResultEnvelope
        {
            public string Message { get; set; } = "";
        }

        private class LoginResponse
        {
            public string Message { get; set; } = "";
            public string Token { get; set; } = "";
            public User? User { get; set; }
        }

        private class RegisterResponse
        {
            public string Message { get; set; } = "";
            public string Token { get; set; } = "";
            public User? User { get; set; }
        }

        private class ApiErrorMessage
        {
            public string Message { get; set; } = "";
        }
    }

    /// <summary>Baris favorit dari GET /api/users/favorites (event ikut disertakan server).</summary>
    public class FavoriteItem
    {
        public int FavoriteId { get; set; }
        public int UserId { get; set; }
        public int EventId { get; set; }
        public GigEvent? Event { get; set; }
    }

    /// <summary>Baris permohonan role untuk konsol Admin (GET /api/users/role-requests).</summary>
    public class RoleRequestItem
    {
        public int RequestId { get; set; }
        public int UserId { get; set; }
        public string UserName { get; set; } = "";
        public string UserEmail { get; set; } = "";
        public string RequestedRole { get; set; } = "";
        public string Status { get; set; } = "Pending";
        public DateTime CreatedAt { get; set; }

        public string RequestedRoleDisplay => RequestedRole switch
        {
            "EO" => "Event Organizer",
            "Artist" => "Artist",
            _ => RequestedRole
        };
    }
}
