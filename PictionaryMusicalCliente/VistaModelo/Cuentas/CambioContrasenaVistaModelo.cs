using System;
using System.Threading.Tasks;
using System.Windows.Input;
using PictionaryMusicalCliente.Comandos;
using PictionaryMusicalCliente.Modelo;
using PictionaryMusicalCliente.Properties.Langs;
using PictionaryMusicalCliente.Servicios;
using PictionaryMusicalCliente.Servicios.Abstracciones;
using PictionaryMusicalCliente.Servicios.Wcf.Helpers;
using PictionaryMusicalCliente.Utilidades;

namespace PictionaryMusicalCliente.VistaModelo.Cuentas
{
    public class CambioContrasenaVistaModelo : BaseVistaModelo
    {
        [Flags]
        public enum CampoEntrada
        {
            Ninguno = 0,
            NuevaContrasena = 1 << 0,
            ConfirmacionContrasena = 1 << 1
        }

        public sealed class ValidacionCamposEventArgs : EventArgs
        {
            public ValidacionCamposEventArgs(
                CampoEntrada camposInvalidos,
                CampoEntrada camposLimpiar,
                CampoEntrada campoEnfoque)
            {
                CamposInvalidos = camposInvalidos;
                CamposLimpiar = camposLimpiar;
                CampoEnfoque = campoEnfoque;
            }

            public CampoEntrada CamposInvalidos { get; }

            public CampoEntrada CamposLimpiar { get; }

            public CampoEntrada CampoEnfoque { get; }
        }

        private readonly IDialogService _dialogService;
        private readonly ICambioContrasenaService _cambioContrasenaService;
        private readonly string _tokenCodigo;
        private readonly ComandoAsincrono _confirmarCommand;

        private string _nuevaContrasena;
        private string _confirmacionContrasena;
        private bool _estaProcesando;
        private bool _contrasenaActualizada;

        public CambioContrasenaVistaModelo(
            IDialogService dialogService,
            ICambioContrasenaService cambioContrasenaService,
            string tokenCodigo)
        {
            _dialogService = dialogService ?? throw new ArgumentNullException(nameof(dialogService));
            _cambioContrasenaService = cambioContrasenaService ?? throw new ArgumentNullException(nameof(cambioContrasenaService));
            _tokenCodigo = string.IsNullOrWhiteSpace(tokenCodigo)
                ? throw new ArgumentException(Lang.errorTextoTokenCodigoObligatorio, nameof(tokenCodigo))
                : tokenCodigo;

            _confirmarCommand = new ComandoAsincrono(_ => ConfirmarAsync(), _ => !EstaProcesando);
            CancelarCommand = new Comando(() => SolicitarCerrar?.Invoke(this, EventArgs.Empty));
        }

        public event EventHandler SolicitarCerrar;

        public event EventHandler ContrasenaActualizacionCompletada;

        public event EventHandler<ValidacionCamposEventArgs> ValidacionCamposProcesada;

        public string NuevaContrasena
        {
            get => _nuevaContrasena;
            set => EstablecerPropiedad(ref _nuevaContrasena, value);
        }

        public string ConfirmacionContrasena
        {
            get => _confirmacionContrasena;
            set => EstablecerPropiedad(ref _confirmacionContrasena, value);
        }

        public bool EstaProcesando
        {
            get => _estaProcesando;
            private set
            {
                if (EstablecerPropiedad(ref _estaProcesando, value))
                {
                    _confirmarCommand.NotificarPuedeEjecutar();
                }
            }
        }

        public bool ContrasenaActualizada
        {
            get => _contrasenaActualizada;
            private set => EstablecerPropiedad(ref _contrasenaActualizada, value);
        }

        public IComandoAsincrono ConfirmarCommand => _confirmarCommand;

        public ICommand CancelarCommand { get; }

        private async Task ConfirmarAsync()
        {
            if (EstaProcesando)
            {
                return;
            }

            NotificarValidacionCampos(CampoEntrada.Ninguno);

            if (string.IsNullOrWhiteSpace(NuevaContrasena) || string.IsNullOrWhiteSpace(ConfirmacionContrasena))
            {
                NotificarValidacionCampos(
                    CampoEntrada.NuevaContrasena | CampoEntrada.ConfirmacionContrasena,
                    CampoEntrada.Ninguno,
                    CampoEntrada.NuevaContrasena);
                _dialogService.Aviso(Lang.errorTextoConfirmacionContrasenaRequerida);
                return;
            }

            if (!string.Equals(NuevaContrasena, ConfirmacionContrasena, StringComparison.Ordinal))
            {
                NotificarValidacionCampos(
                    CampoEntrada.NuevaContrasena | CampoEntrada.ConfirmacionContrasena,
                    CampoEntrada.NuevaContrasena | CampoEntrada.ConfirmacionContrasena,
                    CampoEntrada.NuevaContrasena);
                _dialogService.Aviso(Lang.errorTextoContrasenasNoCoinciden);
                return;
            }

            ValidacionEntradaHelper.ResultadoValidacion resultadoContrasena = ValidacionEntradaHelper.ValidarContrasena(NuevaContrasena);

            if (!resultadoContrasena.EsValido)
            {
                NotificarValidacionCampos(
                    CampoEntrada.NuevaContrasena,
                    CampoEntrada.Ninguno,
                    CampoEntrada.NuevaContrasena);
                _dialogService.Aviso(resultadoContrasena.MensajeError ?? Lang.errorTextoActualizarContrasena);
                return;
            }

            EstaProcesando = true;

            try
            {
                ResultadoOperacion resultado = await _cambioContrasenaService.ActualizarContrasenaAsync(
                    _tokenCodigo,
                    resultadoContrasena.ValorNormalizado).ConfigureAwait(true);

                if (resultado == null)
                {
                    _dialogService.Aviso(Lang.errorTextoActualizarContrasena);
                    return;
                }

                string mensajePredeterminado = resultado.OperacionExitosa
                    ? Lang.avisoTextoContrasenaActualizada
                    : Lang.errorTextoActualizarContrasena;

                string mensaje = MensajeServidorHelper.Localizar(resultado.Mensaje, mensajePredeterminado);
                _dialogService.Aviso(mensaje);

                if (resultado.OperacionExitosa)
                {
                    ContrasenaActualizada = true;
                    ContrasenaActualizacionCompletada?.Invoke(this, EventArgs.Empty);
                }
            }
            catch (ServicioException ex)
            {
                string mensajeError = string.IsNullOrWhiteSpace(ex.Message)
                    ? Lang.errorTextoActualizarContrasena
                    : ex.Message;
                _dialogService.Aviso(mensajeError);
            }
            finally
            {
                EstaProcesando = false;
            }
        }

        private void NotificarValidacionCampos(
            CampoEntrada camposInvalidos,
            CampoEntrada camposLimpiar = CampoEntrada.Ninguno,
            CampoEntrada campoEnfoque = CampoEntrada.Ninguno)
        {
            ValidacionCamposProcesada?.Invoke(
                this,
                new ValidacionCamposEventArgs(camposInvalidos, camposLimpiar, campoEnfoque));
        }
    }
}
