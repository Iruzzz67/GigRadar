using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using GigRadarMobile.Helpers;
using GigRadarMobile.Models;
using GigRadarMobile.Services;

namespace GigRadarMobile.ViewModels
{
    /// <summary>Satu baris riwayat check-in pada halaman check-in.</summary>
    public class CheckInLogItem
    {
        public string BuyerName { get; init; } = string.Empty;
        public string TicketType { get; init; } = string.Empty;
        public string TimeLabel { get; init; } = string.Empty;
    }

    /// <summary>
    /// Halaman Check-in EO (GIGRADAR_APP_SPEC.md Bab 14): pindai QR tiket, validasi di
    /// server, tampilkan hasil + progres kehadiran event.
    ///
    /// Validasi selalu di server (bukan di client) dan hanya berlaku untuk event milik
    /// EO/Admin yang memindai (§24).
    /// </summary>
    [QueryProperty(nameof(GigEvent), "Event")]
    public partial class CheckInViewModel : ObservableObject
    {
        private readonly ApiService _api;
        private readonly AuthService _auth;

        /// <summary>Jendela waktu untuk mengabaikan pemindaian berulang kode yang sama.</summary>
        private static readonly TimeSpan DuplicateScanWindow = TimeSpan.FromSeconds(5);

        private string _lastCode = string.Empty;
        private DateTime _lastScanAt = DateTime.MinValue;

        [ObservableProperty]
        public partial GigEvent? GigEvent { get; set; }

        [ObservableProperty]
        public partial bool IsLoading { get; set; }
        [ObservableProperty]
        public partial bool IsBusy { get; set; }
        [ObservableProperty]
        public partial string ErrorMessage { get; set; } = string.Empty;
        [ObservableProperty]
        public partial bool HasError { get; set; }

        [ObservableProperty]
        public partial int CheckedIn { get; set; }
        [ObservableProperty]
        public partial int Total { get; set; }
        [ObservableProperty]
        public partial string ProgressLabel { get; set; } = "Checked in 0 / 0";

        [ObservableProperty]
        public partial string ManualCode { get; set; } = string.Empty;

        // Hasil check-in terakhir ("Valid" / "Invalid" / "Error")
        [ObservableProperty]
        public partial string LastStatus { get; set; } = "None";
        [ObservableProperty]
        public partial string LastMessage { get; set; } = string.Empty;

        public ObservableCollection<CheckInLogItem> RecentCheckIns { get; } = new();

        /// <summary>True bila sudah ada check-in pada sesi ini (menampilkan judul riwayat).</summary>
        [ObservableProperty]
        public partial bool HasCheckIns { get; set; }

        public string EventName => GigEvent?.Name ?? "Check-in";
        public string EventMeta => GigEvent == null
            ? string.Empty
            : $"{GigEvent.VenueName} • {GigEvent.DateFormatted}";

        public bool HasLastResult => LastStatus != "None";

        public bool IsLastValid => LastStatus == "Valid";

        public Color LastStatusColor => LastStatus switch
        {
            "Valid" => Color.FromArgb("#22C55E"),
            "Error" => Color.FromArgb("#F59E0B"),
            _ => Color.FromArgb("#F43F5E")
        };

        public CheckInViewModel(ApiService api, AuthService auth)
        {
            _api = api;
            _auth = auth;
        }

        partial void OnGigEventChanged(GigEvent? value)
        {
            OnPropertyChanged(nameof(EventName));
            OnPropertyChanged(nameof(EventMeta));
        }

        partial void OnLastStatusChanged(string value)
        {
            OnPropertyChanged(nameof(HasLastResult));
            OnPropertyChanged(nameof(IsLastValid));
            OnPropertyChanged(nameof(LastStatusColor));
        }

        partial void OnCheckedInChanged(int value) => UpdateProgressLabel();
        partial void OnTotalChanged(int value) => UpdateProgressLabel();

        private void UpdateProgressLabel() => ProgressLabel = $"Checked in {CheckedIn} / {Total}";

