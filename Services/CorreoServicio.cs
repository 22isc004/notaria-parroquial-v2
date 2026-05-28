using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace NotariaParroquial.Services;

public class CorreoServicio : ICorreoServicio
{
    private readonly IHttpClientFactory _httpFactory;
    private readonly IConfiguration _config;
    private readonly ILogger<CorreoServicio> _logger;

    public CorreoServicio(IHttpClientFactory httpFactory, IConfiguration config,
        ILogger<CorreoServicio> logger)
    {
        _httpFactory = httpFactory;
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
            var apiToken = _config["MailerSend:ApiToken"]!;
            var baseUrl = (_config["MailerSend:ApiBaseUrl"] ?? "https://api.mailersend.com/v1/").TrimEnd('/');

            var payload = new
            {
                from = new { email = "noreply@sneackersstore.com", name = "Sneackers Store" },
                to = new[] { new { email = emailCliente, name = nombreCliente } },
                subject = $"Pedido #{numeroPedido} Confirmado - Sneackers Store",
                html = BuildHtml(nombreCliente, numeroPedido, total)
            };

            var http = _httpFactory.CreateClient("MailerSend");
            http.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", apiToken);

            var content = new StringContent(
                JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");

            var response = await http.PostAsync($"{baseUrl}/email", content);

            if (response.IsSuccessStatusCode)
            {
                _logger.LogInformation("Correo enviado a {Email} — pedido {Pedido}",
                    emailCliente, numeroPedido);
                return true;
            }

            var body = await response.Content.ReadAsStringAsync();
            _logger.LogError("MailerSend respondió {Status}: {Body}", response.StatusCode, body);
            return false;
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
                  <td style="background:linear-gradient(135deg,#198754,#157347);
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
                      A continuación encontrarás el resumen de tu pedido.
                    </p>
                  </td>
                </tr>

                <!-- Order summary card -->
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
                                   color:#6c757d;font-size:14px;width:40%;">
                          N° de Pedido
                        </td>
                        <td style="padding:14px 20px;border-bottom:1px solid #dee2e6;
                                   color:#212529;font-size:14px;font-weight:600;
                                   font-family:monospace;">
                          {numeroPedido}
                        </td>
                      </tr>
                      <tr>
                        <td style="padding:14px 20px;border-bottom:1px solid #dee2e6;
                                   color:#6c757d;font-size:14px;">
                          Estado
                        </td>
                        <td style="padding:14px 20px;border-bottom:1px solid #dee2e6;">
                          <span style="background:#d1e7dd;color:#0f5132;padding:4px 12px;
                                       border-radius:50px;font-size:13px;font-weight:600;">
                            ✓ Confirmado
                          </span>
                        </td>
                      </tr>
                      <tr>
                        <td style="padding:16px 20px;color:#6c757d;font-size:14px;">
                          Total Pagado
                        </td>
                        <td style="padding:16px 20px;color:#198754;
                                   font-size:26px;font-weight:700;">
                          ${total:N2} MXN
                        </td>
                      </tr>
                    </table>
                  </td>
                </tr>

                <!-- Thank you banner -->
                <tr>
                  <td style="padding:0 32px 32px;">
                    <table width="100%" cellpadding="0" cellspacing="0"
                           style="background:#d1e7dd;border-radius:8px;">
                      <tr>
                        <td style="padding:20px 24px;">
                          <p style="color:#0f5132;margin:0 0 6px;font-size:15px;
                                     font-weight:600;">
                            ¡Gracias por tu compra!
                          </p>
                          <p style="color:#0f5132;margin:0;font-size:14px;line-height:1.6;">
                            Pronto recibirás más información sobre el estado de tu envío.
                            Si tienes alguna duda, no dudes en contactarnos.
                          </p>
                        </td>
                      </tr>
                    </table>
                  </td>
                </tr>

                <!-- Divider -->
                <tr>
                  <td style="padding:0 32px;">
                    <hr style="border:none;border-top:1px solid #dee2e6;margin:0;">
                  </td>
                </tr>

                <!-- Footer -->
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
