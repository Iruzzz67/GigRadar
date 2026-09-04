# GigRadar — Perbaikan Arsitektur .NET MAUI Multi-Platform

## 1. Tujuan Perbaikan

GigRadar harus benar-benar menjadi aplikasi **.NET MAUI multi-platform**, bukan aplikasi Windows yang kebetulan memakai target MAUI.

Target resmi:

- Android
- iOS
- Mac Catalyst
- Windows

Backend tetap menggunakan ASP.NET Core Web API dan database SQLite.

Masalah utama pada rekapan sebelumnya adalah konfigurasi dan cara menjalankan aplikasi belum dibuat cukup portable. `GigRadarMobile` memang memiliki target Windows, tetapi keberadaan target di `.csproj` saja tidak menjamin target akan muncul di Visual Studio pada setiap mesin. Target platform membutuhkan workload, SDK, dan toolchain masing-masing.

Selain itu, `localhost` tidak boleh dipakai sebagai alamat API universal karena arti `localhost` berbeda pada emulator, device fisik, dan komputer Windows.

---

# 2. Arsitektur Final

```text
                    ┌──────────────────────┐
                    │      GigRadar API    │
                    │    ASP.NET Core 8    │
                    │      REST + JWT      │
                    └──────────┬───────────┘
                               │
                         HTTP / HTTPS
                               │
             ┌─────────────────┼─────────────────┐
             │                 │                 │
             ▼                 ▼                 ▼
      Android Device      Windows PC       iOS / Mac
      .NET MAUI           .NET MAUI        .NET MAUI
             │                 │                 │
             └─────────────── GigRadarMobile ────┘
```

Prinsip:

1. Satu project `GigRadarMobile`.
2. Satu codebase utama.
3. Platform-specific code hanya ditempatkan pada folder `Platforms`.
4. Backend tidak boleh diakses langsung oleh aplikasi.
5. URL API harus configurable per platform/environment.
6. UI tidak boleh memiliki kode yang hanya bekerja di Windows.
7. File `GigRadarMobile.csproj` harus menjadi source of truth untuk target platform.

---

# 3. Target Framework yang Disarankan

Gunakan .NET 10 untuk MAUI:

```xml
<TargetFrameworks>net10.0-android;net10.0-ios;net10.0-maccatalyst;net10.0-windows10.0.19041.0</TargetFrameworks>
```

Untuk pengembangan Windows, project dapat dibangun dengan:

```bash
dotnet build GigRadarMobile -f net10.0-windows10.0.19041.0
```

Untuk Android:

```bash
dotnet build GigRadarMobile -f net10.0-android
```

Untuk iOS:

```bash
dotnet build GigRadarMobile -f net10.0-ios
```

Untuk Mac Catalyst:

```bash
dotnet build GigRadarMobile -f net10.0-maccatalyst
```

Catatan:

- Android dapat dikembangkan dari Windows.
- Windows hanya dapat dibuild pada Windows.
- iOS dan Mac Catalyst membutuhkan lingkungan Apple/macOS untuk proses build/run native.
- Jika target tidak muncul di Visual Studio, periksa workload dan SDK terlebih dahulu. Jangan menghapus target hanya karena tidak muncul di dropdown.

---

# 4. Struktur Project Final

```text
GigRadar/
│
├── GigRadarApi/
│   ├── Controllers/
│   ├── Data/
│   ├── Models/
│   ├── Services/
│   ├── Helpers/
│   ├── Program.cs
│   └── appsettings.json
│
├── GigRadarMobile/
│   ├── Platforms/
│   │   ├── Android/
│   │   ├── iOS/
│   │   ├── MacCatalyst/
│   │   └── Windows/
│   │
│   ├── Models/
│   ├── Views/
│   ├── ViewModels/
│   ├── Services/
│   │   ├── ApiService.cs
│   │   ├── AuthService.cs
│   │   └── ApiConfiguration.cs
│   │
│   ├── Helpers/
│   ├── Resources/
│   ├── App.xaml
│   ├── App.xaml.cs
│   ├── AppShell.xaml
│   ├── AppShell.xaml.cs
│   ├── MauiProgram.cs
│   └── GigRadarMobile.csproj
│
├── GigRadarLauncher/
├── StartGigRadar.bat
├── StartMobileApp.bat
└── GIGRADAR_MULTIPLATFORM_FIX.md
```

