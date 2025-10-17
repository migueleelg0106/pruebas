using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Media;
using PictionaryMusicalCliente.Comandos;
using PictionaryMusicalCliente.Modelo;
using PictionaryMusicalCliente.Modelo.Catalogos;
using PictionaryMusicalCliente.Modelo.Cuentas;
using PictionaryMusicalCliente.Properties.Langs;
using PictionaryMusicalCliente.Servicios;
using PictionaryMusicalCliente.Servicios.Abstracciones;
using PictionaryMusicalCliente.Servicios.Wcf.Helpers;
using PictionaryMusicalCliente.Sesiones;
using PictionaryMusicalCliente.Utilidades;
using CodigoVerificacionSrv = PictionaryMusicalCliente.PictionaryServidorServicioCodigoVerificacion;
using ReenvioSrv = PictionaryMusicalCliente.PictionaryServidorServicioReenvioCodigoVerificacion;

namespace PictionaryMusicalCliente.VistaModelo.Cuentas
{
    public class PerfilVistaModelo : BaseVistaModelo
    {
        [Flags]
        public enum CampoEntrada
        {
            Ninguno = 0,
            Nombre = 1 << 0,
            Apellido = 1 << 1
        }

        public sealed class ValidacionCamposEventArgs : EventArgs
        {
            public ValidacionCamposEventArgs(CampoEntrada camposInvalidos)
            {
                CamposInvalidos = camposInvalidos;
            }

            public CampoEntrada CamposInvalidos { get; }
        }

        private readonly IDialogService _dialogService;
        private readonly IPerfilService _perfilService;
        private readonly ISeleccionarAvatarService _seleccionarAvatarService;
        private readonly IRecuperacionCuentaDialogService _recuperacionCuentaDialogService;
        private readonly SesionUsuarioActual _sesionUsuarioActual;
        private readonly ObservableCollection<RedSocialPerfil> _redesSociales;
        private readonly ComandoAsincrono _guardarCambiosCommand;
        private readonly ComandoAsincrono _cambiarContrasenaCommand;

        private IReadOnlyList<ObjetoAvatar> _catalogoAvatares;
        private ObjetoAvatar _avatarActual;
        private ObjetoAvatar _avatarSeleccionado;

        private string _usuario;
        private string _nombre;
        private string _apellido;
        private string _correo;
        private ImageSource _avatarSeleccionadoImagen;
        private string _avatarSeleccionadoNombre;
        private bool _estaProcesando;
        private bool _estaCambiandoContrasena;

        public PerfilVistaModelo(
            IDialogService dialogService,
            IPerfilService perfilService,
            ISeleccionarAvatarService seleccionarAvatarService,
            IRecuperacionCuentaDialogService recuperacionCuentaDialogService,
            SesionUsuarioActual sesionUsuarioActual = null)
        {
            _dialogService = dialogService ?? throw new ArgumentNullException(nameof(dialogService));
            _perfilService = perfilService ?? throw new ArgumentNullException(nameof(perfilService));
            _seleccionarAvatarService = seleccionarAvatarService ?? throw new ArgumentNullException(nameof(seleccionarAvatarService));
            _recuperacionCuentaDialogService = recuperacionCuentaDialogService ?? throw new ArgumentNullException(nameof(recuperacionCuentaDialogService));
            _sesionUsuarioActual = sesionUsuarioActual ?? SesionUsuarioActual.Instancia;

            _redesSociales = new ObservableCollection<RedSocialPerfil>();
            InicializarRedesSociales();

            _guardarCambiosCommand = new ComandoAsincrono(_ => GuardarCambiosAsync(), _ => !EstaProcesando);
            _cambiarContrasenaCommand = new ComandoAsincrono(_ => CambiarContrasenaAsync(), _ => !EstaCambiandoContrasena);

            SeleccionarAvatarCommand = new ComandoAsincrono(_ => SeleccionarAvatarAsync());
            CerrarCommand = new Comando(() => SolicitarCerrar?.Invoke(this, EventArgs.Empty));
        }

        public event EventHandler SolicitarCerrar;

        public event EventHandler<ValidacionCamposEventArgs> ValidacionCamposProcesada;

        public ICommand SeleccionarAvatarCommand { get; }

        public IComandoAsincrono GuardarCambiosCommand => _guardarCambiosCommand;

        public IComandoAsincrono CambiarContrasenaCommand => _cambiarContrasenaCommand;

        public ICommand CerrarCommand { get; }

        public IEnumerable<RedSocialPerfil> RedesSociales => _redesSociales;

