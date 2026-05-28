using Resend;

namespace NotariaParroquial.Services;

public class CorreoServicio : ICorreoServicio
{
    private readonly IResend _resend;
    private readonly ILogger<CorreoServicio> _logger;

    public CorreoServicio(IResend resend, ILogger<CorreoServicio> logger)
    {
        _resend = resend;
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
            var message = new EmailMessage
            {
                From = "onboarding@resend.dev",
                To = emailCliente,
                Subject = $"Pedido #{numeroPedido} Confirmado - Sneackers Store",
                HtmlBody = BuildHtml(nombreCliente, numeroPedido, total)
            };

            await _resend.EmailSendAsync(message);
            _logger.LogInformation("Correo enviado a {Email} — pedido {Pedido}",
                emailCliente, numeroPedido);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al enviar correo a {Email}", emailCliente);
            return false;
        }
    }

    private static string BuildHtml(string nombre, string numeroPedido, decimal total) => $"""
        <!DOCTYPE html>
        <html lang="es">
        <head>
          <meta charset="UTF-8">
          <meta name="viewport" content="width=device-width,initial-scale=1.0">
        </head>
        <body style="margin:0;padding:0;background:#f8f9fa;font-family:'Segoe UI',Arial,sans-serif;">

          <table width="100%" cellpadding="0" cellspacing="0"
                 style="background:#f8f9fa;padding:40px 16px;">
            <tr><td align="center">

              <table width="600" cellpadding="0" cellspacing="0"
                     style="max-width:600px;width:100%;background:#fff;border-radius:12px;
                            overflow:hidden;box-shadow:0 2px 16px rgba(0,0,0,.08);">

                <!-- Header -->
                <tr>
                  <td style="background:linear-gradient(135deg,#6366f1,#8b5cf6);
                             padding:40px 32px;text-align:center;">
                    <p style="color:rgba(255,255,255,.8);margin:0 0 8px;font-size:13px;
                               letter-spacing:2px;text-transform:uppercase;">Sneackers Store</p>
                    <h1 style="color:#fff;margin:0;font-size:28px;font-weight:700;">
                      ¡Pago Confirmado!
                    </h1>
                    <p style="color:rgba(255,255,255,.75);margin:12px 0 0;font-size:15px;">
                      Tu pedido está en camino 🚀
                    </p>
                  </td>
                </tr>

                <!-- Greeting -->
                <tr>
                  <td style="padding:36px 32px 0;">
                    <h2 style="color:#212529;margin:0 0 12px;font-size:20px;font-weight:600;">
                      Hola, {nombre} 👋
                    </h2>
                    <p style="color:#6c757d;margin:0;font-size:15px;line-height:1.7;">
                      Hemos recibido y confirmado tu pago correctamente.
                      A continuación el resumen de tu pedido.
                    </p>
                  </td>
                </tr>

                <!-- Order card -->
                <tr>
                  <td style="padding:24px 32px;">
                    <table width="100%" cellpadding="0" cellspacing="0"
                           style="border:1px solid #dee2e6;border-radius:8px;overflow:hidden;">
                      <tr>
                        <td colspan="2"
                            style="background:#f8f9fa;padding:12px 20px;
                                   border-bottom:1px solid #dee2e6;">
                          <span style="color:#495057;font-size:13px;font-weight:600;
                                       text-transform:uppercase;letter-spacing:.5px;">
                            Resumen del Pedido
                          </span>
                        </td>
                      </tr>
                      <tr>
                        <td style="padding:14px 20px;border-bottom:1px solid #dee2e6;
                                   color:#6c757d;font-size:14px;width:40%;">N° de Pedido</td>
                        <td style="padding:14px 20px;border-bottom:1px solid #dee2e6;
                                   color:#212529;font-size:14px;font-weight:600;
                                   font-family:monospace;">{numeroPedido}</td>
                      </tr>
                      <tr>
                        <td style="padding:14px 20px;border-bottom:1px solid #dee2e6;
                                   color:#6c757d;font-size:14px;">Estado</td>
                        <td style="padding:14px 20px;border-bottom:1px solid #dee2e6;">
                          <span style="background:#ede9fe;color:#5b21b6;padding:4px 12px;
                                       border-radius:50px;font-size:13px;font-weight:600;">
                            ✓ Confirmado
                          </span>
                        </td>
                      </tr>
                      <tr>
                        <td style="padding:16px 20px;color:#6c757d;font-size:14px;">
                          Total Pagado
                        </td>
                        <td style="padding:16px 20px;color:#6366f1;
                                   font-size:26px;font-weight:700;">
                          ${total:N2} MXN
                        </td>
                      </tr>
                    </table>
                  </td>
                </tr>

                <!-- Thank you -->
                <tr>
                  <td style="padding:0 32px 32px;">
                    <div style="background:#ede9fe;border-left:4px solid #6366f1;
                                border-radius:4px;padding:16px 20px;">
                      <p style="color:#4c1d95;margin:0;font-size:14px;line-height:1.6;">
                        <strong>¡Gracias por tu compra!</strong> Pronto recibirás
                        información sobre el envío. Para cualquier duda escríbenos
                        respondiendo este correo.
                      </p>
                    </div>
                  </td>
                </tr>

                <!-- Divider + Footer -->
                <tr>
                  <td style="padding:0 32px;">
                    <hr style="border:none;border-top:1px solid #dee2e6;margin:0;">
                  </td>
                </tr>
                <tr>
                  <td style="padding:24px 32px;text-align:center;">
                    <p style="color:#adb5bd;font-size:12px;margin:0 0 4px;">
                      © 2026 Sneackers Store · Todos los derechos reservados
                    </p>
                    <p style="color:#adb5bd;font-size:12px;margin:0;">
                      Este es un correo automático, por favor no responder directamente.
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
