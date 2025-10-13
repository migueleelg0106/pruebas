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
                AvisoHelper.Mostrar(LangResources.Lang.errorTextoConfirmacionContrasenaRequerida);
                return;
            }

            if (!string.Equals(nuevaContrasena, confirmacion, StringComparison.Ordinal))
            {
                AvisoHelper.Mostrar(LangResources.Lang.errorTextoContrasenasNoCoinciden);
                bloqueContrasenaNueva.Clear();
                bloqueContrasenaConfirmacion.Clear();
                bloqueContrasenaNueva.Focus();
                return;
            }

            if (!PatronContrasenaValida.IsMatch(nuevaContrasena))
            {
                AvisoHelper.Mostrar(LangResources.Lang.errorTextoContrasenaFormato);
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
                        AvisoHelper.Mostrar(LangResources.Lang.errorTextoActualizarContrasena);
                        return;
                    }

                    if (resultado.OperacionExitosa)
                    {
                        ContrasenaActualizada = true;
                        string mensaje = MensajeServidorHelper.Localizar(
                            resultado.Mensaje,
                            LangResources.Lang.avisoTextoContrasenaActualizada);
                        AvisoHelper.Mostrar(mensaje);
                        DialogResult = true;
                        Close();
                        return;
                    }

                    string mensajeError = MensajeServidorHelper.Localizar(
                        resultado.Mensaje,
                        LangResources.Lang.errorTextoActualizarContrasena);

                    AvisoHelper.Mostrar(mensajeError);
                }
            }
            catch (FaultException<ServidorProxy.ErrorDetalleServicio> ex)
            {
                string mensaje = ErrorServicioHelper.ObtenerMensaje(
                    ex,
                    LangResources.Lang.errorTextoServidorActualizarContrasena);
                AvisoHelper.Mostrar(mensaje);
            }
            catch (EndpointNotFoundException)
            {
                AvisoHelper.Mostrar(LangResources.Lang.errorTextoServidorNoDisponible);
            }
            catch (TimeoutException)
            {
                AvisoHelper.Mostrar(LangResources.Lang.errorTextoServidorTiempoAgotado);
            }
            catch (CommunicationException)
            {
                AvisoHelper.Mostrar(LangResources.Lang.errorTextoServidorNoDisponible);
            }
            catch (InvalidOperationException)
            {
                AvisoHelper.Mostrar(LangResources.Lang.errorTextoPrepararSolicitudCambioContrasena);
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