---

# 5. Perbaikan `.csproj`

`GigRadarMobile.csproj` harus memiliki target platform lengkap.

Contoh konfigurasi dasar:

```xml
<Project Sdk="Microsoft.NET.Sdk">

  <PropertyGroup>
    <TargetFrameworks>
      net10.0-android;
      net10.0-ios;
      net10.0-maccatalyst;
      net10.0-windows10.0.19041.0
    </TargetFrameworks>

    <OutputType>Exe</OutputType>
    <RootNamespace>GigRadarMobile</RootNamespace>
    <UseMaui>true</UseMaui>
    <SingleProject>true</SingleProject>
    <ImplicitUsings>enable</ImplicitUsings>
    <Nullable>enable</Nullable>

    <ApplicationTitle>GigRadar</ApplicationTitle>
    <ApplicationId>com.gigradar.app</ApplicationId>
    <ApplicationDisplayVersion>1.0</ApplicationDisplayVersion>
    <ApplicationVersion>1</ApplicationVersion>

    <SupportedOSPlatformVersion Condition="$([MSBuild]::GetTargetPlatformIdentifier('$(TargetFramework)')) == 'android'">23.0</SupportedOSPlatformVersion>
    <SupportedOSPlatformVersion Condition="$([MSBuild]::GetTargetPlatformIdentifier('$(TargetFramework)')) == 'ios'">15.0</SupportedOSPlatformVersion>
    <SupportedOSPlatformVersion Condition="$([MSBuild]::GetTargetPlatformIdentifier('$(TargetFramework)')) == 'maccatalyst'">15.0</SupportedOSPlatformVersion>
    <TargetPlatformMinVersion Condition="$([MSBuild]::GetTargetPlatformIdentifier('$(TargetFramework)')) == 'windows'">10.0.19041.0</TargetPlatformMinVersion>
  </PropertyGroup>

  <ItemGroup>
    <MauiIcon Include="Resources\AppIcon\appicon.svg" />
    <MauiSplashScreen Include="Resources\Splash\splash.svg" />
    <MauiImage Include="Resources\Images\**" />
    <MauiFont Include="Resources\Fonts\**" />
    <MauiAsset Include="Resources\Raw\**" />
  </ItemGroup>

  <ItemGroup>
    <PackageReference Include="Microsoft.Maui.Controls" Version="$(MauiVersion)" />
    <PackageReference Include="Microsoft.Extensions.Logging.Debug" Version="10.0.0" />
    <PackageReference Include="CommunityToolkit.Mvvm" Version="8.4.0" />
  </ItemGroup>

</Project>
```

**Penting:** jangan menambahkan satu platform sebagai `TargetFramework` tunggal. Jika hanya menggunakan:

```xml
<TargetFramework>net10.0-windows...</TargetFramework>
```

project akan berubah menjadi Windows-only.

---

# 6. API URL Harus Multi-Platform

Konfigurasi lama:

```text
Android emulator : http://10.0.2.2:5000
Windows          : http://localhost:5000
Physical device  : harus ganti IP manual
```

Ini menjadi sumber masalah.

Gunakan konfigurasi terpusat.

## `ApiConfiguration.cs`

```csharp
namespace GigRadarMobile.Services;

public static class ApiConfiguration
{
#if ANDROID
    // Android Emulator dapat mengakses host PC melalui 10.0.2.2
    public const string BaseUrl = "http://10.0.2.2:5000";
#elif WINDOWS
    // Windows app berjalan di PC yang sama dengan API
    public const string BaseUrl = "http://localhost:5000";
#elif IOS
    // Simulator iOS dapat memakai localhost.
    // Device fisik harus menggunakan IP PC.
    public const string BaseUrl = "http://localhost:5000";
#elif MACCATALYST
    public const string BaseUrl = "http://localhost:5000";
#else
    public const string BaseUrl = "http://localhost:5000";
#endif
}
```