        public string Usuario
        {
            get => _usuario;
            private set => EstablecerPropiedad(ref _usuario, value);
        }

        public string Nombre
        {
            get => _nombre;
            set => EstablecerPropiedad(ref _nombre, value);
        }

        public string Apellido
        {
            get => _apellido;
            set => EstablecerPropiedad(ref _apellido, value);
        }

        public string Correo
        {
            get => _correo;
            private set => EstablecerPropiedad(ref _correo, value);
        }

        public ImageSource AvatarSeleccionadoImagen
        {
            get => _avatarSeleccionadoImagen;
            private set => EstablecerPropiedad(ref _avatarSeleccionadoImagen, value);
        }

        public string AvatarSeleccionadoNombre
        {
            get => _avatarSeleccionadoNombre;
            private set => EstablecerPropiedad(ref _avatarSeleccionadoNombre, value);
        }

        public bool EstaProcesando
        {
            get => _estaProcesando;
            private set
            {
                if (EstablecerPropiedad(ref _estaProcesando, value))
                {
                    _guardarCambiosCommand.NotificarPuedeEjecutar();
                }
            }
        }

        public bool EstaCambiandoContrasena
        {
            get => _estaCambiandoContrasena;
            private set
            {
                if (EstablecerPropiedad(ref _estaCambiandoContrasena, value))
                {
                    _cambiarContrasenaCommand.NotificarPuedeEjecutar();
                }
            }
        }

        public async Task InicializarAsync()
        {
            if (EstaProcesando)
            {
                return;
            }

            EstaProcesando = true;

            try
            {
                await CargarCatalogoAvataresAsync().ConfigureAwait(true);

                UsuarioAutenticado usuarioSesion = _sesionUsuarioActual.Usuario;

                if (usuarioSesion == null)
                {
                    _dialogService.Aviso(Lang.errorTextoCuentaNoEncontradaSesion);
                    SolicitarCerrar?.Invoke(this, EventArgs.Empty);
                    return;
                }

                try
                {
                    UsuarioAutenticado perfilActualizado = await _perfilService
                        .ObtenerPerfilAsync(usuarioSesion.IdUsuario)
                        .ConfigureAwait(true);

                    if (perfilActualizado != null)
                    {
                        _sesionUsuarioActual.EstablecerUsuario(perfilActualizado);
                        usuarioSesion = perfilActualizado;
                    }
                }
                catch (ServicioException ex)
                {
                    string mensajeError = string.IsNullOrWhiteSpace(ex.Message)
                        ? Lang.errorTextoPerfilActualizarInformacion
                        : ex.Message;
                    _dialogService.Aviso(mensajeError);
                }

                EstablecerDatosUsuario(usuarioSesion);
            }
            finally
            {
                EstaProcesando = false;
            }
        }

        private async Task SeleccionarAvatarAsync()
        {
            ObjetoAvatar avatar = await _seleccionarAvatarService.SeleccionarAsync().ConfigureAwait(true);

            if (avatar != null)
            {
                _avatarSeleccionado = avatar;
                ActualizarAvatarSeleccionado(_avatarSeleccionado);
            }
        }

