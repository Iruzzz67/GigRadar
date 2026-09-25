# GigRadar V2 — Phase 1 Audit & Gap Analysis

- **Sumber spec:** `GIGRADAR_APP_SPEC_V2.md`
- **Branch yang diaudit:** `feat/postgresql-migration`
- **Sifat pekerjaan:** read-only. Tidak ada perubahan kode pada fase ini.
- **Prinsip yang dipakai:** "Upgrade, don't rebuild" (spec §31).
- **Cara membaca:** setiap bab spec V2 dipetakan ke status **KEEP / MODIFY / ADD / REMOVE**, dengan catatan teknis dan referensi file.

> **Status setelah audit (diperbarui).** Temuan di bawah yang sudah dikerjakan:
> kepemilikan tiket saat check-in (§6.1), hardening password akun demo (§6.2),
> pengurangan stok race-safe (§6.3), increment `ViewsCount` pada detail event & sinkronisasi
> `SavesCount` pada toggle simpan (§6.4), tombol Follow + halaman Following (A1/A2),
> filter harga & opsi tanggal Explore (A7), scanner QR check-in + halaman Attendees (A3/A4),
> serta admin CRUD akun & genre (A5/A6). Isi bab lain masih berupa rekomendasi dan
> **belum** dikerjakan — terutama status event `PENDING_REVIEW`/`ONGOING`/`CANCELLED`
> (§16), paging/parameter query di `GET /api/events` (§4 dari tabel bab), dan
> moderasi/konten lain di tabel ADD.
> Catatan kontrak: `POST /api/tickets/validate` sekarang **wajib** menyertakan
> `eventId` di body — dipenuhi oleh UI scanner (`CheckInPage`).
> Verifikasi terakhir: `GigRadarApi.Tests` 206/206, `GigRadarMobile.Tests` 77/77,
> build Android & Windows 0 error.

---

## 1. Ringkasan Eksekutif

Aplikasi existing **jauh lebih maju daripada yang diasumsikan spec V2**. Dari fitur MVP di spec §27, mayoritas sudah ada. Yang benar-benar hilang ada **tiga blok besar**, dan semuanya berada di sisi *setelah* penemuan event:

| # | Temuan | Dampak | Prioritas |
| :-- | :--- | :--- | :--- |
| 1 | **Follow artist: API lengkap, UI nol.** `FollowArtistAsync` / `UnfollowArtistAsync` ada di `ApiService` tapi tidak dipanggil dari mana pun. `ArtistDetailPage` hanya menampilkan jumlah follower. Tidak ada tab **Following**. | Core loop spec §30 putus di langkah "Follow". | **Tinggi** |
| 2 | **QR check-in EO tidak ada.** Endpoint `POST /api/tickets/validate` ada, tapi tidak ada scanner, tidak ada halaman Attendees, tidak ada paket barcode scanner di `csproj`. | Phase 5 spec tidak bisa dimulai tanpa menambah dependency baru. | **Tinggi** |
| 3 | **Admin CRUD akun tidak ada.** `UsersController` hanya punya `GET` (list + communities). Tidak ada create/update/delete/suspend akun, dan tidak ada CRUD Genre (`GenresController` hanya `GET`). | Spec §4.4 dan §23 belum terpenuhi. | **Sedang** |

Selain itu, audit menemukan **6 isu teknis/keamanan** yang tidak tercakup spec tapi harus diselesaikan sebelum fitur baru ditumpuk di atasnya (bagian §6).

**Kesimpulan:** Phase 2 dan Phase 3 spec sebagian sudah selesai. Urutan implementasi yang diusulkan di §8 berbeda dari urutan spec karena alasan ini.

---

## 2. Metode Audit

Yang diperiksa:

- Seluruh `GigRadarApi/Models/*` (10 file, 28 entitas terdaftar di `AppDbContext`).
- Seluruh `GigRadarApi/Controllers/*` (15 controller — inventarisasi route & atribut `[Authorize]`).
- Seluruh `GigRadarApi/Services/*` (7 service, dibaca penuh untuk event, tiket, auth, verifikasi).
- Seluruh `GigRadarMobile/ViewModels/*` (36 VM) dan `Services/ApiService.cs` (1.048 baris).
- `Shells/*.xaml` (4 shell per role) + `Helpers/ShellRouter.cs`.
- `GigRadarMobile/Models/*` (15 file).
- 6 migration EF Core terakhir + `AppDbContextModelSnapshot.cs`.
- Proyek test: `GigRadarApi.Tests` (9 file), `GigRadarMobile.Tests` (3 file).

Yang **tidak** dilakukan di fase ini: build, menjalankan test, menjalankan aplikasi, atau menyentuh database. Klaim di bawah berasal dari pembacaan kode.

---

## 3. Baseline Existing

### 3.1 API — 15 controller

| Controller | Ringkas |
| :--- | :--- |
| `AuthController` | register, login (BCrypt + JWT) |
| `EventsController` | list, detail, nearby, tonight, weekend, **recommended**, managed, managed/summary, CRUD + ubah status. Mutasi dibatasi `Roles=EO,Admin` + policy `VerifiedOrganizer` |
| `ArtistsController` | list, detail (members/tracks/albums/posts/journey), tracks, albums, posts, journey, events |
| `ArtistController` | area artist yang login: `me`, `me/dashboard`, CRUD tracks/albums/posts/journey/members, pengajuan verifikasi |
| `OrganizerController` | `me`, CRUD portfolio, pengajuan verifikasi EO |
| `TicketsController` | tiket saya, tipe tiket per event, CRUD tipe tiket, detail, beli, **validate** |
| `UsersController` | list user (Admin), communities, `me` (GET/PUT), role-requests (approve/reject), verifikasi artist + EO (approve/reject), preferences, favorites, **follows (GET/POST/DELETE)** |
| `GenresController` | **hanya** `GET` |
| `VenuesController` | `GET`, `POST` |
| `InvitationsController` | invite artist ke event, list, accept/reject/cancel |
| `MerchandiseController` | CRUD merch artist (di file yang sama juga: `NotificationsController`, `ReportsController`) |
| `PostsController` | like/unlike, komentar |
| `AnalyticsController` | `eo` (EO/Admin), `platform` (Admin) |

### 3.2 Database

EF Core + Npgsql, PostgreSQL, 6 migration (terakhir: `AddNotificationMerchReport`). 28 `DbSet`: User, UserPreference, Genre, Artist, ArtistAlbum, ArtistPost, ArtistMember, ArtistJourneyItem, ArtistVerification, Venue, Event, EventArtist, EventTicketType, Ticket, Favorite, Follow, AudioTrack, RoleRequest, Organizer, OrganizerPortfolioItem, OrganizerVerification, PostLike, PostComment, EventInvitation, Notification, Merchandise, Report, AuditLog.

Yang **tidak ada** dari spec V2: `TicketOrder`, `RSVP`, `SavedEvent` (digantikan `Favorite`), `UserGenre` (digantikan `UserPreference`), `ArtistUpdate` (digantikan `ArtistPost`).

### 3.3 Mobile — 4 shell, 24 halaman

| Shell | Tab sekarang | Spec §5 minta | Selisih |
| :--- | :--- | :--- | :--- |
| **User** | Discover, Explore, Radar, Tickets, Notifikasi, Profile | Home, Explore, Tickets, **Following**, Profile | Kurang **Following**; "Radar" adalah fitur khas existing yang tidak ada di spec (KEEP) |
| **Artist** | Dashboard, Undangan, Merch, Notifikasi, Profile | Dashboard, Profile, **Music**, **Updates**, **Gigs**, Notifications, Settings | Music/Updates/Gigs belum ada (sudah ditandai TODO di `ArtistShell.xaml`) |
| **EO** | Dashboard, Events, Undangan, Tickets, Analytics, Notifikasi, Profile | Dashboard, My Events, Create Event, Tickets, **Attendees**, **Check-in**, Analytics, Profile | Kurang **Attendees** + **Check-in** |
| **Admin** | Dashboard, Events, Tickets, Verifikasi, Users, Laporan, Analitik, Notifikasi, Profile | Dashboard, Users, Artists, EO, Events, Tickets, **Genres**, Reports, Settings | **Genres** belum ada; Artists/EO sudah tercakup halaman Verifikasi + Analitik komunitas |

Catatan: **Radar** (visualisasi gig di sekitar user) adalah pembeda produk yang sudah matang (`RadarDrawable`, `RadarBuilder`, `RadarPage`) dan **tidak disebut sama sekali di spec V2**. Ini masuk kategori KEEP dan sebaiknya diangkat ke spec, bukan dibuang.

### 3.4 Test

12 file test: `EventServiceTests`, `EventValidatorTests`, `ArtistVerificationTests`, `OrganizerVerificationTests`, `InvitationTests`, `PostSocialTests`, `MerchNotificationReportTests`, `VerifiedOrganizerAuthorizationTests` (API) + `CheckoutValidatorsTests`, `GeoHelperTests`, `RadarBuilderTests` (mobile). Belum ada test untuk `TicketService` (termasuk alur pembelian & validasi QR) dan `RecommendationService`/`GetRecommendedEventsAsync`.

---

## 4. Peta KEEP / MODIFY / ADD / REMOVE per Bab Spec V2

| Bab spec | Status | Catatan |
| :--- | :--- | :--- |
| §1–3 Status dokumen, prinsip, konsep produk | **KEEP** | Identitas produk sudah cocok dengan README. |
| §4.1 User | **KEEP** | Semua fitur user sudah ada kecuali "Following" (lihat §9/§10). |
| §4.2 Artist | **KEEP** | Melebihi spec: album, journey, member, merch, verifikasi. |
| §4.3 EO | **MODIFY** | Kurang **attendee list** + **QR check-in** + **analytics per event** (analytics ada di level EO/platform, belum per event selain summary). |
| §4.4 Admin | **MODIFY** | Dashboard statistik ✅, event management ✅, moderasi laporan ✅. **CRUD akun user/artist/EO ✗**, **CRUD genre ✗**, **suspend ✗**. |
| §5 Navigation | **MODIFY** | Lihat tabel §3.3. Jangan ganti shell; tambah tab/route. |
| §6 Home User | **MODIFY** | Blok Recommended/Nearby/Tonight/Weekend/Genre sudah ada. **Kurang blok "Artist Updates"** dan `Recommended` masih `Take(5)` polos, bukan `/api/events/recommended`. |
| §7 Event Discovery (search + filter) | **MODIFY** | Search by nama/venue/genre/lineup ✅ (client-side). **Filter harga ✗**. Filter tanggal hanya `Semua / Malam ini / Akhir pekan` — tidak ada `Tomorrow / This Week / Custom`. Radius ✅ (10/25/50 km). |
| §8 Event Detail | **KEEP** | Lineup, tipe tiket, sisa kuota per tipe (`StockLabel`), peta, share, simpan — sudah lengkap. |
| §9 Artist Discovery | **MODIFY** | Kartu & profil artist ✅ (follower count dari server). **Tombol Follow ✗** — meski endpoint & method client sudah ada. |
| §10 Follow System | **MODIFY (UI) / KEEP (API)** | API `GET/POST/DELETE /api/users/follows` lengkap dan idempoten. Yang hilang hanya UI: tombol follow/unfollow + daftar following + tidak ada notifikasi saat artist post update. |
| §11 Recommendation System | **MODIFY** | Scoring rule-based sudah ada di `EventService.GetRecommendedEventsAsync` (genre/lokasi/popularity/recency). Tapi **tidak dipakai Home**, dan `ViewsCount`/`SavesCount` **tidak pernah bertambah** sehingga bobot popularity selalu 0. |
| §12 Ticketing (ticket type) | **KEEP** | `EventTicketType` (Name/Price/Stock/SortOrder) + CRUD oleh EO/Admin. Spec menyebut Early Bird/Free/RSVP sebagai tipe — bisa dibuat sebagai *nama* tipe, kecuali RSVP yang butuh model sendiri. |
| §13 Digital Ticket | **MODIFY** | Alur beli → tiket → QR sudah jalan (`Ticket.QRCode` GUID, ditampilkan sebagai barcode via `TicketBarcodeDrawable`). **Tidak ada `TicketOrder`** (order = 1 tiket), dan status `CANCELLED`/`EXPIRED` tidak pernah dipakai. |
| §14 QR Check-in | **ADD** | Belum ada scanner, halaman check-in, atau dependensi barcode. Endpoint validasi ada tapi **belum aman** (lihat §6.1). |
| §15 RSVP / Reservation | **ADD** | Tidak ada model `RSVP`, tidak ada konsep event gratis tanpa tiket. Sesuai spec: opsional, jangan dikerjakan sebelum MVP stabil. |
| §16 Event Status | **MODIFY** | Existing: `Published / Draft / SoldOut / Completed` (`EventService.AllowedStatuses`). Spec minta `PENDING_REVIEW`, `ONGOING`, `CANCELLED`. `SoldOut` juga masih manual — tidak otomatis saat stok habis. |
| §17 Database Model | **KEEP + EXTEND** | Nama field berbeda dari spec (User.UserId vs Id, Event.Name vs Title, Event.CreatedBy vs OrganizerId, EventArtist.Order vs PerformanceOrder, Ticket.QRCode vs QRToken) — **jangan rename**, itu memicu migrasi massal tanpa manfaat. Tambahkan hanya entitas yang benar-benar belum ada (`RSVP`, `TicketOrder` bila diperlukan). |
| §18 API / Service Layer | **MODIFY** | Service layer sudah dipakai konsisten (EventService, TicketService, AuthService, VerificationService, NotificationService). Service yang belum ada: `RecommendationService` (masih di dalam EventService), `QRService`, `PaymentService`, `AdminService`. |
| §19 Struktur .NET MAUI | **KEEP** | Struktur existing (`Models/Views/ViewModels/Services/Helpers/Shells`) sudah rapi. **Jangan** migrasi ke struktur contoh spec (§19 sendiri melarangnya). |
| §20 UI Design Rules | **KEEP** | Dark-first, poster-led, sedikit badge — arah visual existing sudah sesuai. |
| §21 Existing UI Compatibility | **KEEP** | Prinsip ini yang dipakai untuk seluruh rekomendasi di dokumen ini. |
| §22 Migration Strategy | **MODIFY** | Fase 2 & 3 sebagian besar sudah tercapai; urutan di §8 disesuaikan. |
| §23 Admin Moderation | **MODIFY** | Sudah ada: verifikasi artist/EO, moderasi laporan (resolve/dismiss), hapus komentar. **Belum ada**: moderation queue event (`PENDING_REVIEW`), suspend akun, dan konfirmasi hapus untuk akun (event sudah pakai dialog konfirmasi ✅). |
| §24 Security Rules | **MODIFY** | Sudah benar: BCrypt, JWT, role di backend, EO hanya event miliknya (`CreatedBy`), harga diambil dari server saat beli. **Belum terpenuhi**: (a) kepemilikan tiket saat validate, (b) kredensial seed default, (c) race condition stok. Lihat §6. |
| §25 Notification | **MODIFY** | Model + endpoint notifikasi (list, unread-count, read, read-all) sudah ada. Trigger baru dari verifikasi/invitation/merch/report. **Belum ada trigger**: artist baru upload gig, saved event H-1, pembelian tiket sukses, perubahan jadwal. |
| §26 Future Features | **KEEP (dilarang dikerjakan)** | Konsisten dengan instruksi spec; jangan masuk MVP. |
| §27 MVP | **MODIFY** | Sekitar 80% sudah ada. Sisa: Follow UI, Following tab, check-in, attendee, admin CRUD akun, CRUD genre, filter harga. |
| §28 Definition of Done | **KEEP** | Sudah jadi standar de facto di kode (loading/empty/error state ada di hampir semua VM). |
| §29 Instruksi AI Agent | **KEEP** | Diikuti: audit dulu, tanpa rewrite, tanpa ganti framework. |
| §30 Product Direction & core loop | **KEEP** | Loop putus hanya di "Follow" → akan tersambung oleh item prioritas #1. |
| §31 Final Requirement | **KEEP** | — |

**REMOVE:** tidak ada. Fase audit ini tidak menemukan satu pun fitur existing yang perlu dihapus. Satu-satunya kandidat adalah kondisi mati di kode (bagian §6.4), dan itu diperbaiki, bukan dihapus fiturnya.

---

## 5. Gap Baru (ADD) — daftar lengkap

| # | Item | Layer | Perkiraan effort |
| :-- | :--- | :--- | :--- |
| A1 | Tombol Follow/Unfollow di `ArtistDetailPage` + state `IsFollowing` di model/first-load | Mobile (API sudah siap) | Kecil |
| A2 | Halaman/tab **Following** (pakai `GET /api/users/follows` + `GetFollowsAsync`) | Mobile | Kecil–sedang |
| A3 | Paket barcode scanner + halaman **Check-in** untuk EO (scan → `POST /api/tickets/validate` → tampilkan hasil + counter checked-in) | Mobile + API query | Sedang |
| A4 | Halaman **Attendees** EO (daftar pembeli per event) — butuh endpoint baru `GET /api/tickets/event/{id}/attendees` (EO/Admin, cek kepemilikan) | API + Mobile | Sedang |
| A5 | **Admin CRUD akun** (create/update/delete/suspend untuk User/Artist/EO) — endpoint + halaman | API + Mobile | Sedang–besar |
| A6 | **Admin CRUD Genre** (POST/PUT/DELETE `/api/genres`) | API + Mobile | Kecil |
| A7 | **Filter harga** di Explore (`Free / <50K / <100K / Custom`) | Mobile (+ service bila server-side) | Kecil |
| A8 | Blok **Artist Updates** di Home (§6) dari artist yang diikuti | Mobile (+ endpoint feed bila perlu) | Sedang |
| A9 | Model + alur **RSVP** (§15) | API + Mobile | Sedang, **opsional** |
| A10 | `TicketOrder` + status `Cancelled`/`Expired` (§13) | API + Mobile | Sedang, hanya bila butuh riwayat order multi-tiket |
| A11 | Status event `PendingReview` / `Ongoing` / `Cancelled` + moderation queue (§16/§23) | API + Mobile | Sedang |
| A12 | Trigger notifikasi §25 (artist update, H-1 saved event, tiket sukses) | API | Kecil–sedang |
| A13 | Endpoint `search`/`filter` server-side + paging untuk event/artist | API + Mobile | Sedang (lihat §6.5) |
| A14 | Tab **Music / Updates / Gigs** untuk Artist (§5) | Mobile (API sudah ada) | Sedang |
| A15 | Test `TicketService` + `GetRecommendedEventsAsync` | Test | Kecil |

---

## 6. Temuan Teknis & Risiko (di luar spec, wajib ditangani)

### 6.1 Kepemilikan tiket tidak divalidasi saat check-in (keamanan)

`TicketsController.ValidateTicket` (`GigRadarApi/Controllers/TicketsController.cs:110`, route di baris 108) hanya butuh `Roles = "EO,Admin"`, dan `TicketService.ValidateTicketAsync` (`GigRadarApi/Services/TicketService.cs:215`) mencari tiket **hanya berdasarkan `QRCode`** — tanpa memeriksa `EventId` terhadap event yang dikelola EO tersebut. Artinya EO mana pun yang punya akun terverifikasi dapat menandai tiket event EO lain sebagai `Used`.

Melanggar spec §24 ("EO hanya dapat mengelola event miliknya"). **Perbaikan:** kirim `eventId` dari client, validasi kepemilikan via `CanManageEventAsync` yang sudah ada, dan kembalikan detail tiket (nama pembeli, tipe) — bukan sekadar `valid: true`.

### 6.2 Kredensial default ter-seed ke semua environment

`AppDbContext.OnModelCreating` men-`HasData` tiga user dengan password diketahui (`admin@gigradar.com / admin123`, `eo123`, `artist123`), dan `Program.cs:110` menjalankan `db.Database.Migrate()` tanpa syarat environment. Setiap deployment baru otomatis punya akun Admin dengan password publik di source.

**Perbaikan:** batasi seed ke `IsDevelopment()`, atau paksa ganti password saat login pertama / ambil password awal dari environment variable.

### 6.3 Race condition stok tiket

`TicketService.PurchaseTicketAsync` (`TicketService.cs:24`, pengurangan stok di `TicketService.cs:61`) melakukan `type.Stock -= 1` lalu `SaveChangesAsync` tanpa transaksi eksplisit, concurrency token, atau pengecekan ulang. Dua pembelian bersamaan pada stok terakhir dapat lolos berdua (oversell). Harga sudah benar diambil dari server (bagus — spec §24 ✅).

**Perbaikan:** `RowVersion`/`xmin` pada `EventTicketType`, atau update atomik `UPDATE ... SET Stock = Stock - 1 WHERE Stock > 0` dan cek affected rows.

### 6.4 Logika mati / data statistik yang tidak pernah terisi

- `EventService.GetManagedSummaryAsync` (`EventService.cs:121`) memfilter `t.Status != "Cancelled"`, tapi `"Cancelled"` tidak ada di `AllowedStatuses` → kondisi tidak pernah berpengaruh.
- `Event.ViewsCount` dan `Event.SavesCount` **tidak pernah di-increment** di seluruh codebase (hanya di-set 0 saat create). Akibatnya skor `popularityScore` di `GetRecommendedEventsAsync` (`EventService.cs:78`) **selalu 0**, dan `SavesCount` jadi kolom mati padahal spec §11 mengandalkan sinyal popularitas/interaksi.
- `Event.Status = "SoldOut"` tidak pernah di-set otomatis ketika stok tipe tiket habis.

### 6.5 Discovery berjalan penuh di client

`ExploreViewModel.InitializeAsync` memanggil `GetEventsAsync`, `GetArtistsAsync`, `GetVenuesAsync`, `GetGenresAsync` sekaligus, lalu memfilter di memori; `ApiService` tidak punya dukungan paging atau parameter `q`. Dengan data demo ini aman, tapi ini akan jadi bottleneck begitu jumlah event bertambah. Spec §7 menginginkan search/filter — sebaiknya endpoint `GET /api/events?q=&genreId=&from=&to=&maxPrice=&lat=&lng=&radius=` ditambahkan **tanpa** menghapus filter client (sebagai fallback offline).

### 6.6 Ketidakkonsistenan kecil

- `ShellRouter.CreateForRole` (`Helpers/ShellRouter.cs`) memakai `switch` case-sensitive terhadap `"EO"`, `"Admin"`, `"Artist"`, sedangkan service backend lain membandingkan role dengan `OrdinalIgnoreCase`. Satu sumber nilai yang berubah case akan membuat user salah masuk shell.
- Belum ada test untuk `TicketService` (beli, stok, validasi QR, umur minimum) padahal ini jalur paling sensitif secara bisnis.

---

## 7. Koreksi yang Diusulkan untuk Spec V2

Spec V2 ditulis tanpa mengasumsikan keadaan kode. Beberapa bagian perlu diselaraskan agar tidak mendorong perubahan yang merusak:

1. **§17 Database Model** — jangan gunakan nama field di spec sebagai target rename. Pakai nama existing (`UserId`, `Name`, `CreatedBy`, `Order`, `QRCode`) dan tambahkan entitas baru saja.
2. **§16 Event Status** — tambahan `PENDING_REVIEW`, `ONGOING`, `CANCELLED` harus ditambahkan ke `AllowedStatuses` **dan** ke `GigEvent.StatusLabel/StatusColor/ShowsStatusBadge` di mobile + mapping warna, kalau tidak UI akan menampilkan label kosong.
3. **§12 Ticket type** — "Free" dan "RSVP" sebaiknya tidak dicampur ke `EventTicketType`. Free = harga 0 (mudah), RSVP = model sendiri (§15). Pisahkan agar tidak mengotori alur tiket berbayar.
4. **§13 Digital Ticket** — memperkenalkan `TicketOrder` berarti menyentuh `PurchaseTicketAsync`, `GetUserTicketsAsync`, halaman Checkout/Success, dan migrasi data tiket lama. Bila tidak ada kebutuhan multi-tiket per order, tunda.
5. **§15 RSVP** — spec sudah bilang opsional; audit menyarankan tetap opsional dan diletakkan **setelah** Follow + check-in selesai.
6. **Radar tidak disebut spec.** Tambahkan satu paragraf di §6 atau §7 agar fitur ini punya payung resmi dan tidak dianggap kandidat REMOVE.
7. **§5 Navigation** — daftar "Following" dan "Artist Updates" lebih baik diimplementasikan sebagai bagian dari Home/Profile (mengikuti struktur existing) daripada menambah tab baru yang memecah pola 6 tab saat ini.
8. **§18 Service Layer** — `RecommendationService` dan `QRService` sebaiknya diekstrak dari `EventService`/`TicketService` hanya ketika ukurannya sudah tidak nyaman; mengekstrak sekarang hanya menambah indirection tanpa menambah nilai.

---

## 8. Rencana Implementasi yang Disesuaikan

Urutan di bawah **berbeda** dari spec §22 karena Phase 2–3 largely done dan karena dua isu keamanan (§6.1, §6.2) harus lebih dulu.

### Phase 1b — Hardening & Konsistensi (prasyarat, kecil)
1. Perbaiki validasi kepemilikan tiket saat check-in (§6.1) + test.
2. Batasi seed kredensial default (§6.2).
3. Concurrency stok tiket (§6.3) + test `TicketService`.
4. Increment `ViewsCount` (detail event dibuka) dan `SavesCount` (toggle favorite) — sekaligus menghidupkan skor rekomendasi.
5. Normalisasi perbandingan role di `ShellRouter`.

### Phase 2b — Menutup Event/Artist Discovery (sesuai spec §6–§11)
6. Filter harga di Explore (A7) + opsi tanggal `Besok / Minggu ini`.
7. Home memakai `GET /api/events/recommended` (A: MODIFY §6).
8. Blok Artist Updates di Home (A8).
9. Tombol Follow/Unfollow + tab/daftar Following (A1, A2).
10. Tab Music/Updates/Gigs untuk Artist (A14) — API-nya sudah lengkap.

### Phase 3b — Ticketing & Check-in (spec §13–§14)
11. Tambah paket barcode scanner, halaman Check-in EO dengan counter `Checked In x / y` (A3).
12. Halaman Attendees + endpoint `GET /api/tickets/event/{id}/attendees` (A4).
13. Status tiket `Cancelled`/`Expired` (MODIFY §13). `TicketOrder` hanya bila diperlukan.

### Phase 4b — EO & Admin Completion (spec §4.4, §23)
14. Admin CRUD akun + suspend (A5), dengan dialog konfirmasi untuk operasi destruktif.
15. Admin CRUD Genre (A6).
16. Moderation queue event + status `PendingReview`/`Cancelled` (A11).
17. Analytics per event (MODIFY §4.3).

### Phase 5b — Server-side Discovery & Notifikasi (spec §7, §25)
18. Endpoint search/filter/paging (A13) — dipertahankan sebagai tambahan, filter client tetap fallback.
19. Trigger notifikasi artist update / tiket sukses / H-1 saved event (A12).

### Phase 6 — RSVP & Future (hanya bila MVP stabil)
20. RSVP (A9). Future features §26 tetap dilarang.

---

## 9. Checklist MVP (Spec §27) terhadap Kondisi Sekarang

| Fitur MVP | Status |
| :--- | :--- |
| Login/Register | ✅ |
| Home | ✅ (Recommended belum pakai endpoint rekomendasi) |
| Explore | ✅ |
| Search | ✅ client-side |
| Filter | ⚠️ tanpa filter harga, opsi tanggal terbatas |
| Event detail | ✅ |
| Artist profile | ✅ |
| **Follow artist** | ❌ API ada, UI tidak |
| Buy ticket | ✅ |
| My tickets | ✅ |
| QR ticket | ✅ (tampil sebagai barcode) |
| Profile | ✅ |
| Artist: Profile / Update / Music / Upcoming gigs | ⚠️ API & sebagian UI ada; tab Music/Updates/Gigs belum |
| EO: Create/Edit event, Ticket type, **Attendee**, **QR check-in** | ⚠️ dua item terakhir belum ada |
| Admin: Dashboard, User/Artist/EO **CRUD**, Event management, Ticket overview | ⚠️ CRUD akun belum ada (baru Read) |

---

## 10. Catatan Verifikasi

- Audit ini murni pembacaan kode; **tidak ada build atau test yang dijalankan** pada fase ini.
- Branch `feat/postgresql-migration` memiliki perubahan belum di-commit di 23 file (termasuk `Program.cs`, `EventsController`, `TicketsController`, `AuthService`) plus file baru (`OrganizerGate.cs`, `PendingVerificationBootstrap.cs`, `VerifiedOrganizerAuthorizationTests.cs`). Temuan di dokumen ini mencerminkan **working tree saat ini**, bukan `HEAD`.
- Karena itu, sebelum Phase 1b dijalankan, langkah pertama yang disarankan adalah build + test baseline untuk memastikan kondisi awal hijau.
