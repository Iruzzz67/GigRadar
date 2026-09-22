# ============================================================
# GigRadar - Build Release (exe all-in-one)
#
# Menghasilkan folder distribusi siap pakai:
#
#   dist\GigRadar\
#     GigRadar.exe              <- KLIK INI (launcher self-contained)
#     gigradar.config.json      <- koneksi DB & opsi (opsional diedit)
#     api\GigRadarApi.exe       <- backend, dijalankan otomatis oleh launcher
#     app\GigRadarMobile.exe    <- aplikasi desktop, dibuka otomatis
#
# Plus arsip: dist\GigRadar-win-x64.zip
#
# Pemakaian:
#   powershell -NoProfile -ExecutionPolicy Bypass -File scripts\build-release.ps1
#   ... -SkipApp     : hanya launcher + API (jauh lebih cepat, untuk uji cepat)
#   ... -NoZip       : jangan buat file zip
# ============================================================
param(
    [string]$OutputDir = "dist",
    [switch]$SkipApp,
    [switch]$NoZip
)

$ErrorActionPreference = "Stop"
$env:MSBUILDDISABLENODEREUSE = "1"

$Root       = Split-Path -Parent $PSScriptRoot
$Rid        = "win-x64"
$DistRoot   = Join-Path $Root $OutputDir
$OutDir     = Join-Path $DistRoot "GigRadar"
$ApiOut     = Join-Path $OutDir "api"
$AppOut     = Join-Path $OutDir "app"

$LauncherCsproj = Join-Path $Root "GigRadarLauncher\GigRadarLauncher\GigRadarLauncher.csproj"
$ApiCsproj      = Join-Path $Root "GigRadarApi\GigRadarApi.csproj"
$AppCsproj      = Join-Path $Root "GigRadarMobile\GigRadarMobile.csproj"
$AppTfm         = "net10.0-windows10.0.19041.0"

function Step($text) {
    Write-Host ""
    Write-Host "=== $text ===" -ForegroundColor Cyan
}

function Assert-Ok($what) {
    if ($LASTEXITCODE -ne 0) {
        Write-Host "[x] $what GAGAL (exit $LASTEXITCODE)" -ForegroundColor Red
        exit $LASTEXITCODE
    }
}

# ------------------------------------------------------------
# 0. Bersihkan hasil lama
# ------------------------------------------------------------
Step "Bersihkan $OutDir"
if (Test-Path $OutDir) { Remove-Item $OutDir -Recurse -Force }
New-Item -ItemType Directory -Force -Path $OutDir | Out-Null

# ------------------------------------------------------------
# 1. Launcher -> GigRadar.exe (single file, self-contained)
# ------------------------------------------------------------
Step "Publish launcher (GigRadar.exe)"
dotnet publish $LauncherCsproj `
    -c Release -r $Rid --self-contained true `
    -p:PublishSingleFile=true `
    -p:IncludeNativeLibrariesForSelfExtract=true `
    -p:DebugType=none `
    -p:SatelliteResourceLanguages=en `
    -o $OutDir --nologo
Assert-Ok "Publish launcher"

# ------------------------------------------------------------
# 2. API -> api\GigRadarApi.exe
# ------------------------------------------------------------
Step "Publish API -> api\"
dotnet publish $ApiCsproj `
    -c Release -r $Rid --self-contained true `
    -p:PublishSingleFile=true `
    -p:IncludeNativeLibrariesForSelfExtract=true `
    -p:PublishTrimmed=false `
    -o $ApiOut --nologo
Assert-Ok "Publish API"

# ------------------------------------------------------------
# 3. Aplikasi desktop -> app\GigRadarMobile.exe
# ------------------------------------------------------------
if ($SkipApp) {
    Write-Host "[skip] Build aplikasi desktop dilewati (-SkipApp)" -ForegroundColor Yellow
} else {
    Step "Publish aplikasi desktop -> app\ (butuh beberapa menit)"
    # TargetFrameworks dibatasi ke Windows saja: tanpa ini, -r win-x64 ikut
    # diterapkan ke net10.0-android/ios sehingga restore minta runtime pack Mono
    # untuk win-x64 yang tidak ada (NU1102).
    dotnet publish $AppCsproj `
        -f $AppTfm -c Release -r $Rid --self-contained true `
        -p:TargetFrameworks=$AppTfm `
        -p:WindowsPackageType=None `
        -o $AppOut --nologo
    Assert-Ok "Publish aplikasi desktop"
}

# ------------------------------------------------------------
# 4. File konfigurasi (hanya dibuat bila belum ada)
# ------------------------------------------------------------
Step "Tulis gigradar.config.json"
$ConfigPath = Join-Path $OutDir "gigradar.config.json"
if (-not (Test-Path $ConfigPath)) {
    $Config = [ordered]@{
        _comment             = "Ubah connectionString sesuai PostgreSQL di komputer tujuan."
        connectionString     = "Host=localhost;Port=5432;Database=gigradar;Username=postgres;Password=postgres"
        postgresServiceName  = ""
        openSwagger          = $false
    }
    $Config | ConvertTo-Json | Set-Content -Path $ConfigPath -Encoding UTF8
    Write-Host "[ok] Dibuat: $ConfigPath" -ForegroundColor Green
} else {
    Write-Host "[ok] Sudah ada, dibiarkan: $ConfigPath" -ForegroundColor DarkGray
}

# ------------------------------------------------------------
# 5. Rapikan + ringkasan
# ------------------------------------------------------------
Remove-Item (Join-Path $OutDir "*.pdb") -Force -ErrorAction SilentlyContinue

Step "Ringkasan"
$launcherExe = Join-Path $OutDir "GigRadar.exe"
if (-not (Test-Path $launcherExe)) {
    Write-Host "[x] GigRadar.exe tidak terbentuk!" -ForegroundColor Red
    exit 1
}

$size = (Get-ChildItem $OutDir -Recurse -File | Measure-Object Length -Sum).Sum
Write-Host ("  Folder   : " + $OutDir)
Write-Host ("  Launcher : GigRadar.exe (" + [math]::Round((Get-Item $launcherExe).Length / 1MB, 1) + " MB)")
Write-Host ("  Total    : " + [math]::Round($size / 1MB, 1) + " MB")
Write-Host ""
Write-Host "  Jalankan dengan klik dua kali: GigRadar.exe" -ForegroundColor Green

# ------------------------------------------------------------
# 6. Zip
# ------------------------------------------------------------
if (-not $NoZip) {
    Step "Buat arsip ZIP"
    $ZipPath = Join-Path $DistRoot "GigRadar-$Rid.zip"
    Remove-Item $ZipPath -Force -ErrorAction SilentlyContinue
    Compress-Archive -Path (Join-Path $OutDir "*") -DestinationPath $ZipPath -CompressionLevel Optimal
    Write-Host ("[ok] " + $ZipPath + " (" + [math]::Round((Get-Item $ZipPath).Length / 1MB, 1) + " MB)") -ForegroundColor Green
}

Write-Host ""
Write-Host "=== SELESAI ===" -ForegroundColor Cyan
