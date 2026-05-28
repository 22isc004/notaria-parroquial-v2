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
        // EnsureDeleted does not work on Railway's hosted PostgreSQL.
        // Drop all tables with raw SQL, then migrate fresh.
        logger.LogWarning("Schema mismatch detected — dropping all tables. ex={Msg}", ex.Message);
        db.Database.ExecuteSqlRaw("""
            DO $$ DECLARE r RECORD;
            BEGIN
                FOR r IN (SELECT tablename FROM pg_tables WHERE schemaname = 'public') LOOP
                    EXECUTE 'DROP TABLE IF EXISTS "' || r.tablename || '" CASCADE';
                END LOOP;
            END $$;
            """);
        db.Database.Migrate();
    }

    try
    {
        var um = scope.ServiceProvider.GetRequiredService<UserManager<AppUser>>();
        var rm = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
        await DatabaseSeeder.SeedAsync(um, rm);
    }
    catch (Exception seedEx)
    {
        logger.LogError(seedEx, "Seeder failed — will retry on next startup.");
    }
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
