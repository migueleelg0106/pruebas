using System;
using System.Configuration;
using System.Net;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;

namespace Servicios.Servicios.Notificaciones
{
    public class CorreoCodigoVerificacionNotificador : ICodigoVerificacionNotificador
    {
        private const string AsuntoPredeterminado = "Código de verificación";
        private const string PlantillaMensaje = "Tu código de verificación es {0}. Este código expira en 5 minutos.";

        public async Task EnviarCodigoAsync(string correoDestino, string codigoGenerado)
        {
            if (string.IsNullOrWhiteSpace(correoDestino))
            {
                throw new ArgumentException("El correo destino es requerido.", nameof(correoDestino));
            }

            string remitente = ConfigurationManager.AppSettings["Correo.Remitente.Direccion"];
            string nombreRemitente = ConfigurationManager.AppSettings["Correo.Remitente.Nombre"];
            string host = ConfigurationManager.AppSettings["Correo.Smtp.Host"];
            string puertoCadena = ConfigurationManager.AppSettings["Correo.Smtp.Puerto"];
            string usuario = ConfigurationManager.AppSettings["Correo.Smtp.Usuario"];
            string contrasena = ConfigurationManager.AppSettings["Correo.Smtp.Contrasena"];
            string habilitarSslCadena = ConfigurationManager.AppSettings["Correo.Smtp.HabilitarSsl"];

            if (string.IsNullOrWhiteSpace(remitente) || string.IsNullOrWhiteSpace(host))
            {
                throw new InvalidOperationException("La configuración de correo electrónico no está completa.");
            }

            if (!int.TryParse(puertoCadena, out int puerto))
            {
                puerto = 25;
            }

            bool habilitarSsl = true;
            if (!string.IsNullOrWhiteSpace(habilitarSslCadena))
            {
                bool.TryParse(habilitarSslCadena, out habilitarSsl);
            }

            using (var mensaje = new MailMessage())
            {
                mensaje.From = new MailAddress(remitente, string.IsNullOrWhiteSpace(nombreRemitente) ? remitente : nombreRemitente, Encoding.UTF8);
                mensaje.To.Add(new MailAddress(correoDestino));
                mensaje.Subject = AsuntoPredeterminado;
                mensaje.Body = string.Format(PlantillaMensaje, codigoGenerado);
                mensaje.BodyEncoding = Encoding.UTF8;
                mensaje.SubjectEncoding = Encoding.UTF8;

                using (var cliente = new SmtpClient(host, puerto))
                {
                    cliente.EnableSsl = habilitarSsl;
                    if (!string.IsNullOrWhiteSpace(usuario))
                    {
                        cliente.Credentials = new NetworkCredential(usuario, contrasena);
                    }

                    await cliente.SendMailAsync(mensaje).ConfigureAwait(false);
                }
            }
        }
    }
}
