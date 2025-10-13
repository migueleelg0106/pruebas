using System;
using System.ServiceModel;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Input;
using PictionaryMusicalCliente.Modelo;
using PictionaryMusicalCliente.Servicios;
using PictionaryMusicalCliente.Utilidades;
using LangResources = PictionaryMusicalCliente.Properties.Langs;

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
                throw new ArgumentException(LangResources.Lang.errorTextoTokenRecuperacionObligatorio, nameof(tokenRecuperacion));
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
                new Avisos(LangResources.Lang.errorTextoConfirmacionContrasenaRequerida).ShowDialog();
                return;
            }

            if (!string.Equals(nuevaContrasena, confirmacion, StringComparison.Ordinal))
            {
                new Avisos(LangResources.Lang.errorTextoContrasenasNoCoinciden).ShowDialog();
                bloqueContrasenaNueva.Clear();
                bloqueContrasenaConfirmacion.Clear();
                bloqueContrasenaNueva.Focus();
                return;
            }

            if (!PatronContrasenaValida.IsMatch(nuevaContrasena))
            {
                new Avisos(LangResources.Lang.errorTextoContrasenaFormato).ShowDialog();
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
                        new Avisos(LangResources.Lang.errorTextoActualizarContrasena).ShowDialog();
                        return;
                    }

                    if (resultado.OperacionExitosa)
                    {
                        ContrasenaActualizada = true;
                        string mensaje = string.IsNullOrWhiteSpace(resultado.Mensaje)
                            ? LangResources.Lang.avisoTextoContrasenaActualizada
                            : resultado.Mensaje;
                        new Avisos(mensaje).ShowDialog();
                        DialogResult = true;
                        Close();
                        return;
                    }

                    string mensajeError = string.IsNullOrWhiteSpace(resultado.Mensaje)
                        ? LangResources.Lang.errorTextoActualizarContrasena
                        : resultado.Mensaje;

                    new Avisos(mensajeError).ShowDialog();
                }
            }
            catch (FaultException<ServidorProxy.ErrorDetalleServicio> ex)
            {
                string mensaje = ErrorServicioHelper.ObtenerMensaje(
                    ex,
                    LangResources.Lang.errorTextoServidorActualizarContrasena);
                new Avisos(mensaje).ShowDialog();
            }
            catch (EndpointNotFoundException)
            {
                new Avisos(LangResources.Lang.errorTextoServidorNoDisponible).ShowDialog();
            }
            catch (TimeoutException)
            {
                new Avisos(LangResources.Lang.errorTextoServidorNoRespondioTiempo).ShowDialog();
            }
            catch (CommunicationException)
            {
                new Avisos(LangResources.Lang.errorTextoComunicacionServidorSimple).ShowDialog();
            }
            catch (InvalidOperationException)
            {
                new Avisos(LangResources.Lang.errorTextoPrepararSolicitudCambioContrasena).ShowDialog();
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
