using System;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Media;
using PictionaryMusicalCliente.Comandos;
using PictionaryMusicalCliente.Modelo;
using PictionaryMusicalCliente.Modelo.Cuentas;
using PictionaryMusicalCliente.Properties.Langs;
using PictionaryMusicalCliente.Servicios;
using PictionaryMusicalCliente.Servicios.Abstracciones;
using PictionaryMusicalCliente.Servicios.Wcf.Helpers;
using PictionaryMusicalCliente.Utilidades;

namespace PictionaryMusicalCliente.VistaModelo.Cuentas
{
    public class CrearCuenta : BaseVistaModelo
    {
        private readonly IDialogService _dialogService;
        private readonly ICodigoVerificacionService _codigoVerificacionService;
        private readonly IAvatarService _avatarService;
        private readonly ISeleccionarAvatarService _seleccionarAvatarService;
        private readonly IVerificarCodigoDialogService _verificarCodigoDialogService;

        private string _usuario;
        private string _nombre;
        private string _apellido;
        private string _correo;
        private string _contrasena;
        private ObjetoAvatar _avatarSeleccionado;
        private ImageSource _avatarSeleccionadoImagen;
        private bool _mostrarErrorUsuario;
        private bool _mostrarErrorCorreo;
        private bool _estaProcesando;

        public CrearCuenta(
            IDialogService dialogService,
            ICodigoVerificacionService codigoVerificacionService,
            IAvatarService avatarService,
            ISeleccionarAvatarService seleccionarAvatarService,
            IVerificarCodigoDialogService verificarCodigoDialogService)
        {
            _dialogService = dialogService ?? throw new ArgumentNullException(nameof(dialogService));
            _codigoVerificacionService = codigoVerificacionService ?? throw new ArgumentNullException(nameof(codigoVerificacionService));
            _avatarService = avatarService ?? throw new ArgumentNullException(nameof(avatarService));
            _seleccionarAvatarService = seleccionarAvatarService ?? throw new ArgumentNullException(nameof(seleccionarAvatarService));
            _verificarCodigoDialogService = verificarCodigoDialogService ?? throw new ArgumentNullException(nameof(verificarCodigoDialogService));

            SeleccionarAvatarCommand = new ComandoAsincrono(_ => SeleccionarAvatarAsync());
            CrearCuentaCommand = new ComandoAsincrono(_ => CrearCuentaAsync(), _ => !EstaProcesando);
            CancelarCommand = new Comando(() => SolicitarCerrar?.Invoke(this, EventArgs.Empty));
        }

        public event EventHandler SolicitarCerrar;

        public ICommand SeleccionarAvatarCommand { get; }

        public IComandoAsincrono CrearCuentaCommand { get; }

        public ICommand CancelarCommand { get; }

        public string Usuario
        {
            get => _usuario;
            set => EstablecerPropiedad(ref _usuario, value);
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
            set => EstablecerPropiedad(ref _correo, value);
        }

        public string Contrasena
        {
            get => _contrasena;
            set => EstablecerPropiedad(ref _contrasena, value);
        }

        public ObjetoAvatar AvatarSeleccionado
        {
            get => _avatarSeleccionado;
            private set
            {
                if (EstablecerPropiedad(ref _avatarSeleccionado, value))
                {
                    AvatarSeleccionadoImagen = AvatarImagenHelper.CrearImagen(value);
                }
            }
        }

        public ImageSource AvatarSeleccionadoImagen
        {
            get => _avatarSeleccionadoImagen;
            private set => EstablecerPropiedad(ref _avatarSeleccionadoImagen, value);
        }

        public bool MostrarErrorUsuario
        {
            get => _mostrarErrorUsuario;
            private set => EstablecerPropiedad(ref _mostrarErrorUsuario, value);
        }

        public bool MostrarErrorCorreo
        {
            get => _mostrarErrorCorreo;
            private set => EstablecerPropiedad(ref _mostrarErrorCorreo, value);
        }

        public bool EstaProcesando
        {
            get => _estaProcesando;
            private set
            {
                if (EstablecerPropiedad(ref _estaProcesando, value))
                {
                    CrearCuentaCommand.NotificarPuedeEjecutar();
                }
            }
        }

