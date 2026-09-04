# 🎸 Rekapan Project Terbaru — GigRadar

> **Menghubungkan skena, menemukan suara lokal.**
> Rekapan kondisi project per **September 2026** — berdasarkan isi kode di repository (versi terbaru, mencakup semua fitur yang sudah diimplementasikan sejak rekapan awal).

---

## 1. Ringkasan

GigRadar adalah aplikasi untuk menemukan gigs musik lokal, underground, showcase, konser kecil, dan acara komunitas berdasarkan lokasi serta preferensi musik pengguna — kini dilengkapi **sistem tiket berjenjang, dashboard EO/Admin, dan manajemen event dari dalam aplikasi**.

Project terdiri dari **3 komponen utama** dalam satu direktori:

| Komponen | Jenis | Framework / Teknologi | Target |
|---|---|---|---|
| `GigRadarApi` | Backend REST API | ASP.NET Core Web API (.NET 8) | `net8.0` |
| `GigRadarMobile` | Aplikasi mobile/desktop | .NET MAUI (.NET 10) + MVVM | Android, iOS, Mac Catalyst, Windows |
| `GigRadarLauncher` | Launcher console | .NET Console (.NET 8) | Windows — menjalankan API + membuka Swagger |

File pendukung di root:
- `GigRadarLauncher.exe` — hasil publish launcher
- `StartGigRadar.bat` — shortcut menjalankan launcher (API + Swagger)
- `StartMobileApp.bat` — shortcut menjalankan build Windows app
- `GIGRADAR_MOBILE_APP_NET_MAUI.md` — dokumen desain/visi produk lengkap
- `GIGRADAR_MULTIPLATFORM_FIX.md` — panduan perbaikan arsitektur multi-platform
- `REKAPAN_PROJECT.md` — rekapan versi sebelumnya (beberapa bagian sudah usang)

**Catatan deviasi penting:** dokumen desain menyebut PostgreSQL, namun implementasi backend menggunakan **SQLite** (file `GigRadarApi/GigRadar.db`).

---

## 2. Arsitektur

```text
GigRadarMobile (.NET MAUI, dark theme)
        |
        |  HTTPS/HTTP REST + JWT Bearer
        v
GigRadarApi (ASP.NET Core Web API)
        |
        +-- Auth (register/login, BCrypt, JWT)
        +-- Events (list, detail, nearby, tonight, weekend, recommended,
        |           CRUD + status event, managed/summary untuk EO/Admin)
        +-- Venues (daftar + tambah venue)
        +-- Artists (list, detail + audio tracks)
        +-- Tickets (tipe tiket, beli dengan verifikasi data diri,
        |            riwayat, validasi QR, kelola tipe tiket)
        +-- Users (profil, preferensi genre, favorit)
        |
        v
SQLite (EF Core, file GigRadar.db)

GigRadarLauncher (console) — menyalakan API di http://localhost:5000 dan membuka Swagger
```

Prinsip yang dipakai:
- Mobile app **tidak** mengakses database langsung — semua lewat REST API.
- Autentikasi memakai **JWT Bearer**; password di-hash **BCrypt**.
- Database dibuat otomatis (`EnsureCreated`) + **seed data**; skema baru ditambahkan **idempoten** ke DB lama lewat `TicketSchemaBootstrap` (tanpa menghapus data).
- CORS dibuka penuh (`AllowAll`) — nyaman untuk development.

---

## 3. Backend — `GigRadarApi`

### 3.1 Teknologi & Paket

- ASP.NET Core Web API — `net8.0`, Swagger UI (`/swagger`, aktif saat Development)
- `Microsoft.EntityFrameworkCore.Sqlite` 8.0.8 (+ Design)
- `Microsoft.AspNetCore.Authentication.JwtBearer` 8.0.8
- `BCrypt.Net-Next` 4.0.3
- `Swashbuckle.AspNetCore` 6.4.0

### 3.2 Struktur Folder

