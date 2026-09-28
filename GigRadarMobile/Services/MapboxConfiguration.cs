using System.Reflection;

namespace GigRadarMobile.Services;

/// <summary>
/// Akses token tile Mapbox untuk peta Radar.
///
/// Token **tidak boleh ditulis di source**: repo ini publik dan aturan secret
/// scanning GitHub menolak push yang memuatnya (ini pernah memblokir push
/// branch feat/postgresql-migration). Sumber token, berurutan:
///
///   1. Build property — disuntik sebagai <see cref="AssemblyMetadataAttribute"/>
///      oleh GigRadarMobile.csproj:
///      <c>dotnet build -p:GigRadarMapboxToken=pk.xxxx</c>
///   2. Environment variable <c>GIGRADAR_MAPBOX_TOKEN</c> (dev desktop/Windows).
///
/// Bila keduanya kosong, Radar tetap berjalan memakai tile OpenStreetMap
/// (tanpa token) — hanya gaya peta gelap khas Mapbox yang tidak dipakai.
/// </summary>
public static class MapboxConfiguration
{
    /// <summary>Nama kunci AssemblyMetadata yang diisi dari build property.</summary>
    public const string BuildMetadataKey = "GigRadarMapboxToken";

    /// <summary>Nama environment variable untuk dev desktop.</summary>
    public const string EnvironmentVariable = "GIGRADAR_MAPBOX_TOKEN";

    /// <summary>Token tile Mapbox, atau string kosong bila belum dikonfigurasi.</summary>
    public static string AccessToken { get; } = Resolve();

    /// <summary>True bila tile peta Mapbox tersedia.</summary>
    public static bool HasToken => !string.IsNullOrWhiteSpace(AccessToken);

    private static string Resolve()
    {
        var fromBuild = Assembly.GetExecutingAssembly()
            .GetCustomAttributes<AssemblyMetadataAttribute>()
            .FirstOrDefault(attribute => attribute.Key == BuildMetadataKey)?.Value;
        if (!string.IsNullOrWhiteSpace(fromBuild))
            return fromBuild.Trim();

        var fromEnvironment = Environment.GetEnvironmentVariable(EnvironmentVariable);
        return fromEnvironment?.Trim() ?? string.Empty;
    }
}
