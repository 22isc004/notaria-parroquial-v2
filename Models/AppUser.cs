using Microsoft.AspNetCore.Identity;

namespace NotariaParroquial.Models;

public class AppUser : IdentityUser
{
    public string NombreCompleto { get; set; } = string.Empty;
    public string Rol { get; set; } = "Operador";
    public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;
    public bool Activo { get; set; } = true;
}
