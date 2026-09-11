# 📍 GigRadar — GPS & Device Location Specification

Dokumen ini adalah spesifikasi implementasi GPS/location untuk **GigRadarMobile (.NET MAUI)** agar aplikasi dapat mengambil lokasi perangkat pada platform yang kompatibel, menggunakan API geolokasi native melalui .NET MAUI.

> **Konteks proyek:** GigRadarMobile saat ini menargetkan **Android, iOS, macOS/Mac Catalyst, dan Windows**. Aplikasi sudah mempunyai `GeoHelper.cs`, fitur event `nearby`, serta endpoint backend `GET /api/events/nearby?lat=&lng=&radius=`. fileciteturn0file0L68-L87 fileciteturn0file0L184-L199

---

## 1. Tujuan

Implementasikan sistem lokasi dengan target berikut:

1. Aplikasi meminta izin lokasi **hanya ketika fitur yang membutuhkan lokasi digunakan**.
2. Aplikasi mengambil latitude dan longitude perangkat melalui `Microsoft.Maui.Devices.Sensors.Geolocation`.
3. Sistem kompatibel dengan:
   - Android
   - iOS
   - Mac Catalyst/macOS
   - Windows
4. Jika GPS/lokasi tidak tersedia, aplikasi tetap dapat digunakan dengan fallback yang jelas.
5. Lokasi pengguna dipakai untuk:
   - Radar
   - Gig terdekat
   - Rekomendasi berbasis lokasi
   - Filter radius
   - Event nearby
6. Jangan meminta `LocationAlways` atau background location karena kebutuhan GigRadar adalah lokasi saat fitur aplikasi sedang digunakan.
7. Jangan mengirim lokasi ke backend tanpa kebutuhan fitur atau tanpa persetujuan user.
8. Jangan menyimpan riwayat lokasi secara permanen kecuali fitur khusus di masa depan memang membutuhkannya.

---

# 2. Arsitektur

Gunakan pemisahan tanggung jawab berikut:

```text
GigRadarMobile
│
├── Views
│   ├── HomePage
│   ├── ExplorePage
│   └── RadarPage
│
├── ViewModels
│   ├── HomeViewModel
│   ├── ExploreViewModel
│   └── RadarViewModel
│
├── Services
│   └── LocationService.cs
│
├── Helpers
│   └── GeoHelper.cs
│
└── Platforms
    ├── Android
    ├── iOS
    ├── MacCatalyst
    └── Windows
```

`LocationService` menjadi satu-satunya abstraction yang digunakan ViewModel untuk mengakses perangkat.

Jangan memanggil `Geolocation.Default.GetLocationAsync()` secara langsung dari banyak Page/ViewModel. Gunakan service agar permission, timeout, fallback, error handling, dan logging konsisten.

---

# 3. API .NET MAUI yang digunakan

Gunakan:

```csharp
using Microsoft.Maui.Devices.Sensors;
```

API utama:

```csharp
Geolocation.Default.IsEnabled
Geolocation.Default.GetLastKnownLocationAsync()
Geolocation.Default.GetLocationAsync(...)
Geolocation.Default.StartListeningForegroundAsync(...)
Geolocation.Default.StopListeningForeground()
```

.NET MAUI menyediakan `IGeolocation` melalui `Geolocation.Default` untuk memperoleh koordinat perangkat. API juga menyediakan mekanisme foreground location listening. citeturn578129search0turn578129search2turn578129search4

Untuk GigRadar, **cukup gunakan current location dan foreground listening bila benar-benar diperlukan**.

---

# 4. Permission Strategy

## Prinsip

Permission harus:

- diminta ketika fitur lokasi digunakan;
- menjelaskan manfaat lokasi kepada user;
- menangani `Granted`, `Denied`, `Disabled`, dan kondisi error;
- tidak memblokir aplikasi secara global;
- menyediakan fallback jika user menolak permission.

.NET MAUI menyediakan `Permissions.LocationWhenInUse` untuk Android, iOS, dan Windows; `LocationAlways` tidak diperlukan untuk kebutuhan utama GigRadar. citeturn578129search3

## Permission flow

