using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using NotariaParroquial.Models;

namespace NotariaParroquial.Data;

public class ApplicationDbContext : IdentityDbContext<AppUser>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

    public DbSet<Feligres> Feligreses { get; set; }
    public DbSet<Bautizo> Bautizos { get; set; }
    public DbSet<PrimeraComunion> PrimerasComuniones { get; set; }
    public DbSet<Confirmacion> Confirmaciones { get; set; }
    public DbSet<Matrimonio> Matrimonios { get; set; }
    public DbSet<Pago> Pagos { get; set; }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<Feligres>().HasQueryFilter(f => !f.IsDeleted);
        builder.Entity<Bautizo>().HasQueryFilter(b => !b.IsDeleted);
        builder.Entity<PrimeraComunion>().HasQueryFilter(p => !p.IsDeleted);
        builder.Entity<Confirmacion>().HasQueryFilter(c => !c.IsDeleted);
        builder.Entity<Matrimonio>().HasQueryFilter(m => !m.IsDeleted);
        builder.Entity<Pago>().HasQueryFilter(p => !p.IsDeleted);

        builder.Entity<Bautizo>()
            .HasOne(b => b.Feligres)
            .WithMany(f => f.Bautizos)
            .HasForeignKey(b => b.FeligresId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<PrimeraComunion>()
            .HasOne(p => p.Feligres)
            .WithMany(f => f.PrimerasComuniones)
            .HasForeignKey(p => p.FeligresId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<Confirmacion>()
            .HasOne(c => c.Feligres)
            .WithMany(f => f.Confirmaciones)
            .HasForeignKey(c => c.FeligresId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<Bautizo>()
            .HasOne(b => b.Pago)
            .WithOne(p => p.Bautizo)
            .HasForeignKey<Bautizo>(b => b.PagoId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.Entity<PrimeraComunion>()
            .HasOne(p => p.Pago)
            .WithOne(pago => pago.PrimeraComunion)
            .HasForeignKey<PrimeraComunion>(p => p.PagoId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.Entity<Confirmacion>()
            .HasOne(c => c.Pago)
            .WithOne(p => p.Confirmacion)
            .HasForeignKey<Confirmacion>(c => c.PagoId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.Entity<Matrimonio>()
            .HasOne(m => m.Pago)
            .WithOne(p => p.Matrimonio)
            .HasForeignKey<Matrimonio>(m => m.PagoId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
