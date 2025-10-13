using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.ServiceModel;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Controls.Primitives;
using System.Text;
using PictionaryMusicalCliente.Modelo;
using PictionaryMusicalCliente.Modelo.Catalogos;
using PictionaryMusicalCliente.Sesiones;
using PictionaryMusicalCliente.Servicios;
using PictionaryMusicalCliente.Utilidades;
using LangResources = PictionaryMusicalCliente.Properties.Langs;

namespace PictionaryMusicalCliente
{
    /// <summary>
    /// Lógica de interacción para Perfil.xaml
    /// </summary>
    public partial class Perfil : Window
    {
        private const int LongitudMaximaNombre = 50;
        private const int LongitudMaximaRedSocial = 50;

        private IReadOnlyList<ObjetoAvatar> _catalogoAvatares;
        private UsuarioAutenticado _usuarioSesion;
        private ObjetoAvatar _avatarActual;
        private ObjetoAvatar _avatarSeleccionado;

        public ObservableCollection<RedSocialPerfil> RedesSociales { get; } = new ObservableCollection<RedSocialPerfil>();

        public Perfil()
        {
            InitializeComponent();
            DataContext = this;
            _catalogoAvatares = CatalogoAvataresLocales.ObtenerAvatares();
            InicializarRedesSociales();
        }

        private async void Perfil_Loaded(object sender, RoutedEventArgs e)
        {
            await CargarPerfilAsync();
        }

