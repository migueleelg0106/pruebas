using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using PictionaryMusicalCliente.Modelo;
using PictionaryMusicalCliente.Sesiones;
using PictionaryMusicalCliente.Utilidades;
using LangResources = PictionaryMusicalCliente.Properties.Langs;
using CodigoVerificacionSrv = PictionaryMusicalCliente.PictionaryServidorServicioCodigoVerificacion;
using ReenvioSrv = PictionaryMusicalCliente.PictionaryServidorServicioReenvioCodigoVerificacion;
using InicioSesionSrv = PictionaryMusicalCliente.PictionaryServidorServicioInicioSesion;

namespace PictionaryMusicalCliente
{
    /// <summary>
    /// Lógica de interacción para InicioSesion.xaml
    /// </summary>
    public partial class InicioSesion : Window
    {
        public InicioSesion()
        {
            InitializeComponent();
        }

        private void CuadroCombinadoSeleccionLenguaje(object sender, SelectionChangedEventArgs e)
        {
            if (cuadroCombinadoLenguaje.SelectedIndex == 0)
                Properties.Settings.Default.idiomaCodigo = "es-MX";
            else
                Properties.Settings.Default.idiomaCodigo = "en-US";
            Properties.Settings.Default.Save();

            PictionaryMusicalCliente.Properties.Langs.Lang.Culture = new CultureInfo(Properties.Settings.Default.idiomaCodigo);

            InicioSesion nuevoInicioSesion = new InicioSesion();
            Application.Current.MainWindow = nuevoInicioSesion;
            nuevoInicioSesion.Show();
            this.Close();
        }

