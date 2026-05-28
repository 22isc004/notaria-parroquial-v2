using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using NotariaParroquial.Data;
using NotariaParroquial.Models;
using NotariaParroquial.Services;

var builder = WebApplication.CreateBuilder(args);

// ──────────────────────────────
// Database
// ──────────────────────────────
var databaseUrl = Environment.GetEnvironmentVariable("DATABASE_URL");
var rawConn = !string.IsNullOrWhiteSpace(databaseUrl)
    ? databaseUrl
    : builder.Configuration.GetConnectionString("DefaultConnection") ?? "Data Source=notaria.db";

bool usePostgres = rawConn.StartsWith("postgres", StringComparison.OrdinalIgnoreCase)
                || rawConn.StartsWith("Host=", StringComparison.OrdinalIgnoreCase);

if (usePostgres)
{
    string pgConn = rawConn;
    if (rawConn.StartsWith("postgres://") || rawConn.StartsWith("postgresql://"))
    {
        var uri = new Uri(rawConn);
        var ui = uri.UserInfo.Split(':');
        pgConn = $"Host={uri.Host};Port={uri.Port};Database={uri.AbsolutePath.TrimStart('/')};Username={ui[0]};Password={ui[1]};SSL Mode=Require;Trust Server Certificate=true";
    }
    builder.Services.AddDbContext<ApplicationDbContext>(o => o.UseNpgsql(pgConn));
}
else
{
    builder.Services.AddDbContext<ApplicationDbContext>(o => o.UseSqlite(rawConn));
}

// ──────────────────────────────
// Identity
// ──────────────────────────────
builder.Services.AddIdentity<AppUser, IdentityRole>(o =>
{
    o.Password.RequireDigit = false;
    o.Password.RequiredLength = 6;
    o.Password.RequireNonAlphanumeric = false;
    o.Password.RequireUppercase = false;
    o.SignIn.RequireConfirmedAccount = false;
})
.AddEntityFrameworkStores<ApplicationDbContext>()
.AddDefaultTokenProviders();

builder.Services.ConfigureApplicationCookie(o =>
{
    o.LoginPath = "/Account/Login";
    o.LogoutPath = "/Account/Logout";
    o.AccessDeniedPath = "/Account/AccessDenied";
    o.ExpireTimeSpan = TimeSpan.FromHours(8);
    o.SlidingExpiration = true;
});

// ──────────────────────────────
// Services
// ──────────────────────────────
builder.Services.AddHttpClient("Brevo");
builder.Services.AddScoped<IEmailService, BrevoEmailService>();
builder.Services.AddControllersWithViews();

var app = builder.Build();

// ──────────────────────────────
// Migrate + Seed
// ──────────────────────────────
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();

    try
    {
        db.Database.Migrate();
    }
    catch (Exception ex) when (SchemaAlreadyExists(ex))
    {
        // Schema exists from a previous non-migration setup.
        // Record the migration as already applied so future deployments run correctly.
        logger.LogWarning("Schema already exists — marking migration as applied.");
        try
        {
            db.Database.ExecuteSqlRaw("""
                CREATE TABLE IF NOT EXISTS "__EFMigrationsHistory" (
                    "MigrationId" character varying(150) NOT NULL,
                    "ProductVersion" character varying(32) NOT NULL,
                    CONSTRAINT "PK___EFMigrationsHistory" PRIMARY KEY ("MigrationId")
                );
                INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
                VALUES ('20260528005126_InitialCreate', '8.0.5')
                ON CONFLICT ("MigrationId") DO NOTHING;
                """);
        }
        catch (Exception inner)
        {
            logger.LogError(inner, "Could not record migration history.");
        }
    }

    var um = scope.ServiceProvider.GetRequiredService<UserManager<AppUser>>();
    var rm = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
    await DatabaseSeeder.SeedAsync(um, rm);
}

static bool SchemaAlreadyExists(Exception ex)
{
    var msg = ex.Message + (ex.InnerException?.Message ?? string.Empty);
    return msg.Contains("already exists") || msg.Contains("42P07");
}

// ──────────────────────────────
// Pipeline
// ──────────────────────────────
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseStaticFiles();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

var port = Environment.GetEnvironmentVariable("PORT") ?? "8080";
app.Run($"http://0.0.0.0:{port}");