        /// <summary>Muat angka kehadiran awal saat halaman dibuka (tanpa memindai apa pun).</summary>
        [RelayCommand]
        private async Task LoadCountsAsync()
        {
            if (GigEvent == null) return;

            try
            {
                IsLoading = true;
                HasError = false;
                _api.SetAuthToken(_auth.GetToken());

                var report = await _api.GetEventAttendeesAsync(GigEvent.EventId);
                if (report == null)
                {
                    HasError = true;
                    ErrorMessage = "Data kehadiran tidak bisa dimuat.";
                    return;
                }

                Total = report.Total;
                CheckedIn = report.CheckedIn;
            }
            catch (Exception ex)
            {
                HasError = true;
                ErrorMessage = "Gagal memuat data kehadiran: " + ex.Message;
            }
            finally
            {
                IsLoading = false;
            }
        }

        /// <summary>
        /// Validasi satu kode tiket di server. Dipanggil dari hasil scan maupun input
        /// manual — dua-duanya jalur yang sama supaya aturannya konsisten.
        /// </summary>
        public async Task ValidateAsync(string? code)
        {
            var value = (code ?? string.Empty).Trim();
            if (value.Length == 0 || GigEvent == null) return;

            // Kamera memicu deteksi berulang selama QR masih di depan lensa.
            if (value == _lastCode && DateTime.UtcNow - _lastScanAt < DuplicateScanWindow) return;
            if (IsBusy) return;

            _lastCode = value;
            _lastScanAt = DateTime.UtcNow;

            try
            {
                IsBusy = true;
                _api.SetAuthToken(_auth.GetToken());

                var result = await _api.CheckInTicketAsync(GigEvent.EventId, value);

                if (result == null)
                {
                    LastStatus = "Error";
                    LastMessage = "Tidak bisa menghubungi server. Coba lagi.";
                    return;
                }

                // Angka hanya diperbarui bila server memang mengirimnya (respons 403
                // tidak menyertakan counter, dan 0 bukan berarti kosong).
                if (result.CheckedIn.HasValue) CheckedIn = result.CheckedIn.Value;
                if (result.Total.HasValue) Total = result.Total.Value;

                LastStatus = result.Valid ? "Valid" : "Invalid";
                LastMessage = result.Message;

                if (result.Valid)
                {
                    RecentCheckIns.Insert(0, new CheckInLogItem
                    {
                        BuyerName = string.IsNullOrWhiteSpace(result.Ticket?.BuyerName)
                            ? "Peserta"
                            : result.Ticket!.BuyerName,
                        TicketType = result.Ticket?.TicketType ?? string.Empty,
                        TimeLabel = DateTime.Now.ToString("HH:mm")
                    });
                    HasCheckIns = true;
                }
            }
            catch (Exception ex)
            {
                LastStatus = "Error";
                LastMessage = "Gagal check-in: " + ex.Message;
            }
            finally
            {
                IsBusy = false;
            }
        }

        /// <summary>Kirim kode yang diketik petugas (fallback saat kamera tidak bisa dipakai).</summary>
        [RelayCommand]
        private async Task SubmitManualAsync()
        {
            var code = ManualCode?.Trim() ?? string.Empty;
            if (code.Length == 0)
            {
                LastStatus = "Invalid";
                LastMessage = "Kode tiket wajib diisi.";
                return;
            }

            // Kode yang di-input manual selalu divalidasi ulang, jadi reset penanda duplikat.
            _lastCode = string.Empty;

            await ValidateAsync(code);
            ManualCode = string.Empty;
        }

        /// <summary>Buka daftar peserta event yang sama.</summary>
        [RelayCommand]
        private async Task OpenAttendeesAsync()
        {
            if (GigEvent == null) return;

            await Shell.Current.GoToAsync(nameof(Views.AttendeesPage),
                new Dictionary<string, object> { { "Event", GigEvent } });
        }
    }
}
