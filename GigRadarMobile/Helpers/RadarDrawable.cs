using Microsoft.Maui.Graphics;

namespace GigRadarMobile.Helpers;

/// <summary>Node event pada radar — posisi relatif dari titik user (jarak + bearing).</summary>
public class RadarNode
{
    public required GigRadarMobile.Models.GigEvent Event { get; init; }
    public double DistanceKm { get; init; }
    public double BearingDeg { get; init; }
    public double Popularity { get; init; }
    public Color Color { get; init; } = GenreColors.DefaultGenre;
}

/// <summary>
/// Radar GIGRADAR — visualisasi node gig di sekitar user.
/// Cincin radius berlabel, sweep berputar, node: warna = genre, ukuran = popularitas.
/// Render hanya saat tab terlihat; sweep dimatikan saat reduced motion.
/// </summary>
public class RadarDrawable : IDrawable
{
    private const float Padding = 20f;
    private static readonly Color RingColor = Color.FromArgb("#2B2B33");
    private static readonly Color CrosshairColor = Color.FromArgb("#232329");
    private static readonly Color Accent = Color.FromArgb("#A3FF12");
    private static readonly Color Ink = Color.FromArgb("#0A0A0B");
    private static readonly Color PillBg = Color.FromArgb("#18181B");
    private static readonly Color PillBorder = Color.FromArgb("#34343C");

    public List<RadarNode> Nodes { get; set; } = new();
    public double MaxRadiusKm { get; set; } = 25;
    public double SweepDeg { get; set; }
    public bool ReducedMotion { get; set; }
    public int? SelectedIndex { get; set; }
    public RectF LastRect { get; private set; }

    public void Draw(ICanvas canvas, RectF dirtyRect)
    {
        LastRect = dirtyRect;

        var size = Math.Min(dirtyRect.Width, dirtyRect.Height);
        if (size <= 60)
        {
            DrawEmpty(canvas, dirtyRect, "Radar terlalu kecil");
            return;
        }

        var cx = dirtyRect.Center.X;
        var cy = dirtyRect.Center.Y;
        var maxR = size / 2f - Padding;

        DrawRings(canvas, cx, cy, maxR);
        DrawSweep(canvas, cx, cy, maxR);

        if (Nodes.Count == 0)
        {
            DrawEmpty(canvas, dirtyRect, "Belum ada gig di radius ini");
            return;
        }

        DrawNodes(canvas, cx, cy, maxR);
        DrawCenter(canvas, cx, cy);
    }

    /// <summary>Hit-test node pada koordinat layar (dipanggil dari StartInteraction).</summary>
    public int? HitTest(float x, float y)
    {
        if (LastRect.IsEmpty || Nodes.Count == 0) return null;

        var size = Math.Min(LastRect.Width, LastRect.Height);
        var maxR = size / 2f - Padding;
        var cx = LastRect.Center.X;
        var cy = LastRect.Center.Y;

        for (var i = 0; i < Nodes.Count; i++)
        {
            var (nx, ny) = NodePosition(cx, cy, maxR, Nodes[i]);
            var dx = x - nx;
            var dy = y - ny;
            var hit = NodeRadius(Nodes[i]) + 10f;
            if (dx * dx + dy * dy <= hit * hit) return i;
        }

        return null;
    }

    // ------------------------------------------------------------

    private void DrawRings(ICanvas canvas, float cx, float cy, float maxR)
    {
        canvas.StrokeColor = RingColor;
        canvas.StrokeSize = 1f;

        // Garis silang tipis
        canvas.StrokeColor = CrosshairColor;
        canvas.DrawLine(cx - maxR, cy, cx + maxR, cy);
        canvas.DrawLine(cx, cy - maxR, cx, cy + maxR);

        var ringFont = new Microsoft.Maui.Graphics.Font("SpaceGroteskMedium");
        const float ringFontSize = 10f;
        canvas.Font = ringFont;
        canvas.FontSize = ringFontSize;

        for (var i = 1; i <= 3; i++)
        {
            var r = maxR * i / 3f;
            canvas.StrokeColor = RingColor;
            canvas.DrawCircle(cx, cy, r);

            var km = MaxRadiusKm * i / 3.0;
            var label = km < 1 ? $"{Math.Round(km * 1000)}m" : $"{km:0}km";
            var labelSize = canvas.GetStringSize(label, ringFont, ringFontSize);

            canvas.FontColor = RingColor;
            canvas.DrawString(label, cx - r - labelSize.Width - 6, cy - labelSize.Height / 2f, HorizontalAlignment.Left);
        }
    }

