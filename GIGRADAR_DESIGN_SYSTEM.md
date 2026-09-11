# GIGRADAR Design System
## Menghubungkan skena, menemukan suara lokal.

Dokumen ini adalah hasil *design architecture pass* untuk redesign GIGRADAR sebagai produk
musik lokal yang nyata — bukan template SaaS generik. Semua keputusan di sini diterapkan di
**.NET MAUI (mobile-first)** dengan backend API yang sudah ada, dan konsep "Three.js" pada
brief diterjemahkan menjadi visualisasi native yang hemat daya (`GraphicsView`), karena
produk ini bukan aplikasi web.

---

## 1. Prinsip Desain

1. **Tipografi adalah identitas.** Heading memakai Space Grotesk (geometris, tegas, khas
   produk musik/tekno), body memakai Inter. Tidak ada font "startup AI" generik.
2. **Imagery editorial.** Poster event dan foto artist dipakai besar. Bukan grid kartu identik.
3. **Hierarki, bukan dekorasi.** Setiap elemen menjawab "kenapa ini ada?". Kalau jawabannya
   hanya "biar keren" — hapus.
4. **Densitas yang disengaja.** Informasi padat saat berguna (daftar event, admin),
   lapang saat membaca (hero, artist profile).
5. **Kontras kuat.** Dark theme zinc + aksen "signal lime" yang dipakai strategis,
   bukan neon di mana-mana.
6. **Motion punya fungsi.** Sweep radar, transisi halaman, konfirmasi tiket.
   Tidak ada floating/partikel acak.
7. **Aksesibilitas & performa.** Kontras cukup, target sentuh ≥ 44px, dukungan reduced
   motion, dan visualisasi radar berhenti saat tidak terlihat.

---

## 2. Analisis Produk & Role

| Role | Kebutuhan inti | "Rumah" di aplikasi |
|---|---|---|
| **User / pendengar** | Discover gig, radar sekitar, detail event/artist/venue, beli tiket, tiket digital, profil | `UserShell` — tab Discover / Radar / Tickets / Profile |
| **Artist** | Profil artist, dashboard, jadwal gig, musik, audience | `ArtistShell` |
| **Event Organizer** | Buat/kelola event, tipe tiket, stok, analitik penjualan | `EOShell` |
| **Admin** | Moderasi user/artist/EO/event, laporan, statistik sistem | `AdminShell` |

Satu bahasa visual, empat varian navigasi. Role non-User memakai token yang sama tapi
layout dashboard yang *information-dense* dan tidak dekoratif.

---

## 3. Information Architecture

```
GIGRADAR
├── Auth
│   ├── Login / Register
│   └── Onboarding (pilih genre → kota/lokasi)
├── USER SHELL (4 tab)
│   ├── Discover (Home)
│   │   ├── Hero "Malam ini di {kota}" + event unggulan
│   │   ├── Dekat kamu (horizontal)
│   │   ├── Radar preview → buka tab Radar
│   │   ├── Artist untuk kamu (editorial)
│   │   ├── Minggu ini (list tanggal)
│   │   └── Genre chips → filter Radar
│   ├── Radar
│   │   ├── Visualisasi node event di sekitar user
│   │   ├── Filter: genre · radius · "malam ini"
│   │   ├── Preview event terpilih → EventDetail
│   │   └── List "Gig terdekat" + buka Google Maps
│   ├── Tickets (My Tickets)
│   │   └── Ticket → digital ticket
│   └── Profile
│       ├── Data diri, edit kota
│       ├── Favorit
│       └── (EO/Admin) pintu dashboard
├── USER FLOW UTAMA (route global)
│   EventDetail → TicketSelection → Checkout → TicketSuccess
│   ArtistDetail · VenueDetail (via maps)
├── ARTIST SHELL — Overview · Events · Create Gig · Audience · Profile
├── EO SHELL — Overview · Events · Create Event · Tickets · Venues · Analytics · Settings
└── ADMIN SHELL — Users · Artists · Organizers · Events · Reports · Moderation
```

