using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authorization.Policy;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using GigRadarApi.Authorization;
using GigRadarApi.Data;
using GigRadarApi.Services;

// Npgsql 6+ memetakan DateTime ke `timestamp with time zone` dan menolak Kind=Unspecified
// (seed & tanggal event memakai Kind=Unspecified, seperti perilaku SQLite sebelumnya).
// Switch ini menjaga mapping DateTime -> `timestamp without time zone` agar behavior
// tanggal tidak berubah sama sekali setelah migrasi SQLite -> PostgreSQL.
AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);

var builder = WebApplication.CreateBuilder(args);

// PostgreSQL (EF Core + Npgsql)
// Koneksi: utamakan env var DATABASE_URL (konvensi Render/Neon),
// fallback ke ConnectionStrings__DefaultConnection dari konfigurasi.
var pgConnectionString = Environment.GetEnvironmentVariable("DATABASE_URL")
    ?? builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(pgConnectionString));

// JWT Authentication
var jwtSettings = builder.Configuration.GetSection("JwtSettings");
var key = Encoding.UTF8.GetBytes(jwtSettings["SecretKey"]!);

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtSettings["Issuer"],
        ValidAudience = jwtSettings["Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(key)
    };
});

// Otorisasi + gate anti-penipuan event (§53/§54): aksi tulis event/tiket EO
// hanya untuk organizer yang sudah diverifikasi Admin.
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("VerifiedOrganizer", policy =>
        policy.Requirements.Add(new VerifiedOrganizerRequirement()));
});
builder.Services.AddScoped<IAuthorizationHandler, VerifiedOrganizerHandler>();
builder.Services.AddSingleton<IAuthorizationMiddlewareResultHandler, ApiAuthorizationResultHandler>();

// Services
builder.Services.AddScoped<AuthService>();
builder.Services.AddScoped<EventService>();
builder.Services.AddScoped<GenreService>();
builder.Services.AddScoped<TicketService>();
builder.Services.AddScoped<NotificationService>();
builder.Services.AddScoped<ArtistVerificationService>();
builder.Services.AddScoped<OrganizerVerificationService>();

// Controllers + Swagger
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "GigRadar API", Version = "v1" });
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header. Example: \"Bearer {token}\"",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });
    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" }
            },
            Array.Empty<string>()
        }
    });
});

// CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
        policy.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader());
});

var app = builder.Build();

// Auto-migrate
if (!EF.IsDesignTime)
{
    using (var scope = app.Services.CreateScope())
    {
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        db.Database.Migrate();
        TicketSchemaBootstrap.Ensure(db);
        PendingVerificationBootstrap.Ensure(db);

        // Password default akun demo hanya boleh berlaku di Development (§24).
        // Dijalankan SETELAH bootstrap di atas karena bootstrap itu yang membuat
        // akun demo eo/artist pada database yang belum memilikinya.
        var rotatedSeedAccounts = SeedAccountHardening.Ensure(
            db,
            app.Environment.IsDevelopment(),
            Environment.GetEnvironmentVariable(SeedAccountHardening.PasswordOverrideVariable));

        foreach (var email in rotatedSeedAccounts)
        {
            app.Logger.LogWarning(
                "Akun demo {Email} memakai password default — password telah diacak otomatis. " +
                "Reset password akun ini sebelum dipakai (atau set env var {Variable} untuk memilih password sendiri).",
                email, SeedAccountHardening.PasswordOverrideVariable);
        }
    }
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("AllowAll");
app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();