        private async Task SeleccionarAvatarAsync()
        {
            ObjetoAvatar avatar = await _seleccionarAvatarService.SeleccionarAsync().ConfigureAwait(true);

            if (avatar != null)
            {
                AvatarSeleccionado = avatar;
            }
        }

        private async Task CrearCuentaAsync()
        {
            if (EstaProcesando)
            {
                return;
            }

            MostrarErrorUsuario = false;
            MostrarErrorCorreo = false;

            ValidacionEntradaHelper.ResultadoValidacion resultadoUsuario = ValidacionEntradaHelper.ValidarUsuario(Usuario);

            if (!resultadoUsuario.EsValido)
            {
                _dialogService.Aviso(resultadoUsuario.MensajeError);
                return;
            }

            ValidacionEntradaHelper.ResultadoValidacion resultadoNombre = ValidacionEntradaHelper.ValidarNombre(Nombre);

            if (!resultadoNombre.EsValido)
            {
                _dialogService.Aviso(resultadoNombre.MensajeError);
                return;
            }

            ValidacionEntradaHelper.ResultadoValidacion resultadoApellido = ValidacionEntradaHelper.ValidarApellido(Apellido);

            if (!resultadoApellido.EsValido)
            {
                _dialogService.Aviso(resultadoApellido.MensajeError);
                return;
            }

            ValidacionEntradaHelper.ResultadoValidacion resultadoCorreo = ValidacionEntradaHelper.ValidarCorreo(Correo);

            if (!resultadoCorreo.EsValido)
            {
                _dialogService.Aviso(resultadoCorreo.MensajeError);
                return;
            }

            ValidacionEntradaHelper.ResultadoValidacion resultadoContrasena = ValidacionEntradaHelper.ValidarContrasena(Contrasena);

            if (!resultadoContrasena.EsValido)
            {
                _dialogService.Aviso(resultadoContrasena.MensajeError);
                return;
            }

            if (AvatarSeleccionado == null)
            {
                _dialogService.Aviso(Lang.globalTextoSeleccionarAvatar);
                return;
            }

            EstaProcesando = true;

            try
            {
                int? avatarId = await _avatarService.ObtenerIdPorRutaAsync(AvatarSeleccionado.RutaRelativa).ConfigureAwait(true);

                if (!avatarId.HasValue)
                {
                    _dialogService.Aviso(Lang.errorTextoIdentificarAvatar);
                    return;
                }

                var solicitud = new SolicitudRegistroCuenta
                {
                    Usuario = resultadoUsuario.ValorNormalizado,
                    Correo = resultadoCorreo.ValorNormalizado,
                    Nombre = resultadoNombre.ValorNormalizado,
                    Apellido = resultadoApellido.ValorNormalizado,
                    Contrasena = resultadoContrasena.ValorNormalizado,
                    AvatarId = avatarId.Value
                };

                ResultadoSolicitudCodigo resultado = await _codigoVerificacionService.SolicitarCodigoRegistroAsync(solicitud).ConfigureAwait(true);

                if (resultado == null)
                {
                    _dialogService.Aviso(Lang.errorTextoErrorProcesarSolicitud);
                    return;
                }

                MostrarErrorUsuario = resultado.UsuarioYaRegistrado;
                MostrarErrorCorreo = resultado.CorreoYaRegistrado;

                if (!resultado.CodigoEnviado)
                {
                    if (!MostrarErrorUsuario && !MostrarErrorCorreo)
                    {
                        string mensajeError = MensajeServidorHelper.Localizar(
                            resultado.Mensaje,
                            Lang.errorTextoEnvioCodigoVerificacionDatos);
                        _dialogService.Aviso(mensajeError);
                    }

                    return;
                }

                bool registroCompletado = await _verificarCodigoDialogService.MostrarDialogoAsync(
                    resultado.TokenCodigo,
                    solicitud.Correo).ConfigureAwait(true);

                if (registroCompletado)
                {
                    SolicitarCerrar?.Invoke(this, EventArgs.Empty);
                }
            }
            catch (ServicioException ex)
            {
                _dialogService.Aviso(string.IsNullOrWhiteSpace(ex.Message)
                    ? Lang.errorTextoErrorProcesarSolicitud
                    : ex.Message);
            }
            finally
            {
                EstaProcesando = false;
            }
        }
    }
}