        private async void BotonEntrar(object sender, RoutedEventArgs e)
        {
            string identificador = bloqueTextoUsuario.Text;
            string contrasena = bloqueContrasenaContrasena.Password;

            RestablecerEstadoCampo(bloqueTextoUsuario);
            RestablecerEstadoCampo(bloqueContrasenaContrasena);

            bool hayCamposInvalidos = false;

            string identificadorNormalizado = identificador?.Trim();

            if (!ValidacionEntradaHelper.EsCampoObligatorioValido(identificadorNormalizado))
            {
                hayCamposInvalidos = true;
                MarcarCampoInvalido(bloqueTextoUsuario);
            }

            if (!ValidacionEntradaHelper.EsCampoObligatorioValido(contrasena))
            {
                hayCamposInvalidos = true;
                MarcarCampoInvalido(bloqueContrasenaContrasena);
            }

            if (hayCamposInvalidos)
            {
                AvisoHelper.Mostrar(LangResources.Lang.errorTextoCamposInvalidosGenerico);
                return;
            }

            if (!string.IsNullOrEmpty(identificadorNormalizado) && identificadorNormalizado.Contains("@"))
            {
                if (!ValidacionEntradaHelper.TieneLongitudValidaCorreo(identificadorNormalizado))
                {
                    MarcarCampoInvalido(bloqueTextoUsuario);
                    AvisoHelper.Mostrar(string.Format(
                        LangResources.Lang.errorTextoCampoLongitudMaxima,
                        LangResources.Lang.globalTextoCorreo.ToLowerInvariant(),
                        ValidacionEntradaHelper.LongitudMaximaCorreo));
                    bloqueTextoUsuario.Focus();
                    return;
                }
            }
            else if (!ValidacionEntradaHelper.TieneLongitudValidaUsuario(identificadorNormalizado))
            {
                MarcarCampoInvalido(bloqueTextoUsuario);
                AvisoHelper.Mostrar(string.Format(
                    LangResources.Lang.errorTextoCampoLongitudMaxima,
                    LangResources.Lang.globalTextoUsuario.ToLowerInvariant(),
                    ValidacionEntradaHelper.LongitudMaximaNombreUsuario));
                bloqueTextoUsuario.Focus();
                return;
            }

            if (!ValidacionEntradaHelper.TieneLongitudValidaContrasena(contrasena))
            {
                MarcarCampoInvalido(bloqueContrasenaContrasena);
                AvisoHelper.Mostrar(string.Format(
                    LangResources.Lang.errorTextoCampoLongitudMaxima,
                    LangResources.Lang.globalTextoContrasena.ToLowerInvariant(),
                    ValidacionEntradaHelper.LongitudMaximaContrasena));
                bloqueContrasenaContrasena.Focus();
                return;
            }

            Button boton = sender as Button;

            if (boton != null)
            {
                boton.IsEnabled = false;
            }

            Mouse.OverrideCursor = Cursors.Wait;

            try
            {
                var solicitud = new InicioSesionSrv.CredencialesInicioSesionDTO
                {
                    Identificador = identificadorNormalizado,
                    Contrasena = contrasena
                };

                var cliente = new InicioSesionSrv.InicioSesionManejadorClient(
                    "BasicHttpBinding_IInicioSesionManejador");

                InicioSesionSrv.ResultadoInicioSesionDTO resultado = await WcfClientHelper.UsarAsync(
                    cliente,
                    c => c.IniciarSesionAsync(solicitud));

                if (resultado == null)
                {
                    AvisoHelper.Mostrar(LangResources.Lang.errorTextoServidorTiempoAgotado);
                    return;
                }

                if (resultado.InicioSesionExitoso)
                {
                    UsuarioAutenticado usuario = UsuarioMapper.CrearDesde(resultado.Usuario);
                    SesionUsuarioActual.Instancia.EstablecerUsuario(usuario);
                    VentanaPrincipal ventana = new VentanaPrincipal();
                    Application.Current.MainWindow = ventana;
                    ventana.Show();
                    this.Close();
                    return;
                }

                string mensaje = MensajeServidorHelper.Localizar(
                    resultado.Mensaje,
                    Properties.Langs.Lang.errorTextoCredencialesTitulo);

                AvisoHelper.Mostrar(mensaje);
            }
            catch (FaultException ex)
            {
                string mensaje = ErrorServicioHelper.ObtenerMensaje(
                    ex,
                    LangResources.Lang.errorTextoServidorInicioSesion);
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
                AvisoHelper.Mostrar(LangResources.Lang.errorTextoErrorProcesarSolicitud);
            }
            finally
            {
                if (boton != null)
                {
                    boton.IsEnabled = true;
                }

                Mouse.OverrideCursor = null;
            }
        }

        private void BotonEntrarInvitado(object sender, RoutedEventArgs e)
        {
            UnirsePartidaInvitado ventana = new UnirsePartidaInvitado();
            ventana.ShowDialog();
        }

        private void BotonCrearCuenta(object sender, RoutedEventArgs e)
        {
            CrearCuenta ventana = new CrearCuenta();
            ventana.ShowDialog();
        }

