using CommunityToolkit.Mvvm.ComponentModel;
using GigRadarMobile.Services;

namespace GigRadarMobile.ViewModels
{
    /// <summary>
    /// Tab "Dashboard" ArtistShell — saat ini placeholder.
    /// Fitur Artist (Music / Posts / Gigs / Journey) menyusul di Phase 3
    /// sesuai GIGRADAR_ROLE_SYSTEM.md.
    /// </summary>
    public partial class ArtistDashboardViewModel : ObservableObject
    {
        private readonly AuthService _auth;

        [ObservableProperty] private string _userName = "";
        [ObservableProperty] private string _userRole = "Artist";

        public ArtistDashboardViewModel(AuthService auth)
        {
            _auth = auth;
        }

        public void Load()
        {
            UserName = _auth.GetUserName();
            UserRole = _auth.GetUserRole();
        }
    }
}