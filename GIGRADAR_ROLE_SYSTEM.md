# GigRadar Role System

## 1. Tujuan Sistem

GigRadar adalah aplikasi mobile untuk menemukan dan mengikuti
perkembangan gigs musik lokal, underground, showcase, konser, festival,
serta aktivitas musisi atau band.

Sistem menggunakan satu aplikasi dengan beberapa role utama, tetapi
setiap role memiliki pengalaman pengguna, dashboard, navigasi, dan hak
akses yang berbeda.

Role utama:

-   **User**: pendengar musik dan pembeli tiket.
-   **Artist**: musisi atau band yang mengelola profil, musik, konten,
    dan jadwal gigs.
-   **EO**: event organizer yang membuat dan mengelola event.
-   **Admin**: administrator platform yang mengawasi seluruh sistem.

Prinsip utama:

> Satu aplikasi, satu sistem autentikasi, tetapi UI, navigasi, dan hak
> akses menyesuaikan role pengguna.

------------------------------------------------------------------------

# 2. Arsitektur Multi-Role

Project GigRadar menggunakan struktur:

``` text
GigRadar
├── GigRadarApi
│   └── ASP.NET Core Web API
│
├── GigRadarMobile
│   └── .NET MAUI
│
└── GigRadarLauncher
    └── .NET Console
```

Teknologi utama:

-   Backend: ASP.NET Core Web API .NET 8
-   Mobile: .NET MAUI .NET 10
-   Architecture: MVVM
-   Authentication: JWT Bearer
-   Database: SQLite
-   API communication: REST API
-   Platform mobile: Android, iOS, Mac Catalyst, Windows

Alur utama:

``` text
.NET MAUI
    │
    │ HTTPS REST API
    ▼
ASP.NET Core Web API
    │
    │ Entity Framework Core
    ▼
SQLite Database
```

Authentication:

``` text
Login/Register
      │
      ▼
JWT Token
      │
      ├── UserId
      ├── Name
      ├── Email
      └── Role
```

------------------------------------------------------------------------

# 3. Role System

## 3.1 User

User adalah pengguna umum yang menggunakan GigRadar untuk:

-   menemukan gigs;
-   mencari konser;
-   mencari festival;
-   menemukan artis atau band;
-   mengikuti artis;
-   melihat perkembangan artis;
-   membeli tiket;
-   melihat tiket yang dimiliki;
-   mengatur preferensi musik;
-   mengatur lokasi;
-   menyimpan event favorit.

Role:

``` text
User
```

------------------------------------------------------------------------

## 3.2 Artist

Artist adalah musisi, solo artist, DJ, band, atau grup musik.

Artist dapat:

-   mengatur profil artist;
-   menambahkan genre;
-   mengunggah lagu;
-   mengelola album;
-   membuat post;
-   mengunggah poster rilisan;
-   membagikan aktivitas band;
-   mengelola anggota band;
-   menampilkan gigs;
-   mengelola perjalanan perkembangan artist;
-   melihat follower;
-   membangun audience.

Role:

``` text
Artist
```

------------------------------------------------------------------------

## 3.3 EO

EO atau Event Organizer bertanggung jawab terhadap event.

EO dapat:

-   membuat event;
-   mengedit event miliknya;
-   menghapus event miliknya;
-   mengatur lineup;
-   mengatur venue;
-   membuat tipe tiket;
-   mengatur harga tiket;
-   mengatur status event;
-   melihat penjualan tiket;
-   melihat statistik event miliknya;
-   melihat event publik milik EO lain.

EO tidak boleh mengedit atau menghapus event milik EO lain.

Role:

``` text
EO
```

------------------------------------------------------------------------

## 3.4 Admin

Admin merupakan pengelola platform.

Admin dapat:

-   melihat seluruh user;
-   melihat seluruh artist;
-   melihat seluruh EO;
-   melihat seluruh event;
-   mengelola event;
-   melakukan moderasi;
-   melihat tiket;
-   melihat statistik platform;
-   mengatur sistem.

Role:

``` text
Admin
```

------------------------------------------------------------------------

# 4. User Dashboard

User Dashboard menjadi pusat discovery.

Menu utama:

``` text
Discover
Map
Tickets
Profile
```

Isi dashboard:

-   event berdasarkan lokasi;
-   event terdekat;
-   event malam ini;
-   event akhir pekan;
-   event yang direkomendasikan;
-   artist berdasarkan genre;
-   artist yang diikuti;
-   event favorit;
-   event populer.

