using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Threading.Tasks;
using System.Windows.Input;
using PictionaryMusicalCliente.Comandos;
using PictionaryMusicalCliente.Modelo;
using PictionaryMusicalCliente.Modelo.Cuentas;
using PictionaryMusicalCliente.Properties.Langs;
using PictionaryMusicalCliente.Servicios.Abstracciones;
using PictionaryMusicalCliente.Servicios;
using PictionaryMusicalCliente.Servicios.Wcf.Helpers;
using PictionaryMusicalCliente.Sesiones;
using PictionaryMusicalCliente.Utilidades;
using CodigoVerificacionSrv = PictionaryMusicalCliente.PictionaryServidorServicioCodigoVerificacion;
using InicioSesionSrv = PictionaryMusicalCliente.PictionaryServidorServicioInicioSesion;
using ReenvioSrv = PictionaryMusicalCliente.PictionaryServidorServicioReenvioCodigoVerificacion;

namespace PictionaryMusicalCliente.VistaModelo.Cuentas
{
    public class InicioSesionVistaModelo : BaseVistaModelo
    {
        [Flags]
        public enum CampoEntrada
        {
            Ninguno = 0,
            Identificador = 1 << 0,
            Contrasena = 1 << 1
        }

        public sealed class IdiomaOpcion
        {
            public IdiomaOpcion(string codigo, string descripcion)
            {
                Codigo = codigo;
                Descripcion = descripcion;
            }

            public string Codigo { get; }

            public string Descripcion { get; }
        }

        public sealed class ValidacionCamposEventArgs : EventArgs
        {
            public ValidacionCamposEventArgs(CampoEntrada camposInvalidos)
            {
                CamposInvalidos = camposInvalidos;
            }

            public CampoEntrada CamposInvalidos { get; }
        }

        public sealed class InicioSesionCompletadoEventArgs : EventArgs
        {
            public InicioSesionCompletadoEventArgs(UsuarioAutenticado usuario)
            {
                Usuario = usuario;
            }

            public UsuarioAutenticado Usuario { get; }
        }

        private readonly IDialogService _dialogService;
        private readonly IInicioSesionService _inicioSesionService;
        private readonly IRecuperacionCuentaDialogService _recuperacionCuentaDialogService;
        private readonly IReadOnlyList<IdiomaOpcion> _idiomasDisponibles;
        private readonly ComandoAsincrono _iniciarSesionCommand;
        private readonly Comando _iniciarSesionInvitadoCommand;
        private readonly Comando _abrirCrearCuentaCommand;
        private readonly ComandoAsincrono _recuperarCuentaCommand;

        private string _identificador;
        private string _contrasena;
        private IdiomaOpcion _idiomaSeleccionado;
        private bool _estaProcesando;
        private bool _estaRecuperando;

        public InicioSesionVistaModelo(
            IDialogService dialogService,
            IInicioSesionService inicioSesionService,
            IRecuperacionCuentaDialogService recuperacionCuentaDialogService)
        {
            _dialogService = dialogService ?? throw new ArgumentNullException(nameof(dialogService));
            _inicioSesionService = inicioSesionService ?? throw new ArgumentNullException(nameof(inicioSesionService));
            _recuperacionCuentaDialogService = recuperacionCuentaDialogService ?? throw new ArgumentNullException(nameof(recuperacionCuentaDialogService));

            _idiomasDisponibles = new List<IdiomaOpcion>
            {
                new IdiomaOpcion(Lang.TagEspañol, Lang.idiomaTextoEspañol),
                new IdiomaOpcion(Lang.TagIngles, Lang.idiomaTextoIngles)
            };

            _iniciarSesionCommand = new ComandoAsincrono(_ => IniciarSesionAsync(), _ => !EstaProcesando);
            _iniciarSesionInvitadoCommand = new Comando(() => SolicitarUnirseInvitado?.Invoke(this, EventArgs.Empty));
            _abrirCrearCuentaCommand = new Comando(() => SolicitarCrearCuenta?.Invoke(this, EventArgs.Empty));
            _recuperarCuentaCommand = new ComandoAsincrono(_ => RecuperarCuentaAsync(), _ => !EstaRecuperando);

            string idiomaActual = Properties.Settings.Default.idiomaCodigo;
            IdiomaSeleccionado = _idiomasDisponibles.FirstOrDefault(idioma => string.Equals(idioma.Codigo, idiomaActual, StringComparison.OrdinalIgnoreCase))
                ?? _idiomasDisponibles.First();
        }

        public event EventHandler<ValidacionCamposEventArgs> ValidacionCamposProcesada;

        public event EventHandler<InicioSesionCompletadoEventArgs> InicioSesionCompletado;

