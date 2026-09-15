# GigRadar — Android Release Build Script
# Penggunaan:
#   .\scripts\build-android-release.ps1
#
# Signing (opsional tapi recommended untuk release):
#   Set environment variables sebelum menjalankan script:
#     GIGRADAR_KEYSTORE_PATH, GIGRADAR_KEYSTORE_PASSWORD,
#     GIGRADAR_KEY_ALIAS, GIGRADAR_KEY_PASSWORD
#   Lihat docs/ANDROID_PRODUCTION.md untuk cara membuat keystore.

$ErrorActionPreference = "Stop"

Write-Host "========================================" -ForegroundColor Cyan
Write-Host " GigRadar Android Release Build" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan

# 1. Pastikan project ditemukan
$project = Join-Path $PSScriptRoot "..\GigRadarMobile\GigRadarMobile.csproj"
if (-not (Test-Path $project)) {
    Write-Host "BUILD FAILED: project tidak ditemukan di $project" -ForegroundColor Red
    exit 1
}

# 2. Validasi .NET SDK + workload Android
try {
    $dotnetVersion = dotnet --version 2>$null
    Write-Host ""
    Write-Host ".NET SDK: $dotnetVersion"
} catch {
    Write-Host "BUILD FAILED: .NET SDK tidak ditemukan. Install dari https://dotnet.microsoft.com" -ForegroundColor Red
    exit 1
}

$workloads = dotnet workload list 2>$null | Out-String
if ($workloads -notmatch "android") {
    Write-Host "BUILD FAILED: MAUI Android workload belum terpasang." -ForegroundColor Red
    Write-Host "Jalankan: dotnet workload install maui-android" -ForegroundColor Yellow
    exit 1
}

# 3. Status signing
$signed = $env:GIGRADAR_KEYSTORE_PATH -and $env:GIGRADAR_KEYSTORE_PASSWORD -and $env:GIGRADAR_KEY_ALIAS
if ($signed) {
    Write-Host "Signing: AKTIF (keystore: $env:GIGRADAR_KEYSTORE_PATH, alias: $env:GIGRADAR_KEY_ALIAS)"
} else {
    Write-Host "Signing: TIDAK DISET - APK dihasilkan TANPA signature release (tidak bisa dipasang)." -ForegroundColor Yellow
    Write-Host "Set GIGRADAR_KEYSTORE_PATH / GIGRADAR_KEYSTORE_PASSWORD / GIGRADAR_KEY_ALIAS / GIGRADAR_KEY_PASSWORD" -ForegroundColor Yellow
}

# 4. Restore
Write-Host ""
Write-Host "Restoring..."
dotnet restore $project
if ($LASTEXITCODE -ne 0) {
    Write-Host "BUILD FAILED pada tahap restore." -ForegroundColor Red
    exit 1
}

# 5. Build/publish Release APK
Write-Host ""
Write-Host "Building Android Release..."
dotnet publish $project -f net10.0-android -c Release
if ($LASTEXITCODE -ne 0) {
    Write-Host ""
    Write-Host "BUILD FAILED. Periksa error di atas." -ForegroundColor Red
    exit 1
}

# 6. Tampilkan lokasi APK
$publishDir = Join-Path $PSScriptRoot "..\GigRadarMobile\bin\Release\net10.0-android\publish"
$apks = Get-ChildItem -Path $publishDir -Filter *.apk -ErrorAction SilentlyContinue

Write-Host ""
Write-Host "========================================" -ForegroundColor Green
if ($apks -and $apks.Count -gt 0) {
    Write-Host "BUILD SUCCESSFUL" -ForegroundColor Green
    Write-Host ""
    foreach ($apk in $apks) {
        $sizeMb = [math]::Round($apk.Length / 1MB, 1)
        Write-Host "APK: $($apk.FullName) ($sizeMb MB)"
    }
    Write-Host ""
    Write-Host "Install ke device:"
    Write-Host "  adb install -r `"$($apks[0].FullName)`""
} else {
    Write-Host "BUILD SUCCESSFUL, tetapi APK tidak ditemukan di:" -ForegroundColor Yellow
    Write-Host $publishDir
    Write-Host "Cari manual: Get-ChildItem -Recurse -Filter *.apk GigRadarMobile\bin"
}
Write-Host "========================================" -ForegroundColor Green