        private async void LabelOlvidasteContraseña(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            string identificador = bloqueTextoUsuario.Text?.Trim();

            RestablecerEstadoCampo(bloqueTextoUsuario);

            if (string.IsNullOrWhiteSpace(identificador))
            {
                MarcarCampoInvalido(bloqueTextoUsuario);
                AvisoHelper.Mostrar(LangResources.Lang.errorTextoCamposInvalidosGenerico);
                bloqueTextoUsuario.Focus();
                return;
            }

            Label etiqueta = sender as Label;
            if (etiqueta != null)
            {
                etiqueta.IsEnabled = false;
            }

            Mouse.OverrideCursor = Cursors.Wait;

            try
            {
                CodigoVerificacionSrv.ResultadoSolicitudRecuperacionDTO resultado = await CodigoVerificacionServicioHelper.SolicitarCodigoRecuperacionAsync(identificador);

                if (resultado == null)
                {
                    AvisoHelper.Mostrar(LangResources.Lang.errorTextoIniciarRecuperacion);
                    return;
                }

                if (!resultado.CuentaEncontrada)
                {
                    string mensajeCuenta = MensajeServidorHelper.Localizar(
                        resultado.Mensaje,
                        LangResources.Lang.errorTextoCuentaNoRegistrada);
                    AvisoHelper.Mostrar(mensajeCuenta);
                    return;
                }

                if (!resultado.CodigoEnviado || string.IsNullOrWhiteSpace(resultado.TokenCodigo))
                {
                    string mensajeCodigo = MensajeServidorHelper.Localizar(
                        resultado.Mensaje,
                        LangResources.Lang.errorTextoEnvioCodigoVerificacionMasTarde);
                    AvisoHelper.Mostrar(mensajeCodigo);
                    return;
                }

                string tokenCodigo = resultado.TokenCodigo;
                string correoDestino = resultado.CorreoDestino;

                async Task<VerificarCodigo.ConfirmacionResultado> ConfirmarCodigoAsync(string codigo)
                {
                    CodigoVerificacionSrv.ResultadoOperacionDTO resultadoConfirmacion = await CodigoVerificacionServicioHelper.ConfirmarCodigoRecuperacionAsync(
                        tokenCodigo,
                        codigo);

                    if (resultadoConfirmacion == null)
                    {
                        return null;
                    }

                    return new VerificarCodigo.ConfirmacionResultado(
                        resultadoConfirmacion.OperacionExitosa,
                        resultadoConfirmacion.Mensaje);
                }

                async Task<VerificarCodigo.ReenvioResultado> ReenviarCodigoAsync()
                {
                    ReenvioSrv.ResultadoSolicitudCodigoDTO resultadoReenvio = await CodigoVerificacionServicioHelper.ReenviarCodigoRecuperacionAsync(tokenCodigo);

                    if (resultadoReenvio != null && !string.IsNullOrWhiteSpace(resultadoReenvio.TokenCodigo))
                    {
                        tokenCodigo = resultadoReenvio.TokenCodigo;
                    }

                    return resultadoReenvio == null
                        ? null
                        : new VerificarCodigo.ReenvioResultado(
                            resultadoReenvio.CodigoEnviado,
                            resultadoReenvio.Mensaje,
                            resultadoReenvio.TokenCodigo);
                }

                var ventanaVerificacion = new VerificarCodigo(
                    tokenCodigo,
                    correoDestino,
                    ConfirmarCodigoAsync,
                    ReenviarCodigoAsync,
                    LangResources.Lang.avisoTextoCodigoDescripcionRecuperacion);

                ventanaVerificacion.ShowDialog();

                if (!ventanaVerificacion.OperacionCompletada)
                {
                    return;
                }

                var ventanaCambio = new CambioContrasena(tokenCodigo, identificador);
                bool? resultadoCambio = ventanaCambio.ShowDialog();

                if (ventanaCambio.ContrasenaActualizada || resultadoCambio == true)
                {
                    bloqueContrasenaContrasena.Clear();
                }
            }
            catch (FaultException ex)
            {
                string mensaje = ErrorServicioHelper.ObtenerMensaje(
                    ex,
                    LangResources.Lang.errorTextoServidorInicioRecuperacion);
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
                AvisoHelper.Mostrar(LangResources.Lang.errorTextoErrorProcesarSolicitud);
            }
            finally
            {
                if (etiqueta != null)
                {
                    etiqueta.IsEnabled = true;
                }

                Mouse.OverrideCursor = null;
            }
        }

        private static void RestablecerEstadoCampo(Control control)
        {
            if (control == null)
            {
                return;
            }

            control.ClearValue(Control.BorderBrushProperty);
            control.ClearValue(Control.BorderThicknessProperty);
        }

        private static void MarcarCampoInvalido(Control control)
        {
            if (control == null)
            {
                return;
            }

            control.BorderBrush = Brushes.Red;
            control.BorderThickness = new Thickness(2);
        }
    }
}
