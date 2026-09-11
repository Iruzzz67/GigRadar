# 🎸 GigRadar — Menghubungkan Skena, Menemukan Suara Lokal

> **GigRadar** adalah aplikasi modern untuk menemukan gigs musik lokal, konser indie, underground showcases, komunitas lokal, dan festival musik berdasarkan preferensi genre dan lokasi pengguna. Dilengkapi sistem tiket berjenjang, dashboard Event Organizer (EO) & Admin, serta visualisasi radar interaktif.

---

## 📌 Ringkasan Proyek

GigRadar terdiri dari **3 komponen utama** yang terintegrasi secara harmonis:

| Komponen | Deskripsi | Teknologi / Framework | Target Platform |
| :--- | :--- | :--- | :--- |
| **`GigRadarApi`** | Backend REST API — autentikasi, event, tiket, transaksi, data master. | ASP.NET Core Web API (.NET 8) | `net8.0` (Cross-platform) |
| **`GigRadarMobile`** | Aplikasi mobile & desktop lintas platform dengan desain dark theme modern. | .NET MAUI (.NET 10) + MVVM | Android, iOS, macOS, Windows |
| **`GigRadarLauncher`** | Konsol launcher otomatis untuk mempercepat pengembangan lokal. | .NET Console (.NET 8) | Windows |

> **⚠️ Catatan Deviasi:** Dokumen desain menyebut PostgreSQL, namun implementasi backend saat ini menggunakan **SQLite** (`GigRadarApi/GigRadar.db`).

---

## ⚙️ Arsitektur Sistem

Aplikasi menggunakan arsitektur **Client-Server** murni — Mobile berkomunikasi dengan Backend API melalui RESTful API berkeamanan tinggi dengan JWT Bearer.

```text
GigRadarMobile (.NET MAUI, dark theme)
        |
        |  HTTPS/HTTP REST + JWT Bearer
        v
GigRadarApi (ASP.NET Core Web API)
        |
        +-- Auth (register/login, BCrypt, JWT)
        +-- Events (CRUD, nearby, tonight, weekend, recommended, status, managed/summary)
        +-- Venues (daftar + tambah venue)
        +-- Artists (list, detail + audio tracks)
        +-- Tickets (tipe tiket, beli dengan verifikasi data diri, riwayat, validasi QR)
        +-- Users (profil, preferensi genre, favorit)
        |
        v
SQLite (EF Core, file GigRadar.db)

GigRadarLauncher (console) — menyalakan API di http://localhost:5000 dan membuka Swagger
```

### Karakteristik Teknis:
- **Database:** SQLite (`GigRadar.db`) dengan Entity Framework Core. Skema dibuat otomatis (`EnsureCreated`) + seed data. Skema baru ditambahkan **idempoten** via `TicketSchemaBootstrap` tanpa menghapus data.
- **Otentikasi:** JWT Bearer dengan password di-hash **BCrypt**.
- **Role-based Authorization:** `[Authorize(Roles = "EO,Admin")]` + cek kepemilikan event (`CreatedBy`).
- **CORS:** Dibuka penuh (`AllowAll`) untuk development.

---

## 📁 Struktur Direktori Proyek

