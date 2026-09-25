using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Text.Json;

namespace GigRadarLauncher;

/// <summary>
/// GigRadar Launcher — satu exe untuk menyalakan seluruh aplikasi:
///   1. Pastikan service PostgreSQL jalan (naikkan UAC bila perlu)
///   2. Jalankan GigRadar API di http://localhost:5000
///   3. Buka aplikasi desktop GigRadar (MAUI Windows)
///
/// Dua mode pencarian file:
///   - Portable  : exe ini diletakkan di root folder distribusi bersama
///                 subfolder `api/` dan `app/` (hasil scripts\build-release.ps1).
///   - Dev       : exe dijalankan dari repo (bin/Debug/...) dan mencari
///                 GigRadarApi.csproj / GigRadarMobile.exe di folder anonim.
/// </summary>
class Program
{
    const int ApiPort = 5000;
    const string SwaggerUrl = "http://localhost:5000/swagger";
    const string EventsUrl = "http://127.0.0.1:5000/api/events";

    /// <summary>Muat dari gigradar.config.json (opsional, di sebelah exe).</summary>
    static LauncherConfig _config = new();

    static string _exeDir = AppContext.BaseDirectory.TrimEnd('\\', '/');

    public static int Main(string[] args)
    {
        Console.Title = "GigRadar Launcher";
        _config = LauncherConfig.Load(_exeDir);
        PrintBanner();

        // ── Kebutuhan 1: PostgreSQL ─────────────────────────────────────
        if (!EnsurePostgresRunning())
        {
            Fail("PostgreSQL tidak siap. Pastikan PostgreSQL 16 terinstal dan service-nya bisa dinyalakan.");
            return 1;
        }

        // ── Kebutuhan 2: API ────────────────────────────────────────────
        var api = LocateApi();
        if (api is null)
        {
            Fail("File API tidak ditemukan. Struktur distribusi salah — folder 'api' harus ada di sebelah GigRadar.exe.");
            return 1;
        }

        Process? apiProcess = null;
        bool reusedExistingApi = IsPortOpen(ApiPort);

        if (reusedExistingApi)
        {
            Info("API sudah jalan di port 5000 — memakai yang sudah ada (tidak akan dimatikan saat keluar).");
        }
        else
        {
            Info("Menjalankan API...");
            apiProcess = StartApi(api);
            if (apiProcess is null)
            {
                Fail("Gagal menjalankan API.");
                return 1;
            }

            Console.Write("   Menunggu API siap");
            bool ready = false;
            for (int i = 0; i < 90; i++)
            {
                Thread.Sleep(1000);
                if (i % 5 == 4) Console.Write(".");
                if (IsHttpOk(EventsUrl)) { ready = true; break; }
            }
            Console.WriteLine();

            if (!ready)
            {
                Fail("API tidak siap dalam 90 detik. Cek log di bawah.");
                DumpApiLog();
                TryKill(apiProcess);
                return 1;
            }
        }

        Ok("API siap  ->  " + SwaggerUrl);

        // ── Kebutuhan 3: aplikasi desktop ───────────────────────────────
        Process? appProcess = null;
        string? appPath = LocateAppExecutable();
        if (appPath is null)
        {
            Warn("Aplikasi desktop tidak ditemukan (folder 'app' belum dibuild).");
            Warn("API tetap jalan; buka " + SwaggerUrl + " untuk memakai API saja.");
        }
        else
        {
            Info("Membuka aplikasi GigRadar...");
            appProcess = TryStartApp(appPath);
            if (appProcess is null)
                Warn("Aplikasi desktop gagal dibuka — API tetap jalan.");
        }

        if (_config.OpenSwagger)
        {
            try { Process.Start(new ProcessStartInfo(SwaggerUrl) { UseShellExecute = true }); }
            catch { /* browser tidak wajib */ }
        }

        PrintSummary(appPath is not null);

        // ── Tunggu perintah: Enter = matikan, R = buka ulang app, S = swagger ──
        ControlLoop(ref appProcess, appPath, apiProcess, reusedExistingApi);

        return 0;
    }