Namun untuk **device fisik**, jangan mengandalkan `localhost`.

Gunakan konfigurasi development:

```text
PC menjalankan API
        │
        │ Wi-Fi yang sama
        ▼
192.168.x.x:5000
        │
        ▼
Android / iPhone
```

Contoh:

```csharp
public const string PhysicalDeviceBaseUrl =
    "http://192.168.1.10:5000";
```

IP tersebut harus diganti sesuai IP komputer yang menjalankan API.

---

# 7. API Harus Listen ke Network Interface

API lama menggunakan:

```bash
dotnet run --project GigRadarApi --urls http://localhost:5000
```

Untuk device fisik, ini tidak cukup.

Gunakan development command:

```bash
dotnet run --project GigRadarApi --urls "http://0.0.0.0:5000"
```

Dengan ini API menerima koneksi dari jaringan lokal.

Untuk produksi, gunakan HTTPS dan domain/server yang benar.

---

# 8. Windows Harus Menjadi Target MAUI, Bukan EXE Terpisah

`GigRadarLauncher` boleh tetap digunakan untuk menjalankan API.

Namun launcher **bukan aplikasi GigRadarMobile**.

Jangan menganggap:

```text
GigRadarLauncher.exe
```

sebagai aplikasi Windows MAUI.

Struktur final:

```text
GigRadarLauncher
        │
        └── menjalankan GigRadarApi

GigRadarMobile
        │
        ├── Android
        ├── Windows
        ├── iOS
        └── Mac Catalyst
```

Untuk Windows, jalankan:

```bash
dotnet run --project GigRadarMobile -f net10.0-windows10.0.19041.0
```

Atau dari Visual Studio pilih:

```text
GigRadarMobile
        ↓
Windows Machine
        ↓
Run
```

---

# 9. Windows Unpackaged

Jika ingin aplikasi Windows lebih mudah dijalankan sebagai aplikasi desktop development, konfigurasi unpackaged dapat dipertahankan:

```xml
<PropertyGroup Condition="$([MSBuild]::GetTargetPlatformIdentifier('$(TargetFramework)')) == 'windows'">
    <WindowsPackageType>None</WindowsPackageType>
</PropertyGroup>
```

Ini tidak mengubah project menjadi Windows-only.

Target tetap:

```text
Android
iOS
Mac Catalyst
Windows
```

---

# 10. Perbaikan `MauiProgram.cs`

Gunakan dependency injection yang tidak mengunci aplikasi ke Windows.

```csharp
using CommunityToolkit.Mvvm;
using Microsoft.Extensions.Logging;

namespace GigRadarMobile;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();

        builder
            .UseMauiApp<App>()
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
            });

        builder.Services.AddSingleton<HttpClient>();

        builder.Services.AddSingleton<Services.ApiService>();
        builder.Services.AddSingleton<Services.AuthService>();

        builder.Services.AddTransient<ViewModels.HomeViewModel>();
        builder.Services.AddTransient<ViewModels.EventDetailViewModel>();
        builder.Services.AddTransient<ViewModels.ArtistDetailViewModel>();
        builder.Services.AddTransient<ViewModels.OnboardingViewModel>();
        builder.Services.AddTransient<ViewModels.TicketViewModel>();
        builder.Services.AddTransient<ViewModels.ProfileViewModel>();
        builder.Services.AddTransient<ViewModels.LoginViewModel>();
        builder.Services.AddTransient<ViewModels.MapViewModel>();

        builder.Services.AddTransient<Views.HomePage>();
        builder.Services.AddTransient<Views.EventDetailPage>();
        builder.Services.AddTransient<Views.ArtistDetailPage>();
        builder.Services.AddTransient<Views.OnboardingPage>();
        builder.Services.AddTransient<Views.TicketPage>();
        builder.Services.AddTransient<Views.ProfilePage>();
        builder.Services.AddTransient<Views.LoginPage>();
        builder.Services.AddTransient<Views.MapPage>();

#if DEBUG
        builder.Logging.AddDebug();
#endif

        return builder.Build();
    }
}
```

