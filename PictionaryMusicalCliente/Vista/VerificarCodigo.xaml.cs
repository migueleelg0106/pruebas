using System;
using System.Threading.Tasks;
using System.Windows;
using PictionaryMusicalCliente.Modelo.Cuentas;
using PictionaryMusicalCliente.Servicios.Abstracciones;
using PictionaryMusicalCliente.Servicios.Dialogos;
using PictionaryMusicalCliente.Servicios.Wcf;
using PictionaryMusicalCliente.VistaModelo.Cuentas;
using LangResources = PictionaryMusicalCliente.Properties.Langs;

namespace PictionaryMusicalCliente
{
    public partial class VerificarCodigo : Window
    {
        private readonly VerificarCodigoVistaModelo _vistaModelo;

        public VerificarCodigo(
            string tokenCodigo,
            string correoDestino,
            Func<string, Task<ConfirmacionCodigoResultado>> confirmarCodigoAsync = null,
            Func<Task<ReenvioCodigoResultado>> reenviarCodigoAsync = null,
            string descripcionPersonalizada = null)
        {
            if (string.IsNullOrWhiteSpace(tokenCodigo))
            {
                throw new ArgumentException(LangResources.Lang.errorTextoTokenCodigoObligatorio, nameof(tokenCodigo));
            }

            InitializeComponent();

            IDialogService dialogService = new DialogService();
            IVerificarCodigoService verificarCodigoService = new VerificarCodigoService();

            _vistaModelo = new VerificarCodigoVistaModelo(
                dialogService,
                verificarCodigoService,
                tokenCodigo,
                correoDestino,
                confirmarCodigoAsync,
                reenviarCodigoAsync,
                descripcionPersonalizada);

            _vistaModelo.SolicitarCerrar += VistaModelo_SolicitarCerrar;

            DataContext = _vistaModelo;

            Closed += VerificarCodigo_Closed;
        }

        public bool OperacionCompletada => _vistaModelo?.OperacionCompletada ?? false;

        public bool RegistroCompletado => OperacionCompletada;

        private void VistaModelo_SolicitarCerrar(object sender, EventArgs e)
        {
            DialogResult = OperacionCompletada;
            Close();
        }

        private void VerificarCodigo_Closed(object sender, EventArgs e)
        {
            if (_vistaModelo != null)
            {
                _vistaModelo.SolicitarCerrar -= VistaModelo_SolicitarCerrar;
                _vistaModelo.Dispose();
            }
        }
    }
}
