namespace NotariaParroquial.Models;

public enum TipoServicio
{
    Bautizo = 1,
    PrimeraComunion = 2,
    Confirmacion = 3,
    Matrimonio = 4
}

public enum EstadoSolicitud
{
    Pendiente = 1,
    Programado = 2,
    Completado = 3,
    Cancelado = 4
}

public enum EstadoPago
{
    Pendiente = 1,
    Confirmado = 2,
    Rechazado = 3
}

public enum MetodoPago
{
    Efectivo = 1,
    Transferencia = 2,
    Cheque = 3,
    Otro = 4
}

public enum Genero
{
    Masculino = 1,
    Femenino = 2
}