Contoh struktur:

``` text
HOME
│
├── Search
├── Location
├── Recommended Gigs
├── Nearby Gigs
├── Tonight
├── This Weekend
├── Popular Artists
└── Followed Artists
```

------------------------------------------------------------------------

# 5. User Profile

User dapat mengatur:

-   foto profil;
-   nama;
-   email;
-   lokasi;
-   genre favorit;
-   artist yang diikuti;
-   event favorit;
-   tiket.

Menu:

``` text
Profile
├── Edit Profile
├── Music Preferences
├── Favorite Events
├── Followed Artists
├── My Tickets
└── Logout
```

------------------------------------------------------------------------

# 6. Artist

Artist memiliki halaman publik yang lebih menyerupai profil media sosial
khusus musik.

Struktur:

``` text
Artist Profile
│
├── Cover / Photo
├── Artist Name
├── Genre
├── Bio
├── Followers
├── Music
├── Albums
├── Posts
├── Gigs
└── Journey
```

Artist dapat memperlihatkan perkembangan mereka kepada pengguna.

Contoh:

``` text
2024
│
├── Membentuk band
│
2025
│
├── Rilis single pertama
├── Showcase pertama
│
2026
│
├── Rilis album
├── Festival
└── Tour
```

------------------------------------------------------------------------

# 7. Artist Dashboard

Artist Dashboard menjadi pusat pengelolaan karier dan konten.

Navigasi:

``` text
Dashboard
Music
Posts
Gigs
Profile
```

Dashboard dapat menampilkan:

-   jumlah follower;
-   jumlah lagu;
-   jumlah album;
-   jumlah post;
-   gigs mendatang;
-   performa konten;
-   event yang diikuti;
-   perkembangan artist.

Contoh:

``` text
ARTIST DASHBOARD

Followers       1,250
Songs              12
Albums              2
Upcoming Gigs       4

Upcoming:
- Local Underground Fest
- Jakarta Music Night
- Bandung Showcase
```

------------------------------------------------------------------------

# 8. Artist Profile

Artist dapat mengubah:

-   nama artist;
-   foto;
-   cover;
-   bio;
-   genre;
-   social media;
-   anggota band;
-   informasi kontak;
-   lokasi;
-   link musik.

Profil publik dapat dilihat oleh User dan EO.

------------------------------------------------------------------------

# 9. Artist Music Management

Artist dapat mengelola musik.

Fitur:

-   upload lagu;
-   edit lagu;
-   hapus lagu;
-   mengatur judul;
-   mengatur artwork;
-   mengatur genre;
-   mengatur durasi;
-   mengatur tanggal rilis.

Struktur:

``` text
Music
├── Tracks
├── Albums
└── Releases
```

Track:

``` text
Track
├── Title
├── AudioUrl
├── CoverUrl
├── Duration
├── Genre
└── ReleaseDate
```

Catatan: project saat ini masih memiliki keterbatasan bahwa audio
preview belum menjadi streaming audio penuh.

------------------------------------------------------------------------

# 10. Artist Post System

Artist dapat membuat post seperti media sosial.

Jenis konten:

-   update band;
-   aktivitas latihan;
-   cerita perjalanan;
-   pengumuman gigs;
-   poster konser;
-   poster album;
-   poster single;
-   foto kegiatan;
-   informasi merchandise.

Contoh:

``` text
POST

[Poster Single Baru]

"Single terbaru kami akan segera rilis."

Like
Comment
Share
Follow
```

Artist dapat:

-   create post;
-   edit post;
-   delete post;
-   upload image;
-   melihat engagement.

------------------------------------------------------------------------

# 11. Artist Journey

Artist Journey digunakan untuk mendokumentasikan perkembangan artist.

Contoh:

``` text
Artist Journey

2024
Band terbentuk.

2025
Single pertama dirilis.

2025
Showcase pertama.

2026
Album pertama.

2026
Festival pertama.
```

Data dapat terdiri dari:

-   tahun/tanggal;
-   judul;
-   deskripsi;
-   gambar;
-   kategori.

Kategori:

``` text
Formation
Release
Gig
Achievement
Album
Single
Other
```

------------------------------------------------------------------------

# 12. User ↔ Artist

User dapat mengikuti artist.

Flow:

