using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using PictionaryMusicalCliente.Modelo;
using PictionaryMusicalCliente.Modelo.Catalogos;
using PictionaryMusicalCliente.Sesiones;
using PictionaryMusicalCliente.Servicios;
using PictionaryMusicalCliente.Utilidades;

namespace PictionaryMusicalCliente
{
    /// <summary>
    /// Lógica de interacción para Perfil.xaml
    /// </summary>
    public partial class Perfil : Window
    {
        private const int LongitudMaximaNombre = 50;

        private readonly IReadOnlyList<ObjetoAvatar> _catalogoAvatares;
        private UsuarioAutenticado _usuarioSesion;
        private ObjetoAvatar _avatarActual;
        private ObjetoAvatar _avatarSeleccionado;

        public Perfil()
        {
            InitializeComponent();
            _catalogoAvatares = CatalogoAvataresLocales.ObtenerAvatares();
        }

        private async void Perfil_Loaded(object sender, RoutedEventArgs e)
        {
            await CargarPerfilAsync();
        }

        private async Task CargarPerfilAsync()
        {
            _usuarioSesion = SesionUsuarioActual.Instancia.Usuario;

            if (_usuarioSesion == null)
            {
                new Avisos("No hay una sesión activa. Inicie sesión nuevamente.").ShowDialog();
                Close();
                return;
            }

            try
            {
                using (var proxy = new ServidorProxy())
                {
                    UsuarioAutenticado perfilActualizado = await proxy.ObtenerPerfilAsync(_usuarioSesion.IdUsuario);

                    if (perfilActualizado != null)
                    {
                        SesionUsuarioActual.Instancia.EstablecerUsuario(perfilActualizado);
                        _usuarioSesion = perfilActualizado;
                    }
                }
            }
            catch (EndpointNotFoundException)
            {
                new Avisos("No se pudo contactar al servidor. Se mostrarán los datos actuales de la sesión.").ShowDialog();
            }
            catch (TimeoutException)
            {
                new Avisos("El servidor tardó demasiado en responder. Se mostrarán los datos actuales de la sesión.").ShowDialog();
            }
            catch (CommunicationException)
            {
                new Avisos("Ocurrió un problema de comunicación con el servidor. Se mostrarán los datos actuales de la sesión.").ShowDialog();
            }
            catch (InvalidOperationException)
            {
                new Avisos("No fue posible obtener la información actualizada del perfil.").ShowDialog();
            }

            _avatarActual = ObtenerAvatarPorId(_usuarioSesion.AvatarId);
            _avatarSeleccionado = _avatarActual;

            ActualizarCampos();
        }

        private void ActualizarCampos()
        {
            bloqueTextoUsuario.Text = _usuarioSesion?.NombreUsuario ?? string.Empty;
            bloqueTextoNombre.Text = _usuarioSesion?.Nombre ?? string.Empty;
            bloqueTextoApellido.Text = _usuarioSesion?.Apellido ?? string.Empty;
            bloqueTextoCorreo.Text = _usuarioSesion?.Correo ?? string.Empty;

            ActualizarVistaAvatares();
        }

        private void ActualizarVistaAvatares()
        {
            imagenAvatarActual.ImageSource = AvatarImagenHelper.CrearImagen(_avatarActual);
            textoNombreAvatarActual.Text = _avatarActual?.Nombre ?? string.Empty;

            ObjetoAvatar avatarNuevo = _avatarSeleccionado ?? _avatarActual;
            imagenAvatarNuevo.ImageSource = AvatarImagenHelper.CrearImagen(avatarNuevo);
            textoNombreAvatarNuevo.Text = avatarNuevo?.Nombre ?? string.Empty;
        }

        private ObjetoAvatar ObtenerAvatarPorId(int avatarId)
        {
            return _catalogoAvatares?.FirstOrDefault(a => a.Id == avatarId);
        }

        private bool ValidarTexto(string valor, string descripcionCampo)
        {
            if (string.IsNullOrWhiteSpace(valor))
            {
                new Avisos($"Ingrese el {descripcionCampo}.").ShowDialog();
                return false;
            }

            if (valor.Length > LongitudMaximaNombre)
            {
                new Avisos($"El {descripcionCampo} no debe superar {LongitudMaximaNombre} caracteres.").ShowDialog();
                return false;
            }

            return true;
        }

