namespace GigRadarMobile.Behaviors;

/// <summary>
/// Animasi masuk halus untuk sebuah elemen: fade-in + geser naik sedikit.
///
/// Dipakai pada kartu di dalam <c>DataTemplate</c> — tiap item menganimasikan
/// dirinya sendiri saat pertama kali tampil, sehingga daftar terasa "mengalir"
/// masuk alih-alih muncul kaku sekaligus.
///
/// Animasi hanya dijalankan sekali per elemen, dan dibatalkan aman bila elemen
/// di-recycle/dilepas sebelum selesai. Durasi sengaja pendek (≈300 ms) agar
/// tidak menghambat interaksi.
/// </summary>
public class EntranceAnimationBehavior : Behavior<VisualElement>
{
    /// <summary>Jarak geser awal (px).</summary>
    public double FromOffset { get; set; } = 16;

    /// <summary>Durasi animasi (ms).</summary>
    public uint Duration { get; set; } = 320;

    /// <summary>Jeda sebelum animasi mulai (ms) — untuk efek bertingkat.</summary>
    public int Delay { get; set; }

    /// <summary>Ikut menskalakan elemen dari 0.98 → 1.</summary>
    public bool AnimateScale { get; set; } = true;

    private VisualElement? _view;

    protected override void OnAttachedTo(VisualElement bindable)
    {
        base.OnAttachedTo(bindable);
        _view = bindable;
        bindable.Loaded += OnLoaded;
    }

    protected override void OnDetachingFrom(VisualElement bindable)
    {
        bindable.Loaded -= OnLoaded;
        _view = null;
        base.OnDetachingFrom(bindable);
    }

    private async void OnLoaded(object? sender, EventArgs e)
    {
        var view = _view;
        if (view is null) return;

        // Sekali saja per elemen.
        view.Loaded -= OnLoaded;
        if (!view.IsVisible) return;

        view.Opacity = 0;
        view.TranslationY = FromOffset;
        if (AnimateScale) view.Scale = 0.98;

        if (Delay > 0)
        {
            await Task.Delay(Delay);
            if (_view is null) return;
            view = _view;
        }

        var fade = view.FadeToAsync(1, Duration, Easing.CubicOut);
        var slide = view.TranslateToAsync(0, 0, Duration, Easing.CubicOut);
        var scale = AnimateScale ? view.ScaleToAsync(1, Duration, Easing.CubicOut) : Task.CompletedTask;

        await Task.WhenAll(fade, slide, scale);
    }
}
