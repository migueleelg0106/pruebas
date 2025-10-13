using System;
using System.ServiceModel;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using PictionaryMusicalCliente.Modelo;
using PictionaryMusicalCliente.Properties.Langs;
using PictionaryMusicalCliente.Servicios;
using PictionaryMusicalCliente.Utilidades;

namespace PictionaryMusicalCliente
{
    public partial class CrearCuenta : Window
    {
        private static readonly Regex PatronCorreoValido = new Regex(@"^[^@\s]+@[^@\s]+\.[^@\s]+$", RegexOptions.Compiled);
        private static readonly Regex PatronContrasenaValida = new Regex(@"^(?=.*[A-Z])(?=.*\d)(?=.*[^A-Za-z0-9]).{8,15}$", RegexOptions.Compiled);

        private ObjetoAvatar _avatarSeleccionado;

        public CrearCuenta()
        {
            InitializeComponent();
            textoErrorUsuario.Visibility = Visibility.Collapsed;
            textoErrorCorreo.Visibility = Visibility.Collapsed;
        }

        private void EtiquetaSeleccionarAvatar(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            var ventanaSeleccion = new SeleccionarAvatar();
            if (ventanaSeleccion.ShowDialog() == true && ventanaSeleccion.AvatarSeleccionado != null)
            {
                _avatarSeleccionado = ventanaSeleccion.AvatarSeleccionado;
                MostrarAvatarSeleccionado();
            }
        }

        private async void Boton_CrearCuenta(object sender, RoutedEventArgs e)
        {
            string usuario = bloqueTextoUsuario.Text?.Trim();
            string correo = bloqueTextoCorreo.Text?.Trim();
            string nombre = bloqueTextoNombre.Text?.Trim();
            string apellido = bloqueTextoApellido.Text?.Trim();
            string contrasena = bloqueContrasenaContrasena.Password;

            textoErrorUsuario.Visibility = Visibility.Collapsed;
            textoErrorCorreo.Visibility = Visibility.Collapsed;

            RestablecerEstadoCampo(bloqueTextoUsuario);
            RestablecerEstadoCampo(bloqueTextoCorreo);
            RestablecerEstadoCampo(bloqueTextoNombre);
            RestablecerEstadoCampo(bloqueTextoApellido);
            RestablecerEstadoCampo(bloqueContrasenaContrasena);

            if (!ValidarCamposObligatorios(usuario, correo, nombre, apellido, contrasena))
            {
                return;
            }

            if (!PatronCorreoValido.IsMatch(correo))
            {
                MarcarCampoInvalido(bloqueTextoCorreo);
                AvisoHelper.Mostrar(Lang.errorTextoCorreoInvalido);
                bloqueTextoCorreo.Focus();
                return;
            }

            if (!PatronContrasenaValida.IsMatch(contrasena))
            {
                MarcarCampoInvalido(bloqueContrasenaContrasena);
                AvisoHelper.Mostrar(Lang.errorTextoContrasenaFormato);
                bloqueContrasenaContrasena.Focus();
                return;
            }

            if (_avatarSeleccionado == null)
            {
                AvisoHelper.Mostrar(Lang.globalTextoSeleccionarAvatar);
                return;
            }

            int? avatarId = await ObtenerIdAvatarSeleccionadoAsync();

            if (!avatarId.HasValue)
            {
                AvisoHelper.Mostrar(Lang.errorTextoIdentificarAvatar);
                return;
            }

            var solicitud = new SolicitudRegistrarUsuario
            {
                Usuario = usuario,
                Correo = correo,
                Nombre = nombre,
                Apellido = apellido,
                ContrasenaPlano = contrasena,
                AvatarId = avatarId.Value
            };

            ResultadoSolicitudCodigo resultadoCodigo;

            try
            {
                using (var proxy = new ServidorProxy())
                {
                    resultadoCodigo = await proxy.SolicitarCodigoVerificacionAsync(solicitud);
                }
            }
            catch (FaultException<ServidorProxy.ErrorDetalleServicio> ex)
            {
                string mensaje = ErrorServicioHelper.ObtenerMensaje(
                    ex,
                    Lang.errorTextoServidorCodigoVerificacion);
                AvisoHelper.Mostrar(mensaje);
                return;
            }
            catch (CommunicationException)
            {
                AvisoHelper.Mostrar(Lang.errorTextoServidorNoDisponible);
                return;
            }
            catch (TimeoutException)
            {
                AvisoHelper.Mostrar(Lang.errorTextoServidorTiempoAgotado);
                return;
            }
            catch (InvalidOperationException)
            {
                AvisoHelper.Mostrar(Lang.errorTextoServidorNoDisponible);
                return;
            }

            if (resultadoCodigo == null)
            {
                AvisoHelper.Mostrar(Lang.errorTextoErrorProcesarSolicitud);
                return;
            }

            textoErrorUsuario.Visibility = resultadoCodigo.UsuarioYaRegistrado ? Visibility.Visible : Visibility.Collapsed;
            textoErrorCorreo.Visibility = resultadoCodigo.CorreoYaRegistrado ? Visibility.Visible : Visibility.Collapsed;

            if (!resultadoCodigo.CodigoEnviado)
            {
                if (!resultadoCodigo.UsuarioYaRegistrado && !resultadoCodigo.CorreoYaRegistrado)
                {
                    string mensajeError = MensajeServidorHelper.Localizar(
                        resultadoCodigo.Mensaje,
                        Lang.errorTextoEnvioCodigoVerificacionDatos);

                    AvisoHelper.Mostrar(mensajeError);
                }
                return;
            }

            var ventanaVerificacion = new VerificarCodigo(resultadoCodigo.TokenCodigo, solicitud.Correo);
            ventanaVerificacion.ShowDialog();

            if (ventanaVerificacion.RegistroCompletado)
            {
                Close();
            }
        }

