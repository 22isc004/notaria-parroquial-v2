using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using NotariaParroquial.Data;
using NotariaParroquial.Models;

namespace NotariaParroquial.Controllers;

[Authorize]
public class BautizosController : Controller
{
    private readonly ApplicationDbContext _db;
    public BautizosController(ApplicationDbContext db) => _db = db;

    public async Task<IActionResult> Index(string? q, EstadoSolicitud? estado, int page = 1)
    {
        const int pageSize = 12;
        var query = _db.Bautizos.Include(b => b.Feligres).AsQueryable();

        if (!string.IsNullOrWhiteSpace(q))
        {
            q = q.Trim();
            query = query.Where(b => b.Feligres!.Nombre.Contains(q) ||
                                     b.Feligres!.ApellidoPaterno.Contains(q) ||
                                     (b.NumeroBoleta != null && b.NumeroBoleta.Contains(q)));
        }
        if (estado.HasValue) query = query.Where(b => b.Estado == estado);

        int total = await query.CountAsync();
        var items = await query.OrderByDescending(b => b.FechaBautizo)
            .Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();

        ViewBag.Query = q;
        ViewBag.Estado = estado;
        ViewBag.Page = page;
        ViewBag.TotalPages = (int)Math.Ceiling(total / (double)pageSize);
        ViewBag.Total = total;
        return View(items);
    }

    public async Task<IActionResult> Create()
    {
        await LoadFeligresSelectAsync();
        return View(new Bautizo { FechaBautizo = DateOnly.FromDateTime(DateTime.Today) });
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Bautizo model)
    {
        if (!ModelState.IsValid) { await LoadFeligresSelectAsync(); return View(model); }
        model.NumeroBoleta = await GenerateBoleta("BAU");
        _db.Bautizos.Add(model);
        await _db.SaveChangesAsync();
        TempData["Toast"] = "success|Bautizo registrado correctamente.";
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Edit(int id)
    {
        var item = await _db.Bautizos.FindAsync(id);
        if (item == null) return NotFound();
        await LoadFeligresSelectAsync();
        return View(item);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, Bautizo model)
    {
        if (id != model.Id) return BadRequest();
        if (!ModelState.IsValid) { await LoadFeligresSelectAsync(); return View(model); }
        var item = await _db.Bautizos.FindAsync(id);
        if (item == null) return NotFound();
        item.FeligresId    = model.FeligresId;
        item.FechaBautizo  = model.FechaBautizo;
        item.Sacerdote     = model.Sacerdote;
        item.PadrinoNombre = model.PadrinoNombre;
        item.MadrinaNombre = model.MadrinaNombre;
        item.LugarBautizo  = model.LugarBautizo;
        item.Estado        = model.Estado;
        item.Observaciones = model.Observaciones;
        await _db.SaveChangesAsync();
        TempData["Toast"] = "success|Bautizo actualizado.";
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Details(int id)
    {
        var item = await _db.Bautizos.Include(b => b.Feligres).Include(b => b.Pago)
            .FirstOrDefaultAsync(b => b.Id == id);
        if (item == null) return NotFound();
        return View(item);
    }

    public async Task<IActionResult> Acta(int id)
    {
        var item = await _db.Bautizos.Include(b => b.Feligres)
            .FirstOrDefaultAsync(b => b.Id == id);
        if (item == null) return NotFound();
        return View(item);
    }

    [HttpPost, ValidateAntiForgeryToken, Authorize(Roles = "Administrador")]
    public async Task<IActionResult> Delete(int id)
    {
        var item = await _db.Bautizos.FindAsync(id);
        if (item == null) return NotFound();
        item.IsDeleted = true;
        await _db.SaveChangesAsync();
        TempData["Toast"] = "info|Bautizo eliminado.";
        return RedirectToAction(nameof(Index));
    }

    private async Task LoadFeligresSelectAsync()
    {
        var lista = await _db.Feligreses.OrderBy(f => f.ApellidoPaterno).ToListAsync();
        ViewBag.FeligresList = new SelectList(lista, "Id", "NombreCompleto");
    }

    private async Task<string> GenerateBoleta(string prefix)
    {
        int total = await _db.Bautizos.IgnoreQueryFilters().CountAsync();
        return $"{prefix}-{DateTime.Now.Year}-{(total + 1):D4}";
    }
}
