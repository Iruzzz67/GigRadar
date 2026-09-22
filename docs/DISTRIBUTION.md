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
  "openSwagger": false
}
```

| Field | Keterangan |
| :--- | :--- |
| `connectionString` | Koneksi Npgsql yang dipakai API. **Wajib disesuaikan** bila PostgreSQL komputer tujuan memakai password/port/nama database berbeda. Diteruskan ke API sebagai env var `ConnectionStrings__DefaultConnection`. |
| `postgresServiceName` | Nama service PostgreSQL. Kosongkan agar launcher mendeteksi otomatis (pola `postgresql*`). Isi mis. `postgresql-x64-16` bila ada beberapa versi terpasang. |
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

## 7. Build APK Android (terpisah)

Distribusi Windows di atas tidak menyertakan Android. Untuk APK:

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File scripts\build-android-release.ps1
```

Detail: [ANDROID_PRODUCTION.md](./ANDROID_PRODUCTION.md).
