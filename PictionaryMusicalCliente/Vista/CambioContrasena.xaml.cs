using System;
using System.ServiceModel;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Input;
using PictionaryMusicalCliente.Modelo;
using PictionaryMusicalCliente.Servicios;

namespace PictionaryMusicalCliente
{
    public partial class CambioContrasena : Window
    {
        private static readonly Regex PatronContrasenaValida = new Regex(@"^(?=.*[A-Z])(?=.*\d)(?=.*[^A-Za-z0-9]).{8,15}$", RegexOptions.Compiled);
        private readonly string _tokenRecuperacion;
        public bool ContrasenaActualizada { get; private set; }

        public CambioContrasena(string tokenRecuperacion, string identificador)
        {
            if (string.IsNullOrWhiteSpace(tokenRecuperacion))
            {
                throw new ArgumentException("El token de recuperación es obligatorio.", nameof(tokenRecuperacion));
            }

            InitializeComponent();
            _tokenRecuperacion = tokenRecuperacion;
            ContrasenaActualizada = false;
        }

        private async void BotonConfirmarContrasena(object sender, RoutedEventArgs e)
        {
            string nuevaContrasena = bloqueContrasenaNueva.Password;
            string confirmacion = bloqueContrasenaConfirmacion.Password;

            if (string.IsNullOrWhiteSpace(nuevaContrasena) || string.IsNullOrWhiteSpace(confirmacion))
            {
                new Avisos("Ingrese y confirme la nueva contraseña.").ShowDialog();
                return;
            }

            if (!string.Equals(nuevaContrasena, confirmacion, StringComparison.Ordinal))
            {
                new Avisos("Las contraseñas no coinciden.").ShowDialog();
                bloqueContrasenaNueva.Clear();
                bloqueContrasenaConfirmacion.Clear();
                bloqueContrasenaNueva.Focus();
                return;
            }

            if (!PatronContrasenaValida.IsMatch(nuevaContrasena))
            {
                new Avisos("La contraseña debe tener de 8 a 15 caracteres con al menos una mayúscula, un número y un carácter especial.").ShowDialog();
                bloqueContrasenaNueva.Focus();
                return;
            }

            botonConfirmarContrasena.IsEnabled = false;
            Mouse.OverrideCursor = Cursors.Wait;

            try
            {
                using (var proxy = new ServidorProxy())
                {
                    var solicitud = new SolicitudActualizarContrasena
                    {
                        TokenRecuperacion = _tokenRecuperacion,
                        NuevaContrasena = nuevaContrasena
                    };

                    ResultadoOperacion resultado = await proxy.ActualizarContrasenaAsync(solicitud);

                    if (resultado == null)
                    {
                        new Avisos("No se pudo actualizar la contraseña. Intente nuevamente.").ShowDialog();
                        return;
                    }

                    if (resultado.OperacionExitosa)
                    {
                        ContrasenaActualizada = true;
                        string mensaje = string.IsNullOrWhiteSpace(resultado.Mensaje)
                            ? "La contraseña se actualizó correctamente."
                            : resultado.Mensaje;
                        new Avisos(mensaje).ShowDialog();
                        DialogResult = true;
                        Close();
                        return;
                    }

                    string mensajeError = string.IsNullOrWhiteSpace(resultado.Mensaje)
                        ? "No fue posible actualizar la contraseña."
                        : resultado.Mensaje;

                    new Avisos(mensajeError).ShowDialog();
                }
            }
            catch (EndpointNotFoundException)
            {
                new Avisos("No se pudo contactar al servidor. Intente más tarde.").ShowDialog();
            }
            catch (TimeoutException)
            {
                new Avisos("El servidor no respondió a tiempo. Intente nuevamente.").ShowDialog();
            }
            catch (CommunicationException)
            {
                new Avisos("Ocurrió un problema de comunicación con el servidor.").ShowDialog();
            }
            catch (InvalidOperationException)
            {
                new Avisos("Ocurrió un error al preparar la solicitud de cambio de contraseña.").ShowDialog();
            }
            finally
            {
                if (!ContrasenaActualizada)
                {
                    botonConfirmarContrasena.IsEnabled = true;
                }

                Mouse.OverrideCursor = null;
            }
        }

        private void BotonCancelarContrasena(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}