        private async Task GuardarCambiosAsync()
        {
            if (EstaProcesando)
            {
                return;
            }

            NotificarValidacionCampos(CampoEntrada.Ninguno);

            ValidacionEntradaHelper.ResultadoValidacion resultadoNombre = ValidacionEntradaHelper.ValidarNombre(Nombre);

            if (!resultadoNombre.EsValido)
            {
                NotificarValidacionCampos(CampoEntrada.Nombre);
                _dialogService.Aviso(resultadoNombre.MensajeError);
                return;
            }

            ValidacionEntradaHelper.ResultadoValidacion resultadoApellido = ValidacionEntradaHelper.ValidarApellido(Apellido);

            if (!resultadoApellido.EsValido)
            {
                NotificarValidacionCampos(CampoEntrada.Apellido);
                _dialogService.Aviso(resultadoApellido.MensajeError);
                return;
            }

            if (!TryObtenerRedesSocialesParaSolicitud(
                out string instagram,
                out string facebook,
                out string x,
                out string discord,
                out string mensajeError))
            {
                if (!string.IsNullOrWhiteSpace(mensajeError))
                {
                    _dialogService.Aviso(mensajeError);
                }

                return;
            }

            ObjetoAvatar avatar = _avatarSeleccionado ?? _avatarActual;

            if (avatar == null || avatar.Id <= 0)
            {
                _dialogService.Aviso(Lang.errorTextoSeleccionAvatarValido);
                return;
            }

            UsuarioAutenticado usuarioSesion = _sesionUsuarioActual.Usuario;

            if (usuarioSesion == null)
            {
                _dialogService.Aviso(Lang.errorTextoCuentaNoEncontradaSesion);
                SolicitarCerrar?.Invoke(this, EventArgs.Empty);
                return;
            }

            var solicitud = new ActualizarPerfilSolicitud
            {
                UsuarioId = usuarioSesion.IdUsuario,
                Nombre = resultadoNombre.ValorNormalizado,
                Apellido = resultadoApellido.ValorNormalizado,
                AvatarId = avatar.Id,
                Instagram = instagram,
                Facebook = facebook,
                X = x,
                Discord = discord
            };

            EstaProcesando = true;

            try
            {
                ResultadoOperacion resultado = await _perfilService
                    .ActualizarPerfilAsync(solicitud)
                    .ConfigureAwait(true);

                if (resultado == null)
                {
                    _dialogService.Aviso(Lang.errorTextoActualizarPerfil);
                    return;
                }

                string mensajePredeterminado = resultado.OperacionExitosa
                    ? Lang.avisoTextoPerfilActualizado
                    : Lang.errorTextoActualizarPerfil;

                string mensaje = MensajeServidorHelper.Localizar(resultado.Mensaje, mensajePredeterminado);
                _dialogService.Aviso(mensaje);

                if (!resultado.OperacionExitosa)
                {
                    return;
                }

                _sesionUsuarioActual.ActualizarDatosPersonales(
                    solicitud.Nombre,
                    solicitud.Apellido,
                    solicitud.AvatarId,
                    solicitud.Instagram,
                    solicitud.Facebook,
                    solicitud.X,
                    solicitud.Discord);

                UsuarioAutenticado usuarioActualizado = _sesionUsuarioActual.Usuario;
                _avatarActual = ObtenerAvatarPorId(solicitud.AvatarId) ?? avatar;
                _avatarSeleccionado = _avatarActual;
                EstablecerDatosUsuario(usuarioActualizado);
                NotificarValidacionCampos(CampoEntrada.Ninguno);
            }
            catch (ServicioException ex)
            {
                mensajeError = string.IsNullOrWhiteSpace(ex.Message)
                    ? Lang.errorTextoActualizarPerfil
                    : ex.Message;
                _dialogService.Aviso(mensajeError);
            }
            finally
            {
                EstaProcesando = false;
            }
        }

