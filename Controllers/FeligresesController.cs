using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NotariaParroquial.Data;
using NotariaParroquial.Models;

namespace NotariaParroquial.Controllers;

[Authorize]
public class FeligresesController : Controller
{
    private readonly ApplicationDbContext _db;
    public FeligresesController(ApplicationDbContext db) => _db = db;

    public async Task<IActionResult> Index(string? q, int page = 1)
    {
        const int pageSize = 12;
        var query = _db.Feligreses.AsQueryable();

        if (!string.IsNullOrWhiteSpace(q))
        {
            q = q.Trim();
            query = query.Where(f => f.Nombre.Contains(q) || f.ApellidoPaterno.Contains(q) ||
                                     (f.ApellidoMaterno != null && f.ApellidoMaterno.Contains(q)) ||
                                     f.Comunidad.Contains(q) ||
                                     (f.Curp != null && f.Curp.Contains(q)));
        }

        int total = await query.CountAsync();
        var items = await query.OrderByDescending(f => f.FechaRegistro)
            .Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();

        ViewBag.Query = q;
        ViewBag.Page = page;
        ViewBag.TotalPages = (int)Math.Ceiling(total / (double)pageSize);
        ViewBag.Total = total;
        return View(items);
    }

    public IActionResult Create() => View(new Feligres { FechaNacimiento = DateOnly.FromDateTime(DateTime.Today.AddYears(-18)) });

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Feligres model)
    {
        if (!ModelState.IsValid) return View(model);
        _db.Feligreses.Add(model);
        await _db.SaveChangesAsync();
        TempData["Toast"] = "success|Feligrés registrado correctamente.";
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Edit(int id)
    {
        var item = await _db.Feligreses.FindAsync(id);
        if (item == null) return NotFound();
        return View(item);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, Feligres model)
    {
        if (id != model.Id) return BadRequest();
        if (!ModelState.IsValid) return View(model);

        _db.Update(model);
        await _db.SaveChangesAsync();
        TempData["Toast"] = "success|Feligrés actualizado correctamente.";
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Details(int id)
    {
        var item = await _db.Feligreses
            .Include(f => f.Bautizos)
            .Include(f => f.PrimerasComuniones)
            .Include(f => f.Confirmaciones)
            .FirstOrDefaultAsync(f => f.Id == id);
        if (item == null) return NotFound();
        return View(item);
    }

    [HttpPost, ValidateAntiForgeryToken, Authorize(Roles = "Administrador")]
    public async Task<IActionResult> Delete(int id)
    {
        var item = await _db.Feligreses.FindAsync(id);
        if (item == null) return NotFound();
        item.IsDeleted = true;
        await _db.SaveChangesAsync();
        TempData["Toast"] = "info|Feligrés eliminado del registro.";
        return RedirectToAction(nameof(Index));
    }
}
