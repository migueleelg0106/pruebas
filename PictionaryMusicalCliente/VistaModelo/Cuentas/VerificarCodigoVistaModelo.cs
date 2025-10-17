using System;
using System.ServiceModel;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Threading;
using PictionaryMusicalCliente.Comandos;
using PictionaryMusicalCliente.Modelo.Cuentas;
using PictionaryMusicalCliente.Properties.Langs;
using PictionaryMusicalCliente.Servicios;
using PictionaryMusicalCliente.Servicios.Abstracciones;
using PictionaryMusicalCliente.Servicios.Wcf.Helpers;

namespace PictionaryMusicalCliente.VistaModelo.Cuentas
{
    public class VerificarCodigoVistaModelo : BaseVistaModelo, IDisposable
    {
        private readonly IDialogService _dialogService;
        private readonly IVerificarCodigoService _verificarCodigoService;
        private readonly Func<string, Task<ConfirmacionCodigoResultado>> _confirmarCodigoFunc;
        private readonly Func<Task<ReenvioCodigoResultado>> _reenviarCodigoFunc;
        private readonly DispatcherTimer _temporizador;
        private readonly string _correoDestino;
        private readonly string _textoOriginalReenviar;
        private readonly ComandoAsincrono _verificarCodigoCommand;
        private readonly ComandoAsincrono _reenviarCodigoCommand;

        private string _tokenCodigo;
        private string _codigoVerificacion;
        private string _descripcion;
        private string _textoBotonReenviar;
        private bool _puedeReenviar;
        private bool _estaVerificando;
        private bool _operacionCompletada;
        private DateTime _siguienteReenvioPermitido;

        public VerificarCodigoVistaModelo(
            IDialogService dialogService,
            IVerificarCodigoService verificarCodigoService,
            string tokenCodigo,
            string correoDestino,
            Func<string, Task<ConfirmacionCodigoResultado>> confirmarCodigoAsync = null,
            Func<Task<ReenvioCodigoResultado>> reenviarCodigoAsync = null,
            string descripcionPersonalizada = null)
        {
            if (string.IsNullOrWhiteSpace(tokenCodigo))
            {
                throw new ArgumentException(Lang.errorTextoTokenCodigoObligatorio, nameof(tokenCodigo));
            }

            _dialogService = dialogService ?? throw new ArgumentNullException(nameof(dialogService));
            _verificarCodigoService = verificarCodigoService;
            _tokenCodigo = tokenCodigo;
            _correoDestino = correoDestino ?? string.Empty;
            _textoOriginalReenviar = Lang.cambiarContrasenaTextoReenviarCodigo;
            _temporizador = new DispatcherTimer { Interval = TimeSpan.FromSeconds(1) };
            _temporizador.Tick += TemporizadorTick;

            _confirmarCodigoFunc = confirmarCodigoAsync ?? (verificarCodigoService != null
                ? new Func<string, Task<ConfirmacionCodigoResultado>>(codigo => _verificarCodigoService.ConfirmarCodigoRegistroAsync(_tokenCodigo, codigo))
                : null);

            _reenviarCodigoFunc = reenviarCodigoAsync ?? (verificarCodigoService != null
                ? new Func<Task<ReenvioCodigoResultado>>(() => _verificarCodigoService.ReenviarCodigoRegistroAsync(_tokenCodigo))
                : null);

            if (_confirmarCodigoFunc == null)
            {
                throw new ArgumentNullException(nameof(confirmarCodigoAsync), Lang.errorTextoValidacionCodigoNoDisponible);
            }

            _verificarCodigoCommand = new ComandoAsincrono(_ => VerificarCodigoAsync(), _ => !EstaVerificando);
            _reenviarCodigoCommand = new ComandoAsincrono(_ => ManejarReenvioCodigoAsync(), _ => PuedeReenviar);

            CancelarCommand = new Comando(() => SolicitarCerrar?.Invoke(this, EventArgs.Empty));

            Descripcion = string.IsNullOrWhiteSpace(descripcionPersonalizada)
                ? (string.IsNullOrWhiteSpace(_correoDestino)
                    ? Lang.avisoTextoCodigoDescripcionGenerica
                    : string.Format(Lang.avisoTextoCodigoDescripcionCorreo, _correoDestino))
                : descripcionPersonalizada;

            _siguienteReenvioPermitido = DateTime.UtcNow.AddMinutes(1);
            ActualizarEstadoReenvio();
        }

        public event EventHandler SolicitarCerrar;

        public ICommand CancelarCommand { get; }

        public IComandoAsincrono VerificarCodigoCommand => _verificarCodigoCommand;

        public IComandoAsincrono ReenviarCodigoCommand => _reenviarCodigoCommand;

        public string CodigoVerificacion
        {
            get => _codigoVerificacion;
            set => EstablecerPropiedad(ref _codigoVerificacion, value);
        }

        public string Descripcion
        {
            get => _descripcion;
            private set => EstablecerPropiedad(ref _descripcion, value);
        }

        public string TextoBotonReenviar
        {
            get => _textoBotonReenviar;
            private set => EstablecerPropiedad(ref _textoBotonReenviar, value);
        }

