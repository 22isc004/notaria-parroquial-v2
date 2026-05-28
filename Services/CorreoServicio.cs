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
            var apiToken = _config["Mailtrap:ApiToken"]!;
            var baseUrl = (_config["Mailtrap:ApiBaseUrl"] ?? "https://send.api.mailtrap.io/api/").TrimEnd('/');

            var payload = new
            {
                from = new { email = "noreply@sneackersstore.com", name = "Sneackers Store" },
                to = new[] { new { email = emailCliente, name = nombreCliente } },
                subject = $"✅ Tu pedido {numeroPedido} ha sido confirmado",
                html = BuildHtml(nombreCliente, numeroPedido, total),
                category = "confirmacion_pago"
            };

            var http = _httpFactory.CreateClient("Mailtrap");
            http.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", apiToken);

            var content = new StringContent(
                JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");

            var response = await http.PostAsync($"{baseUrl}/send", content);

            if (response.IsSuccessStatusCode)
            {
                _logger.LogInformation("Correo enviado a {Email} — pedido {Pedido}",
                    emailCliente, numeroPedido);
                return true;
            }

            var body = await response.Content.ReadAsStringAsync();
            _logger.LogError("Mailtrap API respondió {Status}: {Body}", response.StatusCode, body);
            return false;
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
          <meta name="viewport" content="width=device-width, initial-scale=1.0">
        </head>
        <body style="margin:0;padding:0;background-color:#f8f9fa;font-family:'Segoe UI',Arial,sans-serif;">

          <!-- Wrapper -->
          <table width="100%" cellpadding="0" cellspacing="0" style="background-color:#f8f9fa;padding:40px 16px;">
            <tr>
              <td align="center">
                <table width="600" cellpadding="0" cellspacing="0"
                       style="max-width:600px;width:100%;background:#ffffff;
                              border-radius:12px;overflow:hidden;
                              box-shadow:0 2px 12px rgba(0,0,0,.08);">

                  <!-- Header -->
                  <tr>
                    <td style="background:linear-gradient(135deg,#0d6efd,#0a58ca);
                               padding:40px 32px;text-align:center;">
                      <h1 style="color:#ffffff;margin:0 0 6px;font-size:24px;font-weight:700;
                                 letter-spacing:-0.5px;">
                        Sneackers Store
                      </h1>
                      <p style="color:rgba(255,255,255,.75);margin:0;font-size:14px;">
                        Tu tienda de sneakers favorita
                      </p>
                    </td>
                  </tr>

                  <!-- Badge -->
                  <tr>
                    <td align="center" style="padding:32px 32px 0;">
                      <span style="display:inline-block;background:#d1e7dd;color:#0f5132;
                                   padding:8px 20px;border-radius:50px;font-size:13px;
                                   font-weight:600;letter-spacing:.5px;">
                        ✅ PAGO CONFIRMADO
                      </span>
                    </td>
                  </tr>

                  <!-- Greeting -->
                  <tr>
                    <td style="padding:24px 32px 0;">
                      <h2 style="color:#212529;margin:0 0 8px;font-size:20px;font-weight:600;">
                        ¡Hola, {nombre}!
                      </h2>
                      <p style="color:#6c757d;margin:0;font-size:15px;line-height:1.6;">
                        Tu pago fue procesado exitosamente. Aquí tienes el resumen de tu pedido:
                      </p>
                    </td>
                  </tr>

                  <!-- Order card -->
                  <tr>
                    <td style="padding:24px 32px;">
                      <table width="100%" cellpadding="0" cellspacing="0"
                             style="background:#f8f9fa;border:1px solid #dee2e6;
                                    border-radius:8px;overflow:hidden;">
                        <tr>
                          <td style="padding:16px 20px;border-bottom:1px solid #dee2e6;">
                            <span style="color:#6c757d;font-size:12px;
                                         text-transform:uppercase;letter-spacing:.5px;
                                         font-weight:600;">N° de Pedido</span><br>
                            <span style="color:#212529;font-size:16px;font-weight:600;
                                         font-family:monospace;">{numeroPedido}</span>
                          </td>
                        </tr>
                        <tr>
                          <td style="padding:16px 20px;border-bottom:1px solid #dee2e6;">
                            <span style="color:#6c757d;font-size:12px;
                                         text-transform:uppercase;letter-spacing:.5px;
                                         font-weight:600;">Estado</span><br>
                            <span style="color:#0f5132;font-size:15px;font-weight:600;">
                              Pago confirmado ✓
                            </span>
                          </td>
                        </tr>
                        <tr>
                          <td style="padding:16px 20px;">
                            <span style="color:#6c757d;font-size:12px;
                                         text-transform:uppercase;letter-spacing:.5px;
                                         font-weight:600;">Total Pagado</span><br>
                            <span style="color:#0d6efd;font-size:28px;font-weight:700;">
                              ${total:N2} MXN
                            </span>
                          </td>
                        </tr>
                      </table>
                    </td>
                  </tr>

                  <!-- Thank you message -->
                  <tr>
                    <td style="padding:0 32px 32px;">
                      <div style="background:#cfe2ff;border-left:4px solid #0d6efd;
                                  border-radius:4px;padding:16px 20px;">
                        <p style="color:#084298;margin:0;font-size:14px;line-height:1.6;">
                          <strong>¡Gracias por tu compra!</strong> Tu pedido está siendo
                          procesado y te notificaremos cuando sea enviado. Si tienes alguna
                          pregunta, contáctanos respondiendo a este correo.
                        </p>
                      </div>
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
              </td>
            </tr>
          </table>

        </body>
        </html>
        """;
}
