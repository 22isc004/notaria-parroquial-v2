using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace NotariaParroquial.Models;

public class Pago
{
    public int Id { get; set; }

    [Required(ErrorMessage = "El tipo de servicio es requerido.")]
    [Display(Name = "Tipo de Servicio")]
    public TipoServicio TipoServicio { get; set; }

    [Required(ErrorMessage = "El nombre del solicitante es requerido.")]
    [StringLength(200, ErrorMessage = "El nombre no puede exceder 200 caracteres.")]
    [Display(Name = "Nombre del Solicitante")]
    public string NombreSolicitante { get; set; } = string.Empty;

    [Required(ErrorMessage = "El correo para notificación es requerido.")]
    [StringLength(150, ErrorMessage = "El correo no puede exceder 150 caracteres.")]
    [EmailAddress(ErrorMessage = "El correo electrónico no es válido.")]
    [Display(Name = "Correo para Notificación")]
    public string? EmailNotificacion { get; set; }

    [Required(ErrorMessage = "El monto es requerido.")]
    [Range(0.01, 99999.99, ErrorMessage = "El monto debe ser mayor a $0.00.")]
    [Column(TypeName = "decimal(10,2)")]
    [Display(Name = "Monto")]
    [DataType(DataType.Currency)]
    public decimal Monto { get; set; }

    [Required(ErrorMessage = "La fecha de pago es requerida.")]
    [FechaNoAnterior]
    [Display(Name = "Fecha de Pago")]
    [DataType(DataType.Date)]
    public DateOnly? FechaPago { get; set; }

    [Display(Name = "Método de Pago")]
    public MetodoPago MetodoPago { get; set; } = MetodoPago.Efectivo;

    [Display(Name = "Estado")]
    public EstadoPago Estado { get; set; } = EstadoPago.Pendiente;

    [StringLength(100, ErrorMessage = "La referencia no puede exceder 100 caracteres.")]
    [Display(Name = "Referencia / Folio")]
    public string? Referencia { get; set; }

    [Required(ErrorMessage = "Las notas son requeridas.")]
    [StringLength(500, ErrorMessage = "Las notas no pueden exceder 500 caracteres.")]
    [Display(Name = "Notas")]
    public string? Notas { get; set; }

    [Display(Name = "Fecha de Registro")]
    public DateTime FechaRegistro { get; set; } = DateTime.UtcNow;

    public bool IsDeleted { get; set; } = false;

    public Bautizo? Bautizo { get; set; }
    public PrimeraComunion? PrimeraComunion { get; set; }
    public Confirmacion? Confirmacion { get; set; }
    public Matrimonio? Matrimonio { get; set; }
}
