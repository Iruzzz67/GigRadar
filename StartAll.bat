@echo off
REM ============================================================
REM GigRadar - Start All (satu klik)
REM Menyalakan: PostgreSQL service + API (port 5000) + adb reverse
REM ============================================================
cd /d "%~dp0"
powershell.exe -NoProfile -ExecutionPolicy Bypass -File "scripts\start-all.ps1"
echo.
pause
