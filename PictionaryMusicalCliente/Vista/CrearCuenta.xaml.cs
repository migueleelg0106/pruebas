using System;
using System.ServiceModel;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using PictionaryMusicalCliente.Modelo;
using PictionaryMusicalCliente.Properties.Langs;
using PictionaryMusicalCliente.Utilidades;
using AvataresSrv = PictionaryMusicalCliente.PictionaryServidorServicioAvatares;
using CodigoVerificacionSrv = PictionaryMusicalCliente.PictionaryServidorServicioCodigoVerificacion;

namespace PictionaryMusicalCliente
{
    public partial class CrearCuenta : Window
    {
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

            ControlVisualHelper.RestablecerEstadoCampo(bloqueTextoUsuario);
            ControlVisualHelper.RestablecerEstadoCampo(bloqueTextoCorreo);
            ControlVisualHelper.RestablecerEstadoCampo(bloqueTextoNombre);
            ControlVisualHelper.RestablecerEstadoCampo(bloqueTextoApellido);
            ControlVisualHelper.RestablecerEstadoCampo(bloqueContrasenaContrasena);

            if (!ValidarCamposObligatorios(usuario, correo, nombre, apellido, contrasena))
            {
                return;
            }

            if (!ValidacionEntradaHelper.TieneLongitudValidaUsuario(usuario))
            {
                ControlVisualHelper.MarcarCampoInvalido(bloqueTextoUsuario);
                AvisoHelper.Mostrar(string.Format(
                    Lang.errorTextoCampoLongitudMaxima,
                    Lang.globalTextoUsuario.ToLowerInvariant(),
                    ValidacionEntradaHelper.LongitudMaximaNombreUsuario));
                bloqueTextoUsuario.Focus();
                return;
            }

            if (!ValidacionEntradaHelper.TieneLongitudValidaNombre(nombre))
            {
                ControlVisualHelper.MarcarCampoInvalido(bloqueTextoNombre);
                AvisoHelper.Mostrar(string.Format(
                    Lang.errorTextoCampoLongitudMaxima,
                    Lang.globalTextoNombre.ToLowerInvariant(),
                    ValidacionEntradaHelper.LongitudMaximaNombre));
                bloqueTextoNombre.Focus();
                return;
            }

            if (!ValidacionEntradaHelper.TieneLongitudValidaApellido(apellido))
            {
                ControlVisualHelper.MarcarCampoInvalido(bloqueTextoApellido);
                AvisoHelper.Mostrar(string.Format(
                    Lang.errorTextoCampoLongitudMaxima,
                    Lang.globalTextoApellido.ToLowerInvariant(),
                    ValidacionEntradaHelper.LongitudMaximaApellido));
                bloqueTextoApellido.Focus();
                return;
            }

            if (!ValidacionEntradaHelper.TieneLongitudValidaCorreo(correo))
            {
                ControlVisualHelper.MarcarCampoInvalido(bloqueTextoCorreo);
                AvisoHelper.Mostrar(string.Format(
                    Lang.errorTextoCampoLongitudMaxima,
                    Lang.globalTextoCorreo.ToLowerInvariant(),
                    ValidacionEntradaHelper.LongitudMaximaCorreo));
                bloqueTextoCorreo.Focus();
                return;
            }

            if (!ValidacionEntradaHelper.EsCorreoValido(correo))
            {
                ControlVisualHelper.MarcarCampoInvalido(bloqueTextoCorreo);
                AvisoHelper.Mostrar(Lang.errorTextoCorreoInvalido);
                bloqueTextoCorreo.Focus();
                return;
            }

            if (!ValidacionEntradaHelper.EsContrasenaValida(contrasena))
            {
                ControlVisualHelper.MarcarCampoInvalido(bloqueContrasenaContrasena);
                AvisoHelper.Mostrar(Lang.errorTextoContrasenaFormato);
                bloqueContrasenaContrasena.Focus();
                return;
            }

            if (!ValidacionEntradaHelper.TieneLongitudValidaContrasena(contrasena))
            {
                ControlVisualHelper.MarcarCampoInvalido(bloqueContrasenaContrasena);
                AvisoHelper.Mostrar(string.Format(
                    Lang.errorTextoCampoLongitudMaxima,
                    Lang.globalTextoContrasena.ToLowerInvariant(),
                    ValidacionEntradaHelper.LongitudMaximaContrasena));
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

            var solicitud = new CodigoVerificacionSrv.NuevaCuentaDTO
            {
                Usuario = usuario,
                Correo = correo,
                Nombre = nombre,
                Apellido = apellido,
                Contrasena = contrasena,
                AvatarId = avatarId.Value
            };

            CodigoVerificacionSrv.ResultadoSolicitudCodigoDTO resultadoCodigo;

            try
            {
                resultadoCodigo = await CodigoVerificacionServicioHelper.SolicitarCodigoRegistroAsync(solicitud);
            }
            catch (FaultException ex)
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
                ControlVisualHelper.MarcarCampoInvalido(bloqueTextoUsuario);
            }

            if (string.IsNullOrWhiteSpace(nombre))
            {
                hayError = true;
                if (primerCampo == null)
                {
                    primerCampo = bloqueTextoNombre;
                }
                ControlVisualHelper.MarcarCampoInvalido(bloqueTextoNombre);
            }

            if (string.IsNullOrWhiteSpace(apellido))
            {
                hayError = true;
                if (primerCampo == null)
                {
                    primerCampo = bloqueTextoApellido;
                }
                ControlVisualHelper.MarcarCampoInvalido(bloqueTextoApellido);
            }

            if (string.IsNullOrWhiteSpace(correo))
            {
                hayError = true;
                if (primerCampo == null)
                {
                    primerCampo = bloqueTextoCorreo;
                }
                ControlVisualHelper.MarcarCampoInvalido(bloqueTextoCorreo);
            }

            if (string.IsNullOrWhiteSpace(contrasena))
            {
                hayError = true;
                if (primerCampo == null)
                {
                    primerCampo = bloqueContrasenaContrasena;
                }
                ControlVisualHelper.MarcarCampoInvalido(bloqueContrasenaContrasena);
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
            imagenAvatarSeleccionado.ImageSource = _avatarSeleccionado == null
                ? null
                : AvatarImagenHelper.CrearImagen(_avatarSeleccionado);
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
                var cliente = new AvataresSrv.CatalogoAvataresClient("BasicHttpBinding_ICatalogoAvatares");
                AvataresSrv.AvatarDTO[] avatares = await WcfClientHelper.UsarAsync(
                    cliente,
                    c => c.ObtenerAvataresDisponiblesAsync());

                if (avatares == null)
                {
                    return null;
                }

                foreach (AvataresSrv.AvatarDTO avatar in avatares)
                {
                    string rutaAvatar = NormalizarRutaParaComparacion(avatar?.RutaRelativa);

                    if (!string.IsNullOrEmpty(rutaAvatar)
                        && string.Equals(rutaAvatar, rutaSeleccionada, StringComparison.OrdinalIgnoreCase))
                    {
                        return avatar.Id;
                    }
                }
            }
            catch (FaultException ex)
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

        
    }
}