        private async Task CargarPerfilAsync()
        {
            await CargarCatalogoAvataresAsync();

            _usuarioSesion = SesionUsuarioActual.Instancia.Usuario;

            if (_usuarioSesion == null)
            {
                AvisoHelper.Mostrar(LangResources.Lang.errorTextoCuentaNoEncontradaSesion);
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
            catch (FaultException<ServidorProxy.ErrorDetalleServicio> ex)
            {
                string mensaje = ErrorServicioHelper.ObtenerMensaje(
                    ex,
                    LangResources.Lang.errorTextoServidorObtenerPerfil);
                AvisoHelper.Mostrar(mensaje);
            }
            catch (EndpointNotFoundException)
            {
                AvisoHelper.Mostrar(LangResources.Lang.avisoTextoComunicacionServidorSesion);
            }
            catch (TimeoutException)
            {
                AvisoHelper.Mostrar(LangResources.Lang.avisoTextoServidorTiempoSesion);
            }
            catch (CommunicationException)
            {
                AvisoHelper.Mostrar(LangResources.Lang.avisoTextoComunicacionServidorSesion);
            }
            catch (InvalidOperationException)
            {
                AvisoHelper.Mostrar(LangResources.Lang.errorTextoPerfilActualizarInformacion);
            }

            _avatarActual = ObtenerAvatarPorId(_usuarioSesion.AvatarId);
            if (_avatarActual == null)
            {
                _avatarActual = _catalogoAvatares?.FirstOrDefault();
            }
            _avatarSeleccionado = _avatarActual;

            ActualizarCampos();
        }

        private async Task CargarCatalogoAvataresAsync()
        {
            IReadOnlyList<ObjetoAvatar> avataresLocales = CatalogoAvataresLocales.ObtenerAvatares();

            try
            {
                using (var proxy = new ServidorProxy())
                {
                    List<ObjetoAvatar> avataresServidor = await proxy.ObtenerAvataresAsync();

                    if (avataresServidor != null && avataresServidor.Count > 0)
                    {
                        IReadOnlyList<ObjetoAvatar> avataresSincronizados = SincronizarCatalogoLocal(avataresServidor, avataresLocales);

                        if (avataresSincronizados != null && avataresSincronizados.Count > 0)
                        {
                            _catalogoAvatares = avataresSincronizados;
                            return;
                        }
                    }
                }
            }
            catch (FaultException<ServidorProxy.ErrorDetalleServicio> ex)
            {
                string mensaje = ErrorServicioHelper.ObtenerMensaje(
                    ex,
                    LangResources.Lang.errorTextoServidorNoDisponible);
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

            _catalogoAvatares = avataresLocales;
        }

        private static IReadOnlyList<ObjetoAvatar> SincronizarCatalogoLocal(
            IEnumerable<ObjetoAvatar> avataresServidor,
            IReadOnlyList<ObjetoAvatar> avataresLocales)
        {
            if (avataresServidor == null)
            {
                return avataresLocales;
            }

            if (avataresLocales == null || avataresLocales.Count == 0)
            {
                return avataresServidor.ToList();
            }

            var localesValidos = avataresLocales
                .Where(avatar => avatar != null && !string.IsNullOrWhiteSpace(avatar.RutaRelativa))
                .ToList();

            if (localesValidos.Count == 0)
            {
                return avataresLocales;
            }

            Dictionary<string, ObjetoAvatar> localesPorNombreNormalizado = localesValidos
                .Where(avatar => !string.IsNullOrWhiteSpace(avatar.Nombre))
                .GroupBy(avatar => NormalizarNombre(avatar.Nombre))
                .ToDictionary(grupo => grupo.Key, grupo => grupo.First());

            Dictionary<string, ObjetoAvatar> localesPorRuta = localesValidos
                .GroupBy(avatar => NormalizarRuta(avatar.RutaRelativa))
                .ToDictionary(grupo => grupo.Key, grupo => grupo.First());

            var asignados = new HashSet<ObjetoAvatar>();
            var resultado = new List<ObjetoAvatar>();

            foreach (ObjetoAvatar avatarServidor in avataresServidor)
            {
                if (avatarServidor == null)
                {
                    continue;
                }

                ObjetoAvatar avatarLocal = BuscarCoincidenciaLocal(avatarServidor, localesValidos, localesPorNombreNormalizado, localesPorRuta, asignados);

                if (avatarLocal == null)
                {
                    continue;
                }

                asignados.Add(avatarLocal);

                resultado.Add(new ObjetoAvatar
                {
                    Id = avatarServidor.Id,
                    Nombre = string.IsNullOrWhiteSpace(avatarServidor.Nombre) ? avatarLocal.Nombre : avatarServidor.Nombre,
                    RutaRelativa = avatarLocal.RutaRelativa,
                    ImagenUriAbsoluta = null
                });
            }

            return resultado.Count > 0 ? resultado : avataresLocales;
        }

        private static ObjetoAvatar BuscarCoincidenciaLocal(
            ObjetoAvatar avatarServidor,
            List<ObjetoAvatar> localesValidos,
            Dictionary<string, ObjetoAvatar> localesPorNombreNormalizado,
            Dictionary<string, ObjetoAvatar> localesPorRuta,
            HashSet<ObjetoAvatar> asignados)
        {
            ObjetoAvatar avatarLocal = null;

            if (avatarServidor.Id > 0)
            {
                avatarLocal = localesValidos
                    .FirstOrDefault(avatar => avatar.Id == avatarServidor.Id && !asignados.Contains(avatar));
            }

            if (avatarLocal == null && !string.IsNullOrWhiteSpace(avatarServidor.Nombre))
            {
                string nombreNormalizado = NormalizarNombre(avatarServidor.Nombre);

                if (localesPorNombreNormalizado.TryGetValue(nombreNormalizado, out ObjetoAvatar coincidenciaNombre)
                    && !asignados.Contains(coincidenciaNombre))
                {
                    avatarLocal = coincidenciaNombre;
                }
            }

            if (avatarLocal == null && !string.IsNullOrWhiteSpace(avatarServidor.RutaRelativa))
            {
                string rutaNormalizada = NormalizarRuta(avatarServidor.RutaRelativa);

                if (localesPorRuta.TryGetValue(rutaNormalizada, out ObjetoAvatar coincidenciaRuta)
                    && !asignados.Contains(coincidenciaRuta))
                {
                    avatarLocal = coincidenciaRuta;
                }
            }

            if (avatarLocal == null)
            {
                avatarLocal = localesValidos.FirstOrDefault(avatar => !asignados.Contains(avatar));
            }

            return avatarLocal;
        }

        private static string NormalizarNombre(string nombre)
        {
            if (string.IsNullOrWhiteSpace(nombre))
            {
                return string.Empty;
            }

            string descompuesto = nombre.Normalize(NormalizationForm.FormD);
            var builder = new StringBuilder(descompuesto.Length);

            foreach (char caracter in descompuesto)
            {
                UnicodeCategory categoria = CharUnicodeInfo.GetUnicodeCategory(caracter);

                if (categoria == UnicodeCategory.NonSpacingMark)
                {
                    continue;
                }

                if (char.IsLetterOrDigit(caracter))
                {
                    builder.Append(char.ToLowerInvariant(caracter));
                }
            }

            return builder.ToString();
        }

        private static string NormalizarRuta(string ruta)
        {
            return string.IsNullOrWhiteSpace(ruta)
                ? string.Empty
                : ruta.Trim().Replace("\\", "/").ToLowerInvariant();
        }

        private void InicializarRedesSociales()
        {
            RedesSociales.Clear();

            IReadOnlyList<RedSocialPerfil> redes = CatalogoImagenesPerfilLocales.ObtenerRedesSociales();

            if (redes == null || redes.Count == 0)
            {
                return;
            }

            foreach (RedSocialPerfil red in redes)
            {
                RedesSociales.Add(red);
            }
        }

        private void ActualizarCampos()
        {
            bloqueTextoUsuario.Text = _usuarioSesion?.NombreUsuario ?? string.Empty;
            bloqueTextoNombre.Text = _usuarioSesion?.Nombre ?? string.Empty;
            bloqueTextoApellido.Text = _usuarioSesion?.Apellido ?? string.Empty;
            bloqueTextoCorreo.Text = _usuarioSesion?.Correo ?? string.Empty;

            ActualizarVistaAvatares();
            ActualizarRedesSocialesDesdeSesion();
        }

        private void ActualizarVistaAvatares()
        {
            ObjetoAvatar avatarNuevo = _avatarSeleccionado ?? _avatarActual ?? _catalogoAvatares?.FirstOrDefault();

            if (avatarNuevo == null)
            {
                imagenAvatarNuevo.ImageSource = null;
                textoNombreAvatarNuevo.Text = string.Empty;
                return;
            }

            imagenAvatarNuevo.ImageSource = AvatarImagenHelper.CrearImagen(avatarNuevo);
            textoNombreAvatarNuevo.Text = avatarNuevo.Nombre ?? string.Empty;
        }


        private ObjetoAvatar ObtenerAvatarPorId(int avatarId)
        {
            return _catalogoAvatares?.FirstOrDefault(a => a.Id == avatarId);
        }

        private static bool ValidarTexto(
            string valor,
            string descripcionCampo)
        {
            if (string.IsNullOrWhiteSpace(valor))
            {
                return false;
            }

            if (valor.Length > LongitudMaximaNombre)
            {
                AvisoHelper.Mostrar(string.Format(LangResources.Lang.errorTextoCampoLongitudMaxima, descripcionCampo, LongitudMaximaNombre));
                return false;
            }

            return true;
        }

        private async void BotonCambiarContraseña(object sender, RoutedEventArgs e)
        {
            _usuarioSesion = _usuarioSesion ?? SesionUsuarioActual.Instancia.Usuario;

            if (_usuarioSesion == null)
            {
                AvisoHelper.Mostrar(LangResources.Lang.errorTextoCuentaNoEncontradaSesion);
                Close();
                return;
            }

            string identificador = !string.IsNullOrWhiteSpace(_usuarioSesion.Correo)
                ? _usuarioSesion.Correo
                : _usuarioSesion.NombreUsuario;

            if (string.IsNullOrWhiteSpace(identificador))
            {
                AvisoHelper.Mostrar(LangResources.Lang.errorTextoDeterminarUsuarioCambioContrasena);
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
                        AvisoHelper.Mostrar(LangResources.Lang.errorTextoIniciarCambioContrasena);
                        return;
                    }

                    if (!resultado.CuentaEncontrada)
                    {
                        string mensajeCuenta = MensajeServidorHelper.Localizar(
                            resultado.Mensaje,
                            LangResources.Lang.errorTextoCuentaNoEncontradaSesion);
                        AvisoHelper.Mostrar(mensajeCuenta);
                        return;
                    }

                    if (!resultado.CodigoEnviado || string.IsNullOrWhiteSpace(resultado.TokenRecuperacion))
                    {
                        string mensajeCodigo = MensajeServidorHelper.Localizar(
                            resultado.Mensaje,
                            LangResources.Lang.errorTextoEnvioCodigoVerificacionMasTarde);
                        AvisoHelper.Mostrar(mensajeCodigo);
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

                    Mouse.OverrideCursor = null;

                    var ventanaVerificacion = new VerificarCodigo(
                        tokenRecuperacion,
                        correoDestino,
                        ConfirmarCodigoAsync,
                        ReenviarCodigoAsync,
                        LangResources.Lang.avisoTextoCodigoDescripcionCambio);

                    ventanaVerificacion.ShowDialog();

                    if (!ventanaVerificacion.OperacionCompletada)
                    {
                        return;
                    }

                    Mouse.OverrideCursor = null;

                    var ventanaCambio = new CambioContrasena(tokenRecuperacion, identificador);
                    ventanaCambio.ShowDialog();
                }
            }
            catch (FaultException<ServidorProxy.ErrorDetalleServicio> ex)
            {
                string mensaje = ErrorServicioHelper.ObtenerMensaje(
                    ex,
                    LangResources.Lang.errorTextoServidorSolicitudCambioContrasena);
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

        private async void BotonGuardarCambios(object sender, RoutedEventArgs e)
        {
            if (_usuarioSesion == null)
            {
                AvisoHelper.Mostrar(LangResources.Lang.errorTextoCuentaNoEncontradaSesion);
                Close();
                return;
            }

            RestablecerEstadoCampo(bloqueTextoNombre);
            RestablecerEstadoCampo(bloqueTextoApellido);

            string nombre = bloqueTextoNombre.Text?.Trim();
            string apellido = bloqueTextoApellido.Text?.Trim();

            if (!ValidarCamposObligatoriosPerfil(nombre, apellido))
            {
                return;
            }

            if (!ValidarTexto(
                    nombre,
                    LangResources.Lang.globalTextoNombre.ToLowerInvariant()))
            {
                MarcarCampoInvalido(bloqueTextoNombre);
                bloqueTextoNombre.Focus();
                return;
            }

            if (!ValidarTexto(
                    apellido,
                    LangResources.Lang.globalTextoApellido.ToLowerInvariant()))
            {
                MarcarCampoInvalido(bloqueTextoApellido);
                bloqueTextoApellido.Focus();
                return;
            }

            ObjetoAvatar avatar = _avatarSeleccionado ?? _avatarActual;

            if (avatar == null || avatar.Id <= 0)
            {
                AvisoHelper.Mostrar(LangResources.Lang.errorTextoSeleccionAvatarValido);
                return;
            }

            if (!TryObtenerRedesSocialesParaSolicitud(out string instagram, out string facebook, out string x, out string discord, out string mensajeError))
            {
                if (!string.IsNullOrWhiteSpace(mensajeError))
                {
                    AvisoHelper.Mostrar(mensajeError);
                }
                return;
            }

            var solicitud = new SolicitudActualizarPerfil
            {
                UsuarioId = _usuarioSesion.IdUsuario,
                Nombre = nombre,
                Apellido = apellido,
                AvatarId = avatar.Id,
                Instagram = instagram,
                Facebook = facebook,
                X = x,
                Discord = discord
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
                        AvisoHelper.Mostrar(LangResources.Lang.errorTextoActualizarPerfil);
                        return;
                    }

                    if (resultado.OperacionExitosa)
                    {
                        SesionUsuarioActual.Instancia.ActualizarDatosPersonales(nombre, apellido, solicitud.AvatarId, instagram, facebook, x, discord);
                        _usuarioSesion = SesionUsuarioActual.Instancia.Usuario;
                        _avatarActual = ObtenerAvatarPorId(solicitud.AvatarId) ?? avatar;
                        _avatarSeleccionado = _avatarActual;
                        ActualizarCampos();

                        string mensaje = MensajeServidorHelper.Localizar(
                            resultado.Mensaje,
                            LangResources.Lang.avisoTextoPerfilActualizado);
                        AvisoHelper.Mostrar(mensaje);
                        return;
                    }

                    string mensajeFinal = MensajeServidorHelper.Localizar(
                        resultado.Mensaje,
                        LangResources.Lang.errorTextoActualizarPerfil);
                    AvisoHelper.Mostrar(mensajeFinal);
                }
            }
            catch (FaultException<ServidorProxy.ErrorDetalleServicio> ex)
            {
                string mensaje = ErrorServicioHelper.ObtenerMensaje(
                    ex,
                    LangResources.Lang.errorTextoServidorActualizarPerfil);
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

        private void BotonRegresar(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private bool ValidarCamposObligatoriosPerfil(string nombre, string apellido)
        {
            bool hayError = false;
            Control primerCampo = null;

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

            if (hayError)
            {
                AvisoHelper.Mostrar(LangResources.Lang.errorTextoCamposInvalidosGenerico);
                primerCampo?.Focus();
                return false;
            }

            return true;
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

        private void EtiquetaSeleccionarAvatar(object sender, MouseButtonEventArgs e)
        {
            IReadOnlyCollection<ObjetoAvatar> catalogo = _catalogoAvatares ?? CatalogoAvataresLocales.ObtenerAvatares();
            var ventanaSeleccion = new SeleccionarAvatar(catalogo);
            bool? resultado = ventanaSeleccion.ShowDialog();

            if (resultado == true && ventanaSeleccion.AvatarSeleccionado != null)
            {
                _avatarSeleccionado = ventanaSeleccion.AvatarSeleccionado;
                imagenAvatarNuevo.ImageSource = ventanaSeleccion.AvatarSeleccionadoImagen
                    ?? AvatarImagenHelper.CrearImagen(_avatarSeleccionado);
                textoNombreAvatarNuevo.Text = _avatarSeleccionado?.Nombre ?? string.Empty;
            }
        }

        private void PopupRedSocial_Opened(object sender, EventArgs e)
        {
            if (sender is Popup popup)
            {
                TextBox campo = ObtenerTextBoxDesdePopup(popup);

                if (campo != null)
                {
                    campo.Focus();
                    campo.CaretIndex = campo.Text?.Length ?? 0;
                }
            }
        }

        private void RedSocialTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (sender is TextBox texto && texto.Tag is ToggleButton toggle)
            {
                if (e.Key != Key.Enter && e.Key != Key.Escape)
                {
                    return;
                }
                toggle.IsChecked = false;
                e.Handled = true;
            }
        }

        private static TextBox ObtenerTextBoxDesdePopup(Popup popup)
        {
            if (popup?.Child is Border borde && borde.Child is TextBox texto)
            {
                return texto;
            }

            return null;
        }

        private void ActualizarRedesSocialesDesdeSesion()
        {
            if (RedesSociales == null || RedesSociales.Count == 0)
            {
                return;
            }

            foreach (RedSocialPerfil red in RedesSociales)
            {
                if (red == null)
                {
                    continue;
                }

                string valor = ObtenerValorRedSocialDesdeSesion(red.Clave);
                red.Identificador = string.IsNullOrWhiteSpace(valor) ? "@" : valor;
            }
        }

        private string ObtenerValorRedSocialDesdeSesion(string clave)
        {
            if (_usuarioSesion == null || string.IsNullOrWhiteSpace(clave))
            {
                return null;
            }

            switch (clave.ToLowerInvariant())
            {
                case "instagram":
                    return _usuarioSesion.Instagram;
                case "facebook":
                    return _usuarioSesion.Facebook;
                case "x":
                    return _usuarioSesion.X;
                case "discord":
                    return _usuarioSesion.Discord;
                default:
                    return null;
            }
        }

        private bool TryObtenerRedesSocialesParaSolicitud(
            out string instagram,
            out string facebook,
            out string x,
            out string discord,
            out string mensajeError)
        {
            instagram = null;
            facebook = null;
            x = null;
            discord = null;
            mensajeError = null;

            if (RedesSociales == null)
            {
                return true;
            }

            foreach (RedSocialPerfil red in RedesSociales)
            {
                if (red == null)
                {
                    continue;
                }

                string valor = PrepararValorRedSocial(red.Identificador, red.Nombre, out string error);

                if (!string.IsNullOrWhiteSpace(error))
                {
                    mensajeError = error;
                    return false;
                }

                switch (red.Clave?.ToLowerInvariant())
                {
                    case "instagram":
                        instagram = valor;
                        break;
                    case "facebook":
                        facebook = valor;
                        break;
                    case "x":
                        x = valor;
                        break;
                    case "discord":
                        discord = valor;
                        break;
                }
            }

            return true;
        }

        private static string PrepararValorRedSocial(string identificador, string nombreRed, out string mensajeError)
        {
            mensajeError = null;

            if (string.IsNullOrWhiteSpace(identificador))
            {
                return null;
            }

            string texto = identificador.Trim();

            if (string.Equals(texto, "@", StringComparison.Ordinal))
            {
                return null;
            }

            if (texto.StartsWith("@", StringComparison.Ordinal))
            {
                string contenido = texto.Substring(1);

                if (string.IsNullOrWhiteSpace(contenido))
                {
                    return null;
                }
            }

            if (texto.Length > LongitudMaximaRedSocial)
            {
                string descripcionRed = string.IsNullOrWhiteSpace(nombreRed)
                    ? LangResources.Lang.avisoTextoNombreRedSocialGenerica
                    : nombreRed;
                mensajeError = string.Format(
                    LangResources.Lang.errorTextoIdentificadorRedSocialLongitud,
                    descripcionRed,
                    LongitudMaximaRedSocial);
                return null;
            }

            return texto;
        }
    }
}