        private void Boton_Cancelar(object sender, RoutedEventArgs e) => Close();

        private bool ValidarCamposObligatorios(string usuario, string correo, string nombre, string apellido, string contrasena)
        {
            bool hayError = false;
            Control primerCampo = null;

            if (string.IsNullOrWhiteSpace(usuario))
            {
                hayError = true;
                if (primerCampo == null)
                {
                    primerCampo = bloqueTextoUsuario;
                }
                MarcarCampoInvalido(bloqueTextoUsuario);
            }

            if (string.IsNullOrWhiteSpace(nombre))
            {
                hayError = true;
                if (primerCampo == null)
                {
                    primerCampo = bloqueTextoNombre;
                }
                MarcarCampoInvalido(bloqueTextoNombre);
            }

            if (string.IsNullOrWhiteSpace(apellido))
            {
                hayError = true;
                if (primerCampo == null)
                {
                    primerCampo = bloqueTextoApellido;
                }
                MarcarCampoInvalido(bloqueTextoApellido);
            }

            if (string.IsNullOrWhiteSpace(correo))
            {
                hayError = true;
                if (primerCampo == null)
                {
                    primerCampo = bloqueTextoCorreo;
                }
                MarcarCampoInvalido(bloqueTextoCorreo);
            }

            if (string.IsNullOrWhiteSpace(contrasena))
            {
                hayError = true;
                if (primerCampo == null)
                {
                    primerCampo = bloqueContrasenaContrasena;
                }
                MarcarCampoInvalido(bloqueContrasenaContrasena);
            }

            if (hayError)
            {
                AvisoHelper.Mostrar(Lang.errorTextoCamposInvalidosGenerico);
                primerCampo?.Focus();
                return false;
            }

            return true;
        }

        private void MostrarAvatarSeleccionado()
        {
            if (_avatarSeleccionado == null)
            {
                imagenAvatarSeleccionado.ImageSource = null;
                return;
            }

            ImageSource imagen = ObtenerImagenDesdeAvatar(_avatarSeleccionado);
            imagenAvatarSeleccionado.ImageSource = imagen;
        }

        private async Task<int?> ObtenerIdAvatarSeleccionadoAsync()
        {
            if (_avatarSeleccionado == null || string.IsNullOrWhiteSpace(_avatarSeleccionado.RutaRelativa))
            {
                return null;
            }

            string rutaSeleccionada = NormalizarRutaParaComparacion(_avatarSeleccionado.RutaRelativa);

            try
            {
                using (var proxy = new ServidorProxy())
                {
                    var avatares = await proxy.ObtenerAvataresAsync();

                    if (avatares == null)
                    {
                        return null;
                    }

                    foreach (ObjetoAvatar avatar in avatares)
                    {
                        string rutaAvatar = NormalizarRutaParaComparacion(avatar.RutaRelativa);

                        if (!string.IsNullOrEmpty(rutaAvatar)
                            && string.Equals(rutaAvatar, rutaSeleccionada, StringComparison.OrdinalIgnoreCase))
                        {
                            return avatar.Id;
                        }
                    }
                }
            }
            catch (FaultException<ServidorProxy.ErrorDetalleServicio> ex)
            {
                string mensaje = ErrorServicioHelper.ObtenerMensaje(
                    ex,
                    Lang.errorTextoServidorInformacionAvatar);
                AvisoHelper.Mostrar(mensaje);
            }
            catch (EndpointNotFoundException)
            {
                AvisoHelper.Mostrar(Lang.errorTextoServidorNoDisponible);
            }
            catch (TimeoutException)
            {
                AvisoHelper.Mostrar(Lang.errorTextoServidorTiempoAgotado);
            }
            catch (CommunicationException)
            {
                AvisoHelper.Mostrar(Lang.errorTextoServidorNoDisponible);
            }
            catch (InvalidOperationException)
            {
                AvisoHelper.Mostrar(Lang.errorTextoErrorProcesarSolicitud);
            }

            return null;
        }

        private static ImageSource ObtenerImagenDesdeAvatar(ObjetoAvatar avatar)
        {
            if (avatar == null)
            {
                return null;
            }

            if (!string.IsNullOrWhiteSpace(avatar.ImagenUriAbsoluta)
                && Uri.TryCreate(avatar.ImagenUriAbsoluta, UriKind.Absolute, out Uri uriRemota))
            {
                return new BitmapImage(uriRemota);
            }

            if (!string.IsNullOrWhiteSpace(avatar.RutaRelativa))
            {
                string rutaNormalizada = NormalizarRutaLocal(avatar.RutaRelativa);

                if (Uri.TryCreate($"pack://application:,,,/{rutaNormalizada}", UriKind.Absolute, out Uri uriRecurso))
                {
                    try
                    {
                        return new BitmapImage(uriRecurso);
                    }
                    catch
                    {
                        // Se intentará con la ruta relativa simple.
                    }
                }

                try
                {
                    return new BitmapImage(new Uri($"/{rutaNormalizada}", UriKind.Relative));
                }
                catch
                {
                    return null;
                }
            }

            return null;
        }

        private static string NormalizarRutaLocal(string ruta)
        {
            if (string.IsNullOrWhiteSpace(ruta))
            {
                return null;
            }

            string rutaNormalizada = ruta
                .TrimStart('/')
                .Replace('\\', '/');

            return rutaNormalizada;
        }

        private static string NormalizarRutaParaComparacion(string ruta)
        {
            string rutaNormalizada = NormalizarRutaLocal(ruta);
            return string.IsNullOrWhiteSpace(rutaNormalizada)
                ? null
                : rutaNormalizada.ToLowerInvariant();
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
