using CommunityToolkit.Mvvm.ComponentModel;

namespace GigRadarMobile.Models
{
    public partial class Genre : ObservableObject
    {
        public int GenreId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Icon { get; set; } = string.Empty;

        /// <summary>State pilihan lokal (onboarding) — bukan bagian dari API.</summary>
        [ObservableProperty] private bool _isSelected;
    }
}