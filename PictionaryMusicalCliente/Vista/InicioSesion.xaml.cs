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
using PictionaryMusicalCliente.Servicios;
using PictionaryMusicalCliente.Sesiones;
using PictionaryMusicalCliente.Utilidades;
using LangResources = PictionaryMusicalCliente.Properties.Langs;

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

            if (string.IsNullOrWhiteSpace(identificador) || string.IsNullOrWhiteSpace(contrasena))
            {
                new Avisos(LangResources.Lang.errorTextoCredencialesIncompletas).ShowDialog();
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
                using (var proxy = new ServidorProxy())
                {
                    var solicitud = new SolicitudInicioSesion
                    {
                        Identificador = identificador.Trim(),
                        Contrasena = contrasena
                    };

                    ResultadoInicioSesion resultado = await proxy.IniciarSesionAsync(solicitud);

                    if (resultado == null)
                    {
                        new Avisos(LangResources.Lang.errorTextoServidorSinRespuesta).ShowDialog();
                        return;
                    }

                    if (resultado.InicioSesionExitoso)
                    {
                        SesionUsuarioActual.Instancia.EstablecerUsuario(resultado.Usuario);
                        VentanaPrincipal ventana = new VentanaPrincipal();
                        Application.Current.MainWindow = ventana;
                        ventana.Show();
                        this.Close();
                        return;
                    }

                    string mensaje = string.IsNullOrWhiteSpace(resultado.Mensaje)
                        ? Properties.Langs.Lang.errorTextoCredencialesTitulo
                        : resultado.Mensaje;

                    new Avisos(mensaje).ShowDialog();
                }
            }
            catch (FaultException<ServidorProxy.ErrorDetalleServicio> ex)
            {
                string mensaje = ErrorServicioHelper.ObtenerMensaje(
                    ex,
                    LangResources.Lang.errorTextoServidorInicioSesion);
                new Avisos(mensaje).ShowDialog();
            }
            catch (EndpointNotFoundException)
            {
                new Avisos(LangResources.Lang.errorTextoServidorNoDisponible).ShowDialog();
            }
            catch (TimeoutException)
            {
                new Avisos(LangResources.Lang.errorTextoServidorTiempoAgotado).ShowDialog();
            }
            catch (CommunicationException)
            {
                new Avisos(LangResources.Lang.errorTextoComunicacionServidorNuevamente).ShowDialog();
            }
            catch (InvalidOperationException)
            {
                new Avisos(LangResources.Lang.errorTextoPrepararSolicitudInicioSesion).ShowDialog();
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

            if (string.IsNullOrWhiteSpace(identificador))
            {
                new Avisos(LangResources.Lang.errorTextoIdentificadorRecuperacionRequerido).ShowDialog();
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
                using (var proxy = new ServidorProxy())
                {
                    var solicitud = new SolicitudRecuperarCuenta
                    {
                        Identificador = identificador
                    };

                    ResultadoSolicitudRecuperacion resultado = await proxy.SolicitarCodigoRecuperacionAsync(solicitud);

                    if (resultado == null)
                    {
                        new Avisos(LangResources.Lang.errorTextoIniciarRecuperacion).ShowDialog();
                        return;
                    }

                    if (!resultado.CuentaEncontrada)
                    {
                        string mensajeCuenta = string.IsNullOrWhiteSpace(resultado.Mensaje)
                            ? LangResources.Lang.errorTextoCuentaNoRegistrada
                            : resultado.Mensaje;
                        new Avisos(mensajeCuenta).ShowDialog();
                        return;
                    }

                    if (!resultado.CodigoEnviado || string.IsNullOrWhiteSpace(resultado.TokenRecuperacion))
                    {
                        string mensajeCodigo = string.IsNullOrWhiteSpace(resultado.Mensaje)
                            ? LangResources.Lang.errorTextoEnvioCodigoVerificacionMasTarde
                            : resultado.Mensaje;
                        new Avisos(mensajeCodigo).ShowDialog();
                        return;
                    }

                    string tokenRecuperacion = resultado.TokenRecuperacion;
                    string correoDestino = resultado.CorreoDestino;

                    async Task<ResultadoOperacion> ConfirmarCodigoAsync(string codigo)
                    {
                        var confirmacion = new SolicitudConfirmarCodigoRecuperacion
                        {
                            TokenRecuperacion = tokenRecuperacion,
                            Codigo = codigo
                        };

                        return await proxy.ConfirmarCodigoRecuperacionAsync(confirmacion);
                    }

                    async Task<ResultadoSolicitudCodigo> ReenviarCodigoAsync()
                    {
                        var reenvio = new SolicitudReenviarCodigoRecuperacion
                        {
                            TokenRecuperacion = tokenRecuperacion
                        };

                        ResultadoSolicitudCodigo resultadoReenvio = await proxy.ReenviarCodigoRecuperacionAsync(reenvio);

                        if (resultadoReenvio != null && !string.IsNullOrWhiteSpace(resultadoReenvio.TokenVerificacion))
                        {
                            tokenRecuperacion = resultadoReenvio.TokenVerificacion;
                        }

                        return resultadoReenvio;
                    }

                    var ventanaVerificacion = new VerificarCodigo(
                        tokenRecuperacion,
                        correoDestino,
                        ConfirmarCodigoAsync,
                        ReenviarCodigoAsync,
                        LangResources.Lang.avisoTextoCodigoDescripcionRecuperacion);

                    ventanaVerificacion.ShowDialog();

                    if (!ventanaVerificacion.OperacionCompletada)
                    {
                        return;
                    }

                    var ventanaCambio = new CambioContrasena(tokenRecuperacion, identificador);
                    bool? resultadoCambio = ventanaCambio.ShowDialog();

                    if (ventanaCambio.ContrasenaActualizada || resultadoCambio == true)
                    {
                        bloqueContrasenaContrasena.Clear();
                    }
                }
            }
            catch (FaultException<ServidorProxy.ErrorDetalleServicio> ex)
            {
                string mensaje = ErrorServicioHelper.ObtenerMensaje(
                    ex,
                    LangResources.Lang.errorTextoServidorInicioRecuperacion);
                new Avisos(mensaje).ShowDialog();
            }
            catch (EndpointNotFoundException)
            {
                new Avisos(LangResources.Lang.errorTextoServidorNoDisponible).ShowDialog();
            }
            catch (TimeoutException)
            {
                new Avisos(LangResources.Lang.errorTextoServidorTiempoAgotado).ShowDialog();
            }
            catch (CommunicationException)
            {
                new Avisos(LangResources.Lang.errorTextoComunicacionServidorNuevamente).ShowDialog();
            }
            catch (InvalidOperationException)
            {
                new Avisos(LangResources.Lang.errorTextoProcesarSolicitudRecuperacion).ShowDialog();
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
    }
}