        private async void BotonCambiarContraseña(object sender, RoutedEventArgs e)
        {
            _usuarioSesion ??= SesionUsuarioActual.Instancia.Usuario;

            if (_usuarioSesion == null)
            {
                new Avisos("No hay una sesión activa para cambiar la contraseña.").ShowDialog();
                Close();
                return;
            }

            string identificador = !string.IsNullOrWhiteSpace(_usuarioSesion.Correo)
                ? _usuarioSesion.Correo
                : _usuarioSesion.NombreUsuario;

            if (string.IsNullOrWhiteSpace(identificador))
            {
                new Avisos("No se pudo determinar el usuario actual para cambiar la contraseña.").ShowDialog();
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
                    var solicitud = new SolicitudRecuperarCuenta
                    {
                        Identificador = identificador
                    };

                    ResultadoSolicitudRecuperacion resultado = await proxy.SolicitarCodigoRecuperacionAsync(solicitud);

                    if (resultado == null)
                    {
                        new Avisos("No se pudo iniciar el proceso de cambio de contraseña. Intente nuevamente.").ShowDialog();
                        return;
                    }

                    if (!resultado.CuentaEncontrada)
                    {
                        string mensajeCuenta = string.IsNullOrWhiteSpace(resultado.Mensaje)
                            ? "No se encontró la cuenta asociada a la sesión actual."
                            : resultado.Mensaje;
                        new Avisos(mensajeCuenta).ShowDialog();
                        return;
                    }

                    if (!resultado.CodigoEnviado || string.IsNullOrWhiteSpace(resultado.TokenRecuperacion))
                    {
                        string mensajeCodigo = string.IsNullOrWhiteSpace(resultado.Mensaje)
                            ? "No fue posible enviar el código de verificación. Intente más tarde."
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

                        if (resultadoReenvio != null)
                        {
                            if (!string.IsNullOrWhiteSpace(resultadoReenvio.TokenVerificacion))
                            {
                                tokenRecuperacion = resultadoReenvio.TokenVerificacion;
                            }
                            else if (!string.IsNullOrWhiteSpace(resultadoReenvio.TokenRecuperacion))
                            {
                                tokenRecuperacion = resultadoReenvio.TokenRecuperacion;
                            }
                        }

                        return resultadoReenvio;
                    }

                    var ventanaVerificacion = new VerificarCodigo(
                        tokenRecuperacion,
                        correoDestino,
                        ConfirmarCodigoAsync,
                        ReenviarCodigoAsync,
                        "Ingresa el código que enviamos para confirmar el cambio de contraseña.");

                    ventanaVerificacion.ShowDialog();

                    if (!ventanaVerificacion.OperacionCompletada)
                    {
                        return;
                    }

                    var ventanaCambio = new CambioContrasena(tokenRecuperacion, identificador);
                    ventanaCambio.ShowDialog();
                }
            }
            catch (EndpointNotFoundException)
            {
                new Avisos("No se pudo contactar al servidor. Intente más tarde.").ShowDialog();
            }
            catch (TimeoutException)
            {
                new Avisos("El servidor tardó demasiado en responder. Intente más tarde.").ShowDialog();
            }
            catch (CommunicationException)
            {
                new Avisos("Ocurrió un problema de comunicación con el servidor. Intente nuevamente.").ShowDialog();
            }
            catch (InvalidOperationException)
            {
                new Avisos("No fue posible procesar la solicitud de cambio de contraseña.").ShowDialog();
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

        private async void BotonGuardarCambios(object sender, RoutedEventArgs e)
        {
            if (_usuarioSesion == null)
            {
                new Avisos("No hay una sesión activa para actualizar.").ShowDialog();
                Close();
                return;
            }

            string nombre = bloqueTextoNombre.Text?.Trim();
            string apellido = bloqueTextoApellido.Text?.Trim();

            if (!ValidarTexto(nombre, "nombre"))
            {
                bloqueTextoNombre.Focus();
                return;
            }

            if (!ValidarTexto(apellido, "apellido"))
            {
                bloqueTextoApellido.Focus();
                return;
            }

            ObjetoAvatar avatar = _avatarSeleccionado ?? _avatarActual;

            if (avatar == null || avatar.Id <= 0)
            {
                new Avisos("Selecciona un avatar válido antes de guardar los cambios.").ShowDialog();
                return;
            }

            var solicitud = new SolicitudActualizarPerfil
            {
                UsuarioId = _usuarioSesion.IdUsuario,
                Nombre = nombre,
                Apellido = apellido,
                AvatarId = avatar.Id
            };

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
                    ResultadoOperacion resultado = await proxy.ActualizarPerfilAsync(solicitud);

                    if (resultado == null)
                    {
                        new Avisos("No se pudo actualizar el perfil. Intente nuevamente.").ShowDialog();
                        return;
                    }

                    if (resultado.OperacionExitosa)
                    {
                        SesionUsuarioActual.Instancia.ActualizarDatosPersonales(nombre, apellido, solicitud.AvatarId);
                        _usuarioSesion = SesionUsuarioActual.Instancia.Usuario;
                        _avatarActual = ObtenerAvatarPorId(solicitud.AvatarId) ?? avatar;
                        _avatarSeleccionado = _avatarActual;
                        ActualizarCampos();

                        string mensaje = string.IsNullOrWhiteSpace(resultado.Mensaje)
                            ? "Los datos del perfil se actualizaron correctamente."
                            : resultado.Mensaje;
                        new Avisos(mensaje).ShowDialog();
                        return;
                    }

                    string mensajeError = string.IsNullOrWhiteSpace(resultado.Mensaje)
                        ? "No fue posible actualizar el perfil."
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
                new Avisos("El servidor tardó demasiado en responder. Intente más tarde.").ShowDialog();
            }
            catch (CommunicationException)
            {
                new Avisos("Ocurrió un problema de comunicación con el servidor. Intente nuevamente.").ShowDialog();
            }
            catch (InvalidOperationException)
            {
                new Avisos("No fue posible procesar la solicitud de actualización.").ShowDialog();
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

        private void BotonRegresar(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void EtiquetaSeleccionarAvatar(object sender, MouseButtonEventArgs e)
        {
            var ventanaSeleccion = new SeleccionarAvatar();
            bool? resultado = ventanaSeleccion.ShowDialog();

            if (resultado == true && ventanaSeleccion.AvatarSeleccionado != null)
            {
                _avatarSeleccionado = ventanaSeleccion.AvatarSeleccionado;
                imagenAvatarNuevo.ImageSource = ventanaSeleccion.AvatarSeleccionadoImagen
                    ?? AvatarImagenHelper.CrearImagen(_avatarSeleccionado);
                textoNombreAvatarNuevo.Text = _avatarSeleccionado?.Nombre ?? string.Empty;
            }
        }
    }
}
