# ============================================================
# GigRadar - Start All (PostgreSQL + API + adb reverse)
# Jalankan via StartAll.bat (satu klik) atau:
#   powershell -NoProfile -ExecutionPolicy Bypass -File scripts\start-all.ps1
# Matikan API saja:
#   powershell -NoProfile -ExecutionPolicy Bypass -File scripts\start-all.ps1 -Stop
# ============================================================
param(
    [switch]$Stop
)

$ErrorActionPreference = "Continue"
# Cegah MSBuild node bertahan hidup (menahan exit script saat dijalankan via pipe/CI)
$env:MSBUILDDISABLENODEREUSE = "1"
$Root      = Split-Path -Parent $PSScriptRoot
$ApiDir    = Join-Path $Root "GigRadarApi"
$LogFile   = Join-Path $Root "gigradar-api-pg.log"
$ErrFile   = Join-Path $Root "gigradar-api-pg.err.log"
$AdbPaths  = @(
    (Join-Path $env:LOCALAPPDATA "Android\Sdk\platform-tools\adb.exe"),
    "$env:ANDROID_HOME\platform-tools\adb.exe"
) | Where-Object { $_ -and (Test-Path $_) }
$Adb = if ($AdbPaths) { $AdbPaths[0] } else { $null }

function Stop-Api {
    # 1) Proses dotnet.exe yang menjalankan GigRadarApi (run via dll)
    $procs = Get-CimInstance Win32_Process -Filter "Name = 'dotnet.exe'" |
        Where-Object { $_.CommandLine -like "*GigRadarApi*" }

    # 2) Apphost GigRadarApi.exe (diluncurkan oleh `dotnet run`) + port owner 5000
    $portPids = (Get-NetTCPConnection -LocalPort 5000 -State Listen -ErrorAction SilentlyContinue).OwningProcess | Select-Object -Unique
    if ($portPids) {
        $procs += Get-CimInstance Win32_Process | Where-Object { $portPids -contains $_.ProcessId }
    }

    $procs = $procs | Sort-Object ProcessId -Unique
    foreach ($p in $procs) {
        Stop-Process -Id $p.ProcessId -Force -ErrorAction SilentlyContinue
        Write-Host ("[stop] API lama dimatikan (PID " + $p.ProcessId + " - " + $p.Name + ")") -ForegroundColor Yellow
    }
    if (-not $procs) { Write-Host "[stop] Tidak ada API yang sedang jalan" -ForegroundColor DarkGray }
    # Beri waktu socket port 5000 lepas (TIME_WAIT)
    if ($procs) { Start-Sleep -Seconds 2 }
}

# ------------------------------------------------------------
# Mode -Stop: matikan API lalu keluar
# ------------------------------------------------------------
if ($Stop) {
    Stop-Api
    exit 0
}

Write-Host "=== GigRadar Start All ===" -ForegroundColor Cyan

# ------------------------------------------------------------
# 1. Pastikan service PostgreSQL jalan (start service butuh admin -> UAC)
# ------------------------------------------------------------
$svc = Get-Service | Where-Object { $_.Name -like "postgresql*" } | Select-Object -First 1
if (-not $svc) {
    Write-Host "[db] ERROR: Service PostgreSQL tidak ditemukan. Instal dulu PostgreSQL 16." -ForegroundColor Red
    Read-Host "Tekan Enter untuk keluar"
    exit 1
}

if ($svc.Status -ne "Running") {
    Write-Host ("[db] Menyalakan service " + $svc.Name + " (perlu izin admin/UAC)...") -ForegroundColor Yellow
    try {
        Start-Process -FilePath "net.exe" -ArgumentList "start", $svc.Name -Verb RunAs -Wait -ErrorAction Stop
    } catch {
        Write-Host "[db] Gagal menyalakan service (UAC ditolak?). Coba jalankan manual: net start $($svc.Name)" -ForegroundColor Red
        Read-Host "Tekan Enter untuk keluar"
        exit 1
    }
}
Write-Host ("[db] PostgreSQL OK (" + $svc.Name + ")") -ForegroundColor Green

