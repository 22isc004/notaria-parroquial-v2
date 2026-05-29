using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NotariaParroquial.Models;
using NotariaParroquial.Models.ViewModels;

namespace NotariaParroquial.Controllers;

[Authorize(Roles = "Administrador")]
public class UsuariosController : Controller
{
    private readonly UserManager<AppUser> _users;
    private readonly RoleManager<IdentityRole> _roles;

    public UsuariosController(UserManager<AppUser> users, RoleManager<IdentityRole> roles)
    {
        _users = users;
        _roles = roles;
    }

    public async Task<IActionResult> Index()
    {
        var usuarios = await _users.Users.OrderBy(u => u.NombreCompleto).ToListAsync();
        return View(usuarios);
    }

    [HttpGet]
    public IActionResult Create() => View(new CrearUsuarioViewModel());

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CrearUsuarioViewModel model)
    {
        if (!ModelState.IsValid) return View(model);

        if (await _users.FindByEmailAsync(model.Email) != null)
        {
            ModelState.AddModelError("Email", "Ya existe un usuario con ese correo.");
            return View(model);
        }

        var user = new AppUser
        {
            UserName = model.Email,
            Email = model.Email,
            NombreCompleto = model.NombreCompleto,
            Rol = model.Rol,
            EmailConfirmed = true,
            Activo = true
        };

        var result = await _users.CreateAsync(user, model.Password);
        if (!result.Succeeded)
        {
            foreach (var e in result.Errors)
                ModelState.AddModelError(string.Empty, e.Description);
            return View(model);
        }

        await _users.AddToRoleAsync(user, model.Rol);
        TempData["Toast"] = "success|Usuario creado correctamente.";
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> Edit(string id)
    {
        var user = await _users.FindByIdAsync(id);
        if (user == null) return NotFound();

        return View(new EditarUsuarioViewModel
        {
            Id = user.Id,
            NombreCompleto = user.NombreCompleto,
            Email = user.Email ?? string.Empty,
            Rol = user.Rol,
            Activo = user.Activo
        });
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(EditarUsuarioViewModel model)
    {
        if (!ModelState.IsValid) return View(model);

        var user = await _users.FindByIdAsync(model.Id);
        if (user == null) return NotFound();

        var rolAnterior = user.Rol;
        user.NombreCompleto = model.NombreCompleto;
        user.Rol = model.Rol;
        user.Activo = model.Activo;

        var result = await _users.UpdateAsync(user);
        if (!result.Succeeded)
        {
            foreach (var e in result.Errors)
                ModelState.AddModelError(string.Empty, e.Description);
            return View(model);
        }

        if (rolAnterior != model.Rol)
        {
            await _users.RemoveFromRoleAsync(user, rolAnterior);
            await _users.AddToRoleAsync(user, model.Rol);
        }

        TempData["Toast"] = "success|Usuario actualizado correctamente.";
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> CambiarPassword(string id)
    {
        var user = await _users.FindByIdAsync(id);
        if (user == null) return NotFound();

        return View(new CambiarPasswordViewModel
        {
            Id = user.Id,
            NombreCompleto = user.NombreCompleto
        });
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> CambiarPassword(CambiarPasswordViewModel model)
    {
        if (!ModelState.IsValid) return View(model);

        var user = await _users.FindByIdAsync(model.Id);
        if (user == null) return NotFound();

        var token = await _users.GeneratePasswordResetTokenAsync(user);
        var result = await _users.ResetPasswordAsync(user, token, model.NuevaPassword);

        if (!result.Succeeded)
        {
            foreach (var e in result.Errors)
                ModelState.AddModelError(string.Empty, e.Description);
            return View(model);
        }

        TempData["Toast"] = "success|Contraseña restablecida correctamente.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Desactivar(string id)
    {
        var user = await _users.FindByIdAsync(id);
        if (user == null) return NotFound();

        var currentUserId = _users.GetUserId(User);
        if (user.Id == currentUserId)
        {
            TempData["Toast"] = "warning|No puedes desactivar tu propia cuenta.";
            return RedirectToAction(nameof(Index));
        }

        user.Activo = !user.Activo;
        await _users.UpdateAsync(user);

        var msg = user.Activo ? "Usuario activado." : "Usuario desactivado.";
        TempData["Toast"] = $"info|{msg}";
        return RedirectToAction(nameof(Index));
    }
}
