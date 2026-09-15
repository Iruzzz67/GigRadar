# GigRadar Android — Production APK

## 1. Prerequisites

Install the .NET 10 SDK, the .NET MAUI workload, Android SDK/platform tools, and a JDK supported by the installed .NET MAUI/Android workload.

Verify the environment:

```powershell
dotnet --info
dotnet workload list
adb version
```

## 2. Configure the production API

Edit:

`GigRadarMobile/Services/ApiConfiguration.cs`

Replace:

```csharp
https://YOUR-PRODUCTION-API-DOMAIN
```

with the real HTTPS URL of the deployed `GigRadarApi`, for example:

```csharp
https://api.example.com
```

Do not use `localhost`, `10.0.2.2`, or a LAN IP in a production APK.

The Android production manifest disables cleartext HTTP traffic, so the API must be served over HTTPS.

## 3. Build a Release APK

From the repository root on Windows:

```powershell
dotnet restore GigRadarMobile/GigRadarMobile.csproj

dotnet build GigRadarMobile/GigRadarMobile.csproj -f net10.0-android -c Release
```

The project is configured to produce an APK for Android Release builds. The generated APK is normally under:

`GigRadarMobile/bin/Release/net10.0-android/`

For a distributable production build, sign the APK with your own release keystore.

## 4. Create a release keystore (one time)

Keep the keystore outside the repository. Never commit it, its password, or signing secrets.

Example:

```powershell
keytool -genkeypair -v `
  -keystore "$env:USERPROFILE\gigradar-release.keystore" `
  -alias gigradar `
  -keyalg RSA `
  -keysize 2048 `
  -validity 10000
```

Back up the keystore securely. Losing it can prevent future signed updates from being installed over an existing app.

## 5. Signed Release APK

Pass signing properties to MSBuild rather than storing credentials in the `.csproj`:

```powershell
dotnet publish GigRadarMobile/GigRadarMobile.csproj `
  -f net10.0-android `
  -c Release `
  -p:AndroidKeyStore=true `
  -p:AndroidSigningKeyStore="$env:USERPROFILE\gigradar-release.keystore" `
  -p:AndroidSigningKeyAlias=gigradar `
  -p:AndroidSigningKeyPass="<KEY_PASSWORD>" `
  -p:AndroidSigningStorePass="<STORE_PASSWORD>"
```

Use environment variables or a secure CI secret store for passwords in automated builds. Do not put real passwords in shell history, source control, or documentation.

## 6. Install on a physical Android device

Enable Developer Options and USB debugging, connect the device, then verify:

```powershell
adb devices
```

Install the signed APK:

```powershell
adb install -r "<path-to-signed-apk>"
```

After installation, GigRadar appears as a normal Android application and can be opened from its launcher icon without Visual Studio or `dotnet run`.

## 7. Production checklist

- [ ] Production API is deployed and reachable over HTTPS.
- [ ] `ApiConfiguration.cs` no longer contains the placeholder API domain.
- [ ] Android cleartext traffic remains disabled.
- [ ] Release APK is signed with the permanent GigRadar release keystore.
- [ ] Keystore and passwords are not committed to Git.
- [ ] Login, JWT authorization, event discovery, tickets, profile, maps/location and EO flows are tested on a physical Android device.
- [ ] API does not depend on `localhost`, emulator host mapping, or a developer PC.
- [ ] Backend database/storage is persistent and backed up.