```text
GigRadarApi/
├── Program.cs                  # Setup DI, JWT, Swagger, CORS, auto-create DB + bootstrap skema
├── appsettings.json            # koneksi SQLite + JwtSettings (SecretKey, Issuer, Audience, ExpirationMinutes=1440)
├── Data/
│   ├── AppDbContext.cs         # 12 DbSet + constraint unik + seed data
│   └── TicketSchemaBootstrap.cs# Tambah kolom/tabel baru + seed EO ke DB lama (idempoten)
├── Models/                     # User, Genre, Artist, Venue, Event (+EventTicketType, Favorite, EventArtist), Ticket
├── Controllers/                # Auth, Events, Artists, Genres, Tickets, Users, Venues (7 controller)
├── Services/                   # AuthService, EventService, TicketService
└── Helpers/
    └── Constants.cs
```

### 3.3 Model Database (tabel)

| Tabel | Field penting |
|---|---|
| **Users** | UserId, Name, Email (unik), PasswordHash, Role (`User`/`EO`/`Admin`/`Artist`), City, Lat/Lng, PhotoUrl |
| **UserPreferences** | UserId, GenreId, Weight — preferensi genre hasil onboarding |
| **Genres** | GenreId, Name, Icon (emoji) |
| **Artists** | ArtistId, Name, Bio, Genre, PhotoUrl, SocialLinks |
| **AudioTracks** | TrackId, ArtistId, Title, AudioUrl, DurationSeconds (default 30 — audio preview) |
| **Venues** | VenueId, Name, Address, City, Lat/Lng, Capacity, PhotoUrl |
| **Events** | EventId, Name, Description, PosterUrl, **TicketLink** (link pembelian eksternal), VenueId, StartDate, EndDate, Lat/Lng, GenreId, **CreatedBy** (pemilik event), **Status** (`Published`/`Draft`/`SoldOut`/`Completed`), MinPrice, MaxPrice, Capacity, ViewsCount, SavesCount |
| **EventArtists** | EventId + ArtistId (unik) + Order — line-up event |
| **EventTicketTypes** | EventTicketTypeId, EventId, Name, Description, Price, Stock, SortOrder — tipe tiket per event (Festival/Tribun/Bundling, dsb.) |
| **Tickets** | TicketId, EventId, UserId, TicketType, Price, **BuyerName/BuyerPhone/BuyerEmail/BuyerDateOfBirth** (data pembeli), QRCode, Status (`Active`/`Used`), PurchasedAt |
| **Favorites** | UserId + EventId (unik) — save event |
| **Follows** | UserId → ArtistId/VenueId/EventOrganizerId (unik per user+artist) |

### 3.4 Endpoint API (32 endpoint)

