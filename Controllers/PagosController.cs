using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NotariaParroquial.Data;
using NotariaParroquial.Models;
using NotariaParroquial.Models.ViewModels;
using NotariaParroquial.Services;

namespace NotariaParroquial.Controllers;

[Authorize]
public class PagosController : Controller
{
    private readonly ApplicationDbContext _db;
    private readonly ICorreoServicio _correo;

    public PagosController(ApplicationDbContext db, ICorreoServicio correo)
    {
        _db = db;
        _correo = correo;
    }

    public async Task<IActionResult> Index(string? q, EstadoPago? estado, int page = 1)
    {
        const int pageSize = 12;
        var query = _db.Pagos.AsQueryable();
        if (!string.IsNullOrWhiteSpace(q))
        {
            q = q.Trim();
            query = query.Where(p => p.NombreSolicitante.Contains(q) ||
                                     (p.Referencia != null && p.Referencia.Contains(q)));
        }
        if (estado.HasValue) query = query.Where(p => p.Estado == estado);
        int total = await query.CountAsync();
        var items = await query.OrderByDescending(p => p.FechaRegistro)
            .Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();
        ViewBag.Query = q; ViewBag.Estado = estado;
        ViewBag.Page = page; ViewBag.TotalPages = (int)Math.Ceiling(total / (double)pageSize); ViewBag.Total = total;

        // Sacramentos sin pago vinculado
        var sinPago = new List<ServicioSinPago>();

        var bautizos = await _db.Bautizos.Include(b => b.Feligres)
            .Where(b => b.PagoId == null && b.Estado != EstadoSolicitud.Cancelado)
            .OrderByDescending(b => b.FechaRegistro).ToListAsync();
        sinPago.AddRange(bautizos.Select(b => new ServicioSinPago
        {
            Tipo = "Bautizo", TipoIcono = "bi-droplet-fill", TipoColor = "text-primary",
            Id = b.Id, Nombre = b.Feligres?.NombreCompleto ?? "—",
            Folio = b.NumeroBoleta, Fecha = b.FechaBautizo, Estado = b.Estado
        }));

        var comuniones = await _db.PrimerasComuniones.Include(p => p.Feligres)
            .Where(p => p.PagoId == null && p.Estado != EstadoSolicitud.Cancelado)
            .OrderByDescending(p => p.FechaRegistro).ToListAsync();
        sinPago.AddRange(comuniones.Select(c => new ServicioSinPago
        {
            Tipo = "PrimeraComunion", TipoIcono = "bi-cup-hot-fill", TipoColor = "text-success",
            Id = c.Id, Nombre = c.Feligres?.NombreCompleto ?? "—",
            Folio = c.NumeroBoleta, Fecha = c.FechaComunion, Estado = c.Estado
        }));

        var confirmaciones = await _db.Confirmaciones.Include(c => c.Feligres)
            .Where(c => c.PagoId == null && c.Estado != EstadoSolicitud.Cancelado)
            .OrderByDescending(c => c.FechaRegistro).ToListAsync();
        sinPago.AddRange(confirmaciones.Select(c => new ServicioSinPago
        {
            Tipo = "Confirmacion", TipoIcono = "bi-shield-fill-check", TipoColor = "text-warning",
            Id = c.Id, Nombre = c.Feligres?.NombreCompleto ?? "—",
            Folio = c.NumeroBoleta, Fecha = c.FechaConfirmacion, Estado = c.Estado
        }));

        var matrimonios = await _db.Matrimonios
            .Where(m => m.PagoId == null && m.Estado != EstadoSolicitud.Cancelado)
            .OrderByDescending(m => m.FechaRegistro).ToListAsync();
        sinPago.AddRange(matrimonios.Select(m => new ServicioSinPago
        {
            Tipo = "Matrimonio", TipoIcono = "bi-heart-fill", TipoColor = "text-danger",
            Id = m.Id, Nombre = $"{m.Contrayente1Nombre} & {m.Contrayente2Nombre}",
            Folio = m.NumeroActa, Fecha = m.FechaMatrimonio, Estado = m.Estado
        }));

        ViewBag.SinPago = sinPago.OrderByDescending(s => s.Fecha).ToList();
        return View(items);
    }

