using GigRadarMobile.Models;

namespace GigRadarMobile.Helpers
{
    /// <summary>
    /// Aturan filter tanggal &amp; harga untuk Explore (GIGRADAR_APP_SPEC.md §7).
    /// Dipisah dari ViewModel supaya aturannya bisa diuji tanpa UI.
    /// </summary>
    public static class EventFilter
    {
        /// <summary>Nilai sentinel "tidak memfilter".</summary>
        public const string Any = "Semua";

        public static readonly string[] DateOptions = { Any, "Malam ini", "Besok", "Minggu ini", "Akhir pekan" };
        public static readonly string[] PriceOptions = { Any, "Gratis", "<50rb", "<100rb" };

        public const decimal CheapThreshold = 50_000m;
        public const decimal AffordableThreshold = 100_000m;

        public static bool IsValidDateOption(string? option) =>
            option != null && Array.IndexOf(DateOptions, option) >= 0;

        public static bool IsValidPriceOption(string? option) =>
            option != null && Array.IndexOf(PriceOptions, option) >= 0;

        /// <summary>
        /// Filter tanggal. <paramref name="today"/> bisa diisi pada test agar hasilnya
        /// tidak bergantung pada tanggal saat test dijalankan.
        /// </summary>
        public static bool MatchesDate(GigEvent gigEvent, string? option, DateTime? today = null)
        {
            if (gigEvent == null) return false;

            var reference = (today ?? DateTime.Today).Date;
            var start = gigEvent.StartDate.Date;

            return option switch
            {
                "Malam ini" => start == reference,
                "Besok" => start == reference.AddDays(1),
                // Termasuk hari ini sampai 7 hari ke depan.
                "Minggu ini" => start >= reference && start <= reference.AddDays(7),
                "Akhir pekan" => IsUpToWeekendEnd(start, reference),
                _ => true
            };
        }

        /// <summary>
        /// Filter harga memakai MinPrice (tipe tiket termurah). Batas bersifat kumulatif:
        /// "&lt;50rb" juga mencakup event gratis.
        /// </summary>
        public static bool MatchesPrice(GigEvent gigEvent, string? option)
        {
            if (gigEvent == null) return false;

            return option switch
            {
                "Gratis" => gigEvent.MinPrice == 0,
                "<50rb" => gigEvent.MinPrice < CheapThreshold,
                "<100rb" => gigEvent.MinPrice < AffordableThreshold,
                _ => true
            };
        }

        /// <summary>
        /// Hari ini s/d akhir pekan (Minggu) — sengaja satu arti dengan
        /// GET /api/events/weekend (README: "Event sampai akhir pekan") dan blok
        /// Weekend di HomeViewModel, supaya label "Akhir pekan" tidak berbeda-beda
        /// antar layar.
        /// </summary>
        private static bool IsUpToWeekendEnd(DateTime start, DateTime reference)
        {
            var weekendEnd = reference.AddDays(7 - (int)reference.DayOfWeek);
            return start >= reference && start <= weekendEnd;
        }
    }
}
