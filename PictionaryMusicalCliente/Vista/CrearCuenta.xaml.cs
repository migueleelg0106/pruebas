using System.Windows;
using PictionaryMusicalCliente.Servicios.Abstracciones;
using PictionaryMusicalCliente.Servicios.Dialogos;
using PictionaryMusicalCliente.Servicios.Wcf;
using PictionaryMusicalCliente.Utilidades;
using PictionaryMusicalCliente.VistaModelo.Cuentas;

namespace PictionaryMusicalCliente
{
    public partial class CrearCuenta : Window
    {
        private readonly VistaModelo.Cuentas.CrearCuenta _vistaModelo;

        public CrearCuenta()
        {
            InitializeComponent();

            IDialogService dialogService = new DialogService();
            var codigoVerificacionService = new CodigoVerificacionService();
            var avatarService = new AvatarService();
            ISeleccionarAvatarService seleccionarAvatarService = new SeleccionarAvatarDialogService();
            IVerificarCodigoDialogService verificarCodigoDialogService = new VerificarCodigoDialogService();

            _vistaModelo = new VistaModelo.Cuentas.CrearCuenta(
                dialogService,
                codigoVerificacionService,
                avatarService,
                seleccionarAvatarService,
                verificarCodigoDialogService);

            _vistaModelo.SolicitarCerrar += (_, __) => Close();
            _vistaModelo.ValidacionCamposProcesada += VistaModelo_ValidacionCamposProcesada;
            DataContext = _vistaModelo;

            Closed += (_, __) => _vistaModelo.ValidacionCamposProcesada -= VistaModelo_ValidacionCamposProcesada;
        }

        private void PasswordBoxChanged(object sender, RoutedEventArgs e)
        {
            if (_vistaModelo != null)
            {
                _vistaModelo.Contrasena = bloqueContrasena.Password;
            }
        }

        private void VistaModelo_ValidacionCamposProcesada(object sender, VistaModelo.Cuentas.CrearCuenta.ValidacionCamposEventArgs e)
        {
            ControlVisualHelper.RestablecerEstadoCampo(bloqueTextoUsuario);
            ControlVisualHelper.RestablecerEstadoCampo(bloqueTextoNombre);
            ControlVisualHelper.RestablecerEstadoCampo(bloqueTextoApellido);
            ControlVisualHelper.RestablecerEstadoCampo(bloqueTextoCorreo);
            ControlVisualHelper.RestablecerEstadoCampo(bloqueContrasena);

            if (e == null)
            {
                return;
            }

            VistaModelo.Cuentas.CrearCuenta.CampoEntrada camposInvalidos = e.CamposInvalidos;

            if (camposInvalidos.HasFlag(VistaModelo.Cuentas.CrearCuenta.CampoEntrada.Usuario))
            {
                ControlVisualHelper.MarcarCampoInvalido(bloqueTextoUsuario);
            }

            if (camposInvalidos.HasFlag(VistaModelo.Cuentas.CrearCuenta.CampoEntrada.Nombre))
            {
                ControlVisualHelper.MarcarCampoInvalido(bloqueTextoNombre);
            }

            if (camposInvalidos.HasFlag(VistaModelo.Cuentas.CrearCuenta.CampoEntrada.Apellido))
            {
                ControlVisualHelper.MarcarCampoInvalido(bloqueTextoApellido);
            }

            if (camposInvalidos.HasFlag(VistaModelo.Cuentas.CrearCuenta.CampoEntrada.Correo))
            {
                ControlVisualHelper.MarcarCampoInvalido(bloqueTextoCorreo);
            }

            if (camposInvalidos.HasFlag(VistaModelo.Cuentas.CrearCuenta.CampoEntrada.Contrasena))
            {
                ControlVisualHelper.MarcarCampoInvalido(bloqueContrasena);
            }
        }
    }
}