---

# 11. `ApiService` Harus Mengambil Base URL dari Konfigurasi

Jangan menulis URL di banyak ViewModel.

Gunakan satu sumber:

```csharp
using System.Net.Http.Headers;

namespace GigRadarMobile.Services;

public class ApiService
{
    private readonly HttpClient _httpClient;

    public ApiService(HttpClient httpClient)
    {
        _httpClient = httpClient;
        _httpClient.BaseAddress = new Uri(ApiConfiguration.BaseUrl);
        _httpClient.Timeout = TimeSpan.FromSeconds(30);
    }

    public void SetToken(string? token)
    {
        _httpClient.DefaultRequestHeaders.Authorization =
            string.IsNullOrWhiteSpace(token)
                ? null
                : new AuthenticationHeaderValue("Bearer", token);
    }

    public HttpClient Client => _httpClient;
}
```

Dengan ini seluruh platform memakai service yang sama.

---

# 12. Android Permission

Pastikan Android memiliki permission internet.

`Platforms/Android/AndroidManifest.xml`:

```xml
<?xml version="1.0" encoding="utf-8"?>
<manifest xmlns:android="http://schemas.android.com/apk/res/android">

    <uses-permission android:name="android.permission.INTERNET" />

    <application
        android:allowBackup="true"
        android:supportsRtl="true" />

</manifest>
```

Untuk development HTTP lokal, Android dapat membutuhkan konfigurasi cleartext.

Jika API masih memakai:

```text
http://
```

pastikan konfigurasi Android mengizinkan traffic HTTP development.

Untuk production, **gunakan HTTPS**.

---

# 13. Windows HTTP Localhost

Windows tidak membutuhkan `10.0.2.2`.

Gunakan:

```text
http://localhost:5000
```

jika API berjalan di PC yang sama.

Jika API dijalankan di komputer lain:

```text
http://IP-KOMPUTER:5000
```

Contoh:

```text
http://192.168.1.10:5000
```

---

# 14. Masalah "Windows Tidak Muncul di Visual Studio"

Urutan pemeriksaan:

## Step 1 — cek SDK

```bash
dotnet --info
```

Pastikan .NET 10 SDK tersedia.

Kemudian:

```bash
dotnet workload list
```

## Step 2 — cek MAUI

Jika workload MAUI belum tersedia:

```bash
dotnet workload install maui
```

Kemudian:

```bash
dotnet workload restore
```

## Step 3 — cek Windows App SDK / Windows workload

Jalankan:

```bash
dotnet workload list
```

Pastikan workload yang dibutuhkan untuk MAUI Windows terpasang.

## Step 4 — buka ulang Visual Studio

Setelah workload berubah:

1. Tutup Visual Studio.
2. Tutup terminal terkait.
3. Buka Visual Studio kembali.
4. Clean solution.
5. Rebuild solution.
6. Pilih `GigRadarMobile`.
7. Periksa dropdown target.

---

# 15. Clean Total Project

Jika project pernah berpindah SDK atau mengalami build cache rusak:

Windows CMD:

```cmd
dotnet nuget locals all --clear

dotnet clean

rmdir /s /q GigRadarMobile\bin
rmdir /s /q GigRadarMobile\obj

dotnet restore

dotnet build GigRadarMobile -f net10.0-windows10.0.19041.0
```

Jika folder `bin` atau `obj` sedang terkunci, tutup Visual Studio terlebih dahulu.

---

# 16. Test Setiap Platform Secara Terpisah

Jangan langsung menguji semua platform.

## Windows

```bash
dotnet build GigRadarMobile -f net10.0-windows10.0.19041.0
```

Lalu:

```bash
dotnet run --project GigRadarMobile -f net10.0-windows10.0.19041.0
```

## Android

Pastikan emulator/device terdeteksi:

```bash
adb devices
```

Lalu build:

```bash
dotnet build GigRadarMobile -f net10.0-android
```

## iOS

Build pada macOS:

```bash
dotnet build GigRadarMobile -f net10.0-ios
```

## Mac Catalyst

```bash
dotnet build GigRadarMobile -f net10.0-maccatalyst
```

