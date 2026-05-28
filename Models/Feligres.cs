using System.ComponentModel.DataAnnotations;

namespace NotariaParroquial.Models;

public class Feligres
{
    public int Id { get; set; }

    [Required, StringLength(80)]
    [Display(Name = "Nombre(s)")]
    public string Nombre { get; set; } = string.Empty;

    [Required, StringLength(80)]
    [Display(Name = "Apellido Paterno")]
    public string ApellidoPaterno { get; set; } = string.Empty;

    [StringLength(80)]
    [Display(Name = "Apellido Materno")]
    public string? ApellidoMaterno { get; set; }

    [Required]
    [Display(Name = "Fecha de Nacimiento")]
    [DataType(DataType.Date)]
    public DateOnly FechaNacimiento { get; set; }

    [Required]
    [Display(Name = "Género")]
    public Genero Genero { get; set; }

    [StringLength(18)]
    [Display(Name = "CURP")]
    public string? Curp { get; set; }

    [StringLength(20)]
    [Display(Name = "Teléfono")]
    public string? Telefono { get; set; }

    [StringLength(150)]
    [EmailAddress]
    [Display(Name = "Correo Electrónico")]
    public string? Email { get; set; }

    [StringLength(200)]
    [Display(Name = "Dirección")]
    public string? Direccion { get; set; }

    [Required, StringLength(100)]
    [Display(Name = "Comunidad")]
    public string Comunidad { get; set; } = string.Empty;

    [StringLength(100)]
    [Display(Name = "Municipio")]
    public string Municipio { get; set; } = "Axtla de Terrazas";

    [Display(Name = "Fecha de Registro")]
    public DateTime FechaRegistro { get; set; } = DateTime.UtcNow;

    public bool IsDeleted { get; set; } = false;

    public string NombreCompleto => $"{Nombre} {ApellidoPaterno} {ApellidoMaterno}".Trim();

    // Navigation
    public ICollection<Bautizo> Bautizos { get; set; } = new List<Bautizo>();
    public ICollection<PrimeraComunion> PrimerasComuniones { get; set; } = new List<PrimeraComunion>();
    public ICollection<Confirmacion> Confirmaciones { get; set; } = new List<Confirmacion>();
}