| Method | Route | Auth | Keterangan |
|---|---|---|---|
| POST | `/api/auth/register` | — | Daftar (BCrypt + langsung dapat token) |
| POST | `/api/auth/login` | — | Login (BCrypt verify + JWT) |
| GET | `/api/events` | — | Semua event (include venue, genre, line-up) |
| GET | `/api/events/{id}` | — | Detail event |
| GET | `/api/events/nearby?lat=&lng=&radius=` | — | Event dalam radius (km, default 50), Haversine |
| GET | `/api/events/tonight` | — | Event hari ini |
| GET | `/api/events/weekend` | — | Event sampai akhir pekan |
| GET | `/api/events/recommended` | 🔒 | Rekomendasi rule-based per user |
| GET | `/api/events/managed` | 🔒 EO/Admin | Event yang dikelola: **Admin semua, EO hanya event miliknya** |
| GET | `/api/events/managed/summary` | 🔒 EO/Admin | Statistik dashboard EO (total event, mendatang, tiket terjual, revenue, per-event) |
| POST | `/api/events` | 🔒 EO/Admin | **Buat event baru** — `CreatedBy` diambil dari JWT, tersimpan ke DB |
| PUT | `/api/events/{id}` | 🔒 EO/Admin | Update event (cek kepemilikan; selain pemilik/Admin → 403) |
| PUT | `/api/events/{id}/status` | 🔒 EO/Admin | Ubah status event: `Published`/`Draft`/`SoldOut`/`Completed` (whitelist, 400 bila tidak valid) |
| DELETE | `/api/events/{id}` | 🔒 EO/Admin | Hapus event + relasinya (tiket, favorit, line-up) — cek kepemilikan |
| GET | `/api/venues` | — | Daftar venue (untuk picker form event) |
| POST | `/api/venues` | 🔒 EO/Admin | Tambah venue baru (nama wajib) |
| GET | `/api/artists` | — | Semua artist + tracks |
| GET | `/api/artists/{id}` | — | Detail artist + tracks + event line-up |
| GET | `/api/genres` | — | Semua genre |
| GET | `/api/tickets` | 🔒 | Tiket milik user login |
| GET | `/api/tickets/{id}` | 🔒 | Detail tiket |
| GET | `/api/tickets/event/{eventId}/types` | 🔒 | Daftar tipe tiket event (urutan SortOrder) |
| POST | `/api/tickets` | 🔒 | **Beli tiket** — validasi tipe, data diri, umur ≥ 17, stok (stok berkurang otomatis), generate QRCode unik |
| POST | `/api/tickets/validate` | 🔒 EO/Admin | Validasi QR → status tiket jadi `Used` |
| POST | `/api/tickets/event/{eventId}/types` | 🔒 EO/Admin | Tambah tipe tiket (cek kepemilikan event, SortOrder otomatis) |
| PUT | `/api/tickets/types/{typeId}` | 🔒 EO/Admin | Edit tipe tiket (nama, deskripsi, harga, stok) |
| DELETE | `/api/tickets/types/{typeId}` | 🔒 EO/Admin | Hapus tipe tiket |
| GET | `/api/users/me` | 🔒 | Profil + preferensi genre |
| PUT | `/api/users/me` | 🔒 | Update nama/kota/photo URL |
| POST | `/api/users/preferences` | 🔒 | Simpan (ganti total) preferensi genre |
| POST | `/api/users/favorites/{eventId}` | 🔒 | Toggle save/un-save event |
| GET | `/api/users/favorites` | 🔒 | Daftar favorit |

### 3.5 Auth, Role & Keamanan

- Token JWT berisi claim `UserId`, `Name`, `Email`, `Role`; kedaluwarsa 1440 menit (1 hari).
- Role yang dikenal: `User` (default), `EO`, `Admin`, `Artist`.
- **Role-based authorization** di semua endpoint kelola: `[Authorize(Roles = "EO,Admin")]` → role User dapat **403**.
- **Cek kepemilikan**: EO hanya bisa kelola/ubah/hapus **event yang ia buat** (`CreatedBy`); Admin bebas semua event. Melanggar → 403 `"Kamu tidak memiliki akses untuk mengelola event ini"`.
- **Status event whitelist** (`Published/Draft/SoldOut/Completed`) — status tak dikenal → 400.
- **Pembelian tiket hanya untuk event `Published`** — SoldOut/Completed/Draft ditolak backend.
- Verifikasi pembeli: nama ≥ 3 karakter, telepon ≥ 9 digit, format email valid, dan **umur minimal 17 tahun** (dari tanggal lahir).
- Catatan keamanan: `register` menerima field role dari client tanpa batasan — perlu perhatian bila dipakai produksi.

### 3.6 Alur Pembelian Tiket (Backend)

```text
POST /api/tickets
  ├─ Event ada?                       → 404 "Event tidak ditemukan"
  ├─ Status == "Published"?           → tolak "Event ini tidak menerima pembelian tiket"
  ├─ Tipe tiket milik event?          → tolak "Tipe tiket tidak ditemukan"
  ├─ Stok > 0?                        → tolak "Tiket sudah habis (sold out)"
  ├─ Nama lengkap ≥ 3 karakter        → tolak
  ├─ Telepon valid (≥ 9 digit)        → tolak
  ├─ Email valid                      → tolak
  ├─ Tanggal lahir diisi + umur ≥ 17  → tolak "belum memenuhi syarat umur minimal 17 tahun"
  └─ ✅ Simpan tiket (QRCode = GUID unik) + stok -1
```

