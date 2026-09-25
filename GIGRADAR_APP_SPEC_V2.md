# GIGRADAR_APP_SPEC_V2.md

## 1. Status Dokumen

- **Project:** GigRadar
- **Platform:** .NET MAUI
- **Target:** Android terlebih dahulu, dengan struktur yang tetap memungkinkan pengembangan platform lain
- **Versi konsep:** V2 - Incremental Upgrade
- **Prinsip utama:** Jangan mengubah 100% aplikasi lama. Pertahankan fitur, role, navigation, dan struktur yang masih relevan. Tambahkan fitur baru secara bertahap.
- **Referensi konsep:** Event discovery, artist discovery, ticketing, QR check-in, dan event management seperti pola yang digunakan platform event/music discovery modern.

---

# 2. Prinsip Perubahan

Dokumen ini adalah **upgrade**, bukan redesign total.

### Yang dipertahankan

1. Role utama:
   - Admin
   - User
   - Artist
   - EO / Event Organizer

2. Konsep GigRadar sebagai aplikasi untuk menemukan event musik/gig lokal.

3. Fitur User:
   - Melihat event berdasarkan lokasi
   - Melihat event berdasarkan dashboard
   - Melihat artist/band
   - Mengikuti artist/band
   - Membeli tiket event

4. Fitur Artist:
   - Profil artist/band
   - Update aktivitas
   - Update lagu
   - Informasi gig
   - Poster/announcement release

5. Fitur EO:
   - Membuat event
   - Mengelola event
   - Melihat event milik sendiri
   - Mengelola tiket

6. Fitur Admin:
   - Melihat akun terdaftar
   - CRUD akun
   - Melihat event
   - Mengelola event dan tiket bila diperlukan

### Yang ditambahkan

- Event discovery yang lebih kuat
- Filter genre/kota/tanggal
- Artist discovery
- Sistem follow artist
- Ticket type
- Digital ticket
- QR ticket
- QR check-in untuk EO
- Event recommendation
- Saved events
- Struktur dashboard EO yang lebih lengkap
- Dukungan RSVP/reservation untuk event tertentu
- Open-table/reservation sebagai fitur opsional, bukan fokus utama

### Yang tidak dilakukan

- Tidak menghapus seluruh UI lama
- Tidak mengganti seluruh database jika model lama masih dapat digunakan
- Tidak mengganti seluruh navigation
- Tidak mengganti seluruh API/service
- Tidak memaksa semua fitur baru langsung aktif
- Tidak mengubah role menjadi sistem baru

---

# 3. Konsep Produk

## 3.1 Identitas

**GigRadar**

> Menghubungkan Skena, Menemukan Suara Lokal.

GigRadar adalah aplikasi discovery untuk menemukan:

- Konser
- Gig lokal
- Festival musik
- Event komunitas
- Event artist/band
- Event venue
- Event nightlife yang legal dan sesuai batasan usia/platform
- Reservation/RSVP event tertentu

Fokus utama tetap pada **musik dan skena lokal**.

---

# 4. Role System

## 4.1 User

User adalah pendengar/pengunjung event.

### Fitur

- Register/Login
- Edit profile
- Pilih genre favorit
- Pilih kota/lokasi
- Explore event
- Search event
- Filter event
- Melihat detail event
- Melihat artist
- Follow artist
- Unfollow artist
- Melihat update artist
- Save event
- Membeli tiket
- Melihat tiket saya
- Menampilkan QR ticket
- Melihat riwayat pembelian
- RSVP/reservation jika event mendukungnya

---

## 4.2 Artist

Artist dapat berupa:

- Solo artist
- Band
- Duo
- Group

### Fitur

- Artist profile
- Genre
- Biography
- Social links
- Foto/banner
- Lagu/release
- Upcoming gigs
- Past gigs
- Update
- Poster event
- Poster release
- Jadwal pertunjukan
- Melihat jumlah followers

### Artist tidak dapat

- Mengubah data user lain
- Mengubah event milik EO tanpa izin
- Mengubah data sistem/admin

---

## 4.3 EO

EO bertanggung jawab terhadap event.

### Fitur

- EO profile
- Create event
- Edit event
- Publish/unpublish event
- Event management
- Menambahkan artist/lineup
- Membuat tipe tiket
- Mengatur harga tiket
- Mengatur kuota
- Melihat jumlah tiket terjual
- Melihat attendee
- QR check-in
- Event analytics sederhana
- Mengelola RSVP/reservation