``` text
User
 │
 ▼
Artist Profile
 │
 ▼
Follow
 │
 ▼
Artist Updates
 │
 ├── New Song
 ├── New Album
 ├── New Post
 ├── New Gig
 └── Artist Journey
```

Data follower perlu disimpan agar sistem dapat membangun feed
berdasarkan artist yang diikuti.

------------------------------------------------------------------------

# 13. Artist Recommendation

Sistem rekomendasi dapat menggunakan data:

-   genre favorit user;
-   genre artist;
-   lokasi;
-   popularitas;
-   kedekatan tanggal event.

Bobot rekomendasi yang sudah ditentukan dalam project:

``` text
Genre        40%
Location     30%
Popularity   10%
Date         20%
```

Formula konsep:

``` text
Recommendation Score =
    Genre Match * 0.4
  + Location Match * 0.3
  + Popularity * 0.1
  + Date Proximity * 0.2
```

Sistem dapat dikembangkan menjadi machine learning pada tahap
berikutnya.

------------------------------------------------------------------------

# 14. EO

EO merupakan pengelola event.

EO dapat melihat:

-   event miliknya;
-   event publik EO lain;
-   event yang sedang berlangsung;
-   event yang akan datang;
-   statistik penjualan event miliknya.

EO tidak dapat mengubah event yang bukan miliknya.

------------------------------------------------------------------------

# 15. EO Dashboard

Navigasi:

``` text
Dashboard
Events
Tickets
Analytics
Profile
```

Dashboard:

``` text
EVENT ORGANIZER DASHBOARD

My Events       12
Published        8
Upcoming         4
Tickets Sold   840

Upcoming Events
- Local Fest
- Underground Night
- Music Festival
```

------------------------------------------------------------------------

# 16. EO Event Management

EO dapat melakukan CRUD terhadap event miliknya:

``` text
Create
Read
Update
Delete
```

Validasi ownership:

``` text
Event.CreatedBy == CurrentUser.UserId
```

Jika benar:

``` text
Allow Edit
Allow Delete
Allow Manage
```

Jika salah:

``` text
Deny Edit
Deny Delete
Deny Manage
```

Admin memiliki akses penuh.

------------------------------------------------------------------------

# 17. Create Event

Form Create Event:

``` text
Event Name
Description
Poster
Date
Start Time
End Time
Venue
City
Genre
Artists
Ticket Types
External Ticket Link
Status
```

Status:

``` text
Draft
Published
SoldOut
Completed
```

Event baru sebaiknya dibuat sebagai:

``` text
Draft
```

Kemudian EO dapat melakukan publish.

------------------------------------------------------------------------

# 18. Event Lineup

EO dapat menentukan artist yang tampil.

Contoh:

``` text
Event
│
├── Artist A
├── Artist B
├── Artist C
└── Artist D
```

Relasi:

``` text
Events
   │
   └── EventArtists
           │
           └── Artists
```

Artist yang masuk lineup dapat melihat event tersebut di halaman gigs
mereka.

------------------------------------------------------------------------

# 19. Event Ticket Management

EO dapat membuat beberapa tipe tiket.

Contoh:

``` text
Presale
Rp75.000
100 Tickets

Regular
Rp100.000
500 Tickets

VIP
Rp200.000
100 Tickets
```

Data ticket type:

``` text
TicketType
├── Name
├── Price
├── Quantity
├── Sold
├── SaleStart
└── SaleEnd
```

------------------------------------------------------------------------

# 20. EO Monitoring

EO dapat melihat event publik yang dibuat EO lain.

Contoh:

``` text
All Public Events

[Event A]
Organizer: EO A
Status: Published

[Event B]
Organizer: EO B
Status: Published

[Event C]
Organizer: EO C
Status: Published
```

EO hanya dapat mengelola:

``` text
Own Events
```

Bukan:

``` text
Other EO Events
```

------------------------------------------------------------------------

# 21. EO Melihat Event EO Lain

Aturan:

``` text
EO A
  │
  ├── Event A1 → OWN → Manage
  ├── Event A2 → OWN → Manage
  │
  ├── Event B1 → PUBLIC → View Only
  └── Event C1 → PUBLIC → View Only
```

Ini penting agar EO dapat memantau ekosistem event tanpa mendapatkan
akses administratif terhadap event milik orang lain.

------------------------------------------------------------------------

# 22. Event Ownership

Setiap event wajib memiliki:

``` text
CreatedBy
```