```text
GigRadar/
├── GigRadarApi/                     # Backend REST API
│   ├── Program.cs                   # Entrypoint, DI, JWT, Swagger, CORS, auto-create DB
│   ├── appsettings.json             # Koneksi SQLite + JwtSettings
│   ├── Controllers/                 # 7 Controller (Auth, Events, Artists, Genres, Tickets, Users, Venues)
│   ├── Data/
│   │   ├── AppDbContext.cs          # 12 DbSet + constraint unik + seed data
│   │   └── TicketSchemaBootstrap.cs # Upgrade skema DB lama secara idempoten
│   ├── Models/                      # User, Genre, Artist, Venue, Event, Ticket, dll.
│   ├── Services/                    # AuthService, EventService, TicketService
│   └── Helpers/Constants.cs
│
├── GigRadarMobile/                  # Aplikasi Mobile & Desktop (.NET MAUI)
│   ├── App.xaml / AppShell.xaml     # Shell navigasi multi-tab (Discover, Explore, Tickets, Profile)
│   ├── MauiProgram.cs               # DI: services, 14 ViewModels, 14 Pages
│   ├── Models/                      # Artist, Event, Genre, Ticket, User, Venue, dll.
│   ├── Views/                       # 14 Halaman XAML
│   ├── ViewModels/                  # 14 ViewModel (CommunityToolkit.Mvvm)
│   ├── Services/
│   │   ├── ApiConfiguration.cs      # Base URL per platform (Android: 10.0.2.2, lainnya: localhost)
│   │   ├── ApiService.cs            # Wrapper REST ke backend
│   │   └── AuthService.cs           # Session via Preferences
│   ├── Helpers/
│   │   ├── Alerts.cs                # Helper alert + konfirmasi
│   │   ├── TicketBarcodeDrawable.cs # Gambar barcode visual (GraphicsView)
│   │   ├── RadarDrawable.cs         # Visualisasi radar interaktif
│   │   ├── RadarState.cs            # State filter lintas-tab
│   │   ├── GeoHelper.cs             # Haversine, bearing, format jarak
│   │   ├── GenreColors.cs           # Map genre → warna
│   │   ├── ReducedMotion.cs         # Deteksi reduced motion per platform
│   │   └── Converter.cs             # Berbagai XAML converter
│   └── Platforms/                   # Platform-specific (Android, iOS, Windows, Mac)
│
├── GigRadarLauncher/                # Launcher utilitas pengembang
│   └── Program.cs                   # Otomasi menjalankan API + membuka Swagger UI
│
├── StartGigRadar.bat                # Shortcut Windows → Launcher (API + Swagger)
├── StartMobileApp.bat               # Shortcut Windows → Mobile App (Windows Native)
├── GEMINI.md                        # Workspace rules untuk AI agents
├── GIGRADAR_DESIGN_SYSTEM.md        # Design system: token warna, tipografi, komponen
├── GIGRADAR_MOBILE_APP_NET_MAUI.md  # Visi produk & roadmap lengkap
├── GIGRADAR_MULTIPLATFORM_FIX.md    # Panduan multi-platform .NET MAUI
├── GIGRADAR_ROLE_SYSTEM.md          # Detail sistem multi-role & otorisasi
├── REKAPAN_PROJECT_TERBARU.md       # Rekap komprehensif isi kode terkini
└── REKAPAN_PROJECT.md               # Rekap versi sebelumnya
```

---

## 🌟 Fitur Utama & Alur Sistem

### 1. Sistem Multi-Role Akun

GigRadar mengimplementasikan **4 peran pengguna** dengan tingkat akses berbeda:

| Role | Deskripsi | Hak Akses Utama |
| :--- | :--- | :--- |
| **`User`** | Penonton / pembeli tiket | Jelajahi gigs, radar, beli tiket, favorit, profil |
| **`EO`** (Event Organizer) | Pengelola event | Buat/edit/hapus event milik sendiri, kelola tiket, statistik |
| **`Admin`** | Administrator platform | Akses penuh ke seluruh event, user, tipe tiket, moderasi |
| **`Artist`** | Musisi / band | Profil artist, jadwal gig, konten (fase lanjutan) |

### 2. Alur Pembelian Tiket Internal (3 Tahap)