---

## 4.4 Admin

Admin memiliki akses pengelolaan platform.

### Dashboard

- Total User
- Total Artist
- Total EO
- Total Event
- Total Ticket
- Event aktif
- Event selesai
- Data akun

### CRUD

Admin dapat:

- Create account
- Read account
- Update account
- Delete account

Untuk:

- User
- Artist
- EO

Admin juga dapat:

- Melihat event
- Edit event
- Menghapus event yang melanggar aturan platform
- Melihat ticket
- Melihat transaksi/event statistics
- Mengelola genre
- Mengelola kategori event

---

# 5. Navigation

Navigation lama harus dipertahankan sebisa mungkin.

## User Navigation

```text
Home
├── Recommended
├── Nearby Events
├── Upcoming Events
└── Artist Updates

Explore
├── Events
├── Artists
├── Genres
└── Locations

Tickets
├── Upcoming
├── Used
└── History

Following
└── Followed Artists

Profile
├── Personal Data
├── Favorite Genres
├── Saved Events
└── Settings
```

## Artist Navigation

```text
Dashboard
Profile
Music
Updates
Gigs
Notifications
Settings
```

## EO Navigation

```text
Dashboard
My Events
Create Event
Tickets
Attendees
Check-in
Analytics
Profile
Settings
```

## Admin Navigation

```text
Dashboard
Users
Artists
EO
Events
Tickets
Genres
Reports
Settings
```

---

# 6. Home User

Home tidak perlu dibuat seperti marketplace tiket biasa.

Tujuan utama:

> Membantu user menemukan event yang mungkin mereka sukai.

Contoh:

```text
Good evening

📍 Depok

Recommended for You
────────────────────
Indie Night
28 September
Jakarta
Rp75.000

Because You Follow
────────────────────
Band A
Live at Venue X

Nearby
────────────────────
Gig B
8 km away

Explore
────────────────────
[Rock] [Indie] [Punk]
[Pop] [Metal] [Jazz]
```

---

# 7. Event Discovery

## Search

User dapat mencari:

- Nama event
- Nama artist
- Nama venue
- Genre
- Kota

## Filter

```text
Date
├── Today
├── Tomorrow
├── This Week
└── Custom

Genre
├── Rock
├── Indie
├── Punk
├── Metal
├── Pop
├── Jazz
└── Other

Distance
├── 5 km
├── 10 km
├── 25 km
└── All

Price
├── Free
├── Under 50K
├── Under 100K
└── Custom
```

---

# 8. Event Detail

```text
[Event Cover]

Event Name

📅 Date
🕐 Time
📍 Venue

Description

LINEUP
🎤 Artist A
🎤 Artist B
🎤 Artist C

TICKET
Regular       Rp50.000
VIP            Rp100.000

Available: 127

[Buy Ticket]
```

Informasi tambahan:

- Venue
- Map/location
- Organizer
- Age requirement jika berlaku
- Event rules
- Cancellation policy
- Lineup
- Ticket types
- Remaining capacity

---

# 9. Artist Discovery

Artist menjadi bagian penting yang membedakan GigRadar dari ticketing platform biasa.

## Artist Card

```text
[Artist Photo]

Band Name
Alternative / Indie

1.2K Followers

[Follow]
```

## Artist Profile

```text
Band Name
Alternative / Indie

1.2K Followers

[Follow]

About

Music
├── Latest Release
└── Previous Release

Upcoming Gigs
├── Event A
└── Event B

Updates
├── New Song
├── New Album
└── Live Announcement
```

---

# 10. Follow System

User dapat mengikuti artist.

Relasi:

```text
User
  │
  └── follows
        │
        ▼
      Artist
```

Ketika artist membuat update:

```text
Artist creates update
        ↓
Follower receives update
        ↓
User opens update
        ↓
User can open related event
```

---

# 11. Recommendation System

Versi awal **tidak harus menggunakan Machine Learning sungguhan**.

Gunakan scoring sederhana terlebih dahulu.

Contoh:

```text
Event Score =
Genre Match
+ Location Match
+ Followed Artist
+ Previous Interaction
+ Event Popularity
```

Contoh bobot awal:

```text
Genre match       +40
Followed artist   +30
Nearby            +20
Saved similar     +10
```

ML dapat ditambahkan pada fase berikutnya setelah data interaksi cukup.

---

# 12. Ticketing

## Ticket Type

EO dapat membuat:

- Early Bird
- Regular
- VIP
- Group
- Free
- RSVP

Contoh:

```text
Regular
Rp75.000
Capacity: 500

VIP
Rp150.000
Capacity: 100
```

---

# 13. Digital Ticket

Setelah pembayaran berhasil:

```text
Order
 ↓
Payment Success
 ↓
Ticket Created
 ↓
QR Generated
```

Ticket menyimpan:

- Ticket ID
- Order ID
- Event ID
- User ID
- Ticket Type
- Status
- QR token

Status:

```text
ACTIVE
USED
CANCELLED
EXPIRED
```

---

# 14. QR Check-in

EO memiliki halaman:

```text
CHECK-IN

[ Scan QR ]

Today's Event

Checked In
127 / 500
```

Flow:

```text
User shows QR
      ↓
EO scans QR
      ↓
System validates
      ↓
Ticket ACTIVE?
      ↓
YES
      ↓
Change status → USED
```

QR tidak boleh hanya berisi informasi yang mudah ditebak. Gunakan token/identifier yang dapat divalidasi server.

---

# 15. RSVP / Reservation

Fitur ini bersifat **opsional**.

Tidak semua event membutuhkan pembelian tiket.

Event dapat memiliki:

```text
Ticketed Event
```

atau

```text
Free RSVP
```

atau

```text
Reservation
```

Contoh:

```text
Open Event

Reservation
Capacity: 100

[Reserve Spot]
```

Jika nanti GigRadar memiliki kerja sama dengan venue, reservation dapat dikembangkan menjadi:

- Table reservation
- Seating area
- Reservation slot
- Guest list

Fitur tersebut **bukan inti GigRadar versi awal** dan tidak boleh mengganggu sistem konser/gig utama.

---

# 16. Event Status

```text
DRAFT
PENDING_REVIEW
PUBLISHED
SOLD_OUT
ONGOING
COMPLETED
CANCELLED
```

EO:

```text
DRAFT → PUBLISH → PUBLISHED
```

Admin dapat melakukan moderation.

---

# 17. Database Model

Model existing harus dipertahankan dan diperluas, bukan langsung diganti.

## User

```csharp
User
{
    Id
    Name
    Email
    PasswordHash
    Role
    ProfileImage
    City
    CreatedAt
}
```

Role:

```text
ADMIN
USER
ARTIST
EO
```

## Artist

```csharp
Artist
{
    Id
    UserId
    ArtistName
    GenreId
    Bio
    ImageUrl
    FollowersCount
}
```

## EO

```csharp
EventOrganizer
{
    Id
    UserId
    OrganizerName
    Description
    LogoUrl
}
```

## Event

```csharp
Event
{
    Id
    OrganizerId
    Title
    Description
    CoverImage
    VenueName
    Address
    Latitude
    Longitude
    StartDate
    EndDate
    Status
}
```

## EventArtist

```csharp
EventArtist
{
    Id
    EventId
    ArtistId
    PerformanceOrder
}
```

## Genre

```csharp
Genre
{
    Id
    Name
}
```

## UserGenre

```csharp
UserGenre
{
    UserId
    GenreId
}
```

## Follow

```csharp
ArtistFollow
{
    UserId
    ArtistId
    CreatedAt
}
```

## TicketType

```csharp
TicketType
{
    Id
    EventId
    Name
    Price
    Capacity
    SoldCount
}
```

## TicketOrder

```csharp
TicketOrder
{
    Id
    UserId
    EventId
    TotalAmount
    Status
    CreatedAt
}
```

## Ticket

```csharp
Ticket
{
    Id
    OrderId
    TicketTypeId
    UserId
    QRToken
    Status
}
```

## ArtistUpdate

```csharp
ArtistUpdate
{
    Id
    ArtistId
    Title
    Content
    ImageUrl
    CreatedAt
}
```

## SavedEvent

```csharp
SavedEvent
{
    UserId
    EventId
    CreatedAt
}
```

## RSVP

```csharp
RSVP
{
    Id
    UserId
    EventId
    Status
    CreatedAt
}
```

---

# 18. API / Service Layer

Gunakan service layer agar UI .NET MAUI tidak langsung menangani database/API.

```text
Services/
├── AuthService
├── UserService
├── ArtistService
├── EventService
├── OrganizerService
├── TicketService
├── PaymentService
├── QRService
├── RecommendationService
├── NotificationService
└── AdminService
```

Contoh:

```csharp
IEventService
{
    GetEventsAsync()
    GetEventByIdAsync(id)
    SearchEventsAsync(query)
    FilterEventsAsync(filter)
    CreateEventAsync(event)
    UpdateEventAsync(event)
    DeleteEventAsync(id)
}
```

---

# 19. .NET MAUI Structure

Struktur dapat mengikuti struktur project lama.

Rekomendasi:

```text
GigRadar/
├── Models/
├── Views/
│   ├── Auth/
│   ├── User/
│   ├── Artist/
│   ├── EO/
│   └── Admin/
├── ViewModels/
│   ├── Auth/
│   ├── User/
│   ├── Artist/
│   ├── EO/
│   └── Admin/
├── Services/
├── Data/
├── Navigation/
├── Resources/
│   ├── Images/
│   ├── Fonts/
│   └── Styles/
└── AppShell.xaml
```

Jika project lama sudah mempunyai struktur berbeda, **jangan melakukan migrasi massal hanya karena struktur rekomendasi ini berbeda**.

---

# 20. UI Design Rules

GigRadar tidak boleh terlihat seperti template AI generik.

## Visual Direction

- Modern
- Dark-first
- Music-focused
- Editorial
- Energetic
- Clean
- Tidak terlalu banyak gradient
- Tidak menggunakan glassmorphism berlebihan
- Gunakan typography yang kuat
- Gunakan artwork/event poster sebagai elemen visual utama

## Event Card

Prioritaskan:

```text
Poster
↓
Event name
↓
Artist / genre
↓
Date + location
↓
Price
```

Bukan:

```text
10 badge
5 gradient
3 shadow
7 icon
```

---

# 21. Existing UI Compatibility

Sebelum membuat halaman baru:

1. Periksa halaman existing.
2. Pertahankan komponen yang masih sesuai.
3. Reuse model yang sudah ada.
4. Reuse service yang sudah ada.
5. Reuse navigation yang sudah ada.
6. Tambahkan fitur baru secara incremental.

Jika sebuah halaman lama sudah bekerja:

> **Jangan rewrite hanya demi terlihat berbeda.**

---

# 22. Migration Strategy

Implementasi dilakukan bertahap.

## Phase 1 - Audit

Periksa:

- Models
- Views
- ViewModels
- Services
- API
- Database
- Navigation
- Authentication

Buat daftar:

```text
KEEP
MODIFY
ADD
REMOVE
```

`REMOVE` harus seminimal mungkin.

---

## Phase 2 - Event Discovery

Tambahkan:

- Search
- Filter
- Genre
- Location
- Event detail
- Save event

Tanpa menghapus event system lama.

---

## Phase 3 - Artist Discovery

Tambahkan:

- Artist profile
- Follow
- Artist update
- Upcoming gigs

---

## Phase 4 - Ticketing

Tambahkan:

- Ticket type
- Order
- Digital ticket
- QR

---

## Phase 5 - EO Management

Tambahkan:

- Ticket management
- Attendee
- QR check-in
- Analytics sederhana

---

## Phase 6 - Recommendation

Tambahkan rule-based recommendation.

ML hanya digunakan jika dataset/interaksi sudah mencukupi.

---

# 23. Admin Moderation

Admin dapat memantau:

```text
Accounts
Events
Artists
EO
Tickets
Reports
```

Admin dapat melakukan:

```text
View
Create
Edit
Delete
Suspend
```

Namun operasi destructive harus memiliki confirmation dialog.

Contoh:

```text
Delete Event?

This action cannot be undone.

[Cancel] [Delete]
```

---

# 24. Security Rules

- Password tidak disimpan plaintext.
- Role harus divalidasi di backend.
- User tidak boleh mengakses endpoint Admin hanya dengan mengubah UI.
- EO hanya dapat mengelola event miliknya.
- Artist hanya dapat mengubah profile/content miliknya.
- Ticket QR harus divalidasi server.
- Ticket yang sudah USED tidak dapat digunakan kembali.
- Harga dan total transaksi harus divalidasi server.
- Jangan percaya data harga dari client.

