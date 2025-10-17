using System;
using System.Windows;
using System.Windows.Controls;
using PictionaryMusicalCliente.Servicios.Abstracciones;
using PictionaryMusicalCliente.Servicios.Dialogos;
using PictionaryMusicalCliente.Servicios.Wcf;
using PictionaryMusicalCliente.Utilidades;
using PictionaryMusicalCliente.VistaModelo.Cuentas;

namespace PictionaryMusicalCliente
{
    public partial class InicioSesion : Window
    {
        private readonly InicioSesionVistaModelo _vistaModelo;

        public InicioSesion()
        {
            InitializeComponent();

            IDialogService dialogService = new DialogService();
            IInicioSesionService inicioSesionService = new InicioSesionService();
            IRecuperacionCuentaDialogService recuperacionCuentaDialogService = new RecuperacionCuentaDialogService();

            _vistaModelo = new InicioSesionVistaModelo(dialogService, inicioSesionService, recuperacionCuentaDialogService);
            _vistaModelo.ValidacionCamposProcesada += VistaModelo_ValidacionCamposProcesada;
            _vistaModelo.InicioSesionCompletado += VistaModelo_InicioSesionCompletado;
            _vistaModelo.IdiomaCambiado += VistaModelo_IdiomaCambiado;
            _vistaModelo.SolicitarCrearCuenta += VistaModelo_SolicitarCrearCuenta;
            _vistaModelo.SolicitarUnirseInvitado += VistaModelo_SolicitarUnirseInvitado;
            _vistaModelo.ContrasenaRestablecida += VistaModelo_ContrasenaRestablecida;

            DataContext = _vistaModelo;

            Loaded += InicioSesion_Loaded;
            Closed += InicioSesion_Closed;
        }

        private void InicioSesion_Loaded(object sender, RoutedEventArgs e)
        {
            Loaded -= InicioSesion_Loaded;
            cuadroCombinadoLenguaje.SelectedIndex = _vistaModelo.ObtenerIndiceIdiomaSeleccionado();
        }

        private void InicioSesion_Closed(object sender, EventArgs e)
        {
            _vistaModelo.ValidacionCamposProcesada -= VistaModelo_ValidacionCamposProcesada;
            _vistaModelo.InicioSesionCompletado -= VistaModelo_InicioSesionCompletado;
            _vistaModelo.IdiomaCambiado -= VistaModelo_IdiomaCambiado;
            _vistaModelo.SolicitarCrearCuenta -= VistaModelo_SolicitarCrearCuenta;
            _vistaModelo.SolicitarUnirseInvitado -= VistaModelo_SolicitarUnirseInvitado;
            _vistaModelo.ContrasenaRestablecida -= VistaModelo_ContrasenaRestablecida;
        }

        private void VistaModelo_ValidacionCamposProcesada(object sender, InicioSesionVistaModelo.ValidacionCamposEventArgs e)
        {
            ControlVisualHelper.RestablecerEstadoCampo(bloqueTextoUsuario);
            ControlVisualHelper.RestablecerEstadoCampo(bloqueContrasenaContrasena);

            if (e == null)
            {
                return;
            }

            if (e.CamposInvalidos.HasFlag(InicioSesionVistaModelo.CampoEntrada.Identificador))
            {
                ControlVisualHelper.MarcarCampoInvalido(bloqueTextoUsuario);
            }

            if (e.CamposInvalidos.HasFlag(InicioSesionVistaModelo.CampoEntrada.Contrasena))
            {
                ControlVisualHelper.MarcarCampoInvalido(bloqueContrasenaContrasena);
            }
        }

        private void VistaModelo_InicioSesionCompletado(object sender, InicioSesionVistaModelo.InicioSesionCompletadoEventArgs e)
        {
            var ventana = new VentanaPrincipal();
            Application.Current.MainWindow = ventana;
            ventana.Show();
            Close();
        }

        private void VistaModelo_IdiomaCambiado(object sender, EventArgs e)
        {
            var nuevaVentana = new InicioSesion();
            Application.Current.MainWindow = nuevaVentana;
            nuevaVentana.Show();
            Closed -= InicioSesion_Closed;
            Close();
        }

        private void VistaModelo_SolicitarCrearCuenta(object sender, EventArgs e)
        {
            var ventana = new CrearCuenta();
            ventana.ShowDialog();
        }

        private void VistaModelo_SolicitarUnirseInvitado(object sender, EventArgs e)
        {
            var ventana = new UnirsePartidaInvitado();
            ventana.ShowDialog();
        }

        private void VistaModelo_ContrasenaRestablecida(object sender, EventArgs e)
        {
            bloqueContrasenaContrasena.Clear();
        }

        private void PasswordBoxChanged(object sender, RoutedEventArgs e)
        {
            if (_vistaModelo != null)
            {
                _vistaModelo.Contrasena = bloqueContrasenaContrasena.Password;
            }
        }
    }
}
