namespace GigRadarMobile.Helpers
{
    /// <summary>
    /// GIGRADAR_ROLE_SYSTEM.md §24/§39 — routing berbasis role setelah login.
    /// Role berasal dari data yang diverifikasi server (JWT), bukan pilihan UI.
    /// </summary>
    public static class ShellRouter
    {
        public static Shell CreateForRole(string role)
        {
            return role switch
            {
                "EO" => new Shells.EOShell(),
                "Admin" => new Shells.AdminShell(),
                "Artist" => new Shells.ArtistShell(),
                _ => new Shells.UserShell()
            };
        }
    }
}