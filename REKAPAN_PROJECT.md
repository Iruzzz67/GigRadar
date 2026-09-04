# 🎸 Rekapan Project — GigRadar

> **Menghubungkan skena, menemukan suara lokal.**
> Rekapan kondisi project per **September 2026** — berdasarkan isi kode di repository.

---

## 1. Ringkasan

GigRadar adalah aplikasi untuk menemukan gigs musik lokal, underground, showcase, konser kecil, dan acara komunitas berdasarkan lokasi serta preferensi musik pengguna.

Project terdiri dari **3 komponen utama** dalam satu direktori:

| Komponen | Jenis | Framework / Teknologi | Target |
|---|---|---|---|
| `GigRadarApi` | Backend REST API | ASP.NET Core Web API (.NET 8) | `net8.0` |
| `GigRadarMobile` | Aplikasi mobile/desktop | .NET MAUI (.NET 10) + MVVM | Android, iOS, Mac Catalyst, Windows |
| `GigRadarLauncher` | Launcher console | .NET Console (.NET 8) | Windows — menjalankan API + membuka Swagger |

Ditambah file pendukung di root:
- `GigRadarLauncher.exe` — hasil publish launcher
- `StartGigRadar.bat` — shortcut menjalankan launcher (API + Swagger)
- `StartMobileApp.bat` — shortcut menjalankan build Windows app
- `GIGRADAR_MOBILE_APP_NET_MAUI.md` — dokumen desain/visi produk lengkap

**Catatan deviasi penting:** dokumen desain menyebut PostgreSQL, namun implementasi backend saat ini menggunakan **SQLite** (file `GigRadarApi/GigRadar.db`). Logo di halaman Profile app masih menulis "ASP.NET Core + PostgreSQL".

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
        +-- Events (list, detail, nearby, tonight, weekend, recommended)
        +-- Artists (list, detail + audio tracks)
        +-- Tickets (beli, riwayat, validasi QR)
        +-- Users (profil, preferensi genre, favorit)
        |
        v
SQLite (EF Core, file GigRadar.db)

GigRadarLauncher (console) — menyalakan API di http://localhost:5000 dan membuka Swagger
```

Prinsip yang dipakai:
- Mobile app **tidak** mengakses database langsung — semua lewat REST API.
- Autentikasi memakai **JWT Bearer**; password di-hash **BCrypt**.
- Database dibuat otomatis (`EnsureCreated`) + **seed data** saat API pertama dijalankan.
- CORS dibuka penuh (`AllowAll`) — nyaman untuk development.

---

## 3. Backend — `GigRadarApi`

### 3.1 Teknologi & Paket

- ASP.NET Core Web API — `net8.0`, Swagger UI (`/swagger`)
- `Microsoft.EntityFrameworkCore.Sqlite` 8.0.8 (+ Design)
- `Microsoft.AspNetCore.Authentication.JwtBearer` 8.0.8
- `BCrypt.Net-Next` 4.0.3
- `Swashbuckle.AspNetCore` 6.4.0

### 3.2 Struktur Folder

```text
GigRadarApi/
├── Program.cs            # Setup DI, JWT, Swagger, CORS, auto-create DB
├── appsettings.json      # koneksi SQLite + JwtSettings (SecretKey, Issuer, Audience, ExpirationMinutes=1440)
├── Data/
│   └── AppDbContext.cs   # 11 DbSet + constraint unik + seed data
├── Models/               # User, Genre, Artist, Venue, Event, Ticket (+ kelas turunan)
├── Controllers/          # Auth, Events, Artists, Genres, Tickets, Users
├── Services/             # AuthService, EventService, TicketService
└── Helpers/
    └── Constants.cs
