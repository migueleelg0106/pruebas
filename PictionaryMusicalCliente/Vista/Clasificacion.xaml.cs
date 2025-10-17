using System;
using System.Windows;
using PictionaryMusicalCliente.Servicios.Abstracciones;
using PictionaryMusicalCliente.Servicios.Dialogos;
using PictionaryMusicalCliente.Servicios.Wcf;
using PictionaryMusicalCliente.VistaModelo.Cuentas;

namespace PictionaryMusicalCliente
{
    public partial class Clasificacion : Window
    {
        private readonly ClasificacionVistaModelo _vistaModelo;
        private readonly EventHandler _solicitarCerrarHandler;

        public Clasificacion()
        {
            InitializeComponent();

            IDialogService dialogService = new DialogService();
            IClasificacionService clasificacionService = new ClasificacionService();

            _vistaModelo = new ClasificacionVistaModelo(dialogService, clasificacionService);
            _solicitarCerrarHandler = (_, __) => Close();
            _vistaModelo.SolicitarCerrar += _solicitarCerrarHandler;
            DataContext = _vistaModelo;

            Loaded += Clasificacion_Loaded;
            Closed += Clasificacion_Closed;
        }

        private async void Clasificacion_Loaded(object sender, RoutedEventArgs e)
        {
            Loaded -= Clasificacion_Loaded;
            await _vistaModelo.CargarClasificacionCommand.EjecutarAsync(null);
        }

        private void Clasificacion_Closed(object sender, EventArgs e)
        {
            _vistaModelo.SolicitarCerrar -= _solicitarCerrarHandler;
            Loaded -= Clasificacion_Loaded;
        }
    }
}