### 3.7 Bootstrap Skema DB Lama (tanpa migrasi)

`TicketSchemaBootstrap.Ensure()` dijalankan setiap API start, **idempoten**:
- Tambah kolom `TicketLink` ke `Events`, kolom `BuyerName/BuyerPhone/BuyerEmail/BuyerDateOfBirth` ke `Tickets` (hanya bila belum ada).
- Buat tabel `EventTicketTypes` bila belum ada + seed 9 tipe tiket bila tabel kosong.
- Set link pembelian eksternal demo event 3 (`loket.com`) bila kosong.
- Seed akun EO (`eo@gigradar.com` / `eo123`) bila belum ada + serahkan event 1 ke EO (hanya bila masih milik Admin seed).

### 3.8 Seed Data (otomatis saat DB dibuat)

- **2 User**: Admin (`admin@gigradar.com`/`admin123`, Role Admin) & **EO** (`eo@gigradar.com`/`eo123`, Role EO, Jakarta)
- **12 Genre**: Indie, Alternative, Rock, Punk, Hardcore, Shoegaze, Emo, Metal, Jazz, Folk, Electronic, Pop (dengan emoji icon)
- **3 Venue**: Graha Bhakti Budaya (Jakarta), Gedung Kesenian (Bandung), Bentara Budaya (Jakarta)
- **6 Artist**: Hollow Men, Concrete Beach, Static Bloom, Pale Circles, Soft Collapse, Meridian
- **3 Event**: Night of Shoegaze (5 Sep 2026, **milik EO**), Midwest Emo Fest (12 Sep 2026, Admin), Underground Indie Night (19 Sep 2026, Admin + **link eksternal loket.com**)
- **9 EventTicketTypes**: 3 per event (Festival / Tribun / Bundling) dengan harga & stok berbeda
- **6 EventArtists** (line-up antar artist & event)

### 3.9 Rekomendasi (rule-based)

Skor event untuk user (di `EventService.GetRecommendedEventsAsync`):

```text
Skor = Genre cocok (0.4)
     + Lokasi < 20 km (0.3, selain itu 0.1)
     + Popularitas ViewsCount/100 × 0.1
     + Kedekatan tanggal < 7 hari (0.2, selain itu 0.1)
```

Urutkan menurun, hanya event berstatus `Published`.

---

## 4. Mobile App — `GigRadarMobile`

### 4.1 Teknologi & Paket

- .NET MAUI `.NET 10` — target: `net10.0-android`, `net10.0-ios`, `net10.0-maccatalyst`, `net10.0-windows10.0.19041.0`
- `CommunityToolkit.Mvvm` 8.4.0 — pola MVVM (`[ObservableProperty]`, `[RelayCommand]`)
- Shell Navigation, XAML source generator (`MauiXamlInflator=SourceGen`)
- Windows: unpackaged (`WindowsPackageType=None`) + self-contained WinAppSDK
- HTTP via `HttpClient` + `System.Text.Json`

### 4.2 Struktur Folder

```text
GigRadarMobile/
├── App.xaml(.cs)          # pilih halaman awal: AppShell (sudah login+onboarding) atau LoginPage
├── AppShell.xaml          # TabBar 4 tab: Discover | Map | Tickets | Profile
├── MauiProgram.cs         # DI: services, 15 ViewModels, 14 Pages
├── Models/                # Artist, CreateEventData, EoDashboard, Event (GigEvent), EventTicketType, Genre, Ticket, User, Venue
├── Views/                 # 14 halaman (lihat tabel di bawah)
├── ViewModels/            # 15 file (14 ViewModel + ManagedEventItem)
├── Services/
│   ├── ApiConfiguration.cs# Base URL per platform (Android: 10.0.2.2, lainnya: localhost)
│   ├── ApiService.cs      # wrapper semua panggilan REST ke backend
│   └── AuthService.cs     # simpan session via Preferences (token, user, onboarding_done)
└── Helpers/
    ├── Constants.cs       # BaseApiUrl + StorageKeys
    ├── Alerts.cs          # helper alert + konfirmasi (ConfirmAsync)
    ├── NavigationHelper.cs
    ├── TicketBarcodeDrawable.cs        # gambar barcode visual (GraphicsView, tanpa library)
    ├── BarcodeDrawableConverter.cs     # converter XAML utk barcode
    └── InvertedBoolConverter.cs / IsZeroConverter.cs / StringNotEmptyConverter.cs
```

