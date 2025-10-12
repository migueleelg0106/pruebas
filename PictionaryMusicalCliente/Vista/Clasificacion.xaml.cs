using PictionaryMusicalCliente.Modelo;
using PictionaryMusicalCliente.Servicios;
using System;
using System.Collections.Generic;
using System.ServiceModel;
using System.Threading.Tasks;
using System.Windows;

namespace PictionaryMusicalCliente
{
    /// <summary>
    /// Lógica de interacción para Clasificacion.xaml
    /// </summary>
    public partial class Clasificacion : Window
    {
        private readonly IDialogService _dialogService;

        public Clasificacion()
        {
            InitializeComponent();
            _dialogService = new DialogService();
        }

        private void BotonRegresar(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private async void VentanaCargada(object sender, RoutedEventArgs e)
        {
            await CargarClasificacionAsync();
        }

        private async Task CargarClasificacionAsync()
        {
            try
            {
                using (var proxy = new ServidorProxy())
                {
                    List<EntradaClasificacion> clasificacion = await proxy.ObtenerClasificacionAsync();
                    TablaClasificacion.ItemsSource = clasificacion;
                }
            }
            catch (CommunicationException ex)
            {
                _dialogService.Aviso(ex.Message);
            }
            catch (TimeoutException ex)
            {
                _dialogService.Aviso(ex.Message);
            }
            catch (InvalidOperationException ex)
            {
                _dialogService.Aviso(ex.Message);
            }
        }
    }
}
