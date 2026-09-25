#:package Npgsql@8.0.8
// Reset status verifikasi artist seed (ArtistId = 1) ke NOT_SUBMITTED.
// Connection string dibaca dari user-secrets proyek GigRadarApi (tidak ditampilkan).
using Npgsql;
using System.Text.Json;
using System.Text.RegularExpressions;

var csproj = File.ReadAllText("GigRadarApi/GigRadarApi.csproj");
var m = Regex.Match(csproj, "<UserSecretsId>([^<]+)</UserSecretsId>");
if (!m.Success) { Console.WriteLine("ERROR: UserSecretsId tidak ditemukan di csproj"); return 1; }

var secretsPath = Path.Combine(
    Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
    "Microsoft", "UserSecrets", m.Groups[1].Value, "secrets.json");
if (!File.Exists(secretsPath)) { Console.WriteLine("ERROR: secrets.json tidak ditemukan"); return 1; }

using var doc = JsonDocument.Parse(File.ReadAllText(secretsPath));
var conn = doc.RootElement.GetProperty("ConnectionStrings:DefaultConnection").GetString()!;

await using var c = new NpgsqlConnection(conn);
await c.OpenAsync();
Console.WriteLine("Terhubung ke database.");

async Task ShowStateAsync(string label)
{
    await using var q = new NpgsqlCommand(
        "SELECT \"VerificationStatus\", \"IsVerified\" FROM \"Artists\" WHERE \"ArtistId\" = 1", c);
    await using var r = await q.ExecuteReaderAsync();
    if (await r.ReadAsync())
        Console.WriteLine($"{label}: VerificationStatus={r.GetString(0)} IsVerified={r.GetBoolean(1)}");
    else
        Console.WriteLine($"{label}: artist ArtistId=1 tidak ditemukan!");
}

await ShowStateAsync("Sebelum");

await using (var cmd = new NpgsqlCommand("DELETE FROM \"ArtistVerifications\" WHERE \"ArtistId\" = 1", c))
    Console.WriteLine($"Riwayat pengajuan dihapus: {await cmd.ExecuteNonQueryAsync()} baris");

await using (var cmd = new NpgsqlCommand(
    "UPDATE \"Artists\" SET \"VerificationStatus\" = 'NOT_SUBMITTED', \"IsVerified\" = false WHERE \"ArtistId\" = 1", c))
    Console.WriteLine($"Artist diupdate: {await cmd.ExecuteNonQueryAsync()} baris");

await ShowStateAsync("Sesudah");
return 0;
