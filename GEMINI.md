# 🧠 Workspace Rules & Architecture Guidelines (GEMINI.md)

This document provides instructions, constraints, and architecture guidelines for Gemini CLI and other AI agents working on the **GigRadar** repository.

---

## 🛠️ Technology Stack & Frameworks

*   **Backend:** ASP.NET Core Web API (`net8.0`).
*   **Database:** PostgreSQL using Entity Framework Core (`Npgsql.EntityFrameworkCore.PostgreSQL` 8.0.x).
    *   *Note:* Skema dikelola via EF Migrations (`db.Database.Migrate()` di `Program.cs`). Connection string disimpan di `dotnet user-secrets` (dev) atau environment variable (prod) — TIDAK di repo.
*   **Konsep produk:** Semua konsep, design system, dan lampiran teknis ada di `GIGRADAR_APP_SPEC.md` (dokumen tunggal; menggantikan dokumen-dokumen konsep lama).
*   **Mobile/Desktop App:** .NET MAUI (`net10.0-windows10.0.19041.0`, Android, iOS, macOS).
*   **MVVM Framework:** `CommunityToolkit.Mvvm` (v8.4.x) utilizing Source Generators (`[ObservableProperty]`, `[RelayCommand]`).
*   **Authentication:** JWT Bearer authentication on ASP.NET Core, storing session JWT token on mobile via MAUI `Preferences`.

---

## 🏛️ Architecture Conventions

### 1. Database Schema Bootstrap
*   Skema database dimiliki **EF Core Migrations** — dijalankan via `db.Database.Migrate()` di `Program.cs` (jangan kembali ke `EnsureCreated()`).
*   `TicketSchemaBootstrap.Ensure(context)` masih dipanggil setelah migrate, tetapi hanya untuk seed/idempotent tambahan — bukan penambal skema.

### 2. Dependency Injection (DI)
*   **Backend API:** Register controllers, services, and repositories in `GigRadarApi/Program.cs`.
*   **Mobile App:** Register views and ViewModels in `GigRadarMobile/MauiProgram.cs`. Every page must have a corresponding ViewModel, registered as either Transient or Singleton.

### 3. Separation of Concerns
*   **Views (.xaml & .xaml.cs):** Only handle UI-specific styling, layouts, and animations. Do not write business logic or HTTP calls directly in the code-behind.
*   **ViewModels:** Use `CommunityToolkit.Mvvm` to bind data, actions, and states to the View. All properties should use `[ObservableProperty]` and methods use `[RelayCommand]`.
*   **Services:** All REST API communication must be encapsulated within `ApiService.cs` or `AuthService.cs`. Do not initiate `HttpClient` directly inside ViewModels.

---

## 🔐 Authentication & Roles

*   Supported roles: `User`, `EO` (Event Organizer), `Admin`, `Artist`.
*   **Role-based API constraints:** 
    *   Admins can manage and edit all events and ticket types.
    *   EOs can only create new events or edit/delete events where they are the owner (`CreatedBy` matches their authenticated `UserId`).
    *   `User` role should receive `403 Forbidden` for any management/dashboard operations.
*   When editing backend APIs, always perform strict ownership checks on modifying requests (e.g. `PUT/DELETE /api/events/{id}`).

---

## 🎨 Styling & Design Tokens

*   Always respect the design system specified in `GIGRADAR_APP_SPEC.md` (Lampiran A — Design System).
*   **Theme:** Pure dark theme with zinc tones.
*   **Primary Background:** `#0A0A0B` (Dark Zinc).
*   **Card / Component Background:** `#18181B`.
*   **Accent Color:** `#A3FF12` (Signal Lime).
*   **Typography:** Space Grotesk (Headers) & Inter (Body text).

---

## 📦 Run and Build Commands

*   **Build & Run Backend:**
    ```bash
    dotnet run --project GigRadarApi/GigRadarApi.csproj --urls http://localhost:5000
    ```
*   **Build & Publish Launcher:**
    ```bash
    dotnet build GigRadarLauncher/GigRadarLauncher.csproj -c Release
    ```
*   **Build Mobile App (Windows Native):**
    ```bash
    dotnet build GigRadarMobile/GigRadarMobile.csproj -f net10.0-windows10.0.19041.0
    ```
