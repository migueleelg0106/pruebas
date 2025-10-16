using System;
using System.ServiceModel;
using System.Windows;
using System.Windows.Input;
using PictionaryMusicalCliente.Utilidades;
using LangResources = PictionaryMusicalCliente.Properties.Langs;
using CambioContrasenaSrv = PictionaryMusicalCliente.PictionaryServidorServicioCambioContrasena;

namespace PictionaryMusicalCliente
{
    public partial class CambioContrasena : Window
    {
        private readonly string _tokenCodigo;
        public bool ContrasenaActualizada { get; private set; }

        public CambioContrasena(string tokenCodigo, string identificador)
        {
            if (string.IsNullOrWhiteSpace(tokenCodigo))
            {
                throw new ArgumentException(LangResources.Lang.errorTextoTokenCodigoObligatorio, nameof(tokenCodigo));
            }

            InitializeComponent();
            _tokenCodigo = tokenCodigo;
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

            ValidacionEntradaHelper.ResultadoValidacion resultadoContrasena = ValidacionEntradaHelper.ValidarContrasena(nuevaContrasena);

            if (!resultadoContrasena.EsValido)
            {
                AvisoHelper.Mostrar(resultadoContrasena.MensajeError);
                bloqueContrasenaNueva.Focus();
                return;
            }

            nuevaContrasena = resultadoContrasena.ValorNormalizado;

            botonConfirmarContrasena.IsEnabled = false;
            Mouse.OverrideCursor = Cursors.Wait;

            try
            {
                var solicitud = new CambioContrasenaSrv.ActualizarContrasenaDTO
                {
                    TokenCodigo = _tokenCodigo,
                    NuevaContrasena = nuevaContrasena
                };

                var cliente = new CambioContrasenaSrv.CambiarContrasenaManejadorClient(
                    "BasicHttpBinding_ICambiarContrasenaManejador");

                CambioContrasenaSrv.ResultadoOperacionDTO resultado = await WcfClientHelper.UsarAsync(
                    cliente,
                    c => c.ActualizarContrasenaAsync(solicitud));

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
            catch (FaultException ex)
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
