using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NotariaParroquial.Data;
using NotariaParroquial.Models;

namespace NotariaParroquial.Controllers;

[Authorize]
public class MatrimoniosController : Controller
{
    private readonly ApplicationDbContext _db;
    public MatrimoniosController(ApplicationDbContext db) => _db = db;

    public async Task<IActionResult> Index(string? q, EstadoSolicitud? estado, int page = 1)
    {
        const int pageSize = 12;
        var query = _db.Matrimonios.AsQueryable();
        if (!string.IsNullOrWhiteSpace(q))
        {
            q = q.Trim();
            query = query.Where(m => m.Contrayente1Nombre.Contains(q) || m.Contrayente2Nombre.Contains(q) ||
                                     (m.NumeroActa != null && m.NumeroActa.Contains(q)));
        }
        if (estado.HasValue) query = query.Where(m => m.Estado == estado);
        int total = await query.CountAsync();
        var items = await query.OrderByDescending(m => m.FechaMatrimonio)
            .Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();
        ViewBag.Query = q; ViewBag.Estado = estado;
        ViewBag.Page = page; ViewBag.TotalPages = (int)Math.Ceiling(total / (double)pageSize); ViewBag.Total = total;
        return View(items);
    }

    public IActionResult Create() => View(new Matrimonio { FechaMatrimonio = DateOnly.FromDateTime(DateTime.Today) });

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Matrimonio model)
    {
        if (!ModelState.IsValid) return View(model);
        model.NumeroActa = await GenerateActa();
        _db.Matrimonios.Add(model);
        await _db.SaveChangesAsync();
        TempData["Toast"] = "success|Matrimonio registrado correctamente.";
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Edit(int id)
    {
        var item = await _db.Matrimonios.FindAsync(id);
        if (item == null) return NotFound();
        return View(item);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, Matrimonio model)
    {
        if (id != model.Id) return BadRequest();
        if (!ModelState.IsValid) return View(model);
        _db.Update(model);
        await _db.SaveChangesAsync();
        TempData["Toast"] = "success|Matrimonio actualizado.";
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Details(int id)
    {
        var item = await _db.Matrimonios.Include(m => m.Pago).FirstOrDefaultAsync(m => m.Id == id);
        if (item == null) return NotFound();
        return View(item);
    }

    [HttpPost, ValidateAntiForgeryToken, Authorize(Roles = "Administrador")]
    public async Task<IActionResult> Delete(int id)
    {
        var item = await _db.Matrimonios.FindAsync(id);
        if (item == null) return NotFound();
        item.IsDeleted = true;
        await _db.SaveChangesAsync();
        TempData["Toast"] = "info|Registro eliminado.";
        return RedirectToAction(nameof(Index));
    }

    private async Task<string> GenerateActa()
    {
        int total = await _db.Matrimonios.IgnoreQueryFilters().CountAsync();
        return $"MAT-{DateTime.Now.Year}-{(total + 1):D4}";
    }
}