        public bool PuedeReenviar
        {
            get => _puedeReenviar;
            private set
            {
                if (EstablecerPropiedad(ref _puedeReenviar, value))
                {
                    _reenviarCodigoCommand.NotificarPuedeEjecutar();
                }
            }
        }

        public bool EstaVerificando
        {
            get => _estaVerificando;
            private set
            {
                if (EstablecerPropiedad(ref _estaVerificando, value))
                {
                    _verificarCodigoCommand.NotificarPuedeEjecutar();
                }
            }
        }

        public bool OperacionCompletada
        {
            get => _operacionCompletada;
            private set => EstablecerPropiedad(ref _operacionCompletada, value);
        }

        public void Dispose()
        {
            _temporizador.Stop();
            _temporizador.Tick -= TemporizadorTick;
        }

        private async Task VerificarCodigoAsync()
        {
            if (EstaVerificando)
            {
                return;
            }

            string codigoIngresado = CodigoVerificacion?.Trim();

            if (string.IsNullOrWhiteSpace(codigoIngresado))
            {
                _dialogService.Aviso(Lang.errorTextoCodigoVerificacionRequerido);
                return;
            }

            EstaVerificando = true;

            try
            {
                ConfirmacionCodigoResultado resultado = await _confirmarCodigoFunc(codigoIngresado).ConfigureAwait(true);

                if (resultado == null)
                {
                    _dialogService.Aviso(Lang.errorTextoVerificarCodigo);
                    return;
                }

                if (resultado.OperacionExitosa)
                {
                    string mensaje = MensajeServidorHelper.Localizar(
                        resultado.Mensaje,
                        Lang.avisoTextoCodigoVerificadoCorrecto);
                    _dialogService.Aviso(mensaje);
                    OperacionCompletada = true;
                    SolicitarCerrar?.Invoke(this, EventArgs.Empty);
                    return;
                }

                string mensajeError = MensajeServidorHelper.Localizar(
                    resultado.Mensaje,
                    Lang.errorTextoCodigoIncorrectoExpirado);
                _dialogService.Aviso(mensajeError);
            }
            catch (ServicioException ex)
            {
                string mensaje = string.IsNullOrWhiteSpace(ex.Message)
                    ? Lang.errorTextoVerificarCodigo
                    : ex.Message;
                _dialogService.Aviso(mensaje);
            }
            catch (FaultException ex)
            {
                string mensaje = ErrorServicioHelper.ObtenerMensaje(ex, Lang.errorTextoServidorValidarCodigo);
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
                _dialogService.Aviso(Lang.errorTextoSolicitudVerificacionInvalida);
            }
            finally
            {
                EstaVerificando = false;
            }
        }

        private async Task ManejarReenvioCodigoAsync()
        {
            if (!PuedeReenviar || _reenviarCodigoFunc == null)
            {
                return;
            }

            PuedeReenviar = false;

            try
            {
                ReenvioCodigoResultado resultado = await _reenviarCodigoFunc().ConfigureAwait(true);

                if (resultado == null)
                {
                    _dialogService.Aviso(Lang.errorTextoSolicitarNuevoCodigo);
                    return;
                }

                if (resultado.CodigoEnviado)
                {
                    if (!string.IsNullOrWhiteSpace(resultado.TokenCodigo))
                    {
                        _tokenCodigo = resultado.TokenCodigo;
                    }

                    _siguienteReenvioPermitido = DateTime.UtcNow.AddMinutes(1);
                    string mensaje = MensajeServidorHelper.Localizar(
                        resultado.Mensaje,
                        Lang.avisoTextoCodigoReenviado);
                    _dialogService.Aviso(mensaje);
                    return;
                }

                string mensajeError = MensajeServidorHelper.Localizar(
                    resultado.Mensaje,
                    Lang.avisoTextoReenvioCodigoNoDisponible);
                _dialogService.Aviso(mensajeError);
            }
            catch (ServicioException ex)
            {
                string mensaje = string.IsNullOrWhiteSpace(ex.Message)
                    ? Lang.errorTextoErrorProcesarSolicitud
                    : ex.Message;
                _dialogService.Aviso(mensaje);
            }
            catch (FaultException ex)
            {
                string mensaje = ErrorServicioHelper.ObtenerMensaje(ex, Lang.errorTextoServidorReenviarCodigo);
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
                ActualizarEstadoReenvio();
            }
        }

        private void TemporizadorTick(object sender, EventArgs e)
        {
            ActualizarEstadoReenvio();
        }

        private void ActualizarEstadoReenvio()
        {
            if (_reenviarCodigoFunc == null)
            {
                PuedeReenviar = false;
                TextoBotonReenviar = _textoOriginalReenviar;
                _temporizador.Stop();
                return;
            }

            DateTime ahora = DateTime.UtcNow;

            if (ahora >= _siguienteReenvioPermitido)
            {
                PuedeReenviar = true;
                TextoBotonReenviar = _textoOriginalReenviar;
                _temporizador.Stop();
                return;
            }

            TimeSpan restante = _siguienteReenvioPermitido - ahora;
            int segundos = Math.Max(1, (int)Math.Ceiling(restante.TotalSeconds));
            PuedeReenviar = false;
            TextoBotonReenviar = $"{_textoOriginalReenviar} ({segundos}s)";

            if (!_temporizador.IsEnabled)
            {
                _temporizador.Start();
            }
        }
    }
}