```

### 3.3 Model Database (tabel)

| Tabel | Field penting |
|---|---|
| **Users** | UserId, Name, Email (unik), PasswordHash, Role (`User`/`EO`/`Admin`/`Artist`), City, Latitude, Longitude, PhotoUrl |
| **UserPreferences** | UserId, GenreId, Weight — preferensi genre hasil onboarding |
| **Genres** | GenreId, Name, Icon (emoji) |
| **Artists** | ArtistId, Name, Bio, Genre, PhotoUrl, SocialLinks |
| **AudioTracks** | TrackId, ArtistId, Title, AudioUrl, DurationSeconds (default 30 — audio preview) |
| **Venues** | VenueId, Name, Address, City, Lat/Lng, Capacity, PhotoUrl |
| **Events** | EventId, Name, Description, PosterUrl, VenueId, StartDate, EndDate, Lat/Lng, GenreId, CreatedBy, Status (`Published`), MinPrice, MaxPrice, Capacity, ViewsCount, SavesCount |
| **EventArtists** | EventId + ArtistId (unik) + Order — line-up event |
| **Tickets** | TicketId, EventId, UserId, TicketType, Price, QRCode, Status (`Active`/`Used`), PurchasedAt |
| **Favorites** | UserId + EventId (unik) — save event |
| **Follows** | UserId → ArtistId/VenueId/EventOrganizerId (unik per user+artist) |

### 3.4 Endpoint API

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
| POST | `/api/events` | 🔒 EO/Admin | ⚠️ masih **stub** — belum disimpan ke DB |
| PUT | `/api/events/{id}` | 🔒 | ⚠️ masih **stub** — belum disimpan ke DB |
| DELETE | `/api/events/{id}` | 🔒 | ⚠️ masih **stub** |
| GET | `/api/artists` | — | Semua artist + tracks |
| GET | `/api/artists/{id}` | — | Detail artist + tracks + event line-up |
| GET | `/api/genres` | — | Semua genre |
| GET | `/api/tickets` | 🔒 | Tiket milik user login |
| GET | `/api/tickets/{id}` | 🔒 | Detail tiket |
| POST | `/api/tickets` | 🔒 | Beli tiket (generate QRCode unik) |
| POST | `/api/tickets/validate` | 🔒 EO/Admin | Validasi QR → status jadi `Used` |
| GET | `/api/users/me` | 🔒 | Profil + preferensi genre |
| PUT | `/api/users/me` | 🔒 | Update nama/kota/photo |
| POST | `/api/users/preferences` | 🔒 | Simpan (ganti total) preferensi genre |
| POST | `/api/users/favorites/{eventId}` | 🔒 | Toggle save/un-save event |
| GET | `/api/users/favorites` | 🔒 | Daftar favorit |

### 3.5 Auth & Role

- Token JWT berisi claim `UserId`, `Name`, `Email`, `Role`; kedaluwarsa 1440 menit (1 hari).
- Role yang dikenal: `User` (default), `EO`, `Admin`, `Artist`.
- `register` menerima field role dari client (tanpa batasan) — perlu perhatian keamanan bila dipakai produksi.
- Contoh akun seed: **admin@gigradar.com / admin123** (Role Admin).

### 3.6 Rekomendasi (rule-based)

Skor event untuk user (di `EventService.GetRecommendedEventsAsync`):

```text
Skor = Genre cocok (0.4)
     + Lokasi < 20 km (0.3, selain itu 0.1)
     + Popularitas ViewsCount/100 × 0.1
     + Kedekatan tanggal < 7 hari (0.2, selain itu 0.1)
```

Urutkan menurun, hanya event berstatus `Published`. Sesuai tahap 1 dari strategi rekomendasi di dokumen desain.

### 3.7 Seed Data (otomatis saat DB dibuat)

- **1 User**: Admin (Jakarta)
- **12 Genre**: Indie, Alternative, Rock, Punk, Hardcore, Shoegaze, Emo, Metal, Jazz, Folk, Electronic, Pop (dengan emoji icon)
- **3 Venue**: Graha Bhakti Budaya (Jakarta), Gedung Kesenian (Bandung), Bentara Budaya (Jakarta)
- **6 Artist**: Hollow Men, Concrete Beach, Static Bloom, Pale Circles, Soft Collapse, Meridian (skena shoegaze/emo/indie)
- **3 Event**: Night of Shoegaze (5 Sep 2026), Midwest Emo Fest (12 Sep 2026), Underground Indie Night (19 Sep 2026) — lengkap dengan venue, genre, harga, kapasitas
- **6 EventArtists** (line-up antar artist & event)

---

## 4. Mobile App — `GigRadarMobile`

### 4.1 Teknologi & Paket

- .NET MAUI `.NET 10` — target: `net10.0-android`, `net10.0-ios`, `net10.0-maccatalyst`, `net10.0-windows10.0.19041.0` (Windows hanya dibuild di OS Windows)
- `CommunityToolkit.Mvvm` 8.4.0 — pola MVVM (`[ObservableProperty]`, `[RelayCommand]`)
- Shell Navigation, XAML source generator (`MauiXamlInflator=SourceGen`)
- Windows: unpackaged (`WindowsPackageType=None`) + self-contained WinAppSDK
- HTTP via `HttpClient` + `System.Text.Json`

### 4.2 Struktur Folder

```text
GigRadarMobile/
├── App.xaml(.cs)          # pilih halaman awal: AppShell (sudah login+onboarding) atau LoginPage
├── AppShell.xaml          # TabBar 4 tab: Discover | Map | Tickets | Profile
├── MauiProgram.cs         # DI: services, 8 ViewModels, 8 Pages
├── Models/                # mirror model API: Artist, Event, Genre, Ticket, User, Venue
├── Views/                 # 8 halaman (lihat tabel di bawah)
├── ViewModels/            # 8 ViewModel sesuai halaman
├── Services/
│   ├── ApiService.cs      # wrapper semua panggilan REST ke backend
│   └── AuthService.cs     # simpan session via Preferences (token, user id/name/email/role, onboarding_done)
└── Helpers/
    └── Constants.cs       # BaseApiUrl + StorageKeys
```

### 4.3 Alur & Halaman

```text
Buka App
 ├─ Sudah login + onboarding → AppShell (4 tab)
 └─ Belum → LoginPage
       ├─ mode Login (email + password)
       └─ mode Register (nama + email + password)  ← toggle di halaman yang sama
       ↓
       OnboardingPage — pilih genre favorit (dari GET /api/genres)
       ↓
       AppShell
