using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using NotariaParroquial.Data;
using NotariaParroquial.Models;
using NotariaParroquial.Services;
using Resend;

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
builder.Services.AddResend(o =>
{
    o.ApiToken = builder.Configuration["Resend:ApiKey"] ?? string.Empty;
});
builder.Services.AddScoped<ICorreoServicio, CorreoServicio>();
builder.Services.AddControllersWithViews();

var app = builder.Build();

// ──────────────────────────────
// Migrate + Seed
// ──────────────────────────────
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();

    // If PostgreSQL tables already exist with wrong SQLite-generated types (e.g. Activo=integer
    // instead of boolean), force a full schema reset before running migration.
    if (usePostgres)
    {
        try
        {
            var conn = db.Database.GetDbConnection();
            if (conn.State != System.Data.ConnectionState.Open) await conn.OpenAsync();
            using var cmd = conn.CreateCommand();
            cmd.CommandText = "SELECT data_type FROM information_schema.columns " +
                              "WHERE table_schema='public' AND table_name='AspNetUsers' AND column_name='Activo'";
            var colType = await cmd.ExecuteScalarAsync() as string;
            if (conn.State == System.Data.ConnectionState.Open) await conn.CloseAsync();

            if (colType == "integer")
            {
                logger.LogWarning("Wrong column types detected (bool stored as integer) — resetting schema.");
                db.Database.ExecuteSqlRaw("DROP SCHEMA IF EXISTS public CASCADE");
                db.Database.ExecuteSqlRaw("CREATE SCHEMA public");
            }
        }
        catch { /* table doesn't exist yet — migration will create it correctly */ }
    }

    try
    {
        db.Database.Migrate();
    }
    catch (Exception ex) when (SchemaAlreadyExists(ex))
    {
        logger.LogWarning("Schema conflict on Railway — resetting database. Error: {Msg}", ex.Message);
        try
        {
            // Fastest reset: drop and recreate the whole schema.
            db.Database.ExecuteSqlRaw("DROP SCHEMA IF EXISTS public CASCADE");
            db.Database.ExecuteSqlRaw("CREATE SCHEMA public");
        }
        catch (Exception dropEx)
        {
            // DROP SCHEMA failed (permissions) — fall back to dropping tables individually.
            logger.LogWarning("DROP SCHEMA failed ({Msg}), dropping tables one by one.", dropEx.Message);
            string[] tables =
            [
                "AspNetUserTokens", "AspNetUserRoles", "AspNetUserLogins",
                "AspNetUserClaims", "AspNetRoleClaims",
                "Pagos", "Matrimonios", "Confirmaciones", "PrimerasComuniones",
                "Bautizos", "Feligreses", "AspNetUsers", "AspNetRoles",
                "__EFMigrationsHistory"
            ];
            foreach (var t in tables)
                db.Database.ExecuteSqlRaw($"DROP TABLE IF EXISTS \"{t}\" CASCADE");
        }
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

// Walk the full exception chain so nested PostgresException messages are found.
static bool SchemaAlreadyExists(Exception ex)
{
    for (var e = ex; e != null; e = e.InnerException)
        if (e.Message.Contains("already exists") || e.Message.Contains("42P07"))
            return true;
    return false;
}

// ──────────────────────────────
// Pipeline
// ──────────────────────────────
// Railway (and most PaaS) terminate TLS at the proxy and forward HTTP internally.
// Clearing KnownNetworks/KnownProxies makes ASP.NET Core trust X-Forwarded-Proto
// from any upstream, so Request.Scheme = "https" and anti-CSRF Origin checks pass.
var fwdOpts = new ForwardedHeadersOptions
{
    ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
};
fwdOpts.KnownNetworks.Clear();
fwdOpts.KnownProxies.Clear();
app.UseForwardedHeaders(fwdOpts);

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
