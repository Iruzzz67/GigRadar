using ZXing.Net.Maui;

namespace GigRadarMobile.Helpers;

/// <summary>
/// Membuat gambar QR asli dari kode tiket (ZXing) supaya petugas bisa memindainya
/// di pintu masuk (§13/§14). Sebelumnya tiket hanya menampilkan pola batang dekoratif
/// yang tidak bisa dibaca scanner mana pun.
///
/// Gambar di-cache per kode: daftar tiket tidak perlu membuat ulang QR yang sama
/// setiap kali halaman dibuka.
/// </summary>
public static class QrImageFactory
{
    private static readonly Dictionary<string, ImageSource> Cache = new();

    /// <summary>
    /// Mengembalikan ImageSource QR, atau null bila kode kosong / pembuatan gagal
    /// (kode tetap bisa dibaca petugas sebagai teks, jadi kegagalan tidak fatal).
    /// </summary>
    public static async Task<ImageSource?> CreateAsync(string? code, int size = 320)
    {
        var value = code?.Trim();
        if (string.IsNullOrWhiteSpace(value)) return null;

        lock (Cache)
        {
            if (Cache.TryGetValue(value, out var cached)) return cached;
        }

        try
        {
            using var stream = new MemoryStream();

            await BarcodeGenerator.WriteToStreamAsync(
                value,
                stream,
                new BarcodeGeneratorOptions
                {
                    Format = BarcodeFormat.QrCode,
                    Width = size,
                    Height = size,
                    Margin = 1
                },
                new BarcodeImageOptions { Format = BarcodeImageFormat.Png });

            var bytes = stream.ToArray();
            if (bytes.Length == 0) return null;

            // Stream baru per permintaan: ImageSource bisa dibaca lebih dari sekali.
            var source = ImageSource.FromStream(() => new MemoryStream(bytes));

            lock (Cache)
            {
                Cache[value] = source;
            }

            return source;
        }
        catch
        {
            // Encoder gambar tidak tersedia / kode tidak valid — biarkan UI memakai teks kode.
            return null;
        }
    }
}
