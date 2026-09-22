@echo off
REM ============================================================
REM GigRadar - satu klik
REM
REM Bila paket distribusi sudah dibuild (scripts\build-release.ps1),
REM langsung jalankan dist\GigRadar\GigRadar.exe:
REM   PostgreSQL + API + aplikasi desktop dalam satu perintah.
REM
REM Kalau belum, fallback ke mode pengembangan (dotnet run) yang
REM hanya menyalakan API + Swagger UI.
REM ============================================================

cd /d "%~dp0"

if exist "dist\GigRadar\GigRadar.exe" (
    echo [i] Menjalankan paket distribusi: dist\GigRadar\GigRadar.exe
    start "" "dist\GigRadar\GigRadar.exe"
    exit /b 0
)

echo [i] Paket distribusi belum ada - memakai mode pengembangan ^(API saja^).
echo     Untuk membuat exe all-in-one:
echo       powershell -NoProfile -ExecutionPolicy Bypass -File scripts\build-release.ps1
echo.
dotnet run --project GigRadarLauncher\GigRadarLauncher
