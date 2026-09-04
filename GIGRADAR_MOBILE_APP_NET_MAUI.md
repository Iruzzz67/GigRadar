# GigRadar Mobile App
## Menghubungkan Skena, Menemukan Suara Lokal

## 1. Deskripsi Aplikasi

GigRadar adalah aplikasi mobile untuk menemukan gigs musik lokal, underground, showcase, konser kecil, dan acara komunitas berdasarkan lokasi serta preferensi musik pengguna.

GigRadar menggabungkan:
- Location Based Service (LBS)
- sistem rekomendasi berbasis preferensi dan Machine Learning
- digital ticketing
- audio preview
- komunitas musik lokal
- crowdfunding/support artist

Target utama:
- penikmat musik lokal dan niche
- musisi/band independen
- kolektif musik
- venue
- event organizer lokal

---

# 2. Platform dan Teknologi

## 2.1 Frontend Mobile

Framework utama:

**.NET MAUI (C#)**

Target:
- Android
- iOS
- Windows (opsional untuk testing/admin ringan)

Teknologi .NET:
- .NET 10 atau versi LTS/stabil yang digunakan saat implementasi
- C#
- XAML
- MVVM
- CommunityToolkit.Mvvm
- Shell Navigation
- HttpClient
- System.Text.Json

## 2.2 Backend

**ASP.NET Core Web API**

Tanggung jawab:
- authentication
- user management
- event management
- artist management
- recommendation API
- ticketing
- payment integration
- notification
- analytics

## 2.3 Database

**PostgreSQL**

ORM:
- Entity Framework Core

## 2.4 Integrasi Eksternal

- Google Maps Platform untuk peta dan lokasi
- Firebase Cloud Messaging untuk push notification
- Spotify Web API atau layanan musik lain jika API dan izin pengguna memungkinkan
- payment gateway Indonesia untuk pembayaran tiket
- object storage untuk poster, foto artis, dan aset event

---

# 3. Arsitektur Sistem

```text
.NET MAUI Mobile App
        |
        | HTTPS / REST API
        v
ASP.NET Core Web API
        |
        +---- Authentication
        +---- User Service
        +---- Event Service
        +---- Artist Service
        +---- Recommendation Service
        +---- Ticket Service
        +---- Payment Service
        +---- Notification Service
        |
        v
PostgreSQL Database

External Services:
- Google Maps
- Firebase Cloud Messaging
- Music API
- Payment Gateway
- Object Storage
```

Prinsip arsitektur:
- Mobile app tidak mengakses database secara langsung.
- Semua data utama melewati ASP.NET Core Web API.
- API menggunakan HTTPS.
- API key dan secret tidak ditanam langsung di aplikasi mobile.
- Recommendation Engine dapat dikembangkan terlebih dahulu dengan rule-based scoring lalu ditingkatkan menjadi Machine Learning.

---

# 4. Struktur Project .NET MAUI

```text
GigRadar/
│
├── GigRadar.csproj
├── MauiProgram.cs
├── App.xaml
├── AppShell.xaml
│
├── Models/
│   ├── User.cs
│   ├── Event.cs
│   ├── Artist.cs
│   ├── Venue.cs
│   ├── Ticket.cs
│   ├── Genre.cs
│   └── Recommendation.cs
│
├── ViewModels/
│   ├── LoginViewModel.cs
│   ├── RegisterViewModel.cs
│   ├── HomeViewModel.cs
│   ├── EventDetailViewModel.cs
│   ├── MapViewModel.cs
│   ├── TicketViewModel.cs
│   ├── ProfileViewModel.cs
│   └── RecommendationViewModel.cs
│
├── Views/
│   ├── LoginPage.xaml
│   ├── RegisterPage.xaml
│   ├── OnboardingPage.xaml
│   ├── HomePage.xaml
│   ├── EventDetailPage.xaml
│   ├── MapPage.xaml
│   ├── ArtistPage.xaml
│   ├── TicketPage.xaml
│   ├── ProfilePage.xaml
│   └── SettingsPage.xaml
│
├── Services/
│   ├── ApiService.cs
│   ├── AuthService.cs
│   ├── EventService.cs
│   ├── ArtistService.cs
│   ├── RecommendationService.cs
│   ├── LocationService.cs
│   ├── TicketService.cs
│   └── NotificationService.cs
│
├── Helpers/
│   ├── Constants.cs
│   └── PreferencesHelper.cs
│
└── Resources/
    ├── Images/
    ├── Fonts/
    ├── Styles/
    └── Raw/
```

---

# 5. Fitur Utama

## 5.1 Authentication

Fitur:
- register
- login
- logout
- forgot password
- Google login
- Apple login jika diperlukan
- session/token management

Role:
- User
- Artist
- EO
- Admin

---

# 6. Onboarding dan Music Taste

Saat pertama kali menggunakan aplikasi:

```text
Welcome
   ↓
Pilih Kota/Lokasi
   ↓
Pilih Genre Favorit
   ↓
Pilih Artist Favorit
   ↓
Izinkan Lokasi
   ↓
Home
```

Genre contoh:
- Indie
- Alternative
- Rock
- Punk
- Hardcore
- Shoegaze
- Emo
- Metal
- Jazz
- Folk
- Electronic
- Pop

Data onboarding digunakan sebagai initial preference profile.

---

# 7. Home Page

Home menjadi pusat rekomendasi.

Bagian:

### Recommended For You
Gigs yang paling sesuai dengan preferensi.

### Nearby Gigs
Acara berdasarkan jarak.

### Tonight
Gigs yang berlangsung hari ini.

### This Weekend
Acara akhir pekan.

### Trending
Acara yang sedang banyak dilihat/disimpan.

### New Discovery
Artist dan gigs baru.

---

# 8. AI Recommendation Engine

Sistem rekomendasi tidak harus langsung menggunakan model Machine Learning kompleks.

## Tahap 1: Rule-Based Recommendation

Gunakan scoring:

```text
Recommendation Score =
(Genre Match × 40%)
+
(Location Match × 30%)
+
(Artist Similarity × 20%)
+
(Event Popularity × 10%)
```

Contoh:

User:
- Genre: Shoegaze, Indie
- Lokasi: Bogor

Event:
- Genre: Shoegaze
- Lokasi: Bogor
- Artist: sesuai preferensi

Maka event mendapatkan skor tinggi.

## Tahap 2: Personalized Recommendation

Data tambahan:
- event yang dibuka
- event yang disimpan
- event yang dibeli
- artist yang diikuti
- genre yang sering dilihat
- jarak yang biasanya dipilih pengguna

## Tahap 3: Machine Learning

Jika data pengguna sudah cukup banyak, backend dapat menggunakan model recommendation untuk memprediksi kemungkinan pengguna tertarik terhadap event tertentu.

Model tidak perlu berjalan di perangkat mobile. Model dapat berjalan di backend/recommendation service.

---

# 9. Interactive Gigs Map

Menggunakan Google Maps Platform.

Fitur:
- lokasi pengguna
- marker gigs
- detail event melalui marker
- radius pencarian
- filter genre
- filter tanggal
- filter harga
- filter jarak

Contoh:

```text
[ Map ]

   📍 Gig A
           📍 Gig B

      📍 Gig C

----------------------
Genre: Indie
Jarak: < 10 km
Tanggal: Weekend
```

---

# 10. Event Detail

Informasi:

- poster
- nama event
- tanggal
- waktu
- venue
- alamat
- jarak dari pengguna
- line-up
- genre
- harga tiket
- kapasitas
- deskripsi
- preview artist
- tombol simpan
- tombol beli tiket
- tombol share

Flow:

```text
Event
 ↓
Detail
 ↓
Pilih Tiket
 ↓
Checkout
 ↓
Pembayaran
 ↓
QR Ticket
```

---

# 11. Artist Profile

Profil artist berisi:

- nama
- foto
- genre
- bio
- lagu
- upcoming gigs
- event sebelumnya
- social links
- tombol Follow

---

# 12. Audio Preview

Pengguna dapat mendengarkan preview musik artist.

Fitur:
- preview lagu
- play/pause
- progress
- artist information
- link ke platform musik

Durasi preview mengikuti aturan dan kemampuan API musik yang digunakan.

---

# 13. Digital Ticketing

Fitur:
- daftar tiket
- checkout
- pembayaran
- e-ticket
- QR code
- riwayat pembelian
- status tiket

QR ticket digunakan sebagai bukti masuk dan dapat diverifikasi oleh EO.

---

# 14. Crowdfunding / Support Artist

Pengguna dapat mendukung artist atau proyek musik.

Fitur:
- nominal dukungan
- target crowdfunding
- progress
- daftar proyek
- riwayat dukungan

Semua transaksi harus diproses melalui payment gateway yang sesuai dan memenuhi ketentuan layanan.

---

# 15. Community

Fitur komunitas:

- follow artist
- follow venue
- follow EO
- save event
- komentar
- share event
- activity feed

Moderasi:
- report content
- block user
- admin moderation

---

# 16. Notification

Push notification menggunakan Firebase Cloud Messaging.

Contoh:

```text
Band favoritmu akan tampil
di dekat lokasimu akhir pekan ini.
```

Notifikasi lain:
- event baru
- tiket hampir habis
- perubahan jadwal
- event reminder
- pembayaran berhasil
- crowdfunding update

Pengguna dapat mengatur preferensi notifikasi.

---

# 17. Location Based Service

Aplikasi dapat meminta lokasi pengguna untuk:

- menemukan gigs terdekat
- menghitung jarak
- memberikan rekomendasi lokal
- menampilkan map

Prinsip privacy:
- minta izin lokasi secara eksplisit
- sediakan opsi memasukkan kota secara manual
- jangan menyimpan lokasi presisi jika tidak diperlukan
- gunakan data lokasi sesuai tujuan fitur
- sediakan pengaturan privasi

---

# 18. Search dan Filter

Search berdasarkan:

- nama event
- artist
- venue
- genre
- kota

Filter:
- tanggal
- jarak
- harga
- genre
- venue
- artist

---

# 19. Sistem Event untuk EO

EO dapat:

### Membuat Event

- nama event
- poster
- deskripsi
- tanggal
- waktu
- venue
- lokasi
- line-up
- genre
- harga tiket
- kapasitas

### Mengelola Event

- edit
- publish
- unpublish
- lihat penjualan
- lihat jumlah pengunjung

---

# 20. Dashboard Artist / EO

Statistik:

- jumlah views
- saves
- followers
- ticket sales
- conversion
- popular event
- audience location
- genre interest

Data analitik harus diberikan dalam bentuk agregat/anonim dan mengikuti ketentuan privasi yang berlaku.

---

# 21. Admin Panel

Admin mengelola:

- users
- artists
- EO
- venues
- events
- reports
- transactions
- genres
- featured events

Admin dapat:
- approve event
- reject event
- suspend account
- menghapus konten yang melanggar aturan

---

# 22. Model Database

## Users

```text
UserId
Name
Email
PasswordHash
Role
City
Latitude
Longitude
CreatedAt
```

## Artists

```text
ArtistId
Name
Bio
PhotoUrl
GenreId
CreatedAt
```

## Events

```text
EventId
Name
Description
PosterUrl
VenueId
StartDate
EndDate
Latitude
Longitude
GenreId
CreatedBy
CreatedAt
Status
```

## Venues

```text
VenueId
Name
Address
City
Latitude
Longitude
Capacity
```

## Tickets

```text
TicketId
EventId
UserId
TicketType
Price
QRCode
Status
PurchasedAt
```

## Genres

```text
GenreId
Name
```

## UserPreferences

```text
PreferenceId
UserId
GenreId
Weight
```

## EventArtists

```text
EventId
ArtistId
Order
```

## Favorites

```text
FavoriteId
UserId
EventId
CreatedAt
```

---

# 23. API Endpoint

Base:

```text
/api
```

## Authentication

```http
POST /api/auth/register
POST /api/auth/login
POST /api/auth/refresh
POST /api/auth/logout
```

## Events

```http
GET    /api/events
GET    /api/events/{id}
POST   /api/events
PUT    /api/events/{id}
DELETE /api/events/{id}
```

## Recommendation

```http
GET /api/recommendations
GET /api/recommendations/nearby
```

## Artists

```http
GET /api/artists
GET /api/artists/{id}
POST /api/artists/{id}/follow
```

## Tickets

```http
POST /api/tickets
GET  /api/tickets
GET  /api/tickets/{id}
```

## Map

```http
GET /api/events/nearby?lat={lat}&lng={lng}&radius={radius}
```

---

# 24. MVVM di .NET MAUI

Pola utama:

```text
View
 ↓
ViewModel
 ↓
Service
 ↓
ASP.NET Core API
 ↓
Database
```

Contoh:

```text
HomePage.xaml
       ↓
HomeViewModel.cs
       ↓
EventService.cs
       ↓
ApiService.cs
       ↓
GET /api/recommendations
       ↓
ASP.NET Core
       ↓
PostgreSQL
```

View tidak boleh berisi business logic utama.

Business logic ditempatkan di:
- ViewModel
- Service
- Backend

---

# 25. Navigation

Gunakan .NET MAUI Shell.

Struktur:

```text
Home
├── Recommended Events
├── Nearby
└── Trending

Explore
├── Search
├── Genres
└── Map

Tickets
├── Upcoming
└── History

Profile
├── Favorite Artists
├── Saved Events
└── Settings
```

---

# 26. UI/UX Design

Tema utama:

- Dark Mode
- typography tegas
- aksen neon
- card event
- poster besar
- grain/texture secara ringan
- visual gigs lokal
- navigasi sederhana

Bottom navigation:

```text
┌─────────────────────────────────┐
│                                 │
│             CONTENT             │
│                                 │
├─────────────────────────────────┤
│ Home │ Explore │ Map │ Tickets │ Profile │
└─────────────────────────────────┘
```

---

# 27. Keamanan

Backend wajib menangani:

- JWT authentication
- password hashing
- authorization berdasarkan role
- HTTPS
- input validation
- rate limiting
- secure API keys
- payment verification
- QR ticket validation
- audit logging

Jangan menyimpan:
- password asli
- secret API key di source code mobile
- data pembayaran sensitif yang tidak diperlukan

---

# 28. Model Bisnis

## Komisi Ticketing

Contoh:

```text
2-5% dari transaksi
```

## Promoted Gigs

EO dapat membayar untuk:
- featured event
- posisi rekomendasi tertentu
- campaign promosi

Konten berbayar harus diberi penanda yang jelas.

## Insight untuk EO

Data agregat dapat digunakan untuk:
- genre populer
- kota dengan permintaan tinggi
- waktu event yang populer
- tren pencarian

---

# 29. Roadmap Pengembangan

## Phase 1 - MVP

- .NET MAUI
- Login
- Onboarding
- Home
- Event
- Search
- Map
- Artist
- Ticket
- Basic recommendation

## Phase 2

- Payment gateway
- Push notification
- Favorite
- Follow artist
- Community
- EO dashboard

## Phase 3

- Personalized recommendation
- Audio preview
- Crowdfunding
- Analytics

## Phase 4

- Machine Learning recommendation
- Multi-city expansion
- Advanced personalization

---

# 30. Urutan Implementasi yang Disarankan

```text
1. Setup .NET MAUI
        ↓
2. Buat UI/Navigation
        ↓
3. Buat ASP.NET Core API
        ↓
4. Setup PostgreSQL + EF Core
        ↓
5. Authentication
        ↓
6. Event & Artist CRUD
        ↓
7. Home + Search
        ↓
8. Google Maps
        ↓
9. Recommendation Engine
        ↓
10. Ticketing
        ↓
11. Payment
        ↓
12. Notification
        ↓
13. Community
        ↓
14. EO/Admin
        ↓
15. Testing
        ↓
16. Release
```

---

# 31. MVP Prioritas

Untuk project pertama, jangan langsung membuat seluruh fitur sekaligus.

Prioritas:

### Wajib

- Login/Register
- Onboarding genre
- Home
- Event list
- Event detail
- Search
- Map
- Basic recommendation
- Artist profile
- Favorite
- Ticket

### Setelah MVP

- Payment
- Notification
- Community
- EO dashboard
- Crowdfunding

### Tahap lanjutan

- Machine Learning
- Advanced analytics
- Multi-city recommendation

---

# 32. Kesimpulan

GigRadar akan dibangun sebagai aplikasi mobile menggunakan **.NET MAUI dan C#**, dengan **ASP.NET Core Web API** sebagai backend dan **PostgreSQL** sebagai database.

Arsitektur ini memungkinkan satu codebase mobile untuk mengembangkan aplikasi lintas platform, sementara seluruh data, authentication, ticketing, recommendation, dan business logic utama tetap dikontrol oleh backend.

Konsep utama GigRadar:

> **Temukan gigs. Temukan musik baru. Temukan skena di sekitarmu.**

GigRadar bukan hanya platform pembelian tiket, tetapi ekosistem digital yang menghubungkan penonton, artist, venue, komunitas, dan event organizer lokal.
