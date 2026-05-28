namespace NotariaParroquial.Services;

public interface ICorreoServicio
{
    Task EnviarConfirmacionPagoAsync(string toEmail, string toName, string tipoServicio,
        string referencia, decimal monto, DateOnly fecha);
}
