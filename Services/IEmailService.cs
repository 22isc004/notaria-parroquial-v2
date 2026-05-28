namespace NotariaParroquial.Services;

public interface IEmailService
{
    Task SendPaymentConfirmationAsync(string toEmail, string toName, string tipoServicio,
        string referencia, decimal monto, DateOnly fecha);
}
