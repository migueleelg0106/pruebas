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
            textoErrorUsuario.Visibility = Visibility.Collapsed;
            textoErrorCorreo.Visibility = Visibility.Collapsed;

            ControlVisualHelper.RestablecerEstadoCampo(bloqueTextoUsuario);
            ControlVisualHelper.RestablecerEstadoCampo(bloqueTextoCorreo);
            ControlVisualHelper.RestablecerEstadoCampo(bloqueTextoNombre);
            ControlVisualHelper.RestablecerEstadoCampo(bloqueTextoApellido);
            ControlVisualHelper.RestablecerEstadoCampo(bloqueContrasenaContrasena);

            ValidacionEntradaHelper.ResultadoValidacion resultadoUsuario = ValidacionEntradaHelper.ValidarUsuario(bloqueTextoUsuario.Text);

            if (!resultadoUsuario.EsValido)
            {
                ControlVisualHelper.MarcarCampoInvalido(bloqueTextoUsuario);
                AvisoHelper.Mostrar(resultadoUsuario.MensajeError);
                bloqueTextoUsuario.Focus();
                return;
            }

            string usuario = resultadoUsuario.ValorNormalizado;

            ValidacionEntradaHelper.ResultadoValidacion resultadoNombre = ValidacionEntradaHelper.ValidarNombre(bloqueTextoNombre.Text);

            if (!resultadoNombre.EsValido)
            {
                ControlVisualHelper.MarcarCampoInvalido(bloqueTextoNombre);
                AvisoHelper.Mostrar(resultadoNombre.MensajeError);
                bloqueTextoNombre.Focus();
                return;
            }

            string nombre = resultadoNombre.ValorNormalizado;

            ValidacionEntradaHelper.ResultadoValidacion resultadoApellido = ValidacionEntradaHelper.ValidarApellido(bloqueTextoApellido.Text);

            if (!resultadoApellido.EsValido)
            {
                ControlVisualHelper.MarcarCampoInvalido(bloqueTextoApellido);
                AvisoHelper.Mostrar(resultadoApellido.MensajeError);
                bloqueTextoApellido.Focus();
                return;
            }

            string apellido = resultadoApellido.ValorNormalizado;

            ValidacionEntradaHelper.ResultadoValidacion resultadoCorreo = ValidacionEntradaHelper.ValidarCorreo(bloqueTextoCorreo.Text);

            if (!resultadoCorreo.EsValido)
            {
                ControlVisualHelper.MarcarCampoInvalido(bloqueTextoCorreo);
                AvisoHelper.Mostrar(resultadoCorreo.MensajeError);
                bloqueTextoCorreo.Focus();
                return;
            }

            string correo = resultadoCorreo.ValorNormalizado;

            ValidacionEntradaHelper.ResultadoValidacion resultadoContrasena = ValidacionEntradaHelper.ValidarContrasena(bloqueContrasenaContrasena.Password);

            if (!resultadoContrasena.EsValido)
            {
                ControlVisualHelper.MarcarCampoInvalido(bloqueContrasenaContrasena);
                AvisoHelper.Mostrar(resultadoContrasena.MensajeError);
                bloqueContrasenaContrasena.Focus();
                return;
            }

            string contrasena = resultadoContrasena.ValorNormalizado;

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

            string rutaSeleccionada = AvatarRutaHelper.NormalizarRutaParaComparacion(_avatarSeleccionado.RutaRelativa);

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
                    string rutaAvatar = AvatarRutaHelper.NormalizarRutaParaComparacion(avatar?.RutaRelativa);

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


    }
}
