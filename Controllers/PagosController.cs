using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NotariaParroquial.Data;
using NotariaParroquial.Models;
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
        return View(items);
    }

    public IActionResult Create() => View(new Pago { FechaPago = DateOnly.FromDateTime(DateTime.Today) });

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Pago model)
    {
        if (!ModelState.IsValid) return View(model);
        model.Referencia = GenerateRef();
        _db.Pagos.Add(model);
        await _db.SaveChangesAsync();
        TempData["Toast"] = "success|Pago registrado. Pendiente de confirmación.";
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Details(int id)
    {
        var item = await _db.Pagos.FindAsync(id);
        if (item == null) return NotFound();
        return View(item);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Confirmar(int id)
    {
        var pago = await _db.Pagos.FindAsync(id);
        if (pago == null) return NotFound();

        pago.Estado = EstadoPago.Confirmado;
        pago.FechaPago = DateOnly.FromDateTime(DateTime.Today);
        await _db.SaveChangesAsync();

        if (!string.IsNullOrEmpty(pago.EmailNotificacion))
        {
            await _correo.EnviarConfirmacionPagoAsync(
                pago.EmailNotificacion,
                pago.NombreSolicitante,
                pago.TipoServicio.ToString(),
                pago.Referencia ?? "-",
                pago.Monto,
                pago.FechaPago.Value);
        }

        TempData["Toast"] = "success|Pago confirmado y correo enviado al feligrés.";
        return RedirectToAction(nameof(Details), new { id });
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