    // ════════════════════════════════════════════════════════════════════
    //  Kontrol interaktif
    // ════════════════════════════════════════════════════════════════════
    static void ControlLoop(ref Process? appProcess, string? appPath, Process? apiProcess, bool reusedExistingApi)
    {
        while (true)
        {
            Console.WriteLine();
            Console.Write("[Enter] matikan semua   [R] buka ulang app   [S] buka Swagger  > ");
            var key = Console.ReadLine()?.Trim().ToUpperInvariant() ?? "";

            if (key == "R")
            {
                if (appPath is null) { Warn("Aplikasi desktop tidak tersedia."); continue; }
                if (appProcess is { HasExited: false }) { Warn("Aplikasi masih terbuka."); continue; }
                appProcess = TryStartApp(appPath);
                if (appProcess is null) Warn("Gagal membuka aplikasi desktop."); else Ok("Aplikasi dibuka.");
                continue;
            }

            if (key == "S")
            {
                try { Process.Start(new ProcessStartInfo(SwaggerUrl) { UseShellExecute = true }); }
                catch { Warn("Gagal membuka browser."); }
                continue;
            }

            break; // Enter / input lain = matikan
        }

        if (!reusedExistingApi && apiProcess is not null)
        {
            Info("Mematikan API...");
            TryKill(apiProcess);
        }
        Ok("Selesai. Sampai jumpa!");
    }

    // ════════════════════════════════════════════════════════════════════
    //  PostgreSQL
    // ════════════════════════════════════════════════════════════════════
    static bool EnsurePostgresRunning()
    {
        string? svc = FindPostgresService();
        if (svc is null)
        {
            Warn("Service PostgreSQL tidak ditemukan (mencari nama 'postgresql*').");
            Warn("Instal PostgreSQL 16, atau perbaiki nama service.");
            return false;
        }

        if (IsServiceRunning(svc))
        {
            Ok($"PostgreSQL jalan ({svc})");
            return true;
        }

        Warn($"Menyalakan service {svc} (butuh izin admin / UAC)...");
        try
        {
            var psi = new ProcessStartInfo("net.exe", $"start {svc}")
            {
                UseShellExecute = true,
                Verb = "runas",
                CreateNoWindow = true,
                WindowStyle = ProcessWindowStyle.Hidden
            };
            using var p = Process.Start(psi);
            p?.WaitForExit();
        }
        catch (Exception ex)
        {
            // Verb runas dibatalkan user atau ditolak kebijakan sistem.
            Warn("Gagal menaikkan UAC: " + ex.Message);
        }

        for (int i = 0; i < 30; i++)
        {
            if (IsServiceRunning(svc)) { Ok($"PostgreSQL siap ({svc})"); return true; }
            Thread.Sleep(1000);
        }
        return false;
    }

    static string? FindPostgresService()
    {
        var fromConfig = _config.PostgresServiceName;
        if (!string.IsNullOrWhiteSpace(fromConfig)) return fromConfig;

        // 1) PowerShell: paling mudah dibaca, tetapi bisa diblokir kebijakan (ExecutionPolicy/GPO).
        var (code, output) = RunCapture("powershell.exe",
            "-NoProfile -Command \"(Get-Service | Where-Object { $_.Name -like 'postgresql*' } | Select-Object -First 1).Name\"");
        if (code == 0)
        {
            var name = output.Trim();
            if (!string.IsNullOrEmpty(name)) return name;
        }

        // 2) Cadangan sc.exe: bawaan Windows dan selalu tersedia meski PowerShell dimatikan.
        return FindPostgresServiceViaSc();
    }

    /// <summary>Deteksi service PostgreSQL lewat sc.exe (tanpa PowerShell).</summary>
    static string? FindPostgresServiceViaSc()
    {
        var (code, output) = RunCapture("sc.exe", "query state= all");
        if (code != 0) return null;

        foreach (var line in output.Split('\n'))
        {
            var text = line.Trim();
            if (!text.StartsWith("SERVICE_NAME:", StringComparison.OrdinalIgnoreCase)) continue;
            var name = text["SERVICE_NAME:".Length..].Trim();
            if (name.StartsWith("postgresql", StringComparison.OrdinalIgnoreCase)) return name;
        }
        return null;
    }

    static bool IsServiceRunning(string name)
    {
        var (code, output) = RunCapture("powershell.exe",
            $"-NoProfile -Command \"(Get-Service -Name '{name}' -ErrorAction SilentlyContinue).Status\"");
        if (code == 0 && output.Trim().StartsWith("Running", StringComparison.OrdinalIgnoreCase)) return true;

        // Cadangan sc.exe — "STATE : 4   RUNNING".
        var (scCode, scOutput) = RunCapture("sc.exe", $"query \"{name}\"");
        return scCode == 0 && scOutput.Contains("RUNNING", StringComparison.OrdinalIgnoreCase);
    }

    // ════════════════════════════════════════════════════════════════════
    //  API
    // ════════════════════════════════════════════════════════════════════
    sealed record ApiTarget(string Kind, string Path);

