namespace GigRadarMobile.Helpers
{
    public static class Constants
    {
        // ⚠️ Base URL API terpusat ada di Services/ApiConfiguration.cs
        // (berbeda per platform: Android emulator, Windows, iOS, device fisik).

        public static class StorageKeys
        {
            public const string Token = "auth_token";
            public const string UserId = "user_id";
            public const string UserName = "user_name";
            public const string UserEmail = "user_email";
            public const string UserRole = "user_role";
            public const string OnboardingDone = "onboarding_done";
        }
    }
}
