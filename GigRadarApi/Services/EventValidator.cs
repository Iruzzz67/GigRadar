using GigRadarApi.Models;

namespace GigRadarApi.Services
{
    /// <summary>
    /// Aturan validasi body event (create/update) — dipakai EventsController.
    /// Murni dan tanpa dependency sehingga bisa di-unit-test langsung.
    /// </summary>
    public static class EventValidator
    {
        /// <summary>
        /// Validasi event. Mengembalikan daftar pesan error — kosong berarti valid.
        /// </summary>
        public static List<string> Validate(Event evt)
        {
            var errors = new List<string>();

            if (evt == null)
            {
                errors.Add("Body event wajib dikirim");
                return errors;
            }

            if (string.IsNullOrWhiteSpace(evt.Name))
                errors.Add("Nama event wajib diisi");

            if (evt.EndDate < evt.StartDate)
                errors.Add("EndDate tidak boleh sebelum StartDate");

            if (evt.MinPrice < 0 || evt.MaxPrice < 0)
                errors.Add("Harga tidak boleh negatif");

            if (evt.MinPrice > evt.MaxPrice)
                errors.Add("MinPrice tidak boleh lebih besar dari MaxPrice");

            if (evt.Capacity < 0)
                errors.Add("Kapasitas tidak boleh negatif");

            if (evt.Latitude is < -90 or > 90 || double.IsNaN(evt.Latitude) || double.IsInfinity(evt.Latitude))
                errors.Add("Latitude harus berada di antara -90 dan 90");

            if (evt.Longitude is < -180 or > 180 || double.IsNaN(evt.Longitude) || double.IsInfinity(evt.Longitude))
                errors.Add("Longitude harus berada di antara -180 dan 180");

            if (!EventService.AllowedStatuses.Contains(evt.Status, StringComparer.OrdinalIgnoreCase))
                errors.Add("Status event tidak valid");

            return errors;
        }
    }
}