---

# 17. Jangan Membuat Platform Branch di UI

Hindari:

```csharp
if (OperatingSystem.IsWindows())
{
    // seluruh UI Windows
}
else
{
    // UI berbeda total
}
```

Gunakan platform-specific API hanya jika memang diperlukan.

Contoh yang benar:

```csharp
if (OperatingSystem.IsWindows())
{
    // fitur khusus Windows
}
```

Sedangkan halaman:

```text
LoginPage
HomePage
MapPage
EventDetailPage
ArtistDetailPage
TicketPage
ProfilePage
```

harus tetap shared oleh MAUI.

---

# 18. Navigasi Harus Shared

Gunakan:

```text
App
 ↓
AppShell
 ↓
LoginPage
 ↓
OnboardingPage
 ↓
Main Shell
 ├── Discover
 ├── Map
 ├── Tickets
 └── Profile
```

Jangan membuat:

```text
WindowsHomePage
AndroidHomePage
IOSHomePage
```

kecuali benar-benar diperlukan.

Dengan demikian satu XAML dapat digunakan lintas platform.

---

# 19. Fitur yang Tetap Dipertahankan

Perbaikan multi-platform **tidak boleh menghilangkan konsep utama GigRadar**.

Tetap tersedia:

### Authentication

- Login
- Register
- JWT
- BCrypt
- Session

### Discovery

- Recommended For You
- Tonight
- This Weekend
- Nearby Gigs

### Event

- Detail event
- Venue
- Harga
- Line-up
- Save/Favorite
- Buy Ticket

### Artist

- Artist detail
- Genre
- Bio
- Track preview

### Ticket

- My Tickets
- Ticket code
- Validasi ticket

### Profile

- Nama
- Kota
- Preferensi genre
- Logout

---

# 20. Perbaikan Map

Versi saat ini sebenarnya belum menggunakan peta native. Fitur Map masih berupa daftar nearby gigs dan membuka Google Maps eksternal.

Pertahankan pendekatan tersebut untuk MVP karena lebih portable:

```text
MapPage
   │
   ├── Nearby Gigs
   │
   └── Open Maps
          │
          └── Launcher.Default.OpenAsync(...)
```

Jangan memasukkan SDK peta khusus Windows/Android ke halaman utama jika belum diperlukan.

---

# 21. Launcher

`GigRadarLauncher` tetap menjadi utility development:

```text
StartGigRadar.bat
       ↓
GigRadarLauncher.exe
       ↓
GigRadarApi
       ↓
http://localhost:5000
       ↓
Swagger
```

Sedangkan aplikasi:

```text
GigRadarMobile
       ↓
MAUI
       ↓
Android / iOS / Mac Catalyst / Windows
```

Launcher tidak boleh menjadi dependency aplikasi MAUI.

---

# 22. Database

Backend tetap:

```text
GigRadarApi
    ↓
EF Core
    ↓
SQLite
    ↓
GigRadar.db
```

MAUI:

```text
GigRadarMobile
    ↓
REST API
    ↓
GigRadarApi
```

Jangan membuat setiap device memiliki database server sendiri.

---

# 23. Checklist Perbaikan

## Project

- [ ] `UseMaui=true`
- [ ] `SingleProject=true`
- [ ] 4 target framework tersedia
- [ ] Tidak ada target Windows-only
- [ ] Platform folder lengkap
- [ ] `MauiProgram.cs` tidak Windows-specific

## Windows

- [ ] .NET 10 SDK terinstall
- [ ] MAUI workload terinstall
- [ ] Windows development workload tersedia
- [ ] Windows target muncul
- [ ] Build Windows berhasil

## Android

- [ ] Android SDK tersedia
- [ ] Emulator/device terdeteksi
- [ ] INTERNET permission
- [ ] API URL memakai `10.0.2.2` untuk emulator
- [ ] Device fisik memakai IP PC

## iOS

- [ ] macOS tersedia untuk native build
- [ ] iOS workload tersedia
- [ ] API dapat diakses

## Mac Catalyst