# ------------------------------------------------------------
# 2. Matikan API lama + build
# ------------------------------------------------------------
Stop-Api

Write-Host "[api] Build API..." -ForegroundColor Cyan
$BuildLog = Join-Path $Root "gigradar-build.log"
dotnet build (Join-Path $ApiDir "GigRadarApi.csproj") -v q --nologo *> $BuildLog
Get-Content $BuildLog -Tail 5
if ($LASTEXITCODE -ne 0) {
    Write-Host "[api] Build GAGAL. Cek output di atas." -ForegroundColor Red
    Read-Host "Tekan Enter untuk keluar"
    exit 1
}

# ------------------------------------------------------------
# 3. Start API (koneksi DB dari user-secrets, mode Development via launchSettings)
#    Bind 0.0.0.0 supaya bisa diakses device Android via Wi-Fi juga.
# ------------------------------------------------------------
Write-Host "[api] Menjalankan API di http://0.0.0.0:5000 ..." -ForegroundColor Cyan
Start-Process -FilePath "dotnet.exe" `
    -ArgumentList "run", "--no-build", "--urls", "http://0.0.0.0:5000" `
    -WorkingDirectory $ApiDir `
    -RedirectStandardOutput $LogFile `
    -RedirectStandardError  $ErrFile `
    -WindowStyle Hidden

# ------------------------------------------------------------
# 4. Tunggu sampai API siap (maks 90 detik)
#    Poll pakai curl.exe + 127.0.0.1 eksplisit (Invoke-WebRequest bisa gagal
#    karena localhost resolve ke ::1 sementara API listen di 0.0.0.0/IPv4).
# ------------------------------------------------------------
$ready = $false
foreach ($i in 1..90) {
    Start-Sleep -Seconds 1
    $code = & curl.exe -s -o NUL -w "%{http_code}" --max-time 2 "http://127.0.0.1:5000/api/events" 2>$null
    if ("$code" -eq "200") { $ready = $true; break }
    if ($i % 10 -eq 0) { Write-Host ("[api] menunggu... (" + $i + "s)") -ForegroundColor DarkGray }
}

if (-not $ready) {
    Write-Host "[api] API belum siap dalam 90s. Cek log:" -ForegroundColor Red
    Write-Host ("       " + $LogFile) -ForegroundColor Red
    Write-Host ("       " + $ErrFile) -ForegroundColor Red
    Read-Host "Tekan Enter untuk keluar"
    exit 1
}
Write-Host "[api] API SIAP  ->  http://localhost:5000/swagger" -ForegroundColor Green

# ------------------------------------------------------------
# 5. adb reverse (device USB bisa akses localhost:5000)
# ------------------------------------------------------------
if ($Adb) {
    $devices = & $Adb devices 2>$null | Select-String -Pattern "\tdevice$"
    if ($devices) {
        & $Adb reverse tcp:5000 tcp:5000 2>$null | Out-Null
        Write-Host "[adb] reverse tcp:5000 OK untuk device USB terhubung" -ForegroundColor Green
    } else {
        Write-Host "[adb] Tidak ada device Android terhubung (skip). Sambungkan HP lalu jalankan:" -ForegroundColor Yellow
        Write-Host "      adb reverse tcp:5000 tcp:5000" -ForegroundColor Yellow
    }
} else {
    Write-Host "[adb] adb.exe tidak ditemukan (skip)." -ForegroundColor DarkGray
}

# ------------------------------------------------------------
# Ringkasan
# ------------------------------------------------------------
Write-Host ""
Write-Host "=== SEMUA SIAP ===" -ForegroundColor Cyan
Write-Host "  Swagger     : http://localhost:5000/swagger"
Write-Host "  API events  : http://localhost:5000/api/events"
Write-Host "  Device USB  : pakai BaseUrl http://localhost:5000/ (sudah di-reverse)"
Write-Host "  Device WiFi : pakai BaseUrl http://192.168.68.122:5000"
Write-Host "  Matikan API : powershell -File scripts\start-all.ps1 -Stop"
Write-Host ""