```text
[EventDetailPage] → tekan "🎫 Buy Ticket"
        │
        ├─ Event punya TicketLink (link eksternal)?
        │     └─ Buka link pembelian di browser (mis. loket.com)
        │
        └─ Tidak → [TicketSelectionPage]  Tahap 1: Pilih Tipe Tiket
                │   Festival / Tribun / Bundling (harga, deskripsi, sisa stok, SOLD OUT)
                ↓
        [CheckoutPage]  Tahap 2: Data Diri & Pembayaran
                │   Form: Nama Lengkap · No. Telepon · Email · Tanggal Lahir
                │   Verifikasi: nama ≥ 3, telepon ≥ 9 digit, email valid, umur ≥ 17
                │   Tombol "Bayar Sekarang" (pembayaran simulasi) → POST /api/tickets
                ↓
        [TicketSuccessPage]  Tahap 3: Barcode Tiket
                │   Nama event, tanggal, venue, tipe, harga, atas nama
                │   Barcode visual + kode QR (untuk validasi petugas)
                ↓
        Otomatis tersimpan → muncul di tab My Tickets
```

### 3. Dashboard & Manajemen EO (Event Organizer)

- **Statistik Panel:** 4 kartu interaktif — Total Event, Event Mendatang, Tiket Terjual, Total Pendapatan.
- **Manajemen Tiket:** Tambah, edit, dan hapus tipe tiket langsung dari aplikasi mobile.
- **Kontrol Status Event:** Published (aktif), Draft (konsep), SoldOut (habis), Completed (selesai).
- **Buat Event Baru:** Form lengkap + venue picker / tambah venue baru.
- **Hapus Event:** Konfirmasi Ya/Batal; hapus permanen + relasinya.

### 4. Radar Visual & Eksplorasi

- **RadarPage:** Visualisasi node event menggunakan `GraphicsView` native — filter genre, radius, "malam ini".
- **ExplorePage:** Search & filter event/artis/venue (as-you-type), switch tampilan List / Grid / Radar.
- **Fallback:** Radar selalu punya daftar "Gig terdekat" sebagai akses utama.

---

## 🗄️ Database Schema (12 Tabel)

| Tabel | Field Penting |
| :--- | :--- |
| **Users** | UserId, Name, Email (unik), PasswordHash, Role, City, Lat/Lng, PhotoUrl |
| **UserPreferences** | UserId, GenreId, Weight |
| **Genres** | GenreId, Name, Icon (emoji) |
| **Artists** | ArtistId, Name, Bio, Genre, PhotoUrl, SocialLinks |
| **AudioTracks** | TrackId, ArtistId, Title, AudioUrl, DurationSeconds |
| **Venues** | VenueId, Name, Address, City, Lat/Lng, Capacity, PhotoUrl |
| **Events** | EventId, Name, Description, PosterUrl, TicketLink, VenueId, StartDate, EndDate, Lat/Lng, GenreId, CreatedBy, Status, MinPrice, MaxPrice, Capacity, ViewsCount, SavesCount |
| **EventArtists** | EventId + ArtistId (unik) + Order |
| **EventTicketTypes** | EventTicketTypeId, EventId, Name, Description, Price, Stock, SortOrder |
| **Tickets** | TicketId, EventId, UserId, TicketType, Price, BuyerName, BuyerPhone, BuyerEmail, BuyerDateOfBirth, QRCode, Status, PurchasedAt |
| **Favorites** | UserId + EventId (unik) |
| **Follows** | UserId → ArtistId/VenueId/EventOrganizerId (unik) |

---

## 🔌 API Endpoints (32 Endpoint)

### Authentication
| Method | Route | Auth | Keterangan |
| :--- | :--- | :--- | :--- |
| `POST` | `/api/auth/register` | — | Daftar (BCrypt + token) |
| `POST` | `/api/auth/login` | — | Login (BCrypt verify + JWT) |