    static ApiTarget? LocateApi()
    {
        // Mode portable: api/GigRadarApi.exe
        var portable = Path.Combine(_exeDir, "api", "GigRadarApi.exe");
        if (File.Exists(portable)) return new ApiTarget("exe", portable);

        // Mode dev: cari GigRadarApi.csproj di atas folder exe
        var projectDir = SearchUpwards(root => Path.Combine(root, "GigRadarApi", "GigRadarApi.csproj"), 8);
        if (projectDir is not null) return new ApiTarget("dotnet", projectDir);

        return null;
    }

    static Process? StartApi(ApiTarget api)
    {
        var psi = new ProcessStartInfo
        {
            UseShellExecute = false,
            CreateNoWindow = true,
            RedirectStandardOutput = true,
            RedirectStandardError = true
        };

        if (api.Kind == "exe")
        {
            psi.FileName = api.Path;
            psi.WorkingDirectory = Path.GetDirectoryName(api.Path)!;
            psi.Arguments = $"--urls http://0.0.0.0:{ApiPort}";
        }
        else
        {
            psi.FileName = "dotnet";
            psi.WorkingDirectory = api.Path;
            psi.Arguments = $"run --project \"{api.Path}\" --urls http://0.0.0.0:{ApiPort}";
        }

        // Swagger hanya aktif di Development.
        psi.EnvironmentVariables["ASPNETCORE_ENVIRONMENT"] = "Development";
        // Koneksi DB: dev memakai user-secrets, distribusi memakai config di sini.
        if (!string.IsNullOrWhiteSpace(_config.ConnectionString))
            psi.EnvironmentVariables["ConnectionStrings__DefaultConnection"] = _config.ConnectionString;
        // Kunci JWT: setiap instalasi sebaiknya punya kunci sendiri. Bila tidak diisi,
        // API memakai nilai bawaan dari appsettings.json (cocok untuk demo, tidak untuk produksi).
        if (!string.IsNullOrWhiteSpace(_config.JwtSecret))
            psi.EnvironmentVariables["JwtSettings__SecretKey"] = _config.JwtSecret;

        try
        {
            var p = Process.Start(psi)!;
            // Tampung log agar tidak memblokir pipe dan bisa ditampilkan saat gagal.
            p.OutputDataReceived += (_, e) => AppendLog(e.Data);
            p.ErrorDataReceived += (_, e) => AppendLog(e.Data);
            p.BeginOutputReadLine();
            p.BeginErrorReadLine();
            return p;
        }
        catch (Exception ex)
        {
            Warn("Tidak bisa menjalankan API: " + ex.Message);
            return null;
        }
    }

    static readonly List<string> _log = new();
    static void AppendLog(string? line)
    {
        if (string.IsNullOrWhiteSpace(line)) return;
        lock (_log)
        {
            _log.Add(line);
            if (_log.Count > 200) _log.RemoveAt(0);
        }
    }

    static void DumpApiLog()
    {
        lock (_log)
        {
            if (_log.Count == 0) return;
            Console.WriteLine();
            Console.WriteLine("─── 30 baris log API terakhir ───");
            foreach (var line in _log.TakeLast(30)) Console.WriteLine("  " + line);
        }
    }

    // ════════════════════════════════════════════════════════════════════
    //  Aplikasi desktop (MAUI Windows)
    // ════════════════════════════════════════════════════════════════════
    static string? LocateAppExecutable()
    {
        // Mode portable: app/GigRadarMobile.exe
        var portable = Path.Combine(_exeDir, "app", "GigRadarMobile.exe");
        if (File.Exists(portable)) return portable;

        // Mode dev: cari hasil build di repo (bin/<Config>/net10.0-windows*)
        var repoRoot = SearchUpwards(root => Path.Combine(root, "GigRadarMobile", "GigRadarMobile.csproj"), 8);
        if (repoRoot is null) return null;

        var mobileBin = Path.Combine(repoRoot, "GigRadarMobile", "bin");
        if (!Directory.Exists(mobileBin)) return null;

        try
        {
            return Directory
                .EnumerateFiles(mobileBin, "GigRadarMobile.exe", SearchOption.AllDirectories)
                .Where(p => !p.Contains(Path.DirectorySeparatorChar + "obj" + Path.DirectorySeparatorChar))
                .OrderByDescending(File.GetLastWriteTimeUtc)
                .FirstOrDefault();
        }
        catch { return null; }
    }

    static Process? TryStartApp(string exePath)
    {
        try
        {
            return Process.Start(new ProcessStartInfo(exePath)
            {
                WorkingDirectory = Path.GetDirectoryName(exePath)!,
                UseShellExecute = true
            });
        }
        catch (Exception ex)
        {
            Warn("Gagal membuka aplikasi: " + ex.Message);
            return null;
        }
    }

