using System.ComponentModel.DataAnnotations;

namespace NotariaParroquial.Models;

public class Feligres
{
    public int Id { get; set; }

    [Required(ErrorMessage = "El nombre es requerido.")]
    [StringLength(80, ErrorMessage = "El nombre no puede exceder 80 caracteres.")]
    [Display(Name = "Nombre(s)")]
    public string Nombre { get; set; } = string.Empty;

    [Required(ErrorMessage = "El apellido paterno es requerido.")]
    [StringLength(80, ErrorMessage = "El apellido paterno no puede exceder 80 caracteres.")]
    [Display(Name = "Apellido Paterno")]
    public string ApellidoPaterno { get; set; } = string.Empty;

    [Required(ErrorMessage = "El apellido materno es requerido.")]
    [StringLength(80, ErrorMessage = "El apellido materno no puede exceder 80 caracteres.")]
    [Display(Name = "Apellido Materno")]
    public string? ApellidoMaterno { get; set; }

    [Required(ErrorMessage = "La fecha de nacimiento es requerida.")]
    [FechaNoFutura]
    [Display(Name = "Fecha de Nacimiento")]
    [DataType(DataType.Date)]
    public DateOnly FechaNacimiento { get; set; }

    [Required(ErrorMessage = "El género es requerido.")]
    [Display(Name = "Género")]
    public Genero Genero { get; set; }

    [Required(ErrorMessage = "El CURP es requerido.")]
    [StringLength(18, MinimumLength = 18, ErrorMessage = "El CURP debe tener exactamente 18 caracteres.")]
    [RegularExpression(@"^[A-Z]{4}\d{6}[HM][A-Z]{5}[0-9A-Z]\d$",
        ErrorMessage = "El CURP no tiene el formato correcto.")]
    [Display(Name = "CURP")]
    public string? Curp { get; set; }

    [Required(ErrorMessage = "El teléfono es requerido.")]
    [RegularExpression(@"^\d{10}$", ErrorMessage = "El teléfono debe tener exactamente 10 dígitos.")]
    [Display(Name = "Teléfono")]
    public string? Telefono { get; set; }

    [Required(ErrorMessage = "El correo electrónico es requerido.")]
    [StringLength(150, ErrorMessage = "El correo no puede exceder 150 caracteres.")]
    [EmailAddress(ErrorMessage = "El correo electrónico no es válido.")]
    [Display(Name = "Correo Electrónico")]
    public string? Email { get; set; }

    [Required(ErrorMessage = "La dirección es requerida.")]
    [StringLength(200, ErrorMessage = "La dirección no puede exceder 200 caracteres.")]
    [Display(Name = "Dirección")]
    public string? Direccion { get; set; }

    [Required(ErrorMessage = "La comunidad es requerida.")]
    [StringLength(100, ErrorMessage = "La comunidad no puede exceder 100 caracteres.")]
    [Display(Name = "Comunidad")]
    public string Comunidad { get; set; } = string.Empty;

    [Required(ErrorMessage = "El municipio es requerido.")]
    [StringLength(100, ErrorMessage = "El municipio no puede exceder 100 caracteres.")]
    [Display(Name = "Municipio")]
    public string Municipio { get; set; } = "Axtla de Terrazas";

    [Display(Name = "Fecha de Registro")]
    public DateTime FechaRegistro { get; set; } = DateTime.UtcNow;

    public bool IsDeleted { get; set; } = false;

    public string NombreCompleto => $"{Nombre} {ApellidoPaterno} {ApellidoMaterno}".Trim();

    public ICollection<Bautizo> Bautizos { get; set; } = new List<Bautizo>();
    public ICollection<PrimeraComunion> PrimerasComuniones { get; set; } = new List<PrimeraComunion>();
    public ICollection<Confirmacion> Confirmaciones { get; set; } = new List<Confirmacion>();
}