### Events
| Method | Route | Auth | Keterangan |
| :--- | :--- | :--- | :--- |
| `GET` | `/api/events` | — | Semua event (include venue, genre, line-up) |
| `GET` | `/api/events/{id}` | — | Detail event |
| `GET` | `/api/events/nearby?lat=&lng=&radius=` | — | Event dalam radius (Haversine) |
| `GET` | `/api/events/tonight` | — | Event hari ini |
| `GET` | `/api/events/weekend` | — | Event sampai akhir pekan |
| `GET` | `/api/events/recommended` | 🔒 | Rekomendasi rule-based |
| `GET` | `/api/events/managed` | 🔒 EO/Admin | Event milik EO / semua (Admin) |
| `GET` | `/api/events/managed/summary` | 🔒 EO/Admin | Statistik dashboard |
| `POST` | `/api/events` | 🔒 EO/Admin | Buat event baru |
| `PUT` | `/api/events/{id}` | 🔒 EO/Admin | Update event (cek kepemilikan) |
| `PUT` | `/api/events/{id}/status` | 🔒 EO/Admin | Ubah status event |
| `DELETE` | `/api/events/{id}` | 🔒 EO/Admin | Hapus event + relasinya |

### Venues
| Method | Route | Auth | Keterangan |
| :--- | :--- | :--- | :--- |
| `GET` | `/api/venues` | — | Daftar venue |
| `POST` | `/api/venues` | 🔒 EO/Admin | Tambah venue baru |

### Artists
| Method | Route | Auth | Keterangan |
| :--- | :--- | :--- | :--- |
| `GET` | `/api/artists` | — | Semua artist + tracks |
| `GET` | `/api/artists/{id}` | — | Detail artist + tracks + line-up |

### Genres
| Method | Route | Auth | Keterangan |
| :--- | :--- | :--- | :--- |
| `GET` | `/api/genres` | — | Semua genre |

### Tickets
| Method | Route | Auth | Keterangan |
| :--- | :--- | :--- | :--- |
| `GET` | `/api/tickets` | 🔒 | Tiket milik user login |
| `GET` | `/api/tickets/{id}` | 🔒 | Detail tiket |
| `GET` | `/api/tickets/event/{eventId}/types` | 🔒 | Daftar tipe tiket event |
| `POST` | `/api/tickets` | 🔒 | Beli tiket (validasi + stok -1) |
| `POST` | `/api/tickets/validate` | 🔒 EO/Admin | Validasi QR → status Used |
| `POST` | `/api/tickets/event/{eventId}/types` | 🔒 EO/Admin | Tambah tipe tiket |
| `PUT` | `/api/tickets/types/{typeId}` | 🔒 EO/Admin | Edit tipe tiket |
| `DELETE` | `/api/tickets/types/{typeId}` | 🔒 EO/Admin | Hapus tipe tiket |

### Users
| Method | Route | Auth | Keterangan |
| :--- | :--- | :--- | :--- |
| `GET` | `/api/users/me` | 🔒 | Profil + preferensi genre |
| `PUT` | `/api/users/me` | 🔒 | Update nama/kota/photo URL |
| `POST` | `/api/users/preferences` | 🔒 | Simpan preferensi genre |
| `POST` | `/api/users/favorites/{eventId}` | 🔒 | Toggle save/un-save event |
| `GET` | `/api/users/favorites` | 🔒 | Daftar favorit |

---

## 📱 Halaman Mobile (14 Halaman)

| Tab | Halaman | Fitur Utama |
| :--- | :--- | :--- |
| **Auth** | LoginPage | Login/Register dalam satu halaman (toggle) |
| **Auth** | OnboardingPage | Pilih genre favorit (grid + emoji) |
| **Discover** | HomePage | Recommended, Tonight, Weekend, Nearby — pull-to-refresh |
| **Discover** | EventDetailPage | Info event, lineup, preview audio, ❤️ Save, 🎫 Buy Ticket |
| **Discover** | ArtistDetailPage | Nama, genre, bio, tracks + preview audio |
| **Explore** | ExplorePage | Search & filter + switch List/Grid/Radar |
| **Radar** | RadarPage | Visualisasi node event, filter, preview, daftar terdekat |
| **Tickets** | TicketPage | Daftar tiket user + barcode visual |
| **Tickets** | TicketSelectionPage | Pilih tipe tiket (harga, stok, SOLD OUT) |
| **Tickets** | CheckoutPage | Form data diri + verifikasi + bayar |
| **Tickets** | TicketSuccessPage | Tiket berhasil — barcode + QR code |
| **Profile** | ProfilePage | Profil user, edit, aksi EO/Admin (jika role sesuai) |
| **Profile** | EoProfilePage | Dashboard EO/Admin — statistik, kelola event |
| **Profile** | CreateEventPage | Form buat event + tambah venue |
| **Profile** | ManageTicketsPage | Kelola tipe tiket per event |