    // ════════════════════════════════════════════════════════════════════
    //  Utilitas
    // ════════════════════════════════════════════════════════════════════
    /// <summary>Cari folder terdekat ke atas yang memuat path relatif tertentu.</summary>
    static string? SearchUpwards(Func<string, string> relativeProbe, int maxLevels)
    {
        var dir = _exeDir;
        for (int i = 0; i <= maxLevels && !string.IsNullOrEmpty(dir); i++)
        {
            if (File.Exists(relativeProbe(dir))) return dir;
            dir = Path.GetDirectoryName(dir);
        }
        return null;
    }

    static bool IsPortOpen(int port)
    {
        try
        {
            using var client = new TcpClient();
            client.Connect("127.0.0.1", port);
            return true;
        }
        catch { return false; }
    }

    static readonly HttpClient _http = new() { Timeout = TimeSpan.FromSeconds(3) };

    static bool IsHttpOk(string url)
    {
        try
        {
            using var response = _http.GetAsync(url).GetAwaiter().GetResult();
            return (int)response.StatusCode == 200;
        }
        catch { return false; }
    }

    static (int Code, string Output) RunCapture(string fileName, string arguments)
    {
        try
        {
            using var p = Process.Start(new ProcessStartInfo(fileName, arguments)
            {
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true
            });
            if (p is null) return (-1, "");
            var output = p.StandardOutput.ReadToEnd();
            p.WaitForExit(15000);
            return (p.ExitCode, output);
        }
        catch { return (-1, ""); }
    }

    static void TryKill(Process p)
    {
        try
        {
            if (!p.HasExited)
            {
                p.Kill(entireProcessTree: true);
                p.WaitForExit(5000);
            }
        }
        catch { /* sudah mati */ }
    }

    // ════════════════════════════════════════════════════════════════════
    //  Tampilan konsol
    // ════════════════════════════════════════════════════════════════════
    static void PrintBanner()
    {
        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine(@"
  ==================================================
      GIGRADAR LAUNCHER
      Menghubungkan Skena, Menemukan Suara Lokal
  ==================================================
");
        Console.ResetColor();
    }

    static void PrintSummary(bool hasApp)
    {
        Console.WriteLine();
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine("  === GIGRADAR SIAP ===");
        Console.WriteLine($"    PostgreSQL : jalan");
        Console.WriteLine($"    API        : http://localhost:5000");
        Console.WriteLine($"    Swagger    : {SwaggerUrl}");
        Console.WriteLine($"    Aplikasi   : {(hasApp ? "terbuka di jendela GigRadar" : "tidak tersedia")}");
        Console.ResetColor();
    }

    static void Ok(string m) { Write(m, ConsoleColor.Green, "[ok]"); }
    static void Info(string m) { Write(m, ConsoleColor.Cyan, "[..]"); }
    static void Warn(string m) { Write(m, ConsoleColor.Yellow, "[!]"); }
    static void Fail(string m) { Write(m, ConsoleColor.Red, "[x]"); }

    static void Write(string message, ConsoleColor color, string tag)
    {
        Console.ForegroundColor = color;
        Console.Write(tag + " ");
        Console.ResetColor();
        Console.WriteLine(message);
    }
}

/// <summary>
/// Konfigurasi opsional: letakkan `gigradar.config.json` di sebelah GigRadar.exe.
/// {
///   "connectionString": "Host=localhost;Port=5432;Database=gigradar;Username=postgres;Password=postgres",
///   "postgresServiceName": "postgresql-x64-16",
///   "jwtSecret": "",
///   "openSwagger": false
/// }
/// </summary>
class LauncherConfig
{
    public string ConnectionString { get; set; } = "";
    public string PostgresServiceName { get; set; } = "";

    /// <summary>
    /// Kunci penandatangan JWT untuk instalasi ini. Kosongkan untuk memakai nilai
    /// bawaan appsettings.json (hanya untuk demo). Isi agar tiap instalasi unik.
    /// </summary>
    public string JwtSecret { get; set; } = "";

    public bool OpenSwagger { get; set; }

    public static LauncherConfig Load(string exeDir)
    {
        var path = Path.Combine(exeDir, "gigradar.config.json");
        if (!File.Exists(path)) return new LauncherConfig();
        try
        {
            var json = File.ReadAllText(path);
            var cfg = JsonSerializer.Deserialize<LauncherConfig>(json, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });
            return cfg ?? new LauncherConfig();
        }
        catch
        {
            return new LauncherConfig();
        }
    }
}
