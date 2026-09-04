using System.Diagnostics;
using System.Net;
using System.Net.Sockets;

namespace GigRadarLauncher;

class Program
{
    public static void Main(string[] args)
    {
        Console.Title = "🎸 GigRadar Launcher";
        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine(@"
  ╔══════════════════════════════════════╗
  ║     🎸 GIGRADAR LAUNCHER 🎸         ║
  ║  Menghubungkan Skena, Menemukan     ║
  ║       Suara Lokal                   ║
  ╚══════════════════════════════════════╝
");
        Console.ResetColor();

        // Find the backend project
        var backendPath = FindBackendPath();
        if (backendPath == null)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("❌ Backend project not found!");
            Console.WriteLine("   Make sure GigRadarApi folder is in the same directory.");
            Console.ResetColor();
            Console.WriteLine("\nPress any key to exit...");
            Console.ReadLine();
            return;
        }

        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine("📦 Starting Backend API...");
        Console.ResetColor();

        // Start the backend
        var backendProcess = new Process
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = "dotnet",
                // Bind ke 0.0.0.0 agar API bisa diakses dari device fisik (Android/iPhone) di jaringan yang sama
                Arguments = $"run --project \"{backendPath}\" --urls \"http://0.0.0.0:5000\"",
                UseShellExecute = false,
                CreateNoWindow = true,
                RedirectStandardOutput = true,
                RedirectStandardError = true
            }
        };

        backendProcess.Start();

        // Wait for backend to start
        Console.Write("   Waiting for API");
        for (int i = 0; i < 15; i++)
        {
            Thread.Sleep(1000);
            Console.Write(".");
            if (IsPortOpen(5000))
            {
                Console.WriteLine();
                break;
            }
        }

        if (IsPortOpen(5000))
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("✅ Backend API is running at http://localhost:5000");
            Console.ResetColor();

            // Open Swagger UI
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("🌐 Opening Swagger UI in browser...");
            Console.ResetColor();

            Process.Start(new ProcessStartInfo
            {
                FileName = "http://localhost:5000/swagger",
                UseShellExecute = true
            });

            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine(@"
╔══════════════════════════════════════════════╗
║  ✅ GIGRADAR IS RUNNING!                     ║
║                                              ║
║  📡 API:     http://localhost:5000           ║
║  📖 Swagger: http://localhost:5000/swagger   ║
║                                              ║
║  Press any key to stop the server...         ║
╚══════════════════════════════════════════════╝
");
            Console.ResetColor();
            Console.ReadLine();

            // Stop backend
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("\n🛑 Stopping server...");
            Console.ResetColor();

            try
            {
                backendProcess.Kill();
                backendProcess.WaitForExit(5000);
            }
            catch { }

            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("✅ Server stopped. Goodbye!");
            Console.ResetColor();
        }
        else
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("❌ Backend failed to start!");
            Console.ResetColor();

            // Show error output
            var error = backendProcess.StandardError.ReadToEnd();
            if (!string.IsNullOrEmpty(error))
            {
                Console.WriteLine("\nError output:");
                Console.WriteLine(error);
            }
        }

        Thread.Sleep(1000);
    }

    static string? FindBackendPath()
    {
        var currentDir = Directory.GetCurrentDirectory();

        // Check current directory
        var candidate = Path.Combine(currentDir, "GigRadarApi", "GigRadarApi.csproj");
        if (File.Exists(candidate)) return Path.Combine(currentDir, "GigRadarApi");

        // Check parent directory
        var parent = Directory.GetParent(currentDir)?.FullName;
        if (parent != null)
        {
            candidate = Path.Combine(parent, "GigRadarApi", "GigRadarApi.csproj");
            if (File.Exists(candidate)) return Path.Combine(parent, "GigRadarApi");
        }

        // Check common locations
        string[] searchPaths = {
            @"C:\Users\user\GigRadar\GigRadarApi",
            @"C:\GigRadar\GigRadarApi",
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "GigRadar", "GigRadarApi")
        };

        foreach (var path in searchPaths)
        {
            if (File.Exists(Path.Combine(path, "GigRadarApi.csproj")))
                return path;
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
        catch
        {
            return false;
        }
    }
}