`CreatedBy` menyimpan UserId dari EO yang membuat event.

Authorization:

``` text
if Role == Admin
    allow

else if Role == EO
    allow only when Event.CreatedBy == CurrentUserId

else
    deny management
```

Ownership wajib diperiksa di backend.

Jangan hanya menyembunyikan tombol edit pada UI.

------------------------------------------------------------------------

# 23. Admin

Admin mempunyai akses platform-level.

Navigasi:

``` text
Dashboard
Users
Artists
EO
Events
Tickets
Settings
```

Admin dapat:

-   melihat user;
-   melihat artist;
-   melihat EO;
-   melihat event;
-   moderasi;
-   menghapus konten sesuai aturan sistem;
-   melihat transaksi tiket;
-   melihat statistik.

------------------------------------------------------------------------

# 24. Login Flow

Flow:

``` text
Open App
   │
   ▼
Login
   │
   ▼
API Authentication
   │
   ▼
JWT Token
   │
   ▼
Read Role
   │
   ├── User  → UserShell
   ├── Artist → ArtistShell
   ├── EO → EOShell
   └── Admin → AdminShell
```

Role tidak boleh ditentukan hanya berdasarkan pilihan UI.

Role harus berasal dari data yang telah diverifikasi server.

------------------------------------------------------------------------

# 25. Registration

Masalah keamanan pada project saat ini:

> Register masih menerima role dari client.

Contoh yang tidak aman:

``` json
{
  "email": "user@email.com",
  "password": "password",
  "role": "Admin"
}
```

Client tidak boleh dapat menentukan role privileged.

Sistem yang disarankan:

``` text
Register
   │
   └── Default Role = User
```

Untuk Artist:

``` text
User
 │
 ▼
Artist Application
 │
 ▼
Verification
 │
 ▼
Artist
```

Untuk EO:

``` text
User
 │
 ▼
EO Application
 │
 ▼
Verification
 │
 ▼
EO
```

Admin hanya dapat ditentukan oleh administrator atau proses internal
yang aman.

------------------------------------------------------------------------

# 26. Navigation Berdasarkan Role

## UserShell

``` text
UserShell
├── Discover
├── Map
├── Tickets
└── Profile
```

## ArtistShell

``` text
ArtistShell
├── Dashboard
├── Music
├── Posts
├── Gigs
└── Profile
```

## EOShell

``` text
EOShell
├── Dashboard
├── Events
├── Tickets
├── Analytics
└── Profile
```

## AdminShell

``` text
AdminShell
├── Dashboard
├── Users
├── Artists
├── EO
├── Events
├── Tickets
└── Settings
```

------------------------------------------------------------------------

# 27. Database Architecture

Database utama:

``` text
Users
UserPreferences
Genres
Artists
AudioTracks
Venues
Events
EventArtists
EventTicketTypes
Tickets
Favorites
Follows
```

Pengembangan baru:

``` text
ArtistPosts
ArtistAlbums
ArtistMembers
ArtistFollowers
ArtistGenres
ArtistJourney
PostLikes
PostComments
EventViews
EventAnalytics
```

------------------------------------------------------------------------

# 28. Tabel Baru

## ArtistPosts

``` text
Id
ArtistId
Title
Content
ImageUrl
CreatedAt
UpdatedAt
IsPublished
```

------------------------------------------------------------------------

## ArtistAlbums

``` text
Id
ArtistId
Title
CoverUrl
Description
ReleaseDate
CreatedAt
```

------------------------------------------------------------------------

## ArtistMembers

``` text
Id
ArtistId
Name
Role
PhotoUrl
JoinedAt
```

------------------------------------------------------------------------

## ArtistFollowers

``` text
Id
ArtistId
UserId
CreatedAt
```

Unique constraint:

``` text
ArtistId + UserId
```

------------------------------------------------------------------------

## ArtistGenres

Jika satu artist dapat memiliki banyak genre:

``` text
ArtistId
GenreId
```

------------------------------------------------------------------------

## ArtistJourney

``` text
Id
ArtistId
Title
Description
Date
ImageUrl
Category
```

------------------------------------------------------------------------

## PostLikes

``` text
Id
PostId
UserId
CreatedAt
```

------------------------------------------------------------------------

## PostComments

``` text
Id
PostId
UserId
Content
CreatedAt
UpdatedAt
```

------------------------------------------------------------------------

## EventViews

``` text
Id
EventId
UserId
ViewedAt
```

