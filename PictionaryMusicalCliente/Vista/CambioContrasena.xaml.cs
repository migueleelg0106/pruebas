using System;
using System.Windows;
using PictionaryMusicalCliente.Servicios.Abstracciones;
using PictionaryMusicalCliente.Servicios.Dialogos;
using PictionaryMusicalCliente.Servicios.Wcf;
using PictionaryMusicalCliente.Utilidades;
using PictionaryMusicalCliente.VistaModelo.Cuentas;
using LangResources = PictionaryMusicalCliente.Properties.Langs;

namespace PictionaryMusicalCliente
{
    public partial class CambioContrasena : Window
    {
        private readonly CambioContrasenaVistaModelo _vistaModelo;

        public CambioContrasena(string tokenCodigo, string identificador)
        {
            if (string.IsNullOrWhiteSpace(tokenCodigo))
            {
                throw new ArgumentException(LangResources.Lang.errorTextoTokenCodigoObligatorio, nameof(tokenCodigo));
            }

            _ = identificador;

            InitializeComponent();

            IDialogService dialogService = new DialogService();
            ICambioContrasenaService cambioContrasenaService = new CambioContrasenaService();

            _vistaModelo = new CambioContrasenaVistaModelo(
                dialogService,
                cambioContrasenaService,
                tokenCodigo);

            _vistaModelo.SolicitarCerrar += VistaModelo_SolicitarCerrar;
            _vistaModelo.ContrasenaActualizacionCompletada += VistaModelo_ContrasenaActualizacionCompletada;
            _vistaModelo.ValidacionCamposProcesada += VistaModelo_ValidacionCamposProcesada;

            DataContext = _vistaModelo;

            Closed += CambioContrasena_Closed;
        }

        public bool ContrasenaActualizada => _vistaModelo?.ContrasenaActualizada ?? false;

        private void ContrasenaNuevaPasswordChanged(object sender, RoutedEventArgs e)
        {
            if (_vistaModelo != null)
            {
                _vistaModelo.NuevaContrasena = bloqueContrasenaNueva.Password;
            }
        }

        private void ContrasenaConfirmacionPasswordChanged(object sender, RoutedEventArgs e)
        {
            if (_vistaModelo != null)
            {
                _vistaModelo.ConfirmacionContrasena = bloqueContrasenaConfirmacion.Password;
            }
        }

        private void VistaModelo_ValidacionCamposProcesada(object sender, CambioContrasenaVistaModelo.ValidacionCamposEventArgs e)
        {
            ControlVisualHelper.RestablecerEstadoCampo(bloqueContrasenaNueva);
            ControlVisualHelper.RestablecerEstadoCampo(bloqueContrasenaConfirmacion);

            if (e == null)
            {
                return;
            }

            if (e.CamposLimpiar.HasFlag(CambioContrasenaVistaModelo.CampoEntrada.NuevaContrasena))
            {
                bloqueContrasenaNueva.Clear();
            }

            if (e.CamposLimpiar.HasFlag(CambioContrasenaVistaModelo.CampoEntrada.ConfirmacionContrasena))
            {
                bloqueContrasenaConfirmacion.Clear();
            }

            if (e.CamposInvalidos.HasFlag(CambioContrasenaVistaModelo.CampoEntrada.NuevaContrasena))
            {
                ControlVisualHelper.MarcarCampoInvalido(bloqueContrasenaNueva);
            }

            if (e.CamposInvalidos.HasFlag(CambioContrasenaVistaModelo.CampoEntrada.ConfirmacionContrasena))
            {
                ControlVisualHelper.MarcarCampoInvalido(bloqueContrasenaConfirmacion);
            }

            if (e.CampoEnfoque.HasFlag(CambioContrasenaVistaModelo.CampoEntrada.NuevaContrasena))
            {
                bloqueContrasenaNueva.Focus();
            }
            else if (e.CampoEnfoque.HasFlag(CambioContrasenaVistaModelo.CampoEntrada.ConfirmacionContrasena))
            {
                bloqueContrasenaConfirmacion.Focus();
            }
        }

        private void VistaModelo_SolicitarCerrar(object sender, EventArgs e)
        {
            DialogResult = false;
            Close();
        }

        private void VistaModelo_ContrasenaActualizacionCompletada(object sender, EventArgs e)
        {
            DialogResult = true;
            Close();
        }

        private void CambioContrasena_Closed(object sender, EventArgs e)
        {
            if (_vistaModelo != null)
            {
                _vistaModelo.SolicitarCerrar -= VistaModelo_SolicitarCerrar;
                _vistaModelo.ContrasenaActualizacionCompletada -= VistaModelo_ContrasenaActualizacionCompletada;
                _vistaModelo.ValidacionCamposProcesada -= VistaModelo_ValidacionCamposProcesada;
            }
        }
    }
}