    [HttpGet]
    public async Task<IActionResult> Create(string? tipo, int? sacramentoId)
    {
        var pago = new Pago { FechaPago = DateOnly.FromDateTime(DateTime.Today) };

        // Pre-fill from sacramento context
        if (!string.IsNullOrEmpty(tipo) && sacramentoId.HasValue)
        {
            switch (tipo)
            {
                case "Bautizo":
                    var b = await _db.Bautizos.Include(x => x.Feligres).FirstOrDefaultAsync(x => x.Id == sacramentoId);
                    if (b != null) { pago.TipoServicio = TipoServicio.Bautizo; pago.NombreSolicitante = b.Feligres?.NombreCompleto ?? ""; }
                    break;
                case "PrimeraComunion":
                    var c = await _db.PrimerasComuniones.Include(x => x.Feligres).FirstOrDefaultAsync(x => x.Id == sacramentoId);
                    if (c != null) { pago.TipoServicio = TipoServicio.PrimeraComunion; pago.NombreSolicitante = c.Feligres?.NombreCompleto ?? ""; }
                    break;
                case "Confirmacion":
                    var cf = await _db.Confirmaciones.Include(x => x.Feligres).FirstOrDefaultAsync(x => x.Id == sacramentoId);
                    if (cf != null) { pago.TipoServicio = TipoServicio.Confirmacion; pago.NombreSolicitante = cf.Feligres?.NombreCompleto ?? ""; }
                    break;
                case "Matrimonio":
                    var m = await _db.Matrimonios.FirstOrDefaultAsync(x => x.Id == sacramentoId);
                    if (m != null) { pago.TipoServicio = TipoServicio.Matrimonio; pago.NombreSolicitante = $"{m.Contrayente1Nombre} & {m.Contrayente2Nombre}"; }
                    break;
            }
        }

        ViewBag.Tipo = tipo;
        ViewBag.SacramentoId = sacramentoId;
        return View(pago);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Pago model, string? tipo, int? sacramentoId)
    {
        if (!ModelState.IsValid)
        {
            ViewBag.Tipo = tipo;
            ViewBag.SacramentoId = sacramentoId;
            return View(model);
        }

        model.Referencia = GenerateRef();
        _db.Pagos.Add(model);
        await _db.SaveChangesAsync();

        // Vincular al sacramento de origen
        if (!string.IsNullOrEmpty(tipo) && sacramentoId.HasValue)
        {
            switch (tipo)
            {
                case "Bautizo":
                    var b = await _db.Bautizos.FindAsync(sacramentoId.Value);
                    if (b != null) b.PagoId = model.Id;
                    break;
                case "PrimeraComunion":
                    var c = await _db.PrimerasComuniones.FindAsync(sacramentoId.Value);
                    if (c != null) c.PagoId = model.Id;
                    break;
                case "Confirmacion":
                    var cf = await _db.Confirmaciones.FindAsync(sacramentoId.Value);
                    if (cf != null) cf.PagoId = model.Id;
                    break;
                case "Matrimonio":
                    var m = await _db.Matrimonios.FindAsync(sacramentoId.Value);
                    if (m != null) m.PagoId = model.Id;
                    break;
            }
            await _db.SaveChangesAsync();
        }

        TempData["Toast"] = "success|Pago registrado. Pendiente de confirmación.";
        return RedirectToAction(nameof(Details), new { id = model.Id });
    }

    public async Task<IActionResult> Details(int id)
    {
        var item = await _db.Pagos.FindAsync(id);
        if (item == null) return NotFound();
        return View(item);
    }

    public async Task<IActionResult> Ticket(int id)
    {
        var item = await _db.Pagos.FindAsync(id);
        if (item == null) return NotFound();
        return View(item);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Confirmar(int id)
    {
        var pago = await _db.Pagos.FindAsync(id);
        if (pago == null)
            return Json(new { success = false, mensaje = "Pago no encontrado." });

        pago.Estado = EstadoPago.Confirmado;
        pago.FechaPago = DateOnly.FromDateTime(DateTime.Today);
        await _db.SaveChangesAsync();

        bool correoEnviado = false;
        string mensaje = "Pago confirmado correctamente.";

        if (!string.IsNullOrEmpty(pago.EmailNotificacion))
        {
            correoEnviado = await _correo.EnviarConfirmacionPago(
                pago.EmailNotificacion, pago.NombreSolicitante,
                pago.Referencia ?? "-", pago.Monto);
            mensaje = correoEnviado
                ? "Pago confirmado y correo enviado al feligrés."
                : "Pago confirmado, pero el correo no pudo enviarse.";
        }

        return Json(new { success = true, correoEnviado, mensaje });
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Rechazar(int id)
    {
        var pago = await _db.Pagos.FindAsync(id);
        if (pago == null) return NotFound();
        pago.Estado = EstadoPago.Rechazado;
        await _db.SaveChangesAsync();
        TempData["Toast"] = "warning|Pago marcado como rechazado.";
        return RedirectToAction(nameof(Details), new { id });
    }

    [HttpPost, ValidateAntiForgeryToken, Authorize(Roles = "Administrador")]
    public async Task<IActionResult> Delete(int id)
    {
        var item = await _db.Pagos.FindAsync(id);
        if (item == null) return NotFound();
        item.IsDeleted = true;
        await _db.SaveChangesAsync();
        TempData["Toast"] = "info|Pago eliminado.";
        return RedirectToAction(nameof(Index));
    }

    private static string GenerateRef() =>
        $"REF-{DateTime.Now:yyyyMMdd}-{Random.Shared.Next(1000, 9999)}";
}
