# GigRadar — Android Production (Checklist & Panduan)

Panduan build **Release APK** GigRadar untuk perangkat Android fisik, tanpa Visual Studio.

---

## 1. Konfigurasi URL API (WAJIB sebelum release final)

Status: **BLOCKER — production HTTPS API endpoint belum tersedia.**

| Konfigurasi | File | Nilai saat ini |
|---|---|---|
| Debug (Android) | `GigRadarMobile/Services/ApiConfiguration.cs` | `http://localhost:5000/` (via `adb reverse`) |
| **Release (Android)** | `GigRadarMobile/Services/ApiConfiguration.cs` | `https://YOUR-PRODUCTION-API-DOMAIN/` ← **placeholder, wajib diganti** |

Cara mengganti saat production API sudah siap — cukup **satu baris** di
`GigRadarMobile/Services/ApiConfiguration.cs`, blok `#elif RELEASE && ANDROID`:

```csharp
public const string BaseUrl = "https://api.domain-kamu.com/";
```

Aturan:
- Release **wajib HTTPS** — cleartext HTTP sudah dimatikan untuk Release di `AndroidManifest.xml`.
- Jangan pernah memakai `localhost`, `10.0.2.2`, atau IP LAN di Release.

---

## 2. Membuat keystore release (sekali saja, JANGAN di-commit)

`*.keystore`/`*.jks` sudah masuk `.gitignore`. Simpan di luar repository, mis. `C:\keystore\`.

Cari `keytool` (ikut JDK): `C:\Program Files\Android\jdk\...\bin\keytool.exe`, lalu:

```powershell
& "C:\Program Files\Android\jdk\jdk-8.0.302.8-hotspot\jdk8u302-b08\bin\keytool.exe" -genkeypair -v `
  -keystore C:\keystore\gigradar-release.keystore `
  -alias gigradar `
  -keyalg RSA -keysize 2048 -validity 10000
```

> ⚠️ **Keystore hilang = tidak bisa update app** (harus ganti ApplicationId). Backup password + file.

---

## 3. Build Release APK

### Cara gampang — script otomatis (recommended)

```powershell
.\scripts\build-android-release.ps1
```

Script akan: validasi environment → restore → build Release APK (signed bila env vars diset)
→ cetak lokasi APK.

### Cara manual

```powershell
# 1. Set environment variables signing (sesuai keystore kamu)
$env:GIGRADAR_KEYSTORE_PATH     = "C:\keystore\gigradar-release.keystore"
$env:GIGRADAR_KEYSTORE_PASSWORD = "********"
$env:GIGRADAR_KEY_ALIAS         = "gigradar"
$env:GIGRADAR_KEY_PASSWORD      = "********"

# 2. Build
dotnet publish GigRadarMobile/GigRadarMobile.csproj -f net10.0-android -c Release
```

Output APK ada di:
`GigRadarMobile\bin\Release\net10.0-android\publish\*.apk`

Tanpa env vars, APK Release tetap dihasilkan **tanpa signature release** (tidak bisa dipasang).

---

## 4. Install APK ke perangkat

### Via ADB (USB debugging aktif)

```bash
adb devices              # pastikan device terdeteksi
adb install -r path\ke\GigRadarMobile.apk
```

### Manual (tanpa PC)

```text
Copy APK ke HP (kabel/Drive/WA)
→ Buka file APK dari Files
→ Izinkan install dari sumber tidak dikenal bila ditanya
→ Install
→ Buka GigRadar
```

---

## 5. Checklist produksi

```text
[ ] Production API menggunakan HTTPS
[ ] Tidak menggunakan localhost
[ ] Tidak menggunakan 10.0.2.2
[ ] Cleartext HTTP disabled (Release)
[ ] JWT authentication tested
[ ] Login tested
[ ] Register tested
[ ] Event list tested
[ ] Event detail tested
[ ] Artist tested
[ ] Ticket purchase tested
[ ] My Tickets tested
[ ] EO dashboard tested
[ ] Location permission tested
[ ] App icon tested (bukan default MAUI)
[ ] Splash screen tested
[ ] Release build successful
[ ] APK generated
[ ] APK installed on physical Android
[ ] APK opens without Visual Studio
[ ] API connection tested on mobile data/Wi-Fi
[ ] Signing verified
[ ] No secrets committed
```

---

## 6. Identitas aplikasi

| Properti | Nilai | Catatan |
|---|---|---|
| ApplicationTitle | `GigRadar` | |
| ApplicationId | `com.gigradar.app` | Jangan diubah sembarangan |
| ApplicationDisplayVersion | `1.0` | Versi human-readable |
| ApplicationVersion | `1` | Android `versionCode` — naikkan (+1) tiap release update |

---

## 7. CI/CD (opsional)

Workflow GitHub Actions tersedia di `.github/workflows/android-release.yml`.
Untuk signed build di CI, set repository secrets:

```text
ANDROID_KEYSTORE_BASE64     # keystore di-encode base64
ANDROID_KEYSTORE_PASSWORD
ANDROID_KEY_ALIAS
ANDROID_KEY_PASSWORD
```

Keystore tidak pernah masuk repository.