```text
User membuka Radar / Nearby
        │
        ▼
Cek Location Service
        │
        ├── Disabled
        │      └── tampilkan pesan
        │
        ▼
Cek LocationWhenInUse
        │
        ├── Granted
        │      └── ambil lokasi
        │
        ├── Denied
        │      └── tampilkan penjelasan + Settings
        │
        └── Unknown
               └── RequestAsync
                         │
                         ├── Granted → ambil lokasi
                         └── Denied → fallback
```

---

# 5. Android Configuration

Tambahkan permission berikut ke:

```text
GigRadarMobile/Platforms/Android/AndroidManifest.xml
```

```xml
<manifest xmlns:android="http://schemas.android.com/apk/res/android">

    <uses-permission android:name="android.permission.ACCESS_COARSE_LOCATION" />
    <uses-permission android:name="android.permission.ACCESS_FINE_LOCATION" />

    <uses-feature
        android:name="android.hardware.location"
        android:required="false" />

    <uses-feature
        android:name="android.hardware.location.gps"
        android:required="false" />

    <uses-feature
        android:name="android.hardware.location.network"
        android:required="false" />

</manifest>
```

`ACCESS_FINE_LOCATION` memungkinkan lokasi yang lebih presisi melalui GPS, Wi-Fi, atau network provider; `ACCESS_COARSE_LOCATION` dapat digunakan untuk lokasi yang lebih umum. Microsoft juga mendokumentasikan deklarasi location permission dan hardware feature untuk .NET MAUI Android. citeturn578129search0turn578129search1

### Catatan

Jangan menambahkan:

```xml
<uses-permission android:name="android.permission.ACCESS_BACKGROUND_LOCATION" />
```

kecuali GigRadar nantinya benar-benar mempunyai fitur background location yang sah dan dibutuhkan.

---

# 6. iOS Configuration

Tambahkan ke:

```text
GigRadarMobile/Platforms/iOS/Info.plist
```

```xml
<key>NSLocationWhenInUseUsageDescription</key>
<string>GigRadar menggunakan lokasi Anda untuk menampilkan gig dan event musik terdekat.</string>
```

Untuk Mac Catalyst tambahkan key yang sama ke:

```text
GigRadarMobile/Platforms/MacCatalyst/Info.plist
```

Microsoft menyebut `NSLocationWhenInUseUsageDescription` sebagai konfigurasi yang diperlukan untuk menjelaskan alasan aplikasi menggunakan lokasi pada iOS/Mac Catalyst. citeturn578129search0

---

# 7. Mac Catalyst Location Entitlement

Untuk Mac Catalyst, pastikan:

```text
GigRadarMobile/Platforms/MacCatalyst/Entitlements.plist
```

mempunyai:

```xml
<key>com.apple.security.personal-information.location</key>
<true/>
```

Konfigurasi ini diperlukan untuk mengizinkan akses Location Services pada macOS/Mac Catalyst. citeturn578129search0

---

# 8. Windows

Windows mendukung permission `LocationWhenInUse` pada .NET MAUI. Untuk abstraction service, gunakan API .NET MAUI yang sama dan jangan membuat implementation Windows terpisah kecuali ada kebutuhan khusus. citeturn578129search3

Perlu diperhatikan bahwa **desktop Windows harus mempunyai location services yang aktif dan perangkat harus menyediakan sumber lokasi yang kompatibel**. Karena itu kode wajib menangani kondisi `IsEnabled == false` atau location provider tidak tersedia.

---

# 9. LocationService

Buat:

```text
GigRadarMobile/Services/LocationService.cs
```

Contoh implementasi:

```csharp
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Devices.Sensors;

namespace GigRadarMobile.Services;

public sealed class LocationService
{
    private static readonly TimeSpan RequestTimeout = TimeSpan.FromSeconds(10);

    public bool IsLocationEnabled => Geolocation.Default.IsEnabled;

    public async Task<bool> RequestPermissionAsync()
    {
        var status = await Permissions.CheckStatusAsync<Permissions.LocationWhenInUse>();

        if (status == PermissionStatus.Granted)
            return true;

        if (status == PermissionStatus.Denied)
            return false;

        status = await Permissions.RequestAsync<Permissions.LocationWhenInUse>();

        return status == PermissionStatus.Granted;
    }

    public async Task<Location?> GetCurrentLocationAsync(
        GeolocationAccuracy accuracy = GeolocationAccuracy.Medium,
        CancellationToken cancellationToken = default)
    {
        var permissionGranted = await RequestPermissionAsync();

        if (!permissionGranted)
            return null;

        if (!Geolocation.Default.IsEnabled)
            return null;

        try
        {
            using var timeoutCts = CancellationTokenSource.CreateLinkedTokenSource(
                cancellationToken);

            timeoutCts.CancelAfter(RequestTimeout);

            var request = new GeolocationRequest(
                accuracy,
                RequestTimeout);

#if IOS
            request.RequestFullAccuracy = false;
#endif

            return await Geolocation.Default.GetLocationAsync(
                request,
                timeoutCts.Token);
        }
        catch (PermissionException)
        {
            return null;
        }
        catch (FeatureNotSupportedException)
        {
            return null;
        }
        catch (FeatureNotEnabledException)
        {
            return null;
        }
        catch (OperationCanceledException)
        {
            return null;
        }
    }

    public async Task<Location?> GetLastKnownLocationAsync()
    {
        try
        {
            return await Geolocation.Default.GetLastKnownLocationAsync();
        }
        catch
        {
            return null;
        }
    }
}
```

`GetLocationAsync` dapat meminta permission saat diperlukan, tetapi permission tetap harus dideklarasikan pada konfigurasi platform. Method tersebut mengembalikan `Location?`, sehingga kode harus siap menerima `null`. citeturn578129search7

---

# 10. Register Service di MauiProgram

Di:

```text
GigRadarMobile/MauiProgram.cs
```

tambahkan:

```csharp
builder.Services.AddSingleton<LocationService>();
```

Contoh:

```csharp
using GigRadarMobile.Services;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();

        builder
            .UseMauiApp<App>();

        builder.Services.AddSingleton<LocationService>();

        return builder.Build();
    }
}
```

Sesuaikan dengan konfigurasi `MauiProgram.cs` yang sudah ada. Jangan menghapus dependency injection service yang sekarang.

---

# 11. Model Lokasi

Gunakan model kecil untuk kebutuhan aplikasi:

```csharp
namespace GigRadarMobile.Models;

public sealed record UserLocation(
    double Latitude,
    double Longitude,
    double? AccuracyMeters,
    DateTime Timestamp);
```

Konversi:

```csharp
var userLocation = new UserLocation(
    location.Latitude,
    location.Longitude,
    location.Accuracy,
    location.Timestamp.LocalDateTime);
```

Jangan menyimpan object `Location` langsung ke database karena lokasi user merupakan runtime device state.

---

# 12. Integrasi dengan Nearby API

README GigRadar saat ini sudah memiliki endpoint:

```http
GET /api/events/nearby?lat=&lng=&radius=
```

dan backend menggunakan perhitungan Haversine untuk pencarian event dalam radius. fileciteturn0file0L184-L199

Flow client:

```text
Device GPS
   │
   ▼
LocationService
   │
   ├── latitude
   └── longitude
          │
          ▼
ApiService
          │
          ▼
GET /api/events/nearby
    ?lat=...
    &lng=...
    &radius=...
          │
          ▼
Nearby events
          │
          ▼
Radar / Home / Explore
```

Contoh:

```csharp
var location = await _locationService.GetCurrentLocationAsync();

if (location is null)
{
    // tampilkan fallback
    return;
}

var radiusKm = 10;

var events = await _apiService.GetAsync<List<Event>>(
    $"/api/events/nearby?lat={location.Latitude}&lng={location.Longitude}&radius={radiusKm}");
```

Sesuaikan pemanggilan dengan signature `ApiService` yang sudah ada. Jangan membuat HTTP client baru di ViewModel.

---

# 13. Integrasi Radar

Radar yang sekarang menggunakan `RadarDrawable` dan `GeoHelper.cs`. README menyebut Radar mendukung filter genre, radius, dan "malam ini", serta fallback daftar gig terdekat. fileciteturn0file0L149-L152

Ubahlah source posisi Radar menjadi:

```text
Real Device Location
        │
        ▼
UserLocation
        │
        ├── pusat radar
        ├── jarak user → event
        └── bearing user → event
```

Jangan menggunakan lokasi dummy/hardcoded sebagai pusat radar ketika GPS tersedia.

Gunakan `GeoHelper` untuk:

```csharp
distance = GeoHelper.CalculateDistance(
    userLatitude,
    userLongitude,
    eventLatitude,
    eventLongitude);
```