        private async Task CambiarContrasenaAsync()
        {
            if (EstaCambiandoContrasena)
            {
                return;
            }

            UsuarioAutenticado usuarioSesion = _sesionUsuarioActual.Usuario;

            if (usuarioSesion == null)
            {
                _dialogService.Aviso(Lang.errorTextoCuentaNoEncontradaSesion);
                SolicitarCerrar?.Invoke(this, EventArgs.Empty);
                return;
            }

            string identificador = !string.IsNullOrWhiteSpace(usuarioSesion.Correo)
                ? usuarioSesion.Correo
                : usuarioSesion.NombreUsuario;

            if (string.IsNullOrWhiteSpace(identificador))
            {
                _dialogService.Aviso(Lang.errorTextoDeterminarUsuarioCambioContrasena);
                return;
            }

            EstaCambiandoContrasena = true;

            try
            {
                CodigoVerificacionSrv.ResultadoSolicitudRecuperacionDTO resultado = await CodigoVerificacionServicioHelper
                    .SolicitarCodigoRecuperacionAsync(identificador)
                    .ConfigureAwait(true);

                if (resultado == null)
                {
                    _dialogService.Aviso(Lang.errorTextoIniciarCambioContrasena);
                    return;
                }

                if (!resultado.CuentaEncontrada)
                {
                    string mensajeCuenta = MensajeServidorHelper.Localizar(
                        resultado.Mensaje,
                        Lang.errorTextoCuentaNoEncontradaSesion);
                    _dialogService.Aviso(mensajeCuenta);
                    return;
                }

                if (!resultado.CodigoEnviado || string.IsNullOrWhiteSpace(resultado.TokenCodigo))
                {
                    string mensajeCodigo = MensajeServidorHelper.Localizar(
                        resultado.Mensaje,
                        Lang.errorTextoEnvioCodigoVerificacionMasTarde);
                    _dialogService.Aviso(mensajeCodigo);
                    return;
                }

                string tokenCodigo = resultado.TokenCodigo;
                string correoDestino = resultado.CorreoDestino;

                var funcionesVerificacion = new FuncionesVerificarCodigo(
                    async codigo =>
                    {
                        CodigoVerificacionSrv.ResultadoOperacionDTO confirmacion = await CodigoVerificacionServicioHelper
                            .ConfirmarCodigoRecuperacionAsync(tokenCodigo, codigo)
                            .ConfigureAwait(true);

                        if (confirmacion == null)
                        {
                            return null;
                        }

                        return new ConfirmacionCodigoResultado(confirmacion.OperacionExitosa, confirmacion.Mensaje);
                    },
                    async () =>
                    {
                        ReenvioSrv.ResultadoSolicitudCodigoDTO reenvio = await CodigoVerificacionServicioHelper
                            .ReenviarCodigoRecuperacionAsync(tokenCodigo)
                            .ConfigureAwait(true);

                        if (reenvio != null && !string.IsNullOrWhiteSpace(reenvio.TokenCodigo))
                        {
                            tokenCodigo = reenvio.TokenCodigo;
                        }

                        if (reenvio == null)
                        {
                            return null;
                        }

                        return new ReenvioCodigoResultado(reenvio.CodigoEnviado, reenvio.Mensaje, reenvio.TokenCodigo);
                    });

                bool verificacionCompletada = await _recuperacionCuentaDialogService
                    .MostrarDialogoVerificacionAsync(
                        tokenCodigo,
                        correoDestino,
                        funcionesVerificacion,
                        Lang.avisoTextoCodigoDescripcionCambio)
                    .ConfigureAwait(true);

                if (!verificacionCompletada)
                {
                    return;
                }

                ResultadoCambioContrasena resultadoCambio = await _recuperacionCuentaDialogService
                    .MostrarDialogoCambioContrasenaAsync(tokenCodigo, identificador)
                    .ConfigureAwait(true);

                if (resultadoCambio == null)
                {
                    return;
                }

                if (resultadoCambio.ContrasenaActualizada || resultadoCambio.DialogResult == true)
                {
                    _dialogService.Aviso(Lang.avisoTextoContrasenaActualizada);
                }
            }
            catch (FaultException ex)
            {
                string mensaje = ErrorServicioHelper.ObtenerMensaje(
                    ex,
                    Lang.errorTextoServidorSolicitudCambioContrasena);
                _dialogService.Aviso(mensaje);
            }
            catch (EndpointNotFoundException)
            {
                _dialogService.Aviso(Lang.errorTextoServidorNoDisponible);
            }
            catch (TimeoutException)
            {
                _dialogService.Aviso(Lang.errorTextoServidorTiempoAgotado);
            }
            catch (CommunicationException)
            {
                _dialogService.Aviso(Lang.errorTextoServidorNoDisponible);
            }
            catch (InvalidOperationException)
            {
                _dialogService.Aviso(Lang.errorTextoErrorProcesarSolicitud);
            }
            finally
            {
                EstaCambiandoContrasena = false;
            }
        }

        private void EstablecerDatosUsuario(UsuarioAutenticado usuario)
        {
            Usuario = usuario?.NombreUsuario ?? string.Empty;
            Nombre = usuario?.Nombre ?? string.Empty;
            Apellido = usuario?.Apellido ?? string.Empty;
            Correo = usuario?.Correo ?? string.Empty;

            _avatarActual = ObtenerAvatarPorId(usuario?.AvatarId ?? 0);
            _avatarSeleccionado = _avatarActual ?? _catalogoAvatares?.FirstOrDefault();
            ActualizarAvatarSeleccionado(_avatarSeleccionado);
            ActualizarRedesSocialesDesdeSesion(usuario);
        }

        private void ActualizarAvatarSeleccionado(ObjetoAvatar avatar)
        {
            AvatarSeleccionadoImagen = AvatarImagenHelper.CrearImagen(avatar);
            AvatarSeleccionadoNombre = avatar?.Nombre ?? string.Empty;
        }

        private void InicializarRedesSociales()
        {
            _redesSociales.Clear();

            IReadOnlyList<RedSocialPerfil> redes = CatalogoImagenesPerfilLocales.ObtenerRedesSociales();

            if (redes == null)
            {
                return;
            }

            foreach (RedSocialPerfil red in redes)
            {
                if (red != null)
                {
                    _redesSociales.Add(red);
                }
            }
        }

