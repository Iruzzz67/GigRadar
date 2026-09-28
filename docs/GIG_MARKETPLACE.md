# Gig Marketplace — Gig ↔ Lamaran Artist

- **Sumber riset:** `RECLUB_RESEARCH_GIGRADAR.md` (Bab 5, 6, 19, 20, 28, 35, 40)
- **Branch:** `feat/postgresql-migration`
- **Tanggal:** 2026-09-28
- **Lingkup:** menyelesaikan **loop Gig (Artist ↔ EO)** — item P0 #4 dari daftar prioritas riset.
- **Prinsip:** *upgrade, don't rebuild.* Tidak ada tabel/kolom lama yang di-rename atau dihapus.

> **Ringkasan.** Percobaan sebelumnya ("Copilot") hanya menyelesaikan sisi EO:
> membuat gig dan melihat daftarnya. Artist belum punya cara menemukan gig,
> melamar, atau menarik lamaran; EO belum bisa melihat pelamar, mem-shortlist,
> atau menolak. Dokumen ini mencatat penyelesaian alur tersebut beserta
> aturan otorisasi, migrasi, UI, dan verifikasinya.

---

## 1. Kondisi Sebelum (hasil percobaan sebelumnya)

| Bagian | Status sebelum |
| :--- | :--- |
| `Gig` model + `GigsController` | ✅ ada (hanya `GET`/`POST`) |
| Migrasi `AddGigs` | ✅ ada |
| Halaman `EoGigsPage` (buat + daftar gig) | ✅ ada |
| Lifecycle status gig (DRAFT/OPEN/CLOSED) | ❌ hanya divalidasi saat *create* |
| Edit / hapus gig | ❌ belum ada |
| Penemuan gig untuk artist | ❌ belum ada |
| Model lamaran (`GigApplication`) | ❌ belum ada |
| Apply / withdraw / shortlist / reject | ❌ belum ada |
| Notifikasi lamaran | ❌ belum ada |
| Halaman gig untuk Artist | ❌ belum ada |

Baseline terverifikasi saat audit: API build 0 error, `GigRadarApi.Tests` 223/223 hijau.

---

## 2. Keputusan Lingkup

Daftar P0 riset §28 berisi 14 item. Yang dipilih untuk pass ini adalah
**menutup loop Gig secara end-to-end**, karena:

1. Item P0 lain (Artist Profile, EO Profile, Event, Discover, Invitation,
   Notifications) sudah ada di aplikasi sebelum riset ini;
2. `Gig` adalah objek yang *belum* punya penerus alur (lamaran) — tanpa
   lamaran, fitur gig tidak menghasilkan nilai marketplace apa pun;
3. Booking/Availability/Chat adalah lapisan berikutnya dan dibangun **di atas**
   lamaran, jadi urutannya harus setelah ini.

**Di luar lingkup (tetap belum ada):** Booking, Artist Availability, contextual
Chat, Waitlist, Contract, Invoice, Payment, Matching engine. Lihat §8.

---

## 3. Model Domain

### 3.1 `Gig` (diperluas)

| Field | Catatan |
| :--- | :--- |
| `GigId`, `EventId` | gig selalu milik satu event EO |
| `Name`, `Requirements` | teks bebas, dibatasi panjang |
| `StartAt`, `EndAt` | **wajib di dalam jendela `Event.StartDate`–`EndDate`** |
| `VenueId`, `GenreId` | otomatis disalin dari event, bukan input client |
| `BudgetMin`, `BudgetMax` | `Min ≤ Max`, tidak boleh negatif |
| `RequiredArtistCount` | 1–20 |
| `Status` | `DRAFT` \| `OPEN` \| `CLOSED` |
| `CreatedAt`, **`UpdatedAt`** | `UpdatedAt` baru (migrasi `AddGigApplications`) |
| `Event`, `Venue`, `Genre` | navigasi |
| **`Applications`** | navigasi baru ke `GigApplication` |

### 3.2 `GigApplication` (baru)

| Field | Catatan |
| :--- | :--- |
| `GigApplicationId` | PK |
| `GigId`, `ArtistId` | **unique index `(GigId, ArtistId)`** — satu artist satu lamaran per gig |
| `Message` | pesan perkenalan, opsional, ≤ 1000 karakter |
| `Status` | `PENDING` \| `SHORTLISTED` \| `REJECTED` \| `WITHDRAWN` |
| `CreatedAt`, `UpdatedAt`, `RespondedAt` | `RespondedAt` = waktu diputuskan/ditarik |
| `Artist` | navigasi (dipakai EO untuk melihat pelamar) |

`Event.Gigs` ditambahkan sebagai navigasi dan di-`[JsonIgnore]`.
Status disimpan sebagai teks (bukan enum) mengikuti konvensi existing
(`Event.Status`, `EventInvitation.Status`, `Ticket.Status`).

---

## 4. State Machine (divalidasi server-side)

Riset §7 dan §40 eksplisit meminta transisi eksplisit, bukan boolean tunggal.

### 4.1 Status gig

```text
DRAFT  → OPEN
OPEN   → CLOSED
CLOSED → OPEN
       (status yang sama DITOLAK — bukan transisi)
```

Implementasi: `GigService.AllowedTransitions`, dikonsumsi oleh
`GigService.ChangeStatusAsync` dan `GigService.UpdateAsync`. Pesan error
menyebut status asal dan tujuan agar bisa ditindaklanjuti.

### 4.2 Status lamaran

```text
PENDING ──shortlist──→ SHORTLISTED ──reject──→ REJECTED   (terminal)
   │  └──────────────reject──────────────────→ REJECTED   (terminal)
   └─────────────withdraw───→ WITHDRAWN                   (terminal)
```

| Aksi | Status asal yang diizinkan | Actor |
| :--- | :--- | :--- |
| `shortlist` | `PENDING` saja | EO pemilik event / Admin |
| `reject` | `PENDING`, `SHORTLISTED` | EO pemilik event / Admin |
| `withdraw` | `PENDING`, `SHORTLISTED` | Artist **pemilik lamaran** |
| apply ulang | `WITHDRAWN` saja | Artist pemilik lamaran |

Lamaran bersifat **terminal**: `REJECTED` dan `WITHDRAWN` tidak bisa diubah lagi.
Apply ulang pada lamaran `WITHDRAWN` **tidak** membuat baris baru — baris lama
dibuka kembali, sehingga unique index `(GigId, ArtistId)` tetap terjaga.

---

## 5. API

Semua route memakai konvensi existing (`[Route("api")]`, `[Authorize]`,
`ApiAuthorizationResultHandler`, policy `VerifiedOrganizer`).

| Method | Route | Role | Catatan |
| :--- | :--- | :--- | :--- |
| GET | `/api/events/{id}/gigs` | EO, Admin | kepemilikan event dicek; sertakan `applicantCount` |
| POST | `/api/events/{id}/gigs` | EO, Admin + `VerifiedOrganizer` | |
| PUT | `/api/events/{id}/gigs/{gigId}` | EO, Admin + `VerifiedOrganizer` | status ikut divalidasi transisinya |
| POST | `/api/events/{id}/gigs/{gigId}/status` | EO, Admin + `VerifiedOrganizer` | lifecycle DRAFT/OPEN/CLOSED |
| DELETE | `/api/events/{id}/gigs/{gigId}` | EO, Admin + `VerifiedOrganizer` | lamaran ikut terhapus |
| GET | `/api/gigs` | terautentikasi | Discover: filter `q`, `genreId`, `maxBudget`, `city` |
| POST | `/api/gigs/{gigId}/applications` | Artist | status `201` |
| GET | `/api/applications/me` | Artist | lamaran milik sendiri |
| POST | `/api/applications/{id}/withdraw` | Artist | pemilik lamaran saja |
| GET | `/api/gigs/{gigId}/applications` | EO, Admin | pelamar + profil artist |
| POST | `/api/applications/{id}/shortlist` | EO, Admin | |
| POST | `/api/applications/{id}/reject` | EO, Admin | |

**Kontrak response bertipe** (bukan anonymous object) agar stabil dan bisa
diverifikasi test: `GigResponse` (`GigRadarApi/Services/GigService.cs`) dan
`GigApplicationResponse` (`GigRadarApi/Controllers/GigsController.cs`).

### 5.1 Aturan otorisasi yang ditegakkan server-side

- EO hanya bisa mengelola gig pada event dengan `CreatedBy == UserId`; Admin bebas.
- Artist hanya bisa menarik lamaran miliknya sendiri (`application.ArtistId`).
- Artist hanya bisa melamar bila: punya profil artist, gig `OPEN`, dan gig belum lewat.
- Lamaran duplikat ditolak; lamaran `WITHDRAWN` dibuka kembali.
- EO tidak bisa memutuskan lamaran pada event EO lain (diuji).
- **Klaim client tidak pernah dipercaya:** `eventId`, `gigId`, `applicationId`
  selalu diverifikasi ulang terhadap pemilik di server.

---

## 6. Notifikasi

`NotificationService.AllowedTypes` diperluas dengan dua tipe baru (tipe di luar
daftar akan jatuh ke `ADMIN_NOTICE`, jadi tipe harus terdaftar):

| Tipe | Penerima | Pemicu |
| :--- | :--- | :--- |
| `GIG_APPLICATION` | EO pemilik event | artist melamar |
| `GIG_APPLICATION_UPDATE` | Artist (bila punya akun) & EO | shortlist / reject / withdraw |

Deep-link memakai pola existing: `events/{eventId}/gigs`.

---

## 7. UI (MAUI)

### 7.1 EO — `EoGigsPage`

- Form buat gig (sudah ada) + validasi ganda (client & server).
- Kartu gig kini menampilkan jumlah pelamar dan slot, dengan tiga aksi:
  **Pelamar**, **Tutup/Buka lamaran**, **Hapus**.
- Panel **lamaran masuk** untuk gig terpilih: nama artist, genre/kota, pesan,
  status badge, tombol **Shortlist** dan **Tolak**.
- Semua aksi destruktif memakai dialog konfirmasi (`Alerts.ConfirmAsync`).

### 7.2 Artist — `ArtistGigsPage` (baru, tab **Gigs** di `ArtistShell`)

- Filter: kata kunci (gig/event/venue/kota) + picker genre + pesan lamaran opsional.
- Daftar gig terbuka dengan jadwal, lokasi, budget, jumlah pelamar; tombol
  **Lamar gig** hanya aktif bila `OPEN` dan belum pernah melamar
  (label berubah menjadi *Sudah melamar* / *Ditutup*).
- Seksi **Lamaran saya**: status tiap lamaran + tombol **Tarik lamaran**
  (hanya untuk `PENDING`/`SHORTLISTED`).
- `RefreshView` untuk muat ulang, plus empty state yang menjelaskan langkah
  berikutnya (riset §23).

Picker genre memakai konvensi aplikasi existing: `ItemsSource` = daftar
**string** (`GenreNames`), bukan `ItemDisplayBinding`.

---

## 8. Belum Dikerjakan (rekomendasi urutan berikutnya)

| # | Item | Alasan urutan |
| :-- | :--- | :--- |
| 1 | **Booking** (`Proposed → Negotiating → Confirmed → Completed`, terminal `Cancelled`) | lanjutan langsung dari `SHORTLISTED` |
| 2 | **Artist Availability** (`Available/Tentative/Booked/Unavailable`) | dibutuhkan matching & pencegahan bentrok jadwal |
| 3 | **Contextual Chat** (percakapan terhubung `Booking`/`Application`) | riset §11 melarang chat generik |
| 4 | Undangan **ke gig** (saat ini undangan hanya level event) | melengkapi arah EO → Artist |
| 5 | Waitlist ketika slot gig penuh | P1 riset §7 |
| 6 | Reviews two-sided setelah `COMPLETED` | P1 riset §16 |
| 7 | Matching engine deterministik + penjelasan | P3 riset §31 |

---

## 9. Verifikasi

| Perintah | Hasil |
| :--- | :--- |
| `dotnet build GigRadarApi/GigRadarApi.csproj` | Build succeeded, 0 error |
| `dotnet test GigRadarApi.Tests` | **245/245 lulus** (sebelumnya 223) |
| `dotnet test GigRadarMobile.Tests` | **96/96 lulus** (sebelumnya 78) |
| `dotnet build GigRadarMobile -f net10.0-windows10.0.19041.0` | Build succeeded |
| `dotnet build GigRadarMobile -f net10.0-android` | Build succeeded |

### 9.1 Test baru

**`GigRadarApi.Tests/GigApplicationServiceTests.cs`** — alur lamaran & invariant:

- apply menyimpan `PENDING` + mengirim notifikasi ke EO;
- apply ditolak untuk gig `DRAFT`/`CLOSED` dan gig yang sudah lewat;
- apply duplikat ditolak; apply ulang setelah `WITHDRAWN` membuka baris yang sama;
- user tanpa profil artist tidak bisa melamar;
- artist lain tidak bisa menarik lamaran (403);
- lamaran yang sudah diputuskan tidak bisa ditarik;
- shortlist hanya dari `PENDING`; reject boleh dari `PENDING`/`SHORTLISTED`;
- EO lain tidak bisa memutuskan; Admin bisa;
- daftar pelamar hanya untuk pemilik event.

**`GigRadarApi.Tests/GigServiceTests.cs`** (diperluas) — lifecycle & discover:

- transisi `DRAFT→CLOSED` ditolak, `DRAFT→OPEN`, `OPEN→CLOSED`, `CLOSED→OPEN` diterima, status sama ditolak, status tak dikenal ditolak;
- perubahan status oleh EO lain ditolak;
- update memvalidasi jendela event dan tidak mengubah data saat ditolak;
- hapus gig menghapus lamarannya;
- discover menyaring `DRAFT`/`CLOSED`/gig lampau;
- discover menyaring `q`/`city`/`genreId`/`maxBudget` serta menandai `hasApplied` + `applicantCount`.

**`GigRadarMobile.Tests/GigMarketplaceTests.cs`** — aturan kelayakan UI yang
harus sama dengan validasi server (tombol Lamar/Shortlist/Tolak/Tarik, label status).

### 9.2 Catatan migrasi

Migrasi `20260928030800_AddGigApplications` ditulis hanya berisi perubahan
skema nyata (kolom `Gigs.UpdatedAt` + tabel `GigApplications` + index).
Blok `UpdateData` seed dihapus secara sengaja: `HasData` di `AppDbContext`
memakai `DateTime.UtcNow` dan `BCrypt.HashPassword` (acak), sehingga setiap
`migrations add` menghasilkan pembaruan seed palsu. Pola ini sama dengan
migrasi `AddGigs` sebelumnya.

> **Saran lanjutan (belum dikerjakan):** jadikan seed deterministik
> (konstanta `CreatedAt` + hash BCrypt tetap) supaya snapshot EF tidak lagi
> berubah-ubah di tiap migrasi.

---

## 10. Berkas yang Berubah

**Backend**

```text
GigRadarApi/Models/Gig.cs                      Gig + GigApplication
GigRadarApi/Data/AppDbContext.cs               DbSet + unique index
GigRadarApi/Services/GigService.cs             lifecycle, update/delete, discover, GigResponse
GigRadarApi/Services/GigApplicationService.cs  BARU — apply/withdraw/decide
GigRadarApi/Controllers/GigsController.cs      semua endpoint + GigApplicationResponse
GigRadarApi/Services/NotificationService.cs    +2 tipe notifikasi
GigRadarApi/Program.cs                         registrasi GigApplicationService
GigRadarApi/Services/EventService.cs           hapus gig saat event dihapus
GigRadarApi/Models/Event.cs                    navigasi Gigs
GigRadarApi/Migrations/…_AddGigApplications.cs BARU
```

**Mobile**

```text
GigRadarMobile/Models/GigItem.cs               diperluas (event, kota, pelamar, kelayakan)
GigRadarMobile/Models/GigApplication.cs        BARU
GigRadarMobile/Services/ApiService.cs          +9 method marketplace
GigRadarMobile/ViewModels/EoGigsViewModel.cs   tinjau lamaran, status, hapus
GigRadarMobile/Views/EoGigsPage.xaml           aksi per gig + panel lamaran
GigRadarMobile/ViewModels/ArtistGigsViewModel.cs  BARU
GigRadarMobile/Views/ArtistGigsPage.xaml(.cs)     BARU
GigRadarMobile/Shells/ArtistShell.xaml         tab Gigs
GigRadarMobile/Shells/Routes.cs                route halaman
GigRadarMobile/MauiProgram.cs                  registrasi DI
```

**Test & dokumen**

```text
GigRadarApi.Tests/GigServiceTests.cs
GigRadarApi.Tests/GigApplicationServiceTests.cs
GigRadarMobile.Tests/GigMarketplaceTests.cs
docs/GIG_MARKETPLACE.md (dokumen ini)
```

---

## 11. Catatan Keamanan

- Tidak ada password/token/secret yang ditambahkan ke kode atau dokumen.
- Semua mutasi memeriksa kepemilikan di server (`CreatedBy`, `ArtistId`).
- Policy `VerifiedOrganizer` tetap menjadi gate aksi tulis gig oleh EO.
- Endpoint Discover hanya mengembalikan gig `OPEN` pada event yang belum lewat,
  dan hanya nama kota/venue publik — bukan alamat privat.
- Pesan lamaran dibatasi 1000 karakter untuk mencegah penyalahgunaan payload.