---

# 25. Notification

User dapat menerima notifikasi:

```text
Artist you follow has a new gig.
```

```text
Your saved event is happening tomorrow.
```

```text
Ticket purchase successful.
```

```text
Event schedule changed.
```

Artist:

```text
Your event is approaching.
```

EO:

```text
Your event reached 80% ticket capacity.
```

---

# 26. Future Features

Fitur berikut **tidak wajib dibuat sekarang**:

- Chat
- Social feed penuh
- ML recommendation
- Artist analytics
- EO financial analytics
- Table booking kompleks
- Seating map
- Multiple payment gateway
- Merchandise
- Livestream
- Community forum
- Loyalty system

Jangan memasukkan semuanya ke MVP.

---

# 27. MVP

MVP yang disarankan:

### User

- Login/Register
- Home
- Explore
- Search
- Filter
- Event detail
- Artist profile
- Follow artist
- Buy ticket
- My tickets
- QR ticket
- Profile

### Artist

- Profile
- Update
- Music/release
- Upcoming gigs

### EO

- Create event
- Edit event
- Ticket type
- Attendee
- QR check-in

### Admin

- Dashboard
- User CRUD
- Artist CRUD
- EO CRUD
- Event management
- Ticket overview

---

# 28. Definition of Done

Fitur dianggap selesai apabila:

- UI dapat dibuka tanpa crash.
- Role authorization bekerja.
- Data tersimpan dengan benar.
- Navigation tidak rusak.
- Existing feature tetap bekerja.
- Error state tersedia.
- Loading state tersedia.
- Empty state tersedia.
- Validasi form tersedia.
- API/service tidak bercampur langsung dengan UI.
- Tidak ada fitur lama yang hilang tanpa alasan.

---

# 29. Instruksi Untuk AI Coding Agent

Jika dokumen ini digunakan sebagai prompt/spec untuk Claude, Gemini, DeepSeek, atau coding agent lainnya:

> **Jangan rewrite project dari awal.**

Ikuti aturan:

1. Inspect project terlebih dahulu.
2. Pahami struktur existing.
3. Identifikasi fitur yang sudah tersedia.
4. Jangan menghapus fitur existing yang masih relevan.
5. Implementasikan perubahan secara incremental.
6. Reuse existing model/service/component jika memungkinkan.
7. Jangan membuat duplicate model untuk fungsi yang sama.
8. Jangan mengganti framework.
9. Tetap gunakan .NET MAUI.
10. Jangan memindahkan seluruh project hanya untuk mengikuti struktur contoh di dokumen.
11. Setiap perubahan besar harus memiliki alasan teknis.
12. Prioritaskan compile/build yang stabil.
13. Setelah perubahan, lakukan build dan perbaiki error.
14. Pastikan Android tetap menjadi target utama.
15. Jangan mengimplementasikan fitur future sebelum MVP stabil.

### Prinsip prioritas

```text
Existing functionality
        ↓
Stability
        ↓
New feature
        ↓
UI polish
        ↓
Optimization
```

---

# 30. Product Direction

GigRadar bukan hanya aplikasi ticketing.

Konsep akhirnya:

```text
              GIGRADAR
                  │
       ┌──────────┼──────────┐
       │          │          │
    DISCOVER    ARTIST     EVENT
       │          │          │
       │       Follow       │
       │       Updates      │
       │          │          │
       └──────────┼──────────┘
                  │
              TICKETING
                  │
              QR CHECK-IN
```

**Core loop:**

```text
Discover
   ↓
Find Artist
   ↓
Follow
   ↓
Receive Update
   ↓
Discover Gig
   ↓
Buy / RSVP
   ↓
Attend Event
   ↓
Follow More Artists
   ↓
Discover More Events
```

Ini menjadi pengembangan dari GigRadar sebelumnya, bukan penggantian total.

---

# 31. Final Requirement

Semua pengembangan setelah dokumen ini harus mengikuti prinsip:

> **"Upgrade, don't rebuild."**

GigRadar harus berkembang dari aplikasi yang sudah ada menjadi platform **music/event discovery + artist discovery + ticketing**, sambil mempertahankan identitas, role system, dan fitur inti versi sebelumnya.