        private async Task CargarCatalogoAvataresAsync()
        {
            IReadOnlyList<ObjetoAvatar> avataresLocales = CatalogoAvataresLocales.ObtenerAvatares();

            try
            {
                IReadOnlyList<ObjetoAvatar> avataresRemotos = await _perfilService
                    .ObtenerAvataresDisponiblesAsync()
                    .ConfigureAwait(true);

                if (avataresRemotos != null && avataresRemotos.Count > 0)
                {
                    IReadOnlyList<ObjetoAvatar> sincronizados = SincronizarCatalogoLocal(avataresRemotos, avataresLocales);

                    if (sincronizados != null && sincronizados.Count > 0)
                    {
                        _catalogoAvatares = sincronizados;
                        return;
                    }
                }
            }
            catch (ServicioException ex)
            {
                string mensaje = string.IsNullOrWhiteSpace(ex.Message)
                    ? Lang.errorTextoServidorNoDisponible
                    : ex.Message;
                _dialogService.Aviso(mensaje);
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
                return avataresServidor.Where(avatar => avatar != null).ToList();
            }

            var localesValidos = avataresLocales
                ?.Where(avatar => avatar != null && !string.IsNullOrWhiteSpace(avatar.RutaRelativa))
                .ToList() ?? new List<ObjetoAvatar>();

            if (localesValidos.Count == 0)
            {
                return avataresLocales;
            }

            Dictionary<string, ObjetoAvatar> localesPorNombreNormalizado = localesValidos
                .Where(avatar => !string.IsNullOrWhiteSpace(avatar.Nombre))
                .GroupBy(avatar => NormalizarNombre(avatar.Nombre))
                .ToDictionary(grupo => grupo.Key, grupo => grupo.First());

            Dictionary<string, ObjetoAvatar> localesPorRuta = localesValidos
                .GroupBy(avatar => AvatarRutaHelper.NormalizarRutaParaClaveDiccionario(avatar.RutaRelativa))
                .ToDictionary(grupo => grupo.Key, grupo => grupo.First());

            var asignados = new HashSet<ObjetoAvatar>();
            var resultado = new List<ObjetoAvatar>();

            foreach (ObjetoAvatar avatarServidor in avataresServidor)
            {
                if (avatarServidor == null)
                {
                    continue;
                }

                ObjetoAvatar avatarLocal = BuscarCoincidenciaLocal(
                    avatarServidor,
                    localesValidos,
                    localesPorNombreNormalizado,
                    localesPorRuta,
                    asignados);

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
                    ImagenUriAbsoluta = avatarServidor.ImagenUriAbsoluta
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
                string rutaNormalizada = AvatarRutaHelper.NormalizarRutaParaClaveDiccionario(avatarServidor.RutaRelativa);

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

        private void ActualizarRedesSocialesDesdeSesion(UsuarioAutenticado usuario)
        {
            if (_redesSociales == null || _redesSociales.Count == 0)
            {
                return;
            }

            foreach (RedSocialPerfil red in _redesSociales)
            {
                if (red == null)
                {
                    continue;
                }

                string valor = ObtenerValorRedSocialDesdeSesion(usuario, red.Clave);
                red.Identificador = string.IsNullOrWhiteSpace(valor) ? "@" : valor;
            }
        }

        private static string ObtenerValorRedSocialDesdeSesion(UsuarioAutenticado usuario, string clave)
        {
            if (usuario == null || string.IsNullOrWhiteSpace(clave))
            {
                return null;
            }

            switch (clave.ToLowerInvariant())
            {
                case "instagram":
                    return usuario.Instagram;
                case "facebook":
                    return usuario.Facebook;
                case "x":
                    return usuario.X;
                case "discord":
                    return usuario.Discord;
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

            if (_redesSociales == null)
            {
                return true;
            }

            foreach (RedSocialPerfil red in _redesSociales)
            {
                if (red == null)
                {
                    continue;
                }

                ValidacionEntradaHelper.ResultadoValidacion resultado = ValidacionEntradaHelper
                    .ValidarRedSocial(red.Identificador, red.Nombre);

                if (!resultado.EsValido)
                {
                    mensajeError = resultado.MensajeError;
                    return false;
                }

                switch (red.Clave?.ToLowerInvariant())
                {
                    case "instagram":
                        instagram = resultado.ValorNormalizado;
                        break;
                    case "facebook":
                        facebook = resultado.ValorNormalizado;
                        break;
                    case "x":
                        x = resultado.ValorNormalizado;
                        break;
                    case "discord":
                        discord = resultado.ValorNormalizado;
                        break;
                }
            }

            return true;
        }

        private ObjetoAvatar ObtenerAvatarPorId(int avatarId)
        {
            return _catalogoAvatares?.FirstOrDefault(a => a.Id == avatarId);
        }

        private void NotificarValidacionCampos(CampoEntrada camposInvalidos)
        {
            ValidacionCamposProcesada?.Invoke(this, new ValidacionCamposEventArgs(camposInvalidos));
        }
    }
}