------------------------------------------------------------------------

## EventAnalytics

``` text
Id
EventId
Date
Views
TicketSales
Revenue
Favorites
```

------------------------------------------------------------------------

# 29. API Architecture

API tetap menggunakan ASP.NET Core Web API.

Kelompok endpoint:

``` text
/api/auth
/api/users
/api/artists
/api/artist
/api/eo
/api/events
/api/venues
/api/genres
/api/tickets
```

------------------------------------------------------------------------

# 30. API User

Endpoint yang dibutuhkan:

``` http
GET /api/users/me
PUT /api/users/me

GET /api/users/preferences
PUT /api/users/preferences

GET /api/users/favorites
POST /api/users/favorites/{eventId}
DELETE /api/users/favorites/{eventId}

GET /api/users/follows
POST /api/users/follows/{artistId}
DELETE /api/users/follows/{artistId}
```

------------------------------------------------------------------------

# 31. API Artist Public

Endpoint publik:

``` http
GET /api/artists
GET /api/artists/{id}

GET /api/artists/{id}/tracks
GET /api/artists/{id}/albums
GET /api/artists/{id}/posts
GET /api/artists/{id}/events
GET /api/artists/{id}/journey
```

Endpoint ini digunakan User dan EO untuk discovery.

------------------------------------------------------------------------

# 32. API Artist Management

Endpoint Artist:

``` http
GET /api/artist/me
PUT /api/artist/me

POST /api/artist/tracks
PUT /api/artist/tracks/{id}
DELETE /api/artist/tracks/{id}

POST /api/artist/albums
PUT /api/artist/albums/{id}
DELETE /api/artist/albums/{id}

POST /api/artist/posts
PUT /api/artist/posts/{id}
DELETE /api/artist/posts/{id}

POST /api/artist/journey
PUT /api/artist/journey/{id}
DELETE /api/artist/journey/{id}
```

Semua endpoint management Artist harus menggunakan authorization:

``` text
[Authorize(Roles = "Artist")]
```

------------------------------------------------------------------------

# 33. API EO

Endpoint EO:

``` http
GET /api/eo/me
GET /api/events/managed
GET /api/events/managed/summary

POST /api/events
PUT /api/events/{id}
PATCH /api/events/{id}/status
DELETE /api/events/{id}
```

Authorization event:

``` text
EO → own event only
Admin → all events
```

------------------------------------------------------------------------

# 34. API Event

Endpoint event publik:

``` http
GET /api/events
GET /api/events/{id}
GET /api/events/nearby
GET /api/events/tonight
GET /api/events/weekend
GET /api/events/recommended
```

Management:

``` http
POST /api/events
PUT /api/events/{id}
PATCH /api/events/{id}/status
DELETE /api/events/{id}
```

------------------------------------------------------------------------

# 35. API Ticket

Flow:

``` text
Event Detail
   │
   ▼
Ticket Selection
   │
   ▼
Checkout
   │
   ▼
Payment
   │
   ▼
Ticket
```

Endpoint:

``` http
GET /api/events/{id}/tickets
POST /api/tickets
GET /api/tickets
GET /api/tickets/{id}
```

Pembelian tiket hanya diperbolehkan untuk event:

``` text
Published
```

Project saat ini masih menggunakan simulated payment.

------------------------------------------------------------------------

# 36. Authorization Matrix

  Feature                   User   Artist   EO   Admin
  ----------------------- ------ -------- ---- -------
  Discover Event               ✓        ✓    ✓       ✓
  View Artist                  ✓        ✓    ✓       ✓
  Follow Artist                ✓        ✓    ✓       ✓
  Buy Ticket                   ✓        ✓    ✓       ✓
  Create Artist Content       \-        ✓   \-       ✓
  Manage Own Artist           \-        ✓   \-       ✓
  Create Event                \-       \-    ✓       ✓
  Edit Own Event              \-       \-    ✓       ✓
  Edit Other EO Event         \-       \-   \-       ✓
  Delete Own Event            \-       \-    ✓       ✓
  Delete Other Event          \-       \-   \-       ✓
  Manage Users                \-       \-   \-       ✓
  Platform Analytics          \-       \-   \-       ✓

------------------------------------------------------------------------

# 37. .NET MAUI Architecture

Struktur disarankan:

