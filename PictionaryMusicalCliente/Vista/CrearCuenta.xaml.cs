using System;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using PictionaryMusicalCliente.Modelo;
using PictionaryMusicalCliente.Properties.Langs;
using PictionaryMusicalCliente.Servicios;

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

            if (!ValidarCamposObligatorios(usuario, correo, nombre, apellido, contrasena))
            {
                return;
            }

            if (!PatronCorreoValido.IsMatch(correo))
            {
                new Avisos("Ingrese un correo electrónico válido.").ShowDialog();
                bloqueTextoCorreo.Focus();
                return;
            }

            if (!PatronContrasenaValida.IsMatch(contrasena))
            {
                new Avisos("La contraseña debe tener de 8 a 15 caracteres con al menos una mayúscula, un número y un carácter especial.").ShowDialog();
                bloqueContrasenaContrasena.Focus();
                return;
            }

            if (_avatarSeleccionado == null)
            {
                new Avisos(Lang.globalTextoSeleccionarAvatar).ShowDialog();
                return;
            }

            int? avatarId = await ObtenerIdAvatarSeleccionadoAsync();

            if (!avatarId.HasValue)
            {
                new Avisos("No se pudo identificar el avatar seleccionado. Intente nuevamente.").ShowDialog();
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
            catch (Exception)
            {
                new Avisos("No se pudo contactar al servidor. Intente más tarde.").ShowDialog();
                return;
            }

            if (resultadoCodigo == null)
            {
                new Avisos("Ocurrió un problema al procesar la solicitud.").ShowDialog();
                return;
            }

            textoErrorUsuario.Visibility = resultadoCodigo.UsuarioYaRegistrado ? Visibility.Visible : Visibility.Collapsed;
            textoErrorCorreo.Visibility = resultadoCodigo.CorreoYaRegistrado ? Visibility.Visible : Visibility.Collapsed;

            if (!resultadoCodigo.CodigoEnviado)
            {
                string mensajeError = string.IsNullOrWhiteSpace(resultadoCodigo.Mensaje)
                    ? "No se pudo enviar el código de verificación. Verifique la información e intente de nuevo."
                    : resultadoCodigo.Mensaje;

                new Avisos(mensajeError).ShowDialog();
                return;
            }

            var ventanaVerificacion = new VerificarCodigo(resultadoCodigo.TokenVerificacion, solicitud.Correo);
            ventanaVerificacion.ShowDialog();

            if (ventanaVerificacion.RegistroCompletado)
            {
                Close();
            }
        }

        private void Boton_Cancelar(object sender, RoutedEventArgs e) => Close();

        private bool ValidarCamposObligatorios(string usuario, string correo, string nombre, string apellido, string contrasena)
        {
            if (string.IsNullOrWhiteSpace(usuario))
            {
                new Avisos("Ingrese un nombre de usuario.").ShowDialog();
                bloqueTextoUsuario.Focus();
                return false;
            }

            if (string.IsNullOrWhiteSpace(nombre))
            {
                new Avisos("Ingrese el nombre del jugador.").ShowDialog();
                bloqueTextoNombre.Focus();
                return false;
            }

            if (string.IsNullOrWhiteSpace(apellido))
            {
                new Avisos("Ingrese el apellido del jugador.").ShowDialog();
                bloqueTextoApellido.Focus();
                return false;
            }

            if (string.IsNullOrWhiteSpace(correo))
            {
                new Avisos("Ingrese un correo electrónico.").ShowDialog();
                bloqueTextoCorreo.Focus();
                return false;
            }

            if (string.IsNullOrWhiteSpace(contrasena))
            {
                new Avisos("Ingrese una contraseña.").ShowDialog();
                bloqueContrasenaContrasena.Focus();
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
            catch
            {
                return null;
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
    }
}