        public event EventHandler IdiomaCambiado;

        public event EventHandler SolicitarCrearCuenta;

        public event EventHandler SolicitarUnirseInvitado;

        public event EventHandler ContrasenaRestablecida;

        public string Identificador
        {
            get => _identificador;
            set => EstablecerPropiedad(ref _identificador, value);
        }

        public string Contrasena
        {
            get => _contrasena;
            set => EstablecerPropiedad(ref _contrasena, value);
        }

        public IEnumerable<IdiomaOpcion> IdiomasDisponibles => _idiomasDisponibles;

        public IdiomaOpcion IdiomaSeleccionado
        {
            get => _idiomaSeleccionado;
            set
            {
                if (value != null && EstablecerPropiedad(ref _idiomaSeleccionado, value))
                {
                    CambiarIdioma(value);
                }
            }
        }

        public bool EstaProcesando
        {
            get => _estaProcesando;
            private set
            {
                if (EstablecerPropiedad(ref _estaProcesando, value))
                {
                    _iniciarSesionCommand.NotificarPuedeEjecutar();
                }
            }
        }

        public bool EstaRecuperando
        {
            get => _estaRecuperando;
            private set
            {
                if (EstablecerPropiedad(ref _estaRecuperando, value))
                {
                    _recuperarCuentaCommand.NotificarPuedeEjecutar();
                }
            }
        }

        public IComandoAsincrono IniciarSesionCommand => _iniciarSesionCommand;

        public ICommand IniciarSesionInvitadoCommand => _iniciarSesionInvitadoCommand;

        public ICommand AbrirCrearCuentaCommand => _abrirCrearCuentaCommand;

        public IComandoAsincrono RecuperarCuentaCommand => _recuperarCuentaCommand;

        public int ObtenerIndiceIdiomaSeleccionado()
        {
            return _idiomasDisponibles
            .Select((idioma, index) => new { idioma, index })
            .FirstOrDefault(x => Equals(x.idioma, IdiomaSeleccionado))?.index ?? -1;

        }

        public void EstablecerIdiomaPorIndice(int indice)
        {
            if (indice >= 0 && indice < _idiomasDisponibles.Count)
            {
                IdiomaSeleccionado = _idiomasDisponibles[indice];
            }
        }

        private void CambiarIdioma(IdiomaOpcion idioma)
        {
            if (idioma == null)
            {
                return;
            }

            if (!string.Equals(Properties.Settings.Default.idiomaCodigo, idioma.Codigo, StringComparison.OrdinalIgnoreCase))
            {
                Properties.Settings.Default.idiomaCodigo = idioma.Codigo;
                Properties.Settings.Default.Save();
                Lang.Culture = new System.Globalization.CultureInfo(idioma.Codigo);
                IdiomaCambiado?.Invoke(this, EventArgs.Empty);
            }
        }

        private async Task IniciarSesionAsync()
        {
            if (EstaProcesando)
            {
                return;
            }

            ValidacionEntradaHelper.ResultadoValidacion resultadoIdentificador = ValidacionEntradaHelper.ValidarIdentificadorInicioSesion(Identificador);
            ValidacionEntradaHelper.ResultadoValidacion resultadoContrasena = ValidacionEntradaHelper.ValidarContrasena(Contrasena);

            CampoEntrada camposInvalidos = CampoEntrada.Ninguno;

            if (!resultadoIdentificador.EsValido)
            {
                camposInvalidos |= CampoEntrada.Identificador;
            }

            if (!resultadoContrasena.EsValido)
            {
                camposInvalidos |= CampoEntrada.Contrasena;
            }

            if (camposInvalidos != CampoEntrada.Ninguno)
            {
                NotificarValidacionCampos(camposInvalidos);
                _dialogService.Aviso(resultadoIdentificador.EsValido ? resultadoContrasena.MensajeError : resultadoIdentificador.MensajeError);
                return;
            }

            NotificarValidacionCampos(CampoEntrada.Ninguno);

            EstaProcesando = true;

            try
            {
                var credenciales = new InicioSesionSrv.CredencialesInicioSesionDTO
                {
                    Identificador = resultadoIdentificador.ValorNormalizado,
                    Contrasena = resultadoContrasena.ValorNormalizado
                };

                InicioSesionSrv.ResultadoInicioSesionDTO resultado = await _inicioSesionService.IniciarSesionAsync(credenciales).ConfigureAwait(true);

                if (resultado == null)
                {
                    _dialogService.Aviso(Lang.errorTextoServidorTiempoAgotado);
                    return;
                }

                if (resultado.InicioSesionExitoso)
                {
                    UsuarioAutenticado usuario = UsuarioMapper.CrearDesde(resultado.Usuario);
                    SesionUsuarioActual.Instancia.EstablecerUsuario(usuario);
                    InicioSesionCompletado?.Invoke(this, new InicioSesionCompletadoEventArgs(usuario));
                    return;
                }

                string mensaje = MensajeServidorHelper.Localizar(
                    resultado.Mensaje,
                    Lang.errorTextoCredencialesTitulo);

                _dialogService.Aviso(mensaje);
            }
            catch (FaultException ex)
            {
                string mensaje = ErrorServicioHelper.ObtenerMensaje(
                    ex,
                    Lang.errorTextoServidorInicioSesion);
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
                EstaProcesando = false;
            }
        }

