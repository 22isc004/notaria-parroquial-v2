using System.ComponentModel.DataAnnotations;

namespace NotariaParroquial.Models;

public class Matrimonio
{
    public int Id { get; set; }

    [Required(ErrorMessage = "El nombre del contrayente es requerido.")]
    [StringLength(150, ErrorMessage = "El nombre no puede exceder 150 caracteres.")]
    [Display(Name = "Nombre del Contrayente 1")]
    public string Contrayente1Nombre { get; set; } = string.Empty;

    [Required(ErrorMessage = "El nombre de la contrayente es requerido.")]
    [StringLength(150, ErrorMessage = "El nombre no puede exceder 150 caracteres.")]
    [Display(Name = "Nombre de la Contrayente 2")]
    public string Contrayente2Nombre { get; set; } = string.Empty;

    [Required(ErrorMessage = "La fecha del matrimonio es requerida.")]
    [FechaNoAnterior]
    [Display(Name = "Fecha del Matrimonio")]
    [DataType(DataType.Date)]
    public DateOnly FechaMatrimonio { get; set; }

    [Required(ErrorMessage = "El nombre del sacerdote es requerido.")]
    [StringLength(150, ErrorMessage = "El nombre no puede exceder 150 caracteres.")]
    [Display(Name = "Sacerdote Celebrante")]
    public string Sacerdote { get; set; } = string.Empty;

    [Required(ErrorMessage = "El nombre del testigo 1 es requerido.")]
    [StringLength(150, ErrorMessage = "El nombre del testigo no puede exceder 150 caracteres.")]
    [Display(Name = "Testigo 1")]
    public string? Testigo1 { get; set; }

    [Required(ErrorMessage = "El nombre del testigo 2 es requerido.")]
    [StringLength(150, ErrorMessage = "El nombre del testigo no puede exceder 150 caracteres.")]
    [Display(Name = "Testigo 2")]
    public string? Testigo2 { get; set; }

    [Required(ErrorMessage = "El lugar es requerido.")]
    [StringLength(200, ErrorMessage = "El lugar no puede exceder 200 caracteres.")]
    [Display(Name = "Lugar")]
    public string? Lugar { get; set; }

    [StringLength(30, ErrorMessage = "El número de acta no puede exceder 30 caracteres.")]
    [Display(Name = "No. de Acta")]
    public string? NumeroActa { get; set; }

    [Display(Name = "Estado")]
    public EstadoSolicitud Estado { get; set; } = EstadoSolicitud.Pendiente;

    [Required(ErrorMessage = "Las observaciones son requeridas.")]
    [StringLength(500, ErrorMessage = "Las observaciones no pueden exceder 500 caracteres.")]
    [Display(Name = "Observaciones")]
    public string? Observaciones { get; set; }

    [Display(Name = "Fecha de Registro")]
    public DateTime FechaRegistro { get; set; } = DateTime.UtcNow;

    public bool IsDeleted { get; set; } = false;

    public int? PagoId { get; set; }
    public Pago? Pago { get; set; }
}
