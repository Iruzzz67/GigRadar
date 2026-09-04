using System.Security.Cryptography;
using System.Text;
using Microsoft.Maui.Graphics;

namespace GigRadarMobile.Helpers;

/// <summary>
/// Menggambar barcode batang (bar pattern) yang deterministik dari string kode tiket,
/// sehingga setiap tiket memiliki pola barcode yang berbeda namun stabil. Ini adalah
/// representasi visual sederhana; kode asli (QRCode) tetap ditampilkan sebagai teks
/// untuk validasi oleh petugas (endpoint /api/tickets/validate).
/// </summary>
public class TicketBarcodeDrawable : IDrawable
{
    public string Code { get; set; } = string.Empty;

    public void Draw(ICanvas canvas, RectF rect)
    {
        canvas.FillColor = Colors.White;
        canvas.FillRectangle(rect);

        if (string.IsNullOrEmpty(Code))
            return;

        // Pola batang deterministik dari hash kode.
        var hash = SHA256.HashData(Encoding.UTF8.GetBytes(Code));

        var segments = new List<float> { 1f, 1f, 1f }; // guard kiri (narrow)
        for (var i = 0; i < hash.Length; i++)
            segments.Add(hash[i] % 4 + 1);          // lebar batang 1..4
        segments.AddRange(new[] { 1f, 1f, 1f, 1f }); // guard kanan

        var total = segments.Sum();
        const float padding = 10f;
        var available = Math.Max(rect.Width - padding * 2, 1);
        var x = rect.Left + padding;
        var black = true;

        foreach (var seg in segments)
        {
            var w = Math.Max(available * seg / total, 0.5f);
            if (black)
            {
                canvas.FillColor = Colors.Black;
                canvas.FillRectangle(x, rect.Top + 8, w, Math.Max(rect.Height - 16, 1));
            }
            x += w;
            black = !black;
        }
    }
}