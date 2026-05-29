namespace NotariaParroquial.Models.ViewModels;

public class ServicioSinPago
{
    public string Tipo { get; set; } = "";
    public string TipoIcono { get; set; } = "";
    public string TipoColor { get; set; } = "";
    public int Id { get; set; }
    public string Nombre { get; set; } = "";
    public string? Folio { get; set; }
    public DateOnly Fecha { get; set; }
    public EstadoSolicitud Estado { get; set; }
}
