namespace NotariaParroquial.Services;

public interface ICorreoServicio
{
    Task<bool> EnviarConfirmacionPago(string emailCliente, string nombreCliente,
        string numeroPedido, decimal total);
}
