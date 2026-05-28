using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace NotariaParroquial.Models;

public class Pago
{
    public int Id { get; set; }

    [Required]
    [Display(Name = "Tipo de Servicio")]
    public TipoServicio TipoServicio { get; set; }

    [Required]
    [Display(Name = "Nombre del Solicitante")]
    [StringLength(200)]
    public string NombreSolicitante { get; set; } = string.Empty;

    [StringLength(150)]
    [EmailAddress]
    [Display(Name = "Correo para Notificación")]
    public string? EmailNotificacion { get; set; }

    [Required]
    [Column(TypeName = "decimal(10,2)")]
    [Display(Name = "Monto")]
    [DataType(DataType.Currency)]
    public decimal Monto { get; set; }

    [Display(Name = "Fecha de Pago")]
    [DataType(DataType.Date)]
    public DateOnly? FechaPago { get; set; }

    [Display(Name = "Método de Pago")]
    public MetodoPago MetodoPago { get; set; } = MetodoPago.Efectivo;

    [Display(Name = "Estado")]
    public EstadoPago Estado { get; set; } = EstadoPago.Pendiente;

    [StringLength(100)]
    [Display(Name = "Referencia / Folio")]
    public string? Referencia { get; set; }

    [StringLength(500)]
    [Display(Name = "Notas")]
    public string? Notas { get; set; }

    [Display(Name = "Fecha de Registro")]
    public DateTime FechaRegistro { get; set; } = DateTime.UtcNow;

    public bool IsDeleted { get; set; } = false;

    // Navigation
    public Bautizo? Bautizo { get; set; }
    public PrimeraComunion? PrimeraComunion { get; set; }
    public Confirmacion? Confirmacion { get; set; }
    public Matrimonio? Matrimonio { get; set; }
}
