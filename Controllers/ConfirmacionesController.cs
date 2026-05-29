using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using NotariaParroquial.Data;
using NotariaParroquial.Models;

namespace NotariaParroquial.Controllers;

[Authorize]
public class ConfirmacionesController : Controller
{
    private readonly ApplicationDbContext _db;
    public ConfirmacionesController(ApplicationDbContext db) => _db = db;

    public async Task<IActionResult> Index(string? q, EstadoSolicitud? estado, int page = 1)
    {
        const int pageSize = 12;
        var query = _db.Confirmaciones.Include(c => c.Feligres).AsQueryable();
        if (!string.IsNullOrWhiteSpace(q))
        {
            q = q.Trim();
            query = query.Where(c => c.Feligres!.Nombre.Contains(q) || c.Feligres!.ApellidoPaterno.Contains(q) ||
                                     (c.NumeroBoleta != null && c.NumeroBoleta.Contains(q)));
        }
        if (estado.HasValue) query = query.Where(c => c.Estado == estado);
        int total = await query.CountAsync();
        var items = await query.OrderByDescending(c => c.FechaConfirmacion)
            .Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();
        ViewBag.Query = q; ViewBag.Estado = estado;
        ViewBag.Page = page; ViewBag.TotalPages = (int)Math.Ceiling(total / (double)pageSize); ViewBag.Total = total;
        return View(items);
    }

    public async Task<IActionResult> Create()
    {
        await LoadSelectAsync();
        return View(new Confirmacion { FechaConfirmacion = DateOnly.FromDateTime(DateTime.Today) });
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Confirmacion model)
    {
        if (!ModelState.IsValid) { await LoadSelectAsync(); return View(model); }
        model.NumeroBoleta = await GenerateBoleta();
        _db.Confirmaciones.Add(model);
        await _db.SaveChangesAsync();
        TempData["Toast"] = "success|Confirmación registrada correctamente.";
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Edit(int id)
    {
        var item = await _db.Confirmaciones.FindAsync(id);
        if (item == null) return NotFound();
        await LoadSelectAsync();
        return View(item);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, Confirmacion model)
    {
        if (id != model.Id) return BadRequest();
        if (!ModelState.IsValid) { await LoadSelectAsync(); return View(model); }
        _db.Update(model);
        await _db.SaveChangesAsync();
        TempData["Toast"] = "success|Confirmación actualizada.";
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Details(int id)
    {
        var item = await _db.Confirmaciones.Include(c => c.Feligres).Include(c => c.Pago)
            .FirstOrDefaultAsync(c => c.Id == id);
        if (item == null) return NotFound();
        return View(item);
    }

    public async Task<IActionResult> Acta(int id)
    {
        var item = await _db.Confirmaciones.Include(c => c.Feligres)
            .FirstOrDefaultAsync(c => c.Id == id);
        if (item == null) return NotFound();
        return View(item);
    }

    [HttpPost, ValidateAntiForgeryToken, Authorize(Roles = "Administrador")]
    public async Task<IActionResult> Delete(int id)
    {
        var item = await _db.Confirmaciones.FindAsync(id);
        if (item == null) return NotFound();
        item.IsDeleted = true;
        await _db.SaveChangesAsync();
        TempData["Toast"] = "info|Registro eliminado.";
        return RedirectToAction(nameof(Index));
    }

    private async Task LoadSelectAsync()
    {
        var lista = await _db.Feligreses.OrderBy(f => f.ApellidoPaterno).ToListAsync();
        ViewBag.FeligresList = new SelectList(lista, "Id", "NombreCompleto");
    }

    private async Task<string> GenerateBoleta()
    {
        int total = await _db.Confirmaciones.IgnoreQueryFilters().CountAsync();
        return $"CON-{DateTime.Now.Year}-{(total + 1):D4}";
    }
}
