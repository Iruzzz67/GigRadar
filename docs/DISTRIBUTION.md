# 📦 GigRadar — Build & Distribusi (satu exe)

Dokumen ini menjelaskan cara mengubah GigRadar menjadi **satu paket siap klik**:
`GigRadar.exe` yang otomatis menyalakan PostgreSQL, menjalankan API, dan membuka
aplikasi desktop — tanpa perlu install .NET, tanpa `dotnet run`, tanpa script.

---

## 1. Membangun paket distribusi

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File scripts\build-release.ps1
```

Opsi yang tersedia:

| Opsi | Fungsi |
| :--- | :--- |
| `-SkipApp` | Lewati build aplikasi desktop (jauh lebih cepat, untuk uji launcher + API saja) |
| `-NoZip` | Jangan buat arsip ZIP |
| `-OutputDir <nama>` | Ganti folder keluaran (default `dist`) |

Hasilnya:

```text
dist\
├── GigRadar\
│   ├── GigRadar.exe            <- KLIK DUA KALI INI
│   ├── gigradar.config.json    <- konfigurasi koneksi DB (opsional diedit)
│   ├── api\GigRadarApi.exe     <- backend, dijalankan otomatis oleh launcher
│   └── app\GigRadarMobile.exe  <- aplikasi desktop, dibuka otomatis
└── GigRadar-win-x64.zip        <- arsip siap dibagikan (~170 MB)
```

Semua biner dipublikasikan **self-contained** (`win-x64`), jadi komputer tujuan
**tidak perlu** memasang .NET Runtime, Windows App SDK, maupun Visual Studio.

---

## 2. Menjalankan paket

1. Salin folder `dist\GigRadar` (atau ekstrak ZIP-nya) ke komputer tujuan.
2. Klik dua kali **`GigRadar.exe`**.
3. Launcher akan berjalan berurutan:
   - memastikan **service PostgreSQL** menyala (bila mati, muncul dialog **UAC** — klik *Yes*);
   - menjalankan **API** di `http://localhost:5000` dan menunggu sampai siap;
   - membuka **aplikasi desktop GigRadar**.
4. Jendela konsol launcher tetap terbuka sebagai panel kontrol:

| Perintah | Aksi |
| :--- | :--- |
| `Enter` | Matikan API dan keluar |
| `R` | Buka ulang aplikasi desktop (bila sudah tertutup) |
| `S` | Buka Swagger UI di browser |

> Launcher **tidak** menghentikan PostgreSQL saat keluar — hanya API. Ini
> supaya service DB tidak perlu dinyalakan ulang (dan tidak memicu UAC lagi)
> pada peluncuran berikutnya.

---

## 3. Konfigurasi (`gigradar.config.json`)

File ini dibuat otomatis di samping `GigRadar.exe`. Edit bila perlu:

```json
{
  "connectionString": "Host=localhost;Port=5432;Database=gigradar;Username=postgres;Password=postgres",
  "postgresServiceName": "",
  "jwtSecret": "",
  "openSwagger": false
}
```

| Field | Keterangan |
| :--- | :--- |
| `connectionString` | Koneksi Npgsql yang dipakai API. **Wajib disesuaikan** bila PostgreSQL komputer tujuan memakai password/port/nama database berbeda. Diteruskan ke API sebagai env var `ConnectionStrings__DefaultConnection`. |
| `postgresServiceName` | Nama service PostgreSQL. Kosongkan agar launcher mendeteksi otomatis (pola `postgresql*`, lewat PowerShell dengan cadangan `sc.exe`). Isi mis. `postgresql-x64-16` bila ada beberapa versi terpasang. |
| `jwtSecret` | Kunci penandatangan JWT instalasi ini (diteruskan sebagai `JwtSettings__SecretKey`). Kosongkan untuk memakai nilai bawaan `appsettings.json` — **hanya untuk demo**; untuk pemakaian nyata isi dengan kata acak panjang agar tiap instalasi punya kunci sendiri. |
| `openSwagger` | `true` untuk membuka Swagger UI otomatis di browser setiap kali dijalankan. |

Catatan: bila variabel lingkungan `DATABASE_URL` sudah diset di sistem, API
memprioritaskannya di atas `connectionString` (konvensi deploy Render/Neon).

### Migrasi database

API menjalankan `db.Database.Migrate()` saat startup, jadi database kosong akan
langsung dibuat beserta seluruh tabelnya (plus data seed) pada peluncuran pertama.

---

## 4. Prasyarat di komputer tujuan

| Kebutuhan | Catatan |
| :--- | :--- |
| **PostgreSQL 16** | Service harus terpasang. Launcher menyalakannya otomatis (butuh UAC). |
| **Akun admin Windows** | Hanya untuk menyalakan service DB bila sedang mati. |
| **Database `gigradar`** | Dibuat otomatis oleh migrasi; cukup pastikan user/password di `gigradar.config.json` benar. |

Runtime .NET **tidak** diperlukan — sudah dibundel.

---

## 5. Format distribusi & ukuran