dan bearing/direction jika helper tersebut sudah tersedia.

---

# 14. Fallback Saat GPS Tidak Tersedia

Aplikasi **tidak boleh crash** ketika:

- user menolak location permission;
- GPS/location service dimatikan;
- device tidak memiliki provider lokasi;
- emulator belum mengirim mock location;
- timeout;
- posisi sementara tidak tersedia.

Fallback yang disarankan:

```text
GPS berhasil
    → tampilkan "Gig terdekat dari lokasi Anda"

GPS gagal
    → gunakan kota/profil user jika tersedia
    → tampilkan "Pilih lokasi secara manual"
    → tetap tampilkan event global / berdasarkan search
```

README menunjukkan User memiliki data `City` serta `Lat/Lng`, sementara saat ini fallback radar sudah berupa daftar gig terdekat. fileciteturn0file0L159-L172 fileciteturn0file0L149-L152

**Catatan penting:** jangan menganggap `City` setara dengan GPS. City hanya fallback kasar dan bukan pengganti koordinat realtime.

---

# 15. Permission UX

Sebelum system permission popup muncul, tampilkan UI singkat:

> **Aktifkan lokasi**
>
> GigRadar menggunakan lokasi Anda untuk menemukan gig musik yang paling dekat dengan posisi Anda.
>
> Lokasi hanya digunakan ketika fitur berbasis lokasi sedang aktif.

CTA:

```text
[ Aktifkan Lokasi ]
```

Jika ditolak:

```text
Lokasi tidak diizinkan.

Anda tetap dapat menggunakan GigRadar.
Untuk melihat gig terdekat, aktifkan izin lokasi di Settings.
```

Jangan terus-menerus menampilkan popup permission pada setiap halaman.

---

# 16. Accuracy

Gunakan tingkat akurasi berdasarkan kebutuhan:

### Home / Nearby

```csharp
GeolocationAccuracy.Medium
```

### Radar

```csharp
GeolocationAccuracy.Medium
```

### Fitur yang benar-benar membutuhkan posisi presisi

```csharp
GeolocationAccuracy.High
```

Jangan selalu menggunakan `Best` karena konsumsi daya dapat lebih tinggi dan tidak selalu diperlukan.

Pada iOS, perangkat dapat memberikan **reduced accuracy**. .NET MAUI menyediakan informasi `Location.ReducedAccuracy`, dan aplikasi yang benar-benar membutuhkan lokasi presisi harus menangani kondisi tersebut. citeturn578129search0

Untuk GigRadar:

```csharp
if (location.ReducedAccuracy)
{
    // tetap izinkan penggunaan:
    // nearby masih dapat berjalan,
    // tetapi jangan klaim posisi sangat presisi.
}
```

---

# 17. Foreground Tracking

Untuk tahap awal, GigRadar **tidak membutuhkan continuous background tracking**.

Gunakan current location ketika:

- membuka Radar;
- membuka Nearby;
- melakukan refresh;
- mengubah radius;
- user menekan "gunakan lokasi saya".

Foreground listening (`StartListeningForegroundAsync`) hanya digunakan apabila Radar nantinya membutuhkan update posisi saat user sedang bergerak dalam aplikasi. API .NET MAUI membatasi foreground listening pada saat aplikasi berada di foreground. citeturn578129search2

Contoh konsep:

```csharp
var request = new GeolocationListeningRequest(
    GeolocationAccuracy.Medium);

var started =
    await Geolocation.Default.StartListeningForegroundAsync(request);
```

Berhentikan listener ketika halaman tidak lagi memerlukannya:

```csharp
Geolocation.Default.StopListeningForeground();
```

---

# 18. Security & Privacy

## Jangan lakukan

```text
❌ kirim lokasi setiap beberapa detik ke server
❌ simpan semua history GPS
❌ jalankan background GPS tanpa fitur yang jelas
❌ meminta LocationAlways tanpa kebutuhan
❌ expose lokasi user ke user lain
❌ memasukkan latitude/longitude ke log production
```

## Yang diperbolehkan

```text
✅ gunakan lokasi untuk nearby search
✅ gunakan lokasi sebagai pusat radar
✅ kirim latitude/longitude hanya saat request nearby
✅ gunakan HTTPS
✅ hapus/abaikan data lokasi setelah request selesai bila tidak dibutuhkan
```