```

| Halaman | Isi / Fitur |
|---|---|
| **LoginPage** | Login/Register dalam satu halaman (toggle), dark theme |
| **OnboardingPage** | Grid genre + emoji, multi-pilih; simpan preferensi ke `POST /api/users/preferences` & tandai onboarding selesai |
| **HomePage** ("Discover") | Pull-to-refresh; seksi: 🎯 Recommended For You, 🌙 Tonight, 📅 This Weekend, 📍 Nearby Gigs; card tap → detail event |
| **EventDetailPage** | Info tanggal/waktu/venue/harga/deskripsi, seksi LINEUP (artis + tombol ▶ audio preview 30 detik), tombol ❤️ Save (toggle favorit) & 🎫 Buy Ticket |
| **ArtistDetailPage** | Nama, genre, bio, daftar TRACKS + tombol ▶ preview + status playback |
| **MapPage** | Daftar "NEARBY GIGS" (bukan peta interaktif); tombol 🗺 Map membuka **Google Maps eksternal** via Launcher, tombol Detail |
| **TicketPage** | Daftar "MY TICKETS" (event, tanggal, tipe, harga, kode QR teks) |
| **ProfilePage** | Profil user, edit nama & kota, tombol Logout, footer brand v1.0 |

### 4.4 Catatan implementasi mobile

- **Home** memanggil `GET /api/events` lalu memfilter/memotong **di sisi client** (Recommended = 5 teratas, Nearby = 3 teratas, Tonight/Weekend difilter lokal). Endpoint `/recommended` & `/nearby` di backend ada, tapi belum dipakai Home.
- **Map** belum memakai kontrol peta native — memakai daftar + tautan eksternal Google Maps (default koordinat Jakarta jika geolokasi gagal).
- **QR ticket** masih berupa teks kode (belum gambar QR).
- **Base URL API** (`Helpers/Constants.cs`): `http://10.0.2.2:5000` di emulator Android, `http://localhost:5000` di Windows/iOS simulator. IP perlu diganti manual untuk device fisik.
- **Audio preview** hanya tombol + status playback — belum streaming URL lagu sungguhan.
- **Tema**: dark `#121212`, aksen neon hijau `#39FF14`, ungu `#7B2FFF`, kartu `#1E1E1E`.

---

## 5. Launcher & Script

**GigRadarLauncher** (console app, net8.0):
1. Mencari folder `GigRadarApi` (direktori aktif, parent, atau beberapa path umum).
2. Menjalankan `dotnet run --project ... --urls http://localhost:5000`.
3. Menunggu port 5000 terbuka (maks 15 detik), lalu membuka **Swagger UI** di browser.
4. Menekan tombol apa pun → server dimatikan.

Script di root:
- `StartGigRadar.bat` → menjalankan `GigRadarLauncher.exe`
- `StartMobileApp.bat` → `cd` ke `GigRadarMobile\bin\Debug\net10.0-windows10.0.19041.0\win-x64` lalu menjalankan `GigRadarMobile.exe`

---

## 6. Cara Menjalankan

```bash
# 1) Backend API saja (Swagger di http://localhost:5000/swagger)
dotnet run --project GigRadarApi --urls http://localhost:5000

# 2) Atau via Launcher (Windows)
./GigRadarLauncher.exe        # atau double-click StartGigRadar.bat

# 3) Mobile app (Windows)
dotnet build GigRadarMobile -f net10.0-windows10.0.19041.0
# lalu jalankan exe di bin/Debug/net10.0-windows10.0.19041.0/win-x64 (atau StartMobileApp.bat)
```

Akun seed: `admin@gigradar.com` / `admin123`.

---

## 7. Status Implementasi vs Dokumen Desain

✅ **Sudah ada (dasar MVP):** auth register/login (JWT+BCrypt), onboarding genre, list & detail event, artist + audio preview UI, nearby/tonight/weekend, rekomendasi rule-based (endpoint), beli tiket + kode QR + validasi, favorit, profil & edit, tema dark.

🔶 **Sebagian / perlu disempurnakan:**
- CRUD event backend masih *stub* (belum menyimpan ke DB)
- Peta interaktif (Google Maps SDK) → masih daftar + link eksternal
- Home tidak memakai endpoint rekomendasi/nearby server
- QR belum berupa gambar; audio preview belum memutar lagu
- Register menerima role bebas dari client

❌ **Belum ada (rencana fase lanjut di dokumen desain):** payment gateway, push notification (FCM), komunitas/follow venue-EO, dashboard EO/Admin, crowdfunding, analytics, rekomendasi Machine Learning, audio sungguhan, search & filter, multi-city.

---

## 8. Statistik Singkat

- Proyek: **3** (.NET API, .NET MAUI, Console launcher)
- File kode sumber C#: **±55** · File XAML: **14**
- Controller API: **6** · Endpoint: **±27**
- Tabel database: **11** entity set
- Database: SQLite (`GigRadar.db`) — otomatis dibuat + seed

---

*Rekapan ini dibuat otomatis dari penelusuran kode. Untuk detail konsep & roadmap lengkap, lihat `GIGRADAR_MOBILE_APP_NET_MAUI.md`.*
