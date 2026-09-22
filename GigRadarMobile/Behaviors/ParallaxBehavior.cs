namespace GigRadarMobile.Behaviors;

/// <summary>
/// Parallax sederhana: elemen (biasanya gambar hero) bergerak lebih lambat
/// daripada konten saat halaman di-scroll.
///
/// Supaya tidak muncul celah di tepi, elemen sasaran sengaja diskalakan sedikit
/// (<see cref="Slack"/>) dan hanya digeser sejauh ruang lebih yang tersedia
/// (tinggi × Slack ÷ 2). Semua nilai di-clamp, jadi aman untuk tinggi hero
/// apa pun.
///
/// Tempelkan ke <see cref="ScrollView"/> dan set <see cref="Target"/> ke
/// elemen yang ingin digerakkan.
/// </summary>
public class ParallaxBehavior : Behavior<ScrollView>
{
    /// <summary>Elemen yang digerakkan (mis. Image di dalam hero).</summary>
    public VisualElement? Target { get; set; }

    /// <summary>0 = diam, 1 = ikut konten. 0.35–0.5 terasa paling natural.</summary>
    public double Factor { get; set; } = 0.4;

    /// <summary>Skala lebih di tepi (0.15 = 15% lebih besar). Menutup celah.</summary>
    public double Slack { get; set; } = 0.16;

    private ScrollView? _scroll;

    protected override void OnAttachedTo(ScrollView bindable)
    {
        base.OnAttachedTo(bindable);
        _scroll = bindable;
        bindable.Scrolled += OnScrolled;

        if (Target is not null && Slack > 0)
            Target.Scale = 1 + Slack;
    }

    protected override void OnDetachingFrom(ScrollView bindable)
    {
        bindable.Scrolled -= OnScrolled;
        _scroll = null;
        base.OnDetachingFrom(bindable);
    }

    private void OnScrolled(object? sender, ScrolledEventArgs e)
    {
        var target = Target;
        if (target is null) return;

        // Hanya saat konten naik (scroll ke bawah). y < 0 (pull-to-refresh) diabaikan.
        var y = e.ScrollY <= 0 ? 0 : e.ScrollY;

        var height = target.Height > 0 ? target.Height : 320d;
        var maxOffset = height * Slack / 2;
        var offset = Math.Min(y * Factor, maxOffset);

        target.TranslationY = offset;
    }
}