``` text
GigRadarMobile
│
├── Models
│
├── Services
│   ├── ApiService
│   ├── AuthService
│   ├── EventService
│   ├── ArtistService
│   ├── TicketService
│   └── UserService
│
├── ViewModels
│   ├── User
│   ├── Artist
│   ├── EO
│   └── Admin
│
├── Views
│   ├── User
│   ├── Artist
│   ├── EO
│   └── Admin
│
├── Shells
│   ├── UserShell
│   ├── ArtistShell
│   ├── EOShell
│   └── AdminShell
│
├── Helpers
├── Converters
└── Resources
```

------------------------------------------------------------------------

# 38. Struktur Folder Baru

Contoh:

``` text
GigRadarMobile/
│
├── Shells/
│   ├── UserShell.xaml
│   ├── ArtistShell.xaml
│   ├── EOShell.xaml
│   └── AdminShell.xaml
│
├── Views/
│   ├── User/
│   │   ├── DiscoverPage.xaml
│   │   ├── MapPage.xaml
│   │   ├── TicketsPage.xaml
│   │   └── ProfilePage.xaml
│   │
│   ├── Artist/
│   │   ├── ArtistDashboardPage.xaml
│   │   ├── ArtistMusicPage.xaml
│   │   ├── ArtistPostsPage.xaml
│   │   ├── ArtistGigsPage.xaml
│   │   └── ArtistProfilePage.xaml
│   │
│   ├── EO/
│   │   ├── EoDashboardPage.xaml
│   │   ├── EoEventsPage.xaml
│   │   ├── EoTicketsPage.xaml
│   │   ├── EoAnalyticsPage.xaml
│   │   └── EoProfilePage.xaml
│   │
│   └── Admin/
│       ├── AdminDashboardPage.xaml
│       ├── UsersPage.xaml
│       ├── ArtistsPage.xaml
│       ├── EOPage.xaml
│       ├── EventsPage.xaml
│       ├── TicketsPage.xaml
│       └── SettingsPage.xaml
```

------------------------------------------------------------------------

# 39. Role-Based Shell

Setelah login:

``` csharp
switch (role)
{
    case "User":
        Application.Current.MainPage = new UserShell();
        break;

    case "Artist":
        Application.Current.MainPage = new ArtistShell();
        break;

    case "EO":
        Application.Current.MainPage = new EOShell();
        break;

    case "Admin":
        Application.Current.MainPage = new AdminShell();
        break;
}
```

Role harus berasal dari JWT/server.

Jangan menggunakan role yang dikirim bebas oleh client untuk menentukan
hak akses.

------------------------------------------------------------------------

# 40. User Experience

Target pengalaman:

``` text
Discover
   ↓
Find Artist/Event
   ↓
Follow Artist
   ↓
Receive Updates
   ↓
Find Gig
   ↓
Buy Ticket
   ↓
Attend Event
```

------------------------------------------------------------------------

# 41. Artist Experience

Target pengalaman Artist:

``` text
Create Profile
      ↓
Upload Music
      ↓
Create Post
      ↓
Build Followers
      ↓
Publish New Release
      ↓
Get Gigs
      ↓
Perform
      ↓
Update Journey
```

------------------------------------------------------------------------

# 42. EO Experience

Target pengalaman EO:

``` text
Create Event
      ↓
Add Venue
      ↓
Add Lineup
      ↓
Create Tickets
      ↓
Publish
      ↓
Promote
      ↓
Sell Tickets
      ↓
Manage Event
      ↓
Analyze Performance
```

------------------------------------------------------------------------

# 43. Admin Experience

Target pengalaman Admin:

``` text
Monitor
   ↓
Moderate
   ↓
Manage
   ↓
Analyze
```

Admin menjadi pusat kontrol platform.

------------------------------------------------------------------------

# 44. Event Lifecycle

Lifecycle:

``` text
Draft
  │
  ▼
Published
  │
  ├── SoldOut
  │
  └── Completed
```

Aturan:

``` text
Draft
→ belum tampil sebagai event publik

Published
→ tampil di discovery
→ dapat dibeli tiket

SoldOut
→ event masih ada tetapi tiket habis

Completed
→ event telah selesai
```

------------------------------------------------------------------------

# 45. Ticket Flow

``` text
User
 │
 ▼
Event Detail
 │
 ▼
Ticket Selection
 │
 ▼
Checkout
 │
 ▼
Payment
 │
 ▼
Ticket Success
 │
 ▼
My Tickets
```

Ticket dapat menampilkan:

