namespace NotariaParroquial.Models.ViewModels;

public class DashboardViewModel
{
    public int TotalFeligreses { get; set; }
    public int ServiciosMes { get; set; }
    public int PagosPendientes { get; set; }
    public decimal IngresosMes { get; set; }

    public int TotalBautizos { get; set; }
    public int TotalComuniones { get; set; }
    public int TotalConfirmaciones { get; set; }
    public int TotalMatrimonios { get; set; }

    public List<string> MesesLabels { get; set; } = new();
    public List<int> ServiciosPorMes { get; set; } = new();
    public List<ActividadReciente> ActividadesRecientes { get; set; } = new();
    public List<ServicioProximo> ServiciosProximos { get; set; } = new();
}

public class ActividadReciente
{
    public string Nombre { get; set; } = string.Empty;
    public string TipoServicio { get; set; } = string.Empty;
    public string TipoIcono { get; set; } = string.Empty;
    public DateOnly Fecha { get; set; }
    public string Estado { get; set; } = string.Empty;
    public string EstadoClass { get; set; } = string.Empty;
    public string Url { get; set; } = string.Empty;
}

public class ServicioProximo
{
    public string Nombre { get; set; } = string.Empty;
    public string Tipo { get; set; } = string.Empty;
    public DateOnly Fecha { get; set; }
    public int DiasRestantes { get; set; }
}
