using System.Windows;
using PictionaryMusicalCliente.Servicios.Abstracciones;
using PictionaryMusicalCliente.Servicios.Dialogos;
using PictionaryMusicalCliente.Servicios.Wcf;
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
            DataContext = _vistaModelo;
        }

        private void PasswordBoxChanged(object sender, RoutedEventArgs e)
        {
            if (_vistaModelo != null)
            {
                _vistaModelo.Contrasena = bloqueContrasena.Password;
            }
        }
    }
}