| Bagian | Perkiraan ukuran |
| :--- | :--- |
| `GigRadar.exe` (launcher, single file) | ~64 MB |
| `api\` (single file, self-contained) | ~100 MB |
| `app\` (MAUI Windows, self-contained) | ~255 MB |
| **Total folder** | **~420 MB** (ZIP ~170 MB) |

Aplikasi desktop MAUI tidak bisa dijadikan satu file karena memuat banyak
komponen native WinUI/Windows App SDK. Karena itu yang dibuat single-file
adalah **launcher** dan **API**, sementara aplikasi desktop tetap berupa folder.

---

## 6. Pemecahan masalah

| Gejala | Sebab & solusi |
| :--- | :--- |
| `[x] PostgreSQL tidak siap` | Service tidak terpasang atau UAC ditolak. Jalankan manual: `net start postgresql-x64-16`, atau isi `postgresServiceName`. |
| `[x] File API tidak ditemukan` | Folder `api\` tidak ikut tersalin. Ekstrak ulang ZIP secara utuh (jangan hanya mengambil `GigRadar.exe`). |
| `[x] API tidak siap dalam 90 detik` | Koneksi DB salah. Launcher mencetak 30 baris log API terakhir — biasanya berupa `Npgsql` authentication error. Perbaiki `connectionString`. |
| `[!] Aplikasi desktop tidak ditemukan` | Folder `app\` belum dibuild (mis. dijalankan dengan `-SkipApp`). |
| Port 5000 sudah dipakai | Launcher otomatis memakai API yang sudah jalan dan **tidak** mematikannya saat keluar. |

---

## 7. Uji portabilitas (komputer tanpa .NET)

Paket `dist\GigRadar` diuji agar bisa dipastikan jalan di komputer yang **tidak**
memasang .NET, Visual Studio, maupun Windows App SDK. Ringkasan pengujian:

| Yang diuji | Cara uji | Hasil |
| :--- | :--- | :--- |
| Runtime .NET terpasang | Daftar modul yang benar-benar dimuat proses (`Get-Process ... .Modules`) | **Tidak dibutuhkan** — `coreclr.dll`, `clrjit.dll`, `hostfxr.dll`, `hostpolicy.dll` dimuat dari folder `app\` sendiri |
| Windows App Runtime (MSIX) | Modul + properti MSBuild | **Tidak dibutuhkan** — `Microsoft.WindowsAppRuntime.dll` & `Microsoft.UI.Xaml.dll` dimuat dari `app\`, bukan dari `C:\Program Files\WindowsApps` |
| Visual C++ Redistributable | Modul native yang dimuat | **Tidak dibutuhkan** — hanya `ucrtbase.dll`/`msvcp_win.dll` (komponen bawaan Windows) |
| WebView2 Runtime | Isi folder + modul | **Tidak dipakai** |
| Build self-contained benar-benar aktif | `dotnet msbuild -getProperty` | `SelfContained=true`, `WindowsAppSDKSelfContained=true` |
| API tanpa variabel lingkungan apa pun | Dijalankan dengan environment dikosongkan (`PATH` hanya `System32`, tanpa `DOTNET_ROOT`) | **HTTP 200** di `/api/events` |
| Arsitektur | PE/exe | **x64 saja** (`win-x64`) — ARM64 butuh build terpisah |
| Sistem minimum | Spesifikasi MAUI Windows | Windows 10 build **19041+** (diuji pada 19045) |
| Prasyarat yang tetap wajib | — | **PostgreSQL 16 x64** terpasang sebagai service |

Catatan metode: untuk `app\` (multi-file) asal runtime terbukti dari daftar modul;
untuk `GigRadar.exe` dan `api\GigRadarApi.exe` (single-file) daftar modul tidak bisa
dibaca PSAPI, sehingga buktinya dari tiga hal: ukuran file (67 MB / 103 MB, sedangkan
framework-dependent hanya ~0,2 MB), manifest bundle (182 dan 392 assembly framework,
termasuk `System.Private.CoreLib.dll`), dan berjalannya seluruh alur di atas dengan
environment kosong.

### Yang perlu diperbaiki sebelum dibagikan

Sudah diperbaiki:

- [x] `.pdb` (symbol debug) dan `api\web.config` (artefak IIS) ikut terbawa di paket → kini disaring oleh `build-release.ps1`.
- [x] Deteksi service PostgreSQL bergantung pada PowerShell → ditambah cadangan `sc.exe` agar tetap jalan bila PowerShell diblokir kebijakan/GPO.
- [x] Kunci JWT dipakai bersama semua instalasi → `gigradar.config.json` kini punya `jwtSecret` (isi agar tiap instalasi unik).

Masih terbuka (relevan untuk langkah installer berikutnya):

- [ ] **PostgreSQL belum ikut dipaket.** Installer harus mendeteksi service `postgresql*` dan mengarahkan pengguna memasang PostgreSQL 16, atau menyertakan langkah instalasinya.
- [ ] **exe belum ditandatangani.** Di komputer lain akan muncul *SmartScreen* ("Windows protected your PC"). Perlu sertifikat code-signing, atau dokumentasikan cara menekan *More info → Run anyway*.
- [ ] **Jangan pasang ke `C:\Program Files`.** `gigradar.config.json` perlu diedit pengguna; folder tersebut butuh hak admin. Pakai instalasi per-user, mis. `%LOCALAPPDATA%\Programs\GigRadar`.
- [ ] **Belum ada shortcut Start Menu + uninstaller** — keduanya peran installer.
- [ ] **Password DB default `postgres/postgres`** pada template config: beri peringatan bila masih dipakai.
- [ ] **Build ARM64** bila target memakai laptop Snapdragon (Windows on ARM).
- [ ] Setelah `dist\` dibersihkan manual, ZIP lama masih berisi file tersebut — jalankan ulang `build-release.ps1` untuk menyegarkan paket dan arsipnya.

---

## 8. Build APK Android (terpisah)

Distribusi Windows di atas tidak menyertakan Android. Untuk APK:

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File scripts\build-android-release.ps1
```

Detail: [ANDROID_PRODUCTION.md](./ANDROID_PRODUCTION.md).
