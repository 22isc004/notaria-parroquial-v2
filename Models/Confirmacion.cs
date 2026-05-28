using System.ComponentModel.DataAnnotations;

namespace NotariaParroquial.Models;

public class Confirmacion
{
    public int Id { get; set; }

    [Required]
    [Display(Name = "Feligrés")]
    public int FeligresId { get; set; }
    public Feligres? Feligres { get; set; }

    [Required]
    [Display(Name = "Fecha de Confirmación")]
    [DataType(DataType.Date)]
    public DateOnly FechaConfirmacion { get; set; }

    [Required, StringLength(150)]
    [Display(Name = "Sacerdote / Obispo")]
    public string Sacerdote { get; set; } = string.Empty;

    [StringLength(100)]
    [Display(Name = "Nombre de Confirmación")]
    public string? NombreConfirmacion { get; set; }

    [StringLength(150)]
    [Display(Name = "Nombre del Padrino")]
    public string? PadrinoNombre { get; set; }

    [StringLength(200)]
    [Display(Name = "Lugar")]
    public string? Lugar { get; set; }

    [StringLength(30)]
    [Display(Name = "No. de Boleta")]
    public string? NumeroBoleta { get; set; }

    [Display(Name = "Estado")]
    public EstadoSolicitud Estado { get; set; } = EstadoSolicitud.Pendiente;

    [StringLength(500)]
    [Display(Name = "Observaciones")]
    public string? Observaciones { get; set; }

    [Display(Name = "Fecha de Registro")]
    public DateTime FechaRegistro { get; set; } = DateTime.UtcNow;

    public bool IsDeleted { get; set; } = false;

    public int? PagoId { get; set; }
    public Pago? Pago { get; set; }
}