    private void DrawSweep(ICanvas canvas, float cx, float cy, float maxR)
    {
        if (ReducedMotion) return;

        var rad = ToRad(SweepDeg - 90); // 0 derajat = utara (atas)
        var tipR = maxR * 0.97f;

        // Kipas lembut di belakang blade
        var wedge = ToRad(14);
        var path = new PathF();
        path.MoveTo(cx, cy);
        path.LineTo(cx + (float)Math.Cos(rad - wedge) * tipR, cy + (float)Math.Sin(rad - wedge) * tipR);
        path.LineTo(cx + (float)Math.Cos(rad + wedge) * tipR, cy + (float)Math.Sin(rad + wedge) * tipR);
        path.Close();

        canvas.FillColor = Accent.WithAlpha(0.07f);
        canvas.FillPath(path);

        canvas.StrokeColor = Accent.WithAlpha(0.45f);
        canvas.StrokeSize = 2f;
        canvas.DrawLine(cx, cy, cx + (float)Math.Cos(rad) * maxR, cy + (float)Math.Sin(rad) * maxR);
    }

    private void DrawNodes(ICanvas canvas, float cx, float cy, float maxR)
    {
        for (var i = 0; i < Nodes.Count; i++)
        {
            var node = Nodes[i];
            var (nx, ny) = NodePosition(cx, cy, maxR, node);

            // Di luar ring maksimal → lewati
            var dx = nx - cx;
            var dy = ny - cy;
            if (dx * dx + dy * dy > maxR * maxR) continue;

            var r = NodeRadius(node);
            var isSelected = SelectedIndex == i;

            // Backing gelap supaya node terbaca di atas ring
            canvas.FillColor = Ink;
            canvas.FillCircle(nx, ny, r + (isSelected ? 5f : 2.5f));

            canvas.FillColor = node.Color;
            canvas.FillCircle(nx, ny, r);

            if (isSelected)
            {
                canvas.StrokeColor = Colors.White;
                canvas.StrokeSize = 1.5f;
                canvas.DrawCircle(nx, ny, r + 3.5f);
                DrawNodeLabel(canvas, node, nx, ny, r, maxR);
            }
        }
    }

    private void DrawNodeLabel(ICanvas canvas, RadarNode node, float nx, float ny, float r, float maxR)
    {
        var name = node.Event.Name;
        var labelFont = new Microsoft.Maui.Graphics.Font("SpaceGroteskMedium");
        const float labelFontSize = 11f;
        canvas.Font = labelFont;
        canvas.FontSize = labelFontSize;

        var textSize = canvas.GetStringSize(name, labelFont, labelFontSize);
        var padX = 10f;
        var pillW = textSize.Width + padX * 2;
        var pillH = textSize.Height + 10f;

        var x = nx - pillW / 2f;
        var y = ny - r - pillH - 6f;

        // Jaga label tetap di dalam viewport
        if (y < LastRect.Y + 4) y = ny + r + 6f;
        x = Math.Clamp(x, LastRect.X + 4, LastRect.Right - pillW - 4);

        canvas.FillColor = PillBg;
        canvas.FillRoundedRectangle(x, y, pillW, pillH, 6f);
        canvas.StrokeColor = PillBorder;
        canvas.StrokeSize = 1f;
        canvas.DrawRoundedRectangle(x, y, pillW, pillH, 6f);

        canvas.FontColor = Colors.White;
        canvas.DrawString(name, x + padX, y + 5f, HorizontalAlignment.Left);
    }

    private void DrawCenter(ICanvas canvas, float cx, float cy)
    {
        // Pulsa lembut sinkron dengan sweep (statis saat reduced motion)
        if (!ReducedMotion)
        {
            var p = (SweepDeg % 60) / 60.0;
            canvas.StrokeColor = Accent.WithAlpha((float)(0.28 * (1 - p)));
            canvas.StrokeSize = 1.5f;
            canvas.DrawCircle(cx, cy, 8f + (float)p * 16f);
        }

        canvas.FillColor = Ink;
        canvas.FillCircle(cx, cy, 7f);

        canvas.FillColor = Accent;
        canvas.FillCircle(cx, cy, 4.5f);
    }

    private void DrawEmpty(ICanvas canvas, RectF rect, string message)
    {
        canvas.Font = new Microsoft.Maui.Graphics.Font("Inter");
        canvas.FontSize = 12f;
        canvas.FontColor = Color.FromArgb("#71717A");
        canvas.DrawString(message, rect.Center.X, rect.Center.Y, HorizontalAlignment.Center);
    }

    private (float x, float y) NodePosition(float cx, float cy, float maxR, RadarNode node)
    {
        var distKm = Math.Min(node.DistanceKm, MaxRadiusKm);
        var r = (float)(distKm / MaxRadiusKm) * maxR;
        var rad = ToRad(node.BearingDeg - 90);
        return (cx + (float)Math.Cos(rad) * r, cy + (float)Math.Sin(rad) * r);
    }

    private float NodeRadius(RadarNode node)
        => 5f + Math.Clamp((float)node.Popularity * 8f, 0f, 8f);

    private static double ToRad(double deg) => deg * Math.PI / 180.0;
}