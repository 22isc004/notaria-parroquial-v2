using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NotariaParroquial.Data;
using NotariaParroquial.Models;
using NotariaParroquial.Models.ViewModels;

namespace NotariaParroquial.Controllers;

[Authorize]
public class HomeController : Controller
{
    private readonly ApplicationDbContext _db;

    public HomeController(ApplicationDbContext db) => _db = db;

    public async Task<IActionResult> Index()
    {
        var now = DateTime.UtcNow;
        var inicioMes = new DateTime(now.Year, now.Month, 1);

        var bautizos = await _db.Bautizos.Include(b => b.Feligres).ToListAsync();
        var comuniones = await _db.PrimerasComuniones.Include(p => p.Feligres).ToListAsync();
        var confirmaciones = await _db.Confirmaciones.Include(c => c.Feligres).ToListAsync();
        var matrimonios = await _db.Matrimonios.ToListAsync();
        var pagos = await _db.Pagos.ToListAsync();

        int totalFeligreses = await _db.Feligreses.CountAsync();
        int pagosPendientes = pagos.Count(p => p.Estado == EstadoPago.Pendiente);
        decimal ingresosMes = pagos
            .Where(p => p.Estado == EstadoPago.Confirmado && p.FechaPago.HasValue &&
                        p.FechaPago.Value.Year == now.Year && p.FechaPago.Value.Month == now.Month)
            .Sum(p => p.Monto);

        int serviciosMes = bautizos.Count(b => b.FechaBautizo.Year == now.Year && b.FechaBautizo.Month == now.Month)
            + comuniones.Count(c => c.FechaComunion.Year == now.Year && c.FechaComunion.Month == now.Month)
            + confirmaciones.Count(c => c.FechaConfirmacion.Year == now.Year && c.FechaConfirmacion.Month == now.Month)
            + matrimonios.Count(m => m.FechaMatrimonio.Year == now.Year && m.FechaMatrimonio.Month == now.Month);

        // Servicios por mes (últimos 6 meses)
        var meses = Enumerable.Range(0, 6).Select(i => now.AddMonths(-5 + i)).ToList();
        var mesesLabels = meses.Select(m => m.ToString("MMM yyyy")).ToList();
        var serviciosPorMes = meses.Select(m =>
            bautizos.Count(b => b.FechaBautizo.Year == m.Year && b.FechaBautizo.Month == m.Month) +
            comuniones.Count(c => c.FechaComunion.Year == m.Year && c.FechaComunion.Month == m.Month) +
            confirmaciones.Count(c => c.FechaConfirmacion.Year == m.Year && c.FechaConfirmacion.Month == m.Month) +
            matrimonios.Count(mat => mat.FechaMatrimonio.Year == m.Year && mat.FechaMatrimonio.Month == m.Month)
        ).ToList();

        // Actividades recientes
        var actividades = new List<ActividadReciente>();
        actividades.AddRange(bautizos.OrderByDescending(b => b.FechaRegistro).Take(3).Select(b => new ActividadReciente
        {
            Nombre = b.Feligres?.NombreCompleto ?? "-",
            TipoServicio = "Bautizo",
            TipoIcono = "bi-droplet-fill",
            Fecha = b.FechaBautizo,
            Estado = b.Estado.ToString(),
            EstadoClass = GetEstadoClass(b.Estado),
            Url = $"/Bautizos/Details/{b.Id}"
        }));
        actividades.AddRange(comuniones.OrderByDescending(c => c.FechaRegistro).Take(3).Select(c => new ActividadReciente
        {
            Nombre = c.Feligres?.NombreCompleto ?? "-",
            TipoServicio = "Primera Comunión",
            TipoIcono = "bi-cup-hot-fill",
            Fecha = c.FechaComunion,
            Estado = c.Estado.ToString(),
            EstadoClass = GetEstadoClass(c.Estado),
            Url = $"/PrimeraComunion/Details/{c.Id}"
        }));
        actividades.AddRange(confirmaciones.OrderByDescending(c => c.FechaRegistro).Take(2).Select(c => new ActividadReciente
        {
            Nombre = c.Feligres?.NombreCompleto ?? "-",
            TipoServicio = "Confirmación",
            TipoIcono = "bi-shield-fill-check",
            Fecha = c.FechaConfirmacion,
            Estado = c.Estado.ToString(),
            EstadoClass = GetEstadoClass(c.Estado),
            Url = $"/Confirmaciones/Details/{c.Id}"
        }));
        actividades.AddRange(matrimonios.OrderByDescending(m => m.FechaRegistro).Take(2).Select(m => new ActividadReciente
        {
            Nombre = $"{m.Contrayente1Nombre} & {m.Contrayente2Nombre}",
            TipoServicio = "Matrimonio",
            TipoIcono = "bi-heart-fill",
            Fecha = m.FechaMatrimonio,
            Estado = m.Estado.ToString(),
            EstadoClass = GetEstadoClass(m.Estado),
            Url = $"/Matrimonios/Details/{m.Id}"
        }));

        var vm = new DashboardViewModel
        {
            TotalFeligreses = totalFeligreses,
            ServiciosMes = serviciosMes,
            PagosPendientes = pagosPendientes,
            IngresosMes = ingresosMes,
            TotalBautizos = bautizos.Count,
            TotalComuniones = comuniones.Count,
            TotalConfirmaciones = confirmaciones.Count,
            TotalMatrimonios = matrimonios.Count,
            MesesLabels = mesesLabels,
            ServiciosPorMes = serviciosPorMes,
            ActividadesRecientes = actividades.OrderByDescending(a => a.Fecha).Take(8).ToList()
        };

        return View(vm);
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error() => View();

    private static string GetEstadoClass(EstadoSolicitud estado) => estado switch
    {
        EstadoSolicitud.Pendiente => "badge-warning",
        EstadoSolicitud.Programado => "badge-info",
        EstadoSolicitud.Completado => "badge-success",
        EstadoSolicitud.Cancelado => "badge-danger",
        _ => "badge-secondary"
    };
}