### 4.3 Alur Aplikasi

```text
Buka App
 ├─ Sudah login + onboarding → AppShell (4 tab)
 └─ Belum → LoginPage (Login / Register, toggle)
       ↓
       OnboardingPage — pilih genre favorit (dari GET /api/genres)
       ↓
       AppShell (Discover | Map | Tickets | Profile)
```

### 4.4 Halaman (14)

| Halaman | Isi / Fitur |
|---|---|
| **LoginPage** | Login/Register dalam satu halaman (toggle), dark theme |
| **OnboardingPage** | Grid genre + emoji, multi-pilih; simpan preferensi & tandai onboarding selesai |
| **HomePage** ("Discover") | Pull-to-refresh; seksi: 🎯 Recommended, 🌙 Tonight, 📅 This Weekend, 📍 Nearby; **hanya menampilkan event Published/SoldOut** (Draft & Completed disembunyikan), badge "Tiket Habis" untuk SoldOut; card tap → detail event |
| **EventDetailPage** | Info event, seksi LINEUP (artis + tombol ▶ preview), tombol ❤️ Save, tombol **🎫 Buy Ticket** (atau "Tiket Habis"/"Event Selesai" + badge status bila SoldOut/Completed) |
| **ArtistDetailPage** | Nama, genre, bio, daftar TRACKS + tombol ▶ preview + status playback |
| **MapPage** | Daftar "NEARBY GIGS"; tombol 🗺 Map membuka **Google Maps eksternal** via Launcher |
| **TicketPage** ("My Tickets") | Daftar tiket user: event, tanggal, tipe, harga, **barcode visual per tiket** |
| **TicketSelectionPage** | Tahap 1 pembelian: pilih tipe tiket (Festival/Tribun/Bundling) dengan harga & sisa stok; **SOLD OUT** untuk stok 0 |
| **CheckoutPage** | Tahap 2 pembelian: ringkasan pesanan + form **Nama Lengkap, No. Telepon, Email, Tanggal Lahir** (prefill dari akun, bisa diedit) + tombol **Bayar Sekarang** (pembayaran simulasi) |
| **TicketSuccessPage** | Tahap 3 pembelian: tiket berhasil — nama event/gigs, tanggal, venue, tipe, harga, atas nama, **barcode visual + kode QR teks** |
| **ManageTicketsPage** | Halaman EO/Admin: daftar event (expand), kelola tipe tiket per event — tambah/edit/hapus, harga, stok, SOLD OUT |
| **EoProfilePage** | Profil EO/Admin: edit data diri (nama, kota, URL foto), 4 kartu ringkasan (Total Event, Mendatang, Tiket Terjual, Pendapatan), daftar "Event Saya" + aksi (**Tandai Habis / Aktifkan Lagi, Selesai, Hapus**), tombol Kelola Tiket & Buat Event |
| **CreateEventPage** | Form pembuatan event EO: nama, genre (picker), deskripsi, URL poster, venue (pilih / **tambah venue baru**), jadwal, harga, kapasitas, link pembelian eksternal opsional, status & koordinat |
| **ProfilePage** | Profil user biasa, edit nama & kota, tombol **"📊 Profil Event Organizer"** & **"🎟️ Kelola Tiket Event"** (hanya untuk role EO/Admin), Logout |

### 4.5 Alur Pembelian Tiket (3 Tahap — di Aplikasi)

