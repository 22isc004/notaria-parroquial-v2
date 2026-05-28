using System.ComponentModel.DataAnnotations;

namespace NotariaParroquial.Models;

public class Matrimonio
{
    public int Id { get; set; }

    [Required, StringLength(150)]
    [Display(Name = "Nombre del Contrayente 1")]
    public string Contrayente1Nombre { get; set; } = string.Empty;

    [Required, StringLength(150)]
    [Display(Name = "Nombre de la Contrayente 2")]
    public string Contrayente2Nombre { get; set; } = string.Empty;

    [Required]
    [Display(Name = "Fecha del Matrimonio")]
    [DataType(DataType.Date)]
    public DateOnly FechaMatrimonio { get; set; }

    [Required, StringLength(150)]
    [Display(Name = "Sacerdote Celebrante")]
    public string Sacerdote { get; set; } = string.Empty;

    [StringLength(150)]
    [Display(Name = "Testigo 1")]
    public string? Testigo1 { get; set; }

    [StringLength(150)]
    [Display(Name = "Testigo 2")]
    public string? Testigo2 { get; set; }

    [StringLength(200)]
    [Display(Name = "Lugar")]
    public string? Lugar { get; set; }

    [StringLength(30)]
    [Display(Name = "No. de Acta")]
    public string? NumeroActa { get; set; }

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
