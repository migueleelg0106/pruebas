using PictionaryMusicalCliente.Modelo;
using PictionaryMusicalCliente.Servicios;
using PictionaryMusicalCliente.Utilidades;
using System;
using System.Collections.Generic;
using System.Linq;
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
        private List<EntradaClasificacion> _clasificacionOriginal;

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
                    _clasificacionOriginal = await proxy.ObtenerClasificacionAsync();
                    TablaClasificacion.ItemsSource = _clasificacionOriginal;
                }
            }
            catch (FaultException<ServidorProxy.ErrorDetalleServicio> ex)
            {
                string mensaje = ErrorServicioHelper.ObtenerMensaje(
                    ex,
                    "El servidor reportó un error al obtener la clasificación.");
                _dialogService.Aviso(mensaje);
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

        private void OrdenarPorRondas(object sender, RoutedEventArgs e)
        {
            if (_clasificacionOriginal == null)
            {
                return;
            }

            TablaClasificacion.ItemsSource = _clasificacionOriginal
                .OrderByDescending(entrada => entrada.RondasGanadas)
                .ThenByDescending(entrada => entrada.Puntos)
                .ToList();
        }

        private void OrdenarPorPuntos(object sender, RoutedEventArgs e)
        {
            if (_clasificacionOriginal == null)
            {
                return;
            }

            TablaClasificacion.ItemsSource = _clasificacionOriginal
                .OrderByDescending(entrada => entrada.Puntos)
                .ThenByDescending(entrada => entrada.RondasGanadas)
                .ToList();
        }
    }
}
