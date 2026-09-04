using GigRadarMobile.Helpers;

namespace GigRadarMobile.Services
{
    public class AuthService
    {
        public bool IsLoggedIn => !string.IsNullOrEmpty(GetToken());

        public string? GetToken() => Preferences.Default.Get<string?>(Constants.StorageKeys.Token, null);
        public int GetUserId() => Preferences.Default.Get(Constants.StorageKeys.UserId, 0);
        public string GetUserName() => Preferences.Default.Get(Constants.StorageKeys.UserName, "Guest");
        public string GetUserEmail() => Preferences.Default.Get(Constants.StorageKeys.UserEmail, "");
        public string GetUserRole() => Preferences.Default.Get(Constants.StorageKeys.UserRole, "User");
        public bool IsOnboardingDone() => Preferences.Default.Get(Constants.StorageKeys.OnboardingDone, false);

        public void SaveSession(string token, int userId, string name, string email, string role)
        {
            Preferences.Default.Set(Constants.StorageKeys.Token, token);
            Preferences.Default.Set(Constants.StorageKeys.UserId, userId);
            Preferences.Default.Set(Constants.StorageKeys.UserName, name);
            Preferences.Default.Set(Constants.StorageKeys.UserEmail, email);
            Preferences.Default.Set(Constants.StorageKeys.UserRole, role);
        }

        public void SaveOnboardingDone()
        {
            Preferences.Default.Set(Constants.StorageKeys.OnboardingDone, true);
        }

        public void Logout()
        {
            Preferences.Default.Clear();
        }

        /// <summary>Perbarui nama tersimpan (mis. setelah edit profil) tanpa mengubah token/sesi.</summary>
        public void UpdateStoredName(string name)
        {
            Preferences.Default.Set(Constants.StorageKeys.UserName, name);
        }
    }
}