-   event;
-   venue;
-   tanggal;
-   jam;
-   ticket type;
-   order number;
-   barcode/QR.

Catatan: implementasi payment dan barcode saat ini masih bersifat
simulasi.

------------------------------------------------------------------------

# 46. Artist Event Flow

Artist yang ditambahkan ke lineup event:

``` text
EO creates Event
      │
      ▼
Select Artist
      │
      ▼
EventArtists
      │
      ▼
Artist sees Gig
      │
      ▼
Gig appears on Artist Profile
```

------------------------------------------------------------------------

# 47. Artist Content Flow

``` text
Artist
 │
 ├── Music
 │    ├── Track
 │    └── Album
 │
 ├── Posts
 │    ├── Update
 │    ├── Poster
 │    └── Announcement
 │
 └── Journey
      ├── Formation
      ├── Release
      ├── Gig
      └── Achievement
```

------------------------------------------------------------------------

# 48. Rekomendasi User

Sistem rekomendasi awal tetap menggunakan rule-based recommendation.

Input:

``` text
User Preferences
Artist Genre
Event Location
Event Date
Popularity
```

Bobot:

``` text
Genre        0.40
Location     0.30
Popularity   0.10
Date         0.20
```

Output:

``` text
Recommended Events
Recommended Artists
Recommended Gigs
```

Pengembangan selanjutnya dapat menggunakan:

-   histori event yang dilihat;
-   event yang difavoritkan;
-   artist yang di-follow;
-   tiket yang dibeli;
-   genre yang sering dipilih.

------------------------------------------------------------------------

# 49. Search & Discovery

Fitur discovery yang disarankan:

``` text
Search
├── Events
├── Artists
├── Venues
└── Genres
```

Filter:

``` text
Location
Date
Genre
Price
Event Type
Artist
Venue
```

Sort:

``` text
Nearest
Newest
Popular
Date
Price
```

------------------------------------------------------------------------

# 50. Prinsip UX

## User

Fokus:

``` text
Discover → Follow → Buy → Attend
```

UI harus sederhana dan berorientasi discovery.

## Artist

Fokus:

``` text
Create → Share → Build Audience → Perform
```

UI harus menonjolkan musik, konten, follower, dan gigs.

## EO

Fokus:

``` text
Create → Promote → Sell → Manage → Analyze
```

UI harus berorientasi operasional dan data.

## Admin

Fokus:

``` text
Monitor → Moderate → Manage
```

UI harus berorientasi kontrol platform.

------------------------------------------------------------------------

# 51. Keamanan

## 51.1 Backend Authorization

UI bukan security boundary.

Contoh:

``` text
Button Edit Event
```

Boleh disembunyikan untuk user biasa, tetapi backend tetap harus
memvalidasi authorization.

------------------------------------------------------------------------

## 51.2 Role Escalation

Jangan menerima:

``` json
{
  "role": "Admin"
}
```

dari client sebagai dasar pemberian role.

Role privileged harus ditentukan server.

------------------------------------------------------------------------

## 51.3 Event Ownership

Backend harus memeriksa:

``` text
CurrentUserId == Event.CreatedBy
```

untuk EO.

------------------------------------------------------------------------

## 51.4 JWT

JWT harus berisi claim yang diperlukan:

``` text
UserId
Name
Email
Role
```

Token disimpan dengan aman di sisi mobile.

------------------------------------------------------------------------

## 51.5 Password

Password harus disimpan dalam bentuk hash.

Jangan menyimpan password plaintext di database.

------------------------------------------------------------------------

# 52. Aturan Role

## User

Tidak boleh:

``` text
Create Event
Manage Event
Manage Artist Profile
Manage Platform
```

## Artist

Boleh:

``` text
Manage Own Artist Profile
Manage Own Music
Manage Own Albums
Manage Own Posts
Manage Own Journey
```

Tidak boleh:

``` text
Manage EO Event
Manage Users
Manage Platform
```

## EO

Boleh:

``` text
Create Event
Manage Own Event
Manage Own Tickets
View Public Events
```

Tidak boleh:

``` text
Edit Other EO Event
Delete Other EO Event
Manage Users
```

## Admin

Boleh:

``` text
Manage Everything
```

sesuai kebijakan moderasi dan sistem.

------------------------------------------------------------------------

# 53. Tahapan Implementasi

## Phase 1 - Role Foundation

Implementasikan:

-   role validation;
-   JWT claims;
-   backend authorization;
-   role-based Shell;
-   login routing;
-   secure registration.

Prioritas tinggi:

``` text
Security
↓
Role
↓
Navigation
```

------------------------------------------------------------------------

## Phase 2 - User

Implementasikan:

-   Discover;
-   Map;
-   Event detail;
-   Search;
-   Filter;
-   Favorites;
-   Follow Artist;
-   Tickets;
-   Profile.

------------------------------------------------------------------------

## Phase 3 - Artist

Implementasikan:

-   Artist profile;
-   Music management;
-   Album management;
-   Posts;
-   Artist Journey;
-   Followers;
-   Gigs;
-   Artist dashboard.

------------------------------------------------------------------------

## Phase 4 - EO

Implementasikan:

-   EO dashboard;
-   Event management;
-   Venue;
-   Lineup;
-   Ticket management;
-   Event status;
-   Event analytics.

------------------------------------------------------------------------

## Phase 5 - Admin

Implementasikan:

-   Dashboard;
-   User management;
-   Artist management;
-   EO management;
-   Event moderation;
-   Ticket monitoring;
-   Settings.

------------------------------------------------------------------------

## Phase 6 - Recommendation

Kembangkan:

``` text
Rule Based
     ↓
Behavior Data
     ↓
Personalized Recommendation
     ↓
Machine Learning
```

------------------------------------------------------------------------

## Phase 7 - Advanced Features

Fitur lanjutan:

-   payment gateway;
-   push notification;
-   real audio streaming;
-   social feed;
-   comments;
-   likes;
-   venue/EO following;
-   crowdfunding;
-   analytics;
-   multi-city;
-   advanced search;
-   ML recommendation.

------------------------------------------------------------------------

# 54. Target Akhir

GigRadar pada tahap akhir diharapkan menjadi platform yang
mempertemukan:

``` text
USER
  │
  ├── Discover
  ├── Follow
  ├── Buy Ticket
  └── Attend
       │
       ▼
    GIGRADAR
       ▲
       │
  ┌────┴────┐
  │         │
ARTIST      EO
  │         │
  ├─ Music  ├─ Events
  ├─ Posts  ├─ Tickets
  ├─ Gigs   ├─ Promotion
  └─ Journey└─ Analytics
       │
       ▼
     ADMIN
       │
       └── Platform Management
```

Tujuan akhirnya bukan sekadar aplikasi ticketing.

GigRadar harus menjadi ekosistem musik:

``` text
Artist
   ↓
Create Music
   ↓
Build Audience
   ↓
Get Gigs
   ↓
EO Creates Events
   ↓
User Discovers
   ↓
User Buys Ticket
   ↓
User Attends
   ↓
Artist Gains Audience
```

------------------------------------------------------------------------

# 55. Kesimpulan

GigRadar menggunakan konsep **multi-role application** dengan satu
aplikasi mobile tetapi pengalaman yang berbeda untuk setiap role.

Empat role utama:

``` text
User
Artist
EO
Admin
```

Masing-masing mempunyai Shell dan navigation sendiri:

``` text
UserShell
ArtistShell
EOShell
AdminShell
```

Perbedaan UI harus didukung oleh authorization backend.

Struktur yang direkomendasikan:

``` text
.NET MAUI
    │
    ▼
Role-Based Shell
    │
    ├── User
    ├── Artist
    ├── EO
    └── Admin
    │
    ▼
ASP.NET Core Web API
    │
    ▼
JWT Authorization
    │
    ▼
Entity Framework Core
    │
    ▼
SQLite
```

Dengan struktur ini, GigRadar dapat berkembang dari aplikasi pencarian
gigs menjadi platform yang menghubungkan:

``` text
Pendengar Musik
      ↕
    Artist
      ↕
      EO
      ↕
   Event/Gigs
      ↕
    Ticket
      ↕
    GigRadar
```

## Prioritas implementasi

Urutan pengerjaan yang disarankan:

``` text
1. Secure Role System
2. JWT Authorization
3. Role-Based Shell
4. User Experience
5. Artist Experience
6. EO Experience
7. Admin Experience
8. Database Expansion
9. API Expansion
10. Recommendation System
11. Payment Gateway
12. Advanced Social Features
```

Dokumen ini menjadi spesifikasi dasar untuk pengembangan **GigRadar
Multi-Role System** menggunakan **.NET MAUI + ASP.NET Core Web API**.
