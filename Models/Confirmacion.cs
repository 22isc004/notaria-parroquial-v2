using System.ComponentModel.DataAnnotations;

namespace NotariaParroquial.Models;

public class Confirmacion
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Debe seleccionar un feligrés.")]
    [Display(Name = "Feligrés")]
    public int FeligresId { get; set; }
    public Feligres? Feligres { get; set; }

    [Required(ErrorMessage = "La fecha de confirmación es requerida.")]
    [FechaNoAnterior]
    [Display(Name = "Fecha de Confirmación")]
    [DataType(DataType.Date)]
    public DateOnly FechaConfirmacion { get; set; }

    [Required(ErrorMessage = "El nombre del sacerdote u obispo es requerido.")]
    [StringLength(150, ErrorMessage = "El nombre no puede exceder 150 caracteres.")]
    [Display(Name = "Sacerdote / Obispo")]
    public string Sacerdote { get; set; } = string.Empty;

    [Required(ErrorMessage = "El nombre de confirmación es requerido.")]
    [StringLength(100, ErrorMessage = "El nombre de confirmación no puede exceder 100 caracteres.")]
    [Display(Name = "Nombre de Confirmación")]
    public string? NombreConfirmacion { get; set; }

    [Required(ErrorMessage = "El nombre del padrino es requerido.")]
    [StringLength(150, ErrorMessage = "El nombre del padrino no puede exceder 150 caracteres.")]
    [Display(Name = "Nombre del Padrino")]
    public string? PadrinoNombre { get; set; }

    [Required(ErrorMessage = "El lugar es requerido.")]
    [StringLength(200, ErrorMessage = "El lugar no puede exceder 200 caracteres.")]
    [Display(Name = "Lugar")]
    public string? Lugar { get; set; }

    [StringLength(30, ErrorMessage = "El número de boleta no puede exceder 30 caracteres.")]
    [Display(Name = "No. de Boleta")]
    public string? NumeroBoleta { get; set; }

    [Display(Name = "Estado")]
    public EstadoSolicitud Estado { get; set; } = EstadoSolicitud.Pendiente;

    [StringLength(500, ErrorMessage = "Las observaciones no pueden exceder 500 caracteres.")]
    [Display(Name = "Observaciones")]
    public string? Observaciones { get; set; }

    [Display(Name = "Fecha de Registro")]
    public DateTime FechaRegistro { get; set; } = DateTime.UtcNow;

    public bool IsDeleted { get; set; } = false;

    public int? PagoId { get; set; }
    public Pago? Pago { get; set; }
}