```text
[EventDetailPage] → tekan "🎫 Buy Ticket"
        │
        ├─ Event punya TicketLink (link eksternal)?
        │     └─ Buka link pembelian di browser (mis. loket.com) — tanpa form internal
        │
        └─ Tidak → [TicketSelectionPage]  Tahap 1: Pilih Tipe Tiket
                │   Festival / Tribun / Bundling (harga, deskripsi, sisa stok, SOLD OUT)
                ↓
        [CheckoutPage]  Tahap 2: Data Diri & Pembayaran
                │   Ringkasan pesanan (event, tanggal, venue, tipe, harga)
                │   Form: Nama Lengkap · No. Telepon · Email · Tanggal Lahir (prefill akun)
                │   Verifikasi: nama ≥ 3, telepon ≥ 9 digit, email valid, umur ≥ 17 → pesan jelas bila ditolak
                │   Tombol "Bayar Sekarang" (pembayaran SIMULASI) → POST /api/tickets
                ↓
        [TicketSuccessPage]  Tahap 3: Barcode Tiket
                │   Nama festival/gigs, tanggal, venue, tipe, harga, atas nama
                │   Barcode visual + kode QR (untuk validasi petugas)
                ↓
        Otomatis tersimpan → muncul di tab My Tickets (dengan barcode per tiket)
```

Catatan: pembayaran masih **simulasi** (belum ada gateway seperti Midtrans/Xendit). Barcode yang digambar adalah **pola visual deterministik** — validasi sungguhan tetap lewat endpoint `POST /api/tickets/validate` dengan kode QR teks.

### 4.6 Catatan implementasi mobile

- **Home** memanggil `GET /api/events` lalu memfilter/memotong di sisi client; endpoint `/recommended` & `/nearby` backend ada tapi belum dipakai Home.
- **Discover** menyaring event `Draft` & `Completed`; badge "Tiket Habis" untuk `SoldOut`.
- **Map** belum memakai kontrol peta native — daftar + tautan eksternal Google Maps (default koordinat Jakarta).
- **Base URL API** (`Services/ApiConfiguration.cs`): `http://10.0.2.2:5000` di Android emulator, `http://localhost:5000` di Windows/iOS/Mac. IP perlu diganti manual untuk device fisik.
- **Audio preview** hanya tombol + status playback — belum streaming lagu sungguhan.
- **Tema**: dark `#121212`, aksen neon hijau `#39FF14`, ungu `#7B2FFF`, kartu `#1E1E1E`.

---

## 5. Fitur EO / Admin (dari Aplikasi)

| Fitur | Lokasi | Keterangan |
|---|---|---|
| Kelola tipe tiket & stok per event | Profile → 🎟️ Kelola Tiket Event | Tambah/edit/hapus tipe (nama, deskripsi, harga, stok), SortOrder otomatis, cek kepemilikan |
| Profil EO + edit data diri | Profile → 📊 Profil Event Organizer | Edit nama/kota/URL foto; ringkasan 4 kartu; daftar event + statistik per event |
| Buat event baru | Profil EO → ➕ Buat Event Baru | Form lengkap + venue picker / tambah venue baru; event otomatis milik EO yang login |
| Tandai tiket habis / aktifkan lagi | Kartu event di Profil EO | Status `SoldOut` ↔ `Published`; pembelian langsung diblokir |
| Tandai event selesai | Kartu event di Profil EO | Status `Completed`; disembunyikan dari Discover, tak menerima pembelian |
| Hapus event | Kartu event di Profil EO | Konfirmasi Ya/Batal; hapus permanen + relasinya (tiket, favorit, line-up) |

Semua aksi kelola wajib role **EO/Admin** dan **hanya event milik sendiri** (Admin bebas semua) — selain itu **403**.

---

## 6. Launcher & Script

**GigRadarLauncher** (console app, net8.0):
1. Mencari folder `GigRadarApi` (direktori aktif, parent, atau beberapa path umum).
2. Menjalankan `dotnet run --project ... --urls http://localhost:5000`.
3. Menunggu port 5000 terbuka (maks 15 detik), lalu membuka **Swagger UI** di browser.
4. Menekan tombol apa pun → server dimatikan.

Script di root:
- `StartGigRadar.bat` → menjalankan `GigRadarLauncher.exe`
- `StartMobileApp.bat` → `cd` ke `GigRadarMobile\bin\Debug\net10.0-windows10.0.19041.0\win-x64` lalu menjalankan `GigRadarMobile.exe`