Backend GigRadar saat ini memakai REST API dengan JWT Bearer. fileciteturn0file0L21-L40

---

# 19. API Request Rules

Untuk endpoint:

```http
GET /api/events/nearby?lat={lat}&lng={lng}&radius={radius}
```

validasi client:

```text
Latitude  : -90 .. 90
Longitude : -180 .. 180
Radius    : > 0
```

Contoh:

```csharp
if (location.Latitude is < -90 or > 90)
    return;

if (location.Longitude is < -180 or > 180)
    return;

var radiusKm = Math.Clamp(requestedRadiusKm, 1, 100);
```

Server juga wajib melakukan validasi. Jangan percaya nilai dari client.

---

# 20. State Management

Tambahkan state ke Radar/Home ViewModel:

```csharp
public LocationStatus LocationStatus { get; private set; }

public double? CurrentLatitude { get; private set; }

public double? CurrentLongitude { get; private set; }

public bool IsUsingDeviceLocation =>
    CurrentLatitude.HasValue &&
    CurrentLongitude.HasValue;
```

Contoh enum:

```csharp
public enum LocationStatus
{
    Unknown,
    RequestingPermission,
    PermissionDenied,
    Disabled,
    Available,
    Unavailable,
    Error
}
```

UI bisa memetakan status tersebut:

```text
Available
→ "📍 Menggunakan lokasi Anda"

PermissionDenied
→ "📍 Aktifkan lokasi"

Disabled
→ "📍 Location Services mati"

Unavailable
→ "📍 Lokasi tidak tersedia"
```

---

# 21. UX Radar yang Disarankan

Tambahkan kontrol:

```text
┌──────────────────────────────────────┐
│  RADAR                               │
│                                      │
│  📍 Lokasi saya                      │
│  Radius: 5 km    [ - ] [ + ]         │
│                                      │
│        • Event A                     │
│             • Event B                │
│    YOU ●                             │
│                     • Event C        │
│                                      │
│  8 gig ditemukan                     │
│                                      │
│  [ Lihat daftar gig terdekat ]       │
└──────────────────────────────────────┘
```

Tambahkan tombol:

```text
[ 📍 Gunakan lokasi saya ]
```

ketika user sedang memakai mode manual.

---

# 22. Manual Location Fallback

Buat opsi:

```text
Gunakan lokasi perangkat
        atau
Pilih kota secara manual
```

Manual location berguna ketika:

- user sedang merencanakan perjalanan;
- GPS desktop Windows tidak tersedia;
- permission ditolak;
- user ingin melihat gig kota lain.

Contoh:

```text
LocationMode:
- Device
- ManualCity
```

Jangan mencampur mode manual dan GPS tanpa indikator UI yang jelas.

---

# 23. Testing Matrix

## Android Physical Device

Test:

- GPS aktif
- GPS mati
- permission pertama kali
- permission denied
- permission diberikan
- airplane mode
- outdoor
- indoor
- weak GPS
- network only

## Android Emulator

Test dengan mock location.

Pastikan:

```bash
adb devices
```

mendeteksi device/emulator.

README project memang menyediakan flow Android melalui `adb devices` dan build `net10.0-android`. fileciteturn0file0L283-L295

## iPhone

Test:

- Allow Once / When In Use
- location disabled
- reduced accuracy
- precise location
- permission denied

## Windows

Test:

- Windows Location ON
- Windows Location OFF
- desktop tanpa GPS hardware
- network location tersedia
- manual location fallback

## Mac Catalyst

Test:

- Location Services ON
- permission prompt
- permission denied
- entitlement benar

---

# 24. Definition of Done

Fitur GPS dianggap selesai jika:

- [ ] Android meminta location permission dengan benar.
- [ ] iOS meminta `When In Use`.
- [ ] Mac Catalyst memiliki usage description dan entitlement.
- [ ] Windows dapat menggunakan location API bila provider tersedia.
- [ ] Tidak ada hardcoded latitude/longitude untuk user.
- [ ] Radar menggunakan device location jika tersedia.
- [ ] Nearby memanggil `/api/events/nearby`.
- [ ] Permission denied tidak membuat app crash.
- [ ] GPS disabled tidak membuat app crash.
- [ ] Timeout ditangani.
- [ ] `null` location ditangani.
- [ ] Fallback manual/kota tersedia.
- [ ] Lokasi tidak dipantau di background.
- [ ] Lokasi tidak disimpan sebagai history secara default.
- [ ] UI menunjukkan apakah device location sedang aktif.
- [ ] Semua platform diuji.

---

# 25. Instruksi untuk AI Coding Agent / GitHub Copilot

Gunakan dokumen ini sebagai aturan implementasi.

## Tugas

> Implementasikan sistem GPS/device location untuk GigRadarMobile sesuai `GPS_LOCATION.md`.

### Wajib

1. Inspect project structure terlebih dahulu.
2. Gunakan service abstraction `LocationService`.
3. Gunakan `Geolocation.Default`.
4. Gunakan `Permissions.LocationWhenInUse`.
5. Konfigurasi Android Manifest.
6. Konfigurasi iOS `Info.plist`.
7. Konfigurasi Mac Catalyst `Info.plist`.
8. Konfigurasi Mac Catalyst entitlement.
9. Register service melalui dependency injection.
10. Integrasikan Radar dengan lokasi device.
11. Integrasikan Home/Nearby dengan endpoint existing:
   ```http
   GET /api/events/nearby?lat=&lng=&radius=
   ```
12. Pertahankan `GeoHelper.cs`.
13. Pertahankan `ApiService.cs`.
14. Jangan membuat `HttpClient` baru jika service API yang ada dapat dipakai.
15. Jangan mengubah authentication/JWT.
16. Jangan mengubah schema database tanpa kebutuhan eksplisit.
17. Jangan membuat background tracking.
18. Tangani semua permission/error state dengan UI yang nyaman.

### Sebelum mengedit

Periksa file:

```text
GigRadarMobile/MauiProgram.cs
GigRadarMobile/Services/ApiService.cs
GigRadarMobile/Helpers/GeoHelper.cs
GigRadarMobile/Helpers/RadarState.cs
GigRadarMobile/ViewModels/HomeViewModel.cs
GigRadarMobile/ViewModels/ExploreViewModel.cs
GigRadarMobile/ViewModels/RadarViewModel.cs
GigRadarMobile/Views/HomePage.xaml
GigRadarMobile/Views/ExplorePage.xaml
GigRadarMobile/Views/RadarPage.xaml
GigRadarMobile/Platforms/Android/AndroidManifest.xml
GigRadarMobile/Platforms/iOS/Info.plist
GigRadarMobile/Platforms/MacCatalyst/Info.plist
GigRadarMobile/Platforms/MacCatalyst/Entitlements.plist
```

### Jangan lakukan

```text
DO NOT:
- rewrite entire project
- replace MVVM architecture
- replace ApiService
- replace GeoHelper
- add Google Maps SDK just for obtaining GPS
- add background location
- add permanent GPS history
- expose user's exact coordinates in public profiles
- hardcode one city as the device location
```

### Output yang diharapkan

Setelah implementasi:

```text
1. LocationService.cs
2. Updated platform permissions/configuration
3. Updated MauiProgram.cs
4. Updated Radar ViewModel
5. Updated Home/Nearby ViewModel
6. Updated UI state for location permission
7. Build fixes for all supported targets
```

### Validasi

Jalankan:

```bash
dotnet build GigRadarApi/GigRadarApi.csproj

dotnet build GigRadarMobile/GigRadarMobile.csproj -f net10.0-windows10.0.19041.0

dotnet build GigRadarMobile/GigRadarMobile.csproj -f net10.0-android
```

Jika environment mendukung:

```bash
dotnet build GigRadarMobile/GigRadarMobile.csproj -f net10.0-ios

dotnet build GigRadarMobile/GigRadarMobile.csproj -f net10.0-maccatalyst
```

---

# 26. Referensi

- Microsoft Learn — .NET MAUI Geolocation:
  https://learn.microsoft.com/dotnet/maui/platform-integration/device/geolocation
- Microsoft Learn — .NET MAUI Permissions:
  https://learn.microsoft.com/dotnet/maui/platform-integration/appmodel/permissions
- Microsoft Learn — IGeolocation:
  https://learn.microsoft.com/dotnet/api/microsoft.maui.devices.sensors.igeolocation

Dokumen ini mengikuti API dan konfigurasi .NET MAUI yang didokumentasikan Microsoft untuk geolocation dan permissions. citeturn578129search0turn578129search2turn578129search3
