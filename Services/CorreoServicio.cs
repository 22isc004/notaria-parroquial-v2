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
                Subject = $"✅ Pago Confirmado — {numeroPedido} | Notaría Parroquial Axtla de Terrazas",
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

    private static string BuildHtml(string nombre, string referencia, decimal monto) => $"""
        <!DOCTYPE html>
        <html lang="es">
        <head>
          <meta charset="UTF-8">
          <meta name="viewport" content="width=device-width,initial-scale=1.0">
        </head>
        <body style="margin:0;padding:0;background:#f1f5f9;font-family:'Segoe UI',Arial,sans-serif;">

          <table width="100%" cellpadding="0" cellspacing="0"
                 style="background:#f1f5f9;padding:40px 16px;">
            <tr><td align="center">

              <table width="600" cellpadding="0" cellspacing="0"
                     style="max-width:600px;width:100%;background:#fff;border-radius:16px;
                            overflow:hidden;box-shadow:0 4px 24px rgba(0,0,0,.08);">

                <!-- Header -->
                <tr>
                  <td style="background:linear-gradient(135deg,#6366f1,#8b5cf6);
                             padding:40px 32px;text-align:center;">
                    <div style="font-size:48px;margin-bottom:12px;">⛪</div>
                    <h1 style="color:#fff;margin:0 0 4px;font-size:22px;font-weight:700;">
                      Notaría Parroquial
                    </h1>
                    <p style="color:rgba(255,255,255,.8);margin:0;font-size:14px;">
                      Axtla de Terrazas, S.L.P.
                    </p>
                  </td>
                </tr>

                <!-- Badge -->
                <tr>
                  <td align="center" style="padding:32px 32px 0;">
                    <span style="display:inline-block;background:#dcfce7;color:#16a34a;
                                 padding:8px 20px;border-radius:50px;font-size:13px;
                                 font-weight:600;letter-spacing:.5px;">
                      ✅ PAGO CONFIRMADO
                    </span>
                  </td>
                </tr>

                <!-- Greeting -->
                <tr>
                  <td style="padding:24px 32px 0;">
                    <h2 style="color:#1e293b;margin:0 0 8px;font-size:20px;font-weight:600;">
                      Hola, {nombre}
                    </h2>
                    <p style="color:#64748b;margin:0;font-size:15px;line-height:1.7;">
                      Tu pago ha sido <strong style="color:#6366f1">confirmado exitosamente</strong>
                      en la Notaría Parroquial de Axtla de Terrazas.
                    </p>
                  </td>
                </tr>

                <!-- Detail card -->
                <tr>
                  <td style="padding:24px 32px;">
                    <table width="100%" cellpadding="0" cellspacing="0"
                           style="background:#f8fafc;border:1px solid #e2e8f0;
                                  border-radius:12px;overflow:hidden;">
                      <tr>
                        <td colspan="2"
                            style="background:#f1f5f9;padding:12px 20px;
                                   border-bottom:1px solid #e2e8f0;">
                          <span style="color:#475569;font-size:13px;font-weight:600;
                                       text-transform:uppercase;letter-spacing:.5px;">
                            Comprobante de Pago
                          </span>
                        </td>
                      </tr>
                      <tr>
                        <td style="padding:14px 20px;border-bottom:1px solid #e2e8f0;
                                   color:#64748b;font-size:14px;width:40%;">Referencia</td>
                        <td style="padding:14px 20px;border-bottom:1px solid #e2e8f0;
                                   color:#1e293b;font-size:14px;font-weight:600;
                                   font-family:monospace;">{referencia}</td>
                      </tr>
                      <tr>
                        <td style="padding:14px 20px;border-bottom:1px solid #e2e8f0;
                                   color:#64748b;font-size:14px;">Estado</td>
                        <td style="padding:14px 20px;border-bottom:1px solid #e2e8f0;">
                          <span style="background:#dcfce7;color:#16a34a;padding:4px 12px;
                                       border-radius:50px;font-size:13px;font-weight:600;">
                            ✓ Confirmado
                          </span>
                        </td>
                      </tr>
                      <tr>
                        <td style="padding:16px 20px;color:#64748b;font-size:14px;">
                          Monto Pagado
                        </td>
                        <td style="padding:16px 20px;color:#16a34a;
                                   font-size:26px;font-weight:700;">
                          ${monto:N2} MXN
                        </td>
                      </tr>
                    </table>
                  </td>
                </tr>

                <!-- Note -->
                <tr>
                  <td style="padding:0 32px 32px;">
                    <div style="background:#f0f9ff;border-left:4px solid #6366f1;
                                border-radius:4px;padding:16px 20px;">
                      <p style="color:#1e3a5f;margin:0;font-size:14px;line-height:1.6;">
                        Conserva este correo como comprobante de pago. Para cualquier
                        aclaración, comunícate directamente con la notaría parroquial.
                      </p>
                    </div>
                  </td>
                </tr>

                <!-- Divider + Footer -->
                <tr>
                  <td style="padding:0 32px;">
                    <hr style="border:none;border-top:1px solid #e2e8f0;margin:0;">
                  </td>
                </tr>
                <tr>
                  <td style="padding:24px 32px;text-align:center;">
                    <p style="color:#94a3b8;font-size:12px;margin:0 0 4px;">
                      Notaría Parroquial de Axtla de Terrazas, S.L.P.
                    </p>
                    <p style="color:#94a3b8;font-size:12px;margin:0;">
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
