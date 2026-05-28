using System.Text;
using System.Text.Json;

namespace NotariaParroquial.Services;

public class BrevoEmailService : IEmailService
{
    private readonly HttpClient _http;
    private readonly IConfiguration _config;
    private readonly ILogger<BrevoEmailService> _logger;

    public BrevoEmailService(IHttpClientFactory factory, IConfiguration config, ILogger<BrevoEmailService> logger)
    {
        _http = factory.CreateClient("Brevo");
        _config = config;
        _logger = logger;
    }

    public async Task SendPaymentConfirmationAsync(string toEmail, string toName, string tipoServicio,
        string referencia, decimal monto, DateOnly fecha)
    {
        var apiKey = _config["Brevo:ApiKey"];
        var senderEmail = _config["Brevo:SenderEmail"] ?? "notaria@parroquialaxtla.mx";
        var senderName = _config["Brevo:SenderName"] ?? "Notaría Parroquial";

        if (string.IsNullOrEmpty(apiKey) || string.IsNullOrEmpty(toEmail))
        {
            _logger.LogWarning("Brevo API key o email de destino no configurado, omitiendo envío.");
            return;
        }

        var html = BuildEmailHtml(toName, tipoServicio, referencia, monto, fecha);

        var payload = new
        {
            sender = new { name = senderName, email = senderEmail },
            to = new[] { new { email = toEmail, name = toName } },
            subject = $"✅ Pago Confirmado – {tipoServicio} | Notaría Parroquial Axtla de Terrazas",
            htmlContent = html
        };

        var request = new HttpRequestMessage(HttpMethod.Post, "https://api.brevo.com/v3/smtp/email");
        request.Headers.Add("api-key", apiKey);
        request.Headers.Add("accept", "application/json");
        request.Content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");

        try
        {
            var response = await _http.SendAsync(request);
            if (!response.IsSuccessStatusCode)
            {
                var body = await response.Content.ReadAsStringAsync();
                _logger.LogError("Error Brevo {Status}: {Body}", response.StatusCode, body);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Excepción al enviar correo Brevo");
        }
    }

    private static string BuildEmailHtml(string nombre, string servicio, string referencia, decimal monto, DateOnly fecha)
    {
        return $"""
        <!DOCTYPE html>
        <html lang="es">
        <head><meta charset="UTF-8"><meta name="viewport" content="width=device-width,initial-scale=1"></head>
        <body style="margin:0;padding:0;background:#f1f5f9;font-family:'Segoe UI',Arial,sans-serif">
          <table width="100%" cellpadding="0" cellspacing="0">
            <tr><td align="center" style="padding:40px 16px">
              <table width="600" cellpadding="0" cellspacing="0" style="max-width:600px;width:100%;background:#fff;border-radius:16px;overflow:hidden;box-shadow:0 4px 24px rgba(0,0,0,.08)">
                <!-- Header -->
                <tr><td style="background:linear-gradient(135deg,#6366f1,#8b5cf6);padding:40px;text-align:center">
                  <div style="width:64px;height:64px;background:rgba(255,255,255,.2);border-radius:50%;margin:0 auto 16px;display:flex;align-items:center;justify-content:center;font-size:32px">⛪</div>
                  <h1 style="color:#fff;margin:0;font-size:22px;font-weight:700">Notaría Parroquial</h1>
                  <p style="color:rgba(255,255,255,.8);margin:4px 0 0;font-size:14px">Axtla de Terrazas, S.L.P.</p>
                </td></tr>
                <!-- Body -->
                <tr><td style="padding:40px">
                  <div style="display:inline-block;background:#dcfce7;color:#16a34a;padding:6px 16px;border-radius:999px;font-size:13px;font-weight:600;margin-bottom:24px">✅ PAGO CONFIRMADO</div>
                  <h2 style="color:#1e293b;margin:0 0 8px;font-size:20px">Hola, {nombre}</h2>
                  <p style="color:#64748b;margin:0 0 32px;font-size:15px;line-height:1.6">
                    Tu pago para el servicio de <strong style="color:#6366f1">{servicio}</strong> ha sido confirmado exitosamente.
                  </p>
                  <!-- Detail box -->
                  <div style="background:#f8fafc;border:1px solid #e2e8f0;border-radius:12px;padding:24px;margin-bottom:32px">
                    <table width="100%" cellpadding="0" cellspacing="0">
                      <tr><td style="padding:8px 0;border-bottom:1px solid #e2e8f0">
                        <span style="color:#64748b;font-size:13px">Servicio</span><br>
                        <strong style="color:#1e293b;font-size:15px">{servicio}</strong>
                      </td></tr>
                      <tr><td style="padding:8px 0;border-bottom:1px solid #e2e8f0">
                        <span style="color:#64748b;font-size:13px">Fecha</span><br>
                        <strong style="color:#1e293b;font-size:15px">{fecha:dd/MM/yyyy}</strong>
                      </td></tr>
                      <tr><td style="padding:8px 0;border-bottom:1px solid #e2e8f0">
                        <span style="color:#64748b;font-size:13px">Referencia</span><br>
                        <strong style="color:#1e293b;font-size:15px;font-family:monospace">{referencia}</strong>
                      </td></tr>
                      <tr><td style="padding:8px 0">
                        <span style="color:#64748b;font-size:13px">Monto Pagado</span><br>
                        <strong style="color:#16a34a;font-size:22px">${monto:N2} MXN</strong>
                      </td></tr>
                    </table>
                  </div>
                  <p style="color:#64748b;font-size:13px;line-height:1.6">
                    Conserva este correo como comprobante. Para cualquier aclaración,
                    comunícate directamente con la notaría parroquial.
                  </p>
                </td></tr>
                <!-- Footer -->
                <tr><td style="background:#f8fafc;border-top:1px solid #e2e8f0;padding:24px;text-align:center">
                  <p style="color:#94a3b8;font-size:12px;margin:0">
                    Notaría Parroquial de Axtla de Terrazas, S.L.P.<br>
                    Este es un correo automático, por favor no responder.
                  </p>
                </td></tr>
              </table>
            </td></tr>
          </table>
        </body>
        </html>
        """;
    }
}