---

## 7. Cara Menjalankan

```bash
# 1) Backend API saja (Swagger di http://localhost:5000/swagger)
dotnet run --project GigRadarApi --urls http://localhost:5000

# 2) Atau via Launcher (Windows)
./GigRadarLauncher.exe        # atau double-click StartGigRadar.bat

# 3) Mobile app (Windows)
dotnet build GigRadarMobile -f net10.0-windows10.0.19041.0
# lalu jalankan exe di bin/Debug/net10.0-windows10.0.19041.0/win-x64 (atau StartMobileApp.bat)

# Build cepat
dotnet build GigRadarApi/GigRadarApi.csproj
dotnet build GigRadarMobile/GigRadarMobile.csproj -f net10.0-windows10.0.19041.0
```

**Akun seed:**

| Role | Email | Password |
|---|---|---|
| Admin | `admin@gigradar.com` | `admin123` |
| EO | `eo@gigradar.com` | `eo123` |
| User | daftar baru lewat halaman Register | — |

---

## 8. Status Implementasi

✅ **Sudah ada:**
- Auth register/login (JWT + BCrypt), onboarding genre, list & detail event, artist + audio preview UI, nearby/tonight/weekend, rekomendasi rule-based
- **Alur beli tiket 3 tahap** (pilih tipe → data diri & bayar → barcode) + simpan ke My Tickets
- **Tipe tiket per event** (Festival/Tribun/Bundling) dengan stok & harga; **link pembelian eksternal** (TicketLink)
- **Verifikasi pembeli**: data diri lengkap + umur minimal 17 + cek stok (stok berkurang otomatis)
- **CRUD event lengkap** (buat, update, hapus) — tersimpan ke DB dengan `CreatedBy` dari JWT
- **Status event**: Published / Draft / SoldOut / Completed + blokir pembelian saat bukan Published
- **Dashboard & profil EO/Admin**: edit data diri, ringkasan statistik, kelola tipe tiket, buat event, tandai habis/selesai, hapus event
- **Role-based authorization + cek kepemilikan** (EO hanya event sendiri, 403 untuk non-pemilik / role User)
- **Bootstrap skema idempoten** untuk DB SQLite lama (tanpa migrasi & tanpa kehilangan data)
- Tema dark, favorit, profil & edit, validasi QR tiket

🔶 **Sebagian / perlu disempurnakan:**
- Pembayaran masih **simulasi** (belum ada payment gateway)
- Barcode tiket berupa **pola visual** (bukan QR image standar yang bisa discan langsung); validasi lewat kode QR teks
- Peta interaktif (Google Maps SDK) → masih daftar + link eksternal
- Home tidak memakai endpoint rekomendasi/nearby server (filter di client)
- Audio preview belum memutar lagu sungguhan
- Register menerima role bebas dari client (risiko keamanan bila produksi)
- Edit data event dari aplikasi (backend PUT sudah ada, form edit di UI belum)

❌ **Belum ada (rencana fase lanjut di dokumen desain):** payment gateway (Midtrans/Xendit), push notification (FCM), komunitas/follow venue-EO, crowdfunding, analytics, rekomendasi Machine Learning, audio streaming, search & filter, multi-city, edit line-up artis dari UI.

---

## 9. Statistik Singkat

- Proyek: **3** (.NET API, .NET MAUI, Console launcher)
- File kode sumber C#: **±81** (API 20 · Mobile 60 · Launcher 1) · File XAML: **20**
- Controller API: **7** · Endpoint: **32**
- Tabel database: **12** entity set
- Halaman mobile: **14** · ViewModel: **14** (+ 1 model pendukung)
- Database: SQLite (`GigRadar.db`) — otomatis dibuat + seed + bootstrap skema

---

*Rekapan terbaru dibuat dari penelusuran kode terkini. Untuk detail konsep & roadmap, lihat `GIGRADAR_MOBILE_APP_NET_MAUI.md`; untuk panduan multi-platform, lihat `GIGRADAR_MULTIPLATFORM_FIX.md`.*