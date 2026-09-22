# Artwork band — cara mengganti dengan foto asli

Aplikasi TIDAK mengunduh foto/logo band berhak cipta. Sebagai gantinya, saat
sebuah artist/event belum punya gambar, aplikasi memakai salah satu dari 8
artwork vektor orisinal di folder ini (lihat `Helpers/BandArt.cs`).

Pemilihan bersifat **deterministik**: artwork sebuah artist dihitung dari
namanya (FNV-1a), jadi tidak berubah-ubah tiap kali halaman dibuka, dan tiap
artist/event mendapat visual yang berbeda.

## Daftar artwork default

| File | Gaya |
|---|---|
| `art_aurora.svg`   | veil violet–cyan, orb |
| `art_ember.svg`    | sunburst pink–amber |
| `art_haze.svg`     | orb blur hangat (shoegaze) |
| `art_grid.svg`     | retro grid cyan–magenta |
| `art_monolith.svg` | slab brutalist lime |
| `art_prism.svg`    | berkas prismatik |
| `art_ripple.svg`   | cincin konsentris |
| `art_noise.svg`    | halftone dots amber–pink |

## Cara menempel foto band asli

Ada dua cara, pilih salah satu.

### 1. Lewat aplikasi (tanpa rebuild) — paling mudah

Buka profil artist yang login → **Edit profil artist** → isi kolom
**URL foto profil**. Unggah poster event juga lewat **Create Event → Pilih foto
poster**. URL dari API selalu diprioritaskan di atas artwork default.

### 2. Ganti langsung file artwork (butuh rebuild)

1. Siapkan foto dengan sisi ≥ 800×800 px (disarankan 1200×1200, rasio persegi).
2. Taruh di folder ini dengan **nama dasar yang sama** seperti salah satu file
   di tabel di atas, mis. `art_haze.jpg`.
3. **Hapus** file `.svg` dengan nama yang sama (`art_haze.svg`) — kalau tidak,
   dua file menghasilkan nama resource yang sama dan build akan bentrok.
4. Rebuild: `dotnet build GigRadarMobile/GigRadarMobile.csproj -f net10.0-android`.

> Artwork default hanya menempati peran "foto belum tersedia". Begitu
> artist/event punya URL gambar sendiri, artwork otomatis tidak dipakai.

## Catatan hukum

Foto, logo, dan nama band nyata adalah merek/cipta pihak lain. Jangan
mendistribusikan aset band tanpa izin. Artwork di folder ini dibuat orisinal
untuk proyek ini sehingga aman dipakai dan dimodifikasi.
