using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using PictionaryMusicalCliente.Servicios.Abstracciones;
using PictionaryMusicalCliente.Servicios.Dialogos;
using PictionaryMusicalCliente.Servicios.Wcf;
using PictionaryMusicalCliente.Utilidades;
using PictionaryMusicalCliente.VistaModelo.Cuentas;

namespace PictionaryMusicalCliente
{
    public partial class Perfil : Window
    {
        private readonly PerfilVistaModelo _vistaModelo;

        public Perfil()
        {
            InitializeComponent();

            IDialogService dialogService = new DialogService();
            IPerfilService perfilService = new PerfilService();
            ISeleccionarAvatarService seleccionarAvatarService = new SeleccionarAvatarDialogService();
            IRecuperacionCuentaDialogService recuperacionCuentaDialogService = new RecuperacionCuentaDialogService();

            _vistaModelo = new PerfilVistaModelo(
                dialogService,
                perfilService,
                seleccionarAvatarService,
                recuperacionCuentaDialogService);

            _vistaModelo.SolicitarCerrar += VistaModelo_SolicitarCerrar;
            _vistaModelo.ValidacionCamposProcesada += VistaModelo_ValidacionCamposProcesada;

            DataContext = _vistaModelo;

            Closed += Perfil_Closed;
        }

        private async void Perfil_Loaded(object sender, RoutedEventArgs e)
        {
            if (_vistaModelo != null)
            {
                await _vistaModelo.InicializarAsync();
            }
        }

        private void VistaModelo_SolicitarCerrar(object sender, EventArgs e)
        {
            Close();
        }

        private void VistaModelo_ValidacionCamposProcesada(object sender, PerfilVistaModelo.ValidacionCamposEventArgs e)
        {
            ControlVisualHelper.RestablecerEstadoCampo(bloqueTextoNombre);
            ControlVisualHelper.RestablecerEstadoCampo(bloqueTextoApellido);

            if (e == null)
            {
                return;
            }

            if (e.CamposInvalidos.HasFlag(PerfilVistaModelo.CampoEntrada.Nombre))
            {
                ControlVisualHelper.MarcarCampoInvalido(bloqueTextoNombre);
            }

            if (e.CamposInvalidos.HasFlag(PerfilVistaModelo.CampoEntrada.Apellido))
            {
                ControlVisualHelper.MarcarCampoInvalido(bloqueTextoApellido);
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

        private void Perfil_Closed(object sender, EventArgs e)
        {
            if (_vistaModelo != null)
            {
                _vistaModelo.SolicitarCerrar -= VistaModelo_SolicitarCerrar;
                _vistaModelo.ValidacionCamposProcesada -= VistaModelo_ValidacionCamposProcesada;
            }
        }
    }
}
