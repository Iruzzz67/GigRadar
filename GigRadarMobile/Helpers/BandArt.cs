using System.Globalization;

namespace GigRadarMobile.Helpers;

/// <summary>
/// Resolver visual band/event.
///
/// Latar belakang: seed artist &amp; event tidak punya foto, dan aplikasi tidak
/// boleh mengunduh foto/logo band berhak cipta. Supaya tidak ada permukaan yang
/// tampil kosong DAN tidak monoton, tiap entitas dipetakan ke salah satu dari
/// beberapa artwork vektor orisinal secara <b>deterministik</b> (berdasarkan
/// namanya), bukan acak — jadi artwork sebuah artist tidak berubah-ubah tiap
/// kali halaman dibuka.
///
/// Mengganti dengan foto asli:
///  1. Taruh file foto di <c>GigRadarMobile/Resources/Images/</c> dengan nama
///     dasar yang sama (mis. <c>art_haze</c>), lalu hapus/ubah file .svg-nya
///     agar tidak bentrok nama resource. Ukuran disarankan ≥ 800×800.
///  2. Atau isi kolom URL foto pada form Artist/EO — URL dari API selalu
///     diprioritaskan di atas artwork default.
/// </summary>
public static class BandArt
{
    /// <summary>Daftar artwork default (base filename tanpa ekstensi).</summary>
    public static readonly string[] Pool =
    {
        "art_aurora",
        "art_ember",
        "art_haze",
        "art_grid",
        "art_monolith",
        "art_prism",
        "art_ripple",
        "art_noise",
    };

    /// <summary>Pilih artwork default secara stabil dari sebuah kunci (nama entitas).</summary>
    public static string Pick(string? key)
    {
        if (string.IsNullOrWhiteSpace(key)) return Pool[0];

        // FNV-1a 32-bit: stabil lintas platform/run (string.GetHashCode tidak dijamin).
        unchecked
        {
            uint hash = 2166136261;
            foreach (var ch in key.Trim().ToLowerInvariant())
            {
                hash ^= ch;
                hash *= 16777619;
            }

            return Pool[(int)(hash % (uint)Pool.Length)];
        }
    }

    /// <summary>
    /// Ubah url/string dari API menjadi ImageSource. Bila kosong, jatuh ke
    /// artwork default yang dipilih dari <paramref name="key"/>.
    /// </summary>
    public static ImageSource Resolve(string? url, string? key)
    {
        var trimmed = url?.Trim();

        // Poster hasil unggah EO (data URL base64).
        if (!string.IsNullOrWhiteSpace(trimmed) &&
            trimmed.StartsWith("data:image/", StringComparison.OrdinalIgnoreCase))
        {
            var comma = trimmed.IndexOf(',');
            if (comma > 0)
            {
                try
                {
                    var bytes = Convert.FromBase64String(trimmed[(comma + 1)..]);
                    return ImageSource.FromStream(() => new MemoryStream(bytes));
                }
                catch (FormatException)
                {
                    // Data URL rusak — pakai artwork default daripada blank.
                    return ImageSource.FromFile(Pick(key));
                }
            }
        }

        // URL gambar normal (mis. foto dari CDN).
        if (!string.IsNullOrWhiteSpace(trimmed) &&
            Uri.TryCreate(trimmed, UriKind.Absolute, out var uri))
        {
            return ImageSource.FromUri(uri);
        }

        return ImageSource.FromFile(Pick(key));
    }

    /// <summary>Label peran tujuan aria/aksesibilitas bila gambar tidak punya alt.</summary>
    public static string Describe(string? key) =>
        string.IsNullOrWhiteSpace(key)
            ? "Artwork panggung"
            : $"Artwork {key.Trim()}";
}