---

## 🛠️ Cara Menjalankan Proyek

### Prasyarat:
- [.NET SDK 8](https://dotnet.microsoft.com/download/dotnet/8.0) (untuk Backend & Launcher)
- [.NET SDK 10](https://dotnet.microsoft.com/download/dotnet) dengan workload **MAUI** terpasang (untuk Mobile App)
- IDE seperti Visual Studio 2022 (dengan beban kerja .NET MAUI) atau VS Code dengan extension C# Dev Kit.

### Langkah-langkah:

#### A. Cara Cepat (Khusus Windows)
1. Klik dua kali `StartGigRadar.bat` → menjalankan backend API di `http://localhost:5000` + membuka **Swagger UI**.
2. Klik dua kali `StartMobileApp.bat` → menjalankan aplikasi mobile dalam versi Windows Desktop.

#### B. Cara Manual via CLI

**1. Jalankan Backend API:**
```bash
dotnet run --project GigRadarApi --urls http://localhost:5000
```
*Swagger UI tersedia di: `http://localhost:5000/swagger`*

**2. Jalankan Aplikasi Mobile (.NET MAUI):**
```bash
# Windows Desktop
dotnet build GigRadarMobile -f net10.0-windows10.0.19041.0
# Jalankan exe di bin/Debug/net10.0-windows10.0.19041.0/win-x64/

# Android (pastikan emulator/device terdeteksi)
adb devices
dotnet build GigRadarMobile -f net10.0-android

# iOS / Mac Catalyst (membutuhkan macOS)
dotnet build GigRadarMobile -f net10.0-ios
dotnet build GigRadarMobile -f net10.0-maccatalyst
```

**3. Build cepat tanpa run:**
```bash
dotnet build GigRadarApi/GigRadarApi.csproj
dotnet build GigRadarMobile/GigRadarMobile.csproj -f net10.0-windows10.0.19041.0
```

---

## 🔑 Akun Uji Coba (Seed Data)

| Role | Email | Password | Kegunaan |
| :--- | :--- | :--- | :--- |
| **Admin** | `admin@gigradar.com` | `admin123` | Akses penuh — seluruh event, user, tipe tiket, moderasi |
| **EO** | `eo@gigradar.com` | `eo123` | Kelola event milik sendiri, analitik, tipe tiket |
| **User** | *Register langsung dari aplikasi* | — | Beli tiket, jelajah radar, manajemen profil |

> **Seed data juga termasuk:** 12 genre, 3 venue, 6 artist, 3 event (dengan 9 tipe tiket & 6 line-up).

---

## 🎨 Spesifikasi Desain & Visual

Aplikasi GigRadar mengusung identitas visual indie yang kuat:

### Warna
| Token | Nilai | Kegunaan |
| :--- | :--- | :--- |
| `BackgroundColor` | `#0A0A0B` | Latar aplikasi (Dark Zinc) |
| `SurfaceColor` | `#131316` | Kartu, seksi |
| `CardColor` | `#18181B` | Kartu di atas surface |
| `PrimaryColor` | `#A3FF12` | Aksen "signal lime" — CTA, angka kunci |
| `DangerColor` | `#F43F5E` | Error, tiket habis |
| `WarningColor` | `#F59E0B` | Peringatan, stok menipis |
| `SuccessColor` | `#22C55E` | Sukses, tiket aktif |

### Tipografi
- **Display/Heading:** Space Grotesk (geometris, tegas, khas produk musik)
- **Body:** Inter (nyaman dibaca)
- **Eyebrow:** Uppercase + letter-spacing (gaya editorial)

### Prinsip Desain
1. Tipografi adalah identitas
2. Imagery editorial — poster event & foto artist dipakai besar
3. Hierarki, bukan dekorasi
4. Kontras kuat — dark zinc + aksen signal lime strategis
5. Motion punya fungsi — tidak ada floating/partikel acak

Lihat [GIGRADAR_DESIGN_SYSTEM.md](./GIGRADAR_DESIGN_SYSTEM.md) untuk detail lengkap.

---

## 📊 Statistik Proyek

| Metrik | Nilai |
| :--- | :--- |
| Komponen | 3 (API, Mobile, Launcher) |
| File C# | ±81 (API 20 · Mobile 60 · Launcher 1) |
| File XAML | 20 |
| Controller API | 7 |
| Endpoint API | 32 |
| Tabel Database | 12 entity set |
| Halaman Mobile | 14 |
| ViewModel | 14 |

---

## 📄 Dokumen Referensi

| Dokumen | Deskripsi |
| :--- | :--- |
| [REKAPAN_PROJECT_TERBARU.md](./REKAPAN_PROJECT_TERBARU.md) | Rekap komprehensif isi kode, endpoint, relasi DB, status implementasi |
| [GIGRADAR_DESIGN_SYSTEM.md](./GIGRADAR_DESIGN_SYSTEM.md) | Design system: token warna, tipografi, komponen, spesifikasi Radar |
| [GIGRADAR_MOBILE_APP_NET_MAUI.md](./GIGRADAR_MOBILE_APP_NET_MAUI.md) | Visi produk, fungsionalitas, roadmap fitur |
| [GIGRADAR_MULTIPLATFORM_FIX.md](./GIGRADAR_MULTIPLATFORM_FIX.md) | Panduan arsitektur multi-platform .NET MAUI |
| [GIGRADAR_ROLE_SYSTEM.md](./GIGRADAR_ROLE_SYSTEM.md) | Detail sistem multi-role & otorisasi |
| [GEMINI.md](./GEMINI.md) | Workspace rules untuk AI agents |

---

## 🚧 Status Implementasi

### ✅ Sudah Ada
- Auth register/login (JWT + BCrypt), onboarding genre
- List & detail event, artist + audio preview UI
- Nearby/tonight/weekend, rekomendasi rule-based
- **Alur beli tiket 3 tahap** — pilih tipe → data diri & bayar → barcode
- **Tipe tiket per event** dengan stok & harga
- **CRUD event lengkap** — buat, update, hapus (tersimpan ke DB)
- **Status event:** Published / Draft / SoldOut / Completed
- **Dashboard EO/Admin** — statistik, kelola tiket, buat event
- **Role-based authorization + cek kepemilikan**
- Bootstrap skema idempoten untuk DB SQLite lama
- Tema dark, favorit, profil & edit, validasi QR tiket

### 🔶 Perlu Disempurnakan
- Pembayaran masih **simulasi** (belum ada payment gateway)
- Barcode tiket berupa **pola visual** (belum QR image standar)
- Peta interaktif → masih daftar + link eksternal Google Maps
- Home belum memakai endpoint rekomendasi/nearby server
- Audio preview belum memutar lagu sungguhan
- Register menerima role bebas dari client (risiko keamanan)

### ❌ Belum Ada (Fase Lanjut)
- Payment gateway (Midtrans/Xendit)
- Push notification (FCM)
- Komunitas / follow venue-EO
- Crowdfunding / support artist
- Analytics & Machine Learning rekomendasi
- Audio streaming sungguhan
- Search & filter lanjutan
- Multi-city expansion

---

🎸 *Selamat berkarya! Mari kita hidupkan kembali skena musik lokal bersama GigRadar.*
