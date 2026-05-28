using System.Net;
using System.Net.Mail;

namespace NotariaParroquial.Services;

public class CorreoServicio : ICorreoServicio
{
    private readonly IConfiguration _config;
    private readonly ILogger<CorreoServicio> _logger;

    public CorreoServicio(IConfiguration config, ILogger<CorreoServicio> logger)
    {
        _config = config;
        _logger = logger;
    }

    public async Task<bool> EnviarConfirmacionPago(string emailCliente, string nombreCliente,
        string numeroPedido, decimal total)
    {
        if (string.IsNullOrWhiteSpace(emailCliente))
        {
            _logger.LogWarning("Email de destino vacío, se omite el envío.");
            return false;
        }

        try
        {
            var host = _config["Mailtrap:Host"]!;
            var port = int.Parse(_config["Mailtrap:Port"] ?? "587");
            var username = _config["Mailtrap:Username"]!;
            var password = _config["Mailtrap:Password"]!;
            var enableSsl = bool.Parse(_config["Mailtrap:EnableSSL"] ?? "true");

            using var smtp = new SmtpClient(host, port)
            {
                Credentials = new NetworkCredential(username, password),
                EnableSsl = enableSsl
            };

            using var message = new MailMessage
            {
                From = new MailAddress("noreply@sneackersstore.com", "Notaría Parroquial"),
                Subject = $"✅ Pago Confirmado — {numeroPedido}",
                Body = BuildHtml(nombreCliente, numeroPedido, total),
                IsBodyHtml = true
            };
            message.To.Add(emailCliente);

            await smtp.SendMailAsync(message);
            _logger.LogInformation("Correo enviado a {Email} — pedido {Pedido}", emailCliente, numeroPedido);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al enviar correo de confirmación a {Email}", emailCliente);
            return false;
        }
    }

    private static string BuildHtml(string nombre, string numeroPedido, decimal total) => $"""
        <!DOCTYPE html>
        <html lang="es">
        <head>
          <meta charset="UTF-8">
          <meta name="viewport" content="width=device-width,initial-scale=1">
        </head>
        <body style="margin:0;padding:0;background:#f1f5f9;font-family:'Segoe UI',Arial,sans-serif">
          <table width="100%" cellpadding="0" cellspacing="0">
            <tr><td align="center" style="padding:40px 16px">
              <table width="600" cellpadding="0" cellspacing="0"
                     style="max-width:600px;width:100%;background:#fff;border-radius:16px;
                            overflow:hidden;box-shadow:0 4px 24px rgba(0,0,0,.08)">

                <!-- Header -->
                <tr>
                  <td style="background:linear-gradient(135deg,#6366f1,#8b5cf6);
                             padding:40px;text-align:center">
                    <div style="font-size:48px;margin-bottom:12px">⛪</div>
                    <h1 style="color:#fff;margin:0;font-size:22px;font-weight:700">
                      Notaría Parroquial
                    </h1>
                    <p style="color:rgba(255,255,255,.8);margin:4px 0 0;font-size:14px">
                      Axtla de Terrazas, S.L.P.
                    </p>
                  </td>
                </tr>

                <!-- Body -->
                <tr>
                  <td style="padding:40px">
                    <div style="display:inline-block;background:#dcfce7;color:#16a34a;
                                padding:6px 16px;border-radius:999px;font-size:13px;
                                font-weight:600;margin-bottom:24px">
                      ✅ PAGO CONFIRMADO
                    </div>
                    <h2 style="color:#1e293b;margin:0 0 8px;font-size:20px">
                      Hola, {nombre}
                    </h2>
                    <p style="color:#64748b;margin:0 0 32px;font-size:15px;line-height:1.6">
                      Tu pago ha sido <strong style="color:#6366f1">confirmado exitosamente</strong>.
                      A continuación el resumen:
                    </p>

                    <!-- Detail box -->
                    <div style="background:#f8fafc;border:1px solid #e2e8f0;
                                border-radius:12px;padding:24px;margin-bottom:32px">
                      <table width="100%" cellpadding="0" cellspacing="0">
                        <tr>
                          <td style="padding:10px 0;border-bottom:1px solid #e2e8f0">
                            <span style="color:#64748b;font-size:13px">N° de Pedido</span><br>
                            <strong style="color:#1e293b;font-size:15px;
                                          font-family:monospace">{numeroPedido}</strong>
                          </td>
                        </tr>
                        <tr>
                          <td style="padding:10px 0">
                            <span style="color:#64748b;font-size:13px">Total Pagado</span><br>
                            <strong style="color:#16a34a;font-size:26px;font-weight:700">
                              ${total:N2} MXN
                            </strong>
                          </td>
                        </tr>
                      </table>
                    </div>

                    <p style="color:#64748b;font-size:13px;line-height:1.6">
                      Conserva este correo como comprobante de pago. Para cualquier
                      aclaración, comunícate directamente con la notaría parroquial.
                    </p>
                  </td>
                </tr>

                <!-- Footer -->
                <tr>
                  <td style="background:#f8fafc;border-top:1px solid #e2e8f0;
                             padding:24px;text-align:center">
                    <p style="color:#94a3b8;font-size:12px;margin:0">
                      Notaría Parroquial de Axtla de Terrazas, S.L.P.<br>
                      Este es un correo automático, por favor no responder.
                    </p>
                  </td>
                </tr>

              </table>
            </td></tr>
          </table>
        </body>
        </html>
        """;
}