- [ ] macOS tersedia
- [ ] Mac Catalyst workload tersedia
- [ ] API dapat diakses

## API

- [ ] API dapat dijalankan
- [ ] Port 5000 aktif
- [ ] Device fisik dapat mengakses PC
- [ ] CORS tetap tersedia untuk development
- [ ] Production menggunakan HTTPS

---

# 24. Target Akhir

Setelah perbaikan, Visual Studio harus memperlakukan:

```text
GigRadarMobile
```

sebagai **satu aplikasi MAUI multi-platform**.

Target yang diharapkan:

```text
┌─────────────────────────────────────────┐
│             GigRadarMobile              │
├─────────────────────────────────────────┤
│ Android                                 │
│ iOS                                     │
│ Mac Catalyst                            │
│ Windows                                 │
└─────────────────────────────────────────┘
```

Semua target menggunakan:

```text
Models
Views
ViewModels
Services
API
Authentication
Business Logic
```

yang sama.

Perbedaan platform hanya berada di:

```text
Platforms/
├── Android/
├── iOS/
├── MacCatalyst/
└── Windows/
```

---

# 25. Prioritas Pengerjaan

Urutan perbaikan yang disarankan:

### PRIORITAS 1
Perbaiki `GigRadarMobile.csproj`.

### PRIORITAS 2
Pastikan .NET 10 + MAUI workload terpasang.

### PRIORITAS 3
Pastikan Windows target dapat build dan run.

### PRIORITAS 4
Pisahkan konfigurasi API berdasarkan platform.

### PRIORITAS 5
Pastikan Android emulator dapat terhubung ke API.

### PRIORITAS 6
Pastikan Android device fisik dapat terhubung menggunakan IP PC.

### PRIORITAS 7
Audit seluruh XAML/C# agar tidak ada dependency Windows-only.

### PRIORITAS 8
Uji setiap target satu per satu.

### PRIORITAS 9
Setelah semua target stabil, baru lanjut ke fitur tambahan seperti payment gateway, push notification, Google Maps native, QR image, audio streaming, dashboard EO/Admin, search/filter, dan rekomendasi ML.

---

# 26. Catatan Penting untuk Repository

Rekapan sebelumnya menyatakan target MAUI sudah:

```text
net10.0-android
net10.0-ios
net10.0-maccatalyst
net10.0-windows10.0.19041.0
```

Jadi masalah "Windows tidak muncul" **belum tentu berasal dari desain aplikasi**. Dari dokumentasi yang tersedia, konfigurasi target tersebut memang sudah direncanakan.

Karena file yang tersedia di percakapan ini adalah rekapan proyek, bukan seluruh source repository, file sumber seperti:

```text
GigRadarMobile.csproj
MauiProgram.cs
App.xaml
AppShell.xaml
Platforms/Windows/*
Platforms/Android/*
```

perlu diperiksa langsung sebelum perubahan kode final dilakukan.

Jangan menghapus target Windows hanya karena Windows belum muncul di Visual Studio. Fokus pertama adalah memastikan SDK/workload dan konfigurasi project benar.

---

# 27. Definisi "Selesai"

Perbaikan dianggap selesai apabila:

```text
Windows
   └── GigRadarMobile berjalan

Android Emulator
   └── GigRadarMobile berjalan

Android Physical Device
   └── GigRadarMobile berjalan + API terhubung

iOS
   └── GigRadarMobile berjalan pada lingkungan Apple

Mac Catalyst
   └── GigRadarMobile berjalan
```

dan seluruh platform tetap menggunakan backend:

```text
GigRadarApi
```

tanpa membuat aplikasi Windows, Android, dan iOS sebagai project terpisah.

---

## Kesimpulan

GigRadar seharusnya tidak dibuat ulang menjadi beberapa aplikasi.

Solusi yang benar adalah mempertahankan:

```text
1 Project .NET MAUI
        +
4 Target Platform
        +
1 ASP.NET Core API
        +
1 Shared Codebase
```

Dengan struktur tersebut, GigRadar tetap menjadi aplikasi multi-platform dan Windows menjadi salah satu target resmi, bukan aplikasi terpisah.