        private async Task RecuperarCuentaAsync()
        {
            if (EstaRecuperando)
            {
                return;
            }

            ValidacionEntradaHelper.ResultadoValidacion resultadoIdentificador = ValidacionEntradaHelper.ValidarIdentificadorInicioSesion(Identificador);

            if (!resultadoIdentificador.EsValido)
            {
                NotificarValidacionCampos(CampoEntrada.Identificador);
                _dialogService.Aviso(resultadoIdentificador.MensajeError);
                return;
            }

            NotificarValidacionCampos(CampoEntrada.Ninguno);

            string identificador = resultadoIdentificador.ValorNormalizado;
            EstaRecuperando = true;

            try
            {
                CodigoVerificacionSrv.ResultadoSolicitudRecuperacionDTO resultado = await CodigoVerificacionServicioHelper.SolicitarCodigoRecuperacionAsync(identificador).ConfigureAwait(true);

                if (resultado == null)
                {
                    _dialogService.Aviso(Lang.errorTextoIniciarRecuperacion);
                    return;
                }

                if (!resultado.CuentaEncontrada)
                {
                    string mensajeCuenta = MensajeServidorHelper.Localizar(
                        resultado.Mensaje,
                        Lang.errorTextoCuentaNoRegistrada);
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
                        CodigoVerificacionSrv.ResultadoOperacionDTO resultadoConfirmacion = await CodigoVerificacionServicioHelper.ConfirmarCodigoRecuperacionAsync(
                            tokenCodigo,
                            codigo).ConfigureAwait(true);

                        if (resultadoConfirmacion == null)
                        {
                            return null;
                        }

                        return new ConfirmacionCodigoResultado(
                            resultadoConfirmacion.OperacionExitosa,
                            resultadoConfirmacion.Mensaje);
                    },
                    async () =>
                    {
                        ReenvioSrv.ResultadoSolicitudCodigoDTO resultadoReenvio = await CodigoVerificacionServicioHelper.ReenviarCodigoRecuperacionAsync(tokenCodigo).ConfigureAwait(true);

                        if (resultadoReenvio != null && !string.IsNullOrWhiteSpace(resultadoReenvio.TokenCodigo))
                        {
                            tokenCodigo = resultadoReenvio.TokenCodigo;
                        }

                        if (resultadoReenvio == null)
                        {
                            return null;
                        }

                        return new ReenvioCodigoResultado(
                            resultadoReenvio.CodigoEnviado,
                            resultadoReenvio.Mensaje,
                            resultadoReenvio.TokenCodigo);
                    });

                bool verificacionCompletada = await _recuperacionCuentaDialogService.MostrarDialogoVerificacionAsync(
                    tokenCodigo,
                    correoDestino,
                    funcionesVerificacion,
                    Lang.avisoTextoCodigoDescripcionRecuperacion).ConfigureAwait(true);

                if (!verificacionCompletada)
                {
                    return;
                }

                ResultadoCambioContrasena resultadoCambio = await _recuperacionCuentaDialogService
                    .MostrarDialogoCambioContrasenaAsync(tokenCodigo, identificador).ConfigureAwait(true);

                if (resultadoCambio == null)
                {
                    return;
                }

                if (resultadoCambio.ContrasenaActualizada || resultadoCambio.DialogResult == true)
                {
                    Contrasena = string.Empty;
                    ContrasenaRestablecida?.Invoke(this, EventArgs.Empty);
                }
            }
            catch (FaultException ex)
            {
                string mensaje = ErrorServicioHelper.ObtenerMensaje(
                    ex,
                    Lang.errorTextoServidorInicioRecuperacion);
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
                EstaRecuperando = false;
            }
        }

        private void NotificarValidacionCampos(CampoEntrada camposInvalidos)
        {
            ValidacionCamposProcesada?.Invoke(this, new ValidacionCamposEventArgs(camposInvalidos));
        }
    }
}