Route navigasi (`Shells/Routes.cs`) tetap: `EventDetailPage`, `ArtistDetailPage`,
`TicketSelectionPage`, `CheckoutPage`, `TicketSuccessPage`, dst.

---

## 4. User Flows

**Flow utama user:**
```
Onboarding (genre) → Home
  → Discover: hero/nearby/radar preview
  → Radar: tap node → preview → EventDetail
  → EventDetail: lineup → artist; venue → maps; beli → tiket
  → TicketSelection (pilih tier) → Checkout (data diri, umur ≥ 17) → TicketSuccess (digital ticket)
  → Tickets tab (riwayat, barcode)
```

**Flow EO:** Buat Event → kelola tipe tiket/stok → publikasi → pantau penjualan → validasi QR.
**Flow Admin:** moderasi konten/report, kelola user/EO/artist, statistik sistem.

---

## 5. Visual Language

- **Mood:** malam, skena, panggung kecil, peta kota. Zinc gelap (#0A0A0B) bukan hitam pekat.
- **Aksen "signal lime"** `#A3FF12` — metafora radar GIGRADAR. Dipakai untuk: CTA utama,
  angka kunci (harga, jarak), titik user di radar, status "aktif". Tidak dipakai untuk border
  dekoratif.
- **Genre punya warna.** Warna genre (lihat token) dipakai konsisten di radar node,
  chip genre, dan label. Bukan pelangi neon — palet redup yang tetap bisa dibedakan.
- **Sudut:** 14px untuk kartu, 8px elemen kecil, 999px hanya untuk avatar/chip pill.
  Tidak ada "rounded rectangle berlebihan".
- **Shadow:** hampir tidak ada. Hierarki dibuat lewat warna permukaan (surface/elevated)
  dan border tipis.
- **Tipografi:** display Space Grotesk (hero, nama event, angka), body Inter.
  Label seksi memakai *eyebrow* uppercase + letter-spacing (gaya editorial).

---

## 6. Design Tokens

### Warna

| Token | Value | Pemakaian |
|---|---|---|
| `BackgroundColor` / Ink | `#0A0A0B` | latar aplikasi |
| `SurfaceColor` | `#131316` | kartu, seksi |
| `CardColor` | `#18181B` | kartu di atas surface |
| `CardElevatedColor` | `#1E1E23` | elemen terangkat, input |
| `BorderColor` | `#26262C` | border default |
| `BorderStrong` | `#34343C` | border fokus/interaktif |
| `TextPrimary` | `#F4F4F5` | teks utama |
| `TextSecondary` | `#A1A1AA` | teks pendukung |
| `TextMuted` | `#71717A` | metadata, placeholder |
| `PrimaryColor` / Accent | `#A3FF12` | aksen "signal lime" |
| `AccentSoft` | `#1C2A0D` | latar tint aksen |
| `OnAccent` | `#0A0A0B` | teks di atas aksen |
| `DangerColor` | `#F43F5E` | error, tiket habis |
| `WarningColor` | `#F59E0B` | peringatan, stok menipis |
| `SuccessColor` | `#22C55E` | sukses, tiket active |

### Warna genre (radar node + chip)

| Genre | Warna |
|---|---|
| Indie | `#F4F4F5` |
| Alternative | `#A3FF12` |
| Rock | `#E4572E` |
| Metal | `#9CA3AF` |
| Punk | `#E11D48` |
| Hardcore | `#EF476F` |
| Shoegaze | `#7DD3FC` |
| Emo | `#C084FC` |
| Jazz | `#F2C14E` |
| Folk | `#D9A05B` |
| Electronic | `#2DD4BF` |
| Pop | `#F472B6` |
| Hip-Hop | `#A78BFA` |
| default | `#A1A1AA` |

### Tipografi

| Style | Font | Ukuran | Pemakaian |
|---|---|---|---|
| Display | SpaceGroteskBold | 34 | hero, nama event besar |
| Title | SpaceGroteskBold | 26–28 | judul halaman |
| Heading | SpaceGroteskMedium | 20–22 | judul seksi/event |
| Subheading | InterSemiBold | 17 | subjudul, kartu utama |
| Body | InterRegular | 14–15 | teks isi |
| Caption | InterMedium | 12–13 | metadata |
| Meta | InterMedium | 10–11 | eyebrow, label kecil (letter-spacing 1.2) |
| Numeric | SpaceGroteskMedium | bervariasi | harga, jarak, angka statistik |

### Spasi & radius

- Basis **4px**: 4 · 8 · 12 · 16 · 20 · 24 · 32 · 40 · 48 · 64.
- Padding halaman mobile **20px**; antar seksi **28–32px**; desktop 24–48px.
- Radius: kartu **14**, input/button **12**, kecil **8**, pill **999**.

### Motion

- Radar sweep: rotasi ~8°/frame saat tab aktif, **berhenti saat tab tidak terlihat**
  dan saat `reduced motion` aktif (sweep diganti cincin statis).
- Transisi halaman: default platform (tanpa animasi custom yang mengganggu).
- Konfirmasi tiket: check-in brief (opacity/scale sekali).
- Tidak ada parallax, floating, atau partikel acak.

---

## 7. Komponen & State

| Komponen | Elemen | State |
|---|---|---|
| Button | Primary (lime, teks gelap), Outline, Ghost, Danger, Small, Buy (sticky) | default · pressed · disabled · loading |
| Chip | genre filter, pill | default · selected (lime) |
| Card | Frame `Card` style | default · pressed (opacity) |
| EventCard (hero) | poster besar + tanggal + nama + venue + harga | — |
| EventRow | blok tanggal (day/month) + info + harga | — |
| ArtistCard | foto portrait + nama + genre + kota | — |
| Input | Entry/Editor dengan `SurfaceColor` box | default · focus · error |
| SectionHeader | eyebrow + judul + aksi "Lihat semua" | — |
| TicketCard | digital ticket + barcode + status | Active/Used |
| Empty/Loading/Error | state teks + aksi | — |

A11y: target ≥ 44px, label semantik untuk GraphicsView (radar punya fallback list),
focus state terlihat (border `BorderStrong`), kontras ≥ 4.5:1 untuk teks.

---

## 8. Radar — Spesifikasi Visualisasi Native

Brief meminta pengalaman Three.js; karena produk adalah .NET MAUI, konsep diterjemahkan
ke **`GraphicsView` + `IDrawable`** yang ringan dan lintas platform:

| Konsep brief (Three.js) | Implementasi MAUI |
|---|---|
| Globe 3D yang bisa dirotasi | Radar planar 2D — rotasi tidak relevan di mobile; zoom = pilih radius (10/25/50 km) |
| Radar pulses di kota aktif | **Sweep line** berputar + cincin radius berlabel (jarak km) |
| Event sebagai titik/node | Node bulat; **radius node ∝ popularitas** (views/saves), **warna = genre** |
| Posisi berdasar jarak | Haversine dari titik user → koordinat polar (jarak → radius, bearing → sudut) |
| Tap node → preview | `StartInteraction` → hit-test node → kartu preview di bawah radar |
| Filter dinamis | Chip genre + toggle "Malam ini" + pilihan radius → redraw node |
| Progressive enhancement / fallback | Jika list node kosong → empty state; radar **tetap punya list** "Gig terdekat" sebagai akses utama (radar tidak pernah jadi satu-satunya cara akses info) |
| Performa (lazy, pause, DPR, disposal) | Render hanya saat tab aktif; `Invalidate()` per frame sweep; timer dihentikan saat `OnDisappearing`/`IsVisible=false`; `DeviceDisplay.MainDisplayInfo.Density` dipakai untuk ukuran minimum node; **reduced motion → sweep mati** |

Node yang dipilih diberi cincin + label nama event di atasnya. Jarak antar node dipakai
secara visual (bukan presisi peta) — akurasi tetap di list dan Google Maps.

---

## 9. Spesifikasi Layar (ringkas)

- **Login**: wordmark besar, form kotak, CTA lime. Tanpa hero stock photo.
- **Onboarding**: "Pilih yang kamu dengar" — grid genre (pill besar, icon), lalu pilih kota.
- **Discover (Home)**: header (wordmark + lokasi + avatar) → hero malam ini → dekat kamu
  (horizontal) → preview radar → artist untuk kamu → minggu ini (list tanggal) → genre chips.
- **Explore**: search event/artis/venue (debounce), filter genre · tanggal · radius, **switch view
  List / Grid / Radar** (Radar = visualisasi node + preview, sama seperti tab Radar).
- **Radar**: header + filter + radar + preview + list terdekat.
- **ArtistDetail**: hero cover editorial (scrim), statistik (pengikut/track/album), bio,
  gig mendatang (dari API), track dengan preview, personel.
- **EventDetail**: hero poster (scrim), genre chip + status, nama display, meta baris
  (tanggal/jam/venue/jarak), deskripsi, LINEUP (row artist → detail), TIKET (tier + stok),
  venue block, **sticky buy bar**.
- **TicketSelection**: tier sebagai row radio (nama, deskripsi, harga, sisa stok, SOLD OUT).
- **Checkout**: ringkasan pesanan + form data diri; verifikasi inline.
- **TicketSuccess**: digital ticket — perforasi (notch + dashed), event, tanggal, venue,
  tier, atas nama, barcode, kode QR, status "ACTIVE".
- **Artist/EO/Admin**: token sama, layout dense, tanpa dekorasi.

---

## 10. Arsitektur Komponen

```
GigRadarMobile/
├── Helpers/
│   ├── GeoHelper.cs            # Haversine, bearing, format jarak
│   ├── GenreColors.cs          # map genre → warna
│   ├── RadarDrawable.cs        # IDrawable radar (rings, sweep, node)
│   ├── RadarState.cs           # filter lintas-tab (genre chip Home → Radar)
│   └── ReducedMotion.cs        # deteksi reduced motion per platform
├── ViewModels/  RadarViewModel (ex-MapViewModel), HomeViewModel, EventDetailViewModel, ...
├── Views/       RadarPage (ex-MapPage), HomePage, EventDetailPage, TicketSuccessPage, ...
└── Shells/      UserShell (tab Discover | Radar | Tickets | Profile), EOShell, ArtistShell, AdminShell
```

Logika tetap di ViewModel/Service; halaman hanya konsumsi.

---

## 11. Status Implementasi (sesi ini)

- ✅ Tokens warna + tipografi (Space Grotesk + Inter) + komponen style global
- ✅ HomePage redesign (hero, nearby, radar preview, artist, minggu ini, genre chips)
- ✅ RadarPage + RadarDrawable + filter + preview + fallback list
- ✅ EventDetailPage redesign (hero editorial, lineup, sticky buy)
- ✅ Alur tiket (Selection/Checkout/Success/Tickets) redesign
- ✅ ExplorePage baru — search + filter + switch List/Grid/Radar (tab ke-2 UserShell)
- ✅ ArtistDetailPage editorial (cover hero, statistik, gig mendatang via API, track preview)
- ✅ Polish Login/Onboarding/Profile
- ✅ Pass token & style konsistensi EO/Admin (EoProfile, EoEvents, EoAnalytics, ManageTickets,
  CreateEvent, Users) — emoji/aksen lama diganti token + tombol baru

Review checklist: build Windows ✅ · kontras ✅ · radar berhenti saat tab tersembunyi ✅ ·
reduced motion ✅ · target sentuh ≥ 44px ✅ · tidak ada dekorasi tanpa fungsi ✅.