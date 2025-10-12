using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using LangResources = PictionaryMusicalCliente.Properties.Langs;

namespace PictionaryMusicalCliente
{
    /// <summary>
    /// Lógica de interacción para MainWindow.xaml
    /// </summary>
    public partial class VentanaPrincipal : Window
    {
        public VentanaPrincipal()
        {
            InitializeComponent();
        }

        private void BotonPerfil(object sender, RoutedEventArgs e)
        {
            Perfil perfil = new Perfil();
            perfil.ShowDialog();
        }

        private void BotonUnirse(object sender, RoutedEventArgs e)
        {

        }

        private void BotonComoJugar(object sender, RoutedEventArgs e)
        {
            ComoJugar comoJugar = new ComoJugar();
            comoJugar.ShowDialog();
        }

        private void BotonClasificacion(object sender, RoutedEventArgs e)
        {
            Clasificacion clasificacion = new Clasificacion();
            clasificacion.ShowDialog();
        }

        private void BotonJugar(object sender, RoutedEventArgs e)
        {
            VentanaJuego ventanaJuego = new VentanaJuego();
            ventanaJuego.Show();
            this.Close();
        }

        private void BotonInvitaciones(object sender, RoutedEventArgs e)
        {
            Invitaciones invitaciones = new Invitaciones();
            invitaciones.ShowDialog();
        }

        private void BotonSolicitudes(object sender, RoutedEventArgs e)
        {
            Solicitudes solicitudes = new Solicitudes();
            solicitudes.ShowDialog();
        }

        private void BotonEliminarAmigo(object sender, RoutedEventArgs e)
        {
            EliminarAmigo eliminarAmigo = new EliminarAmigo();
            eliminarAmigo.ShowDialog();
        }

        private void BotonBuscarAmigo(object sender, RoutedEventArgs e)
        {
            BuscarAmigo buscarAmigo = new BuscarAmigo();
            buscarAmigo.ShowDialog();
        }

        private void BotonAjustes(object sender, RoutedEventArgs e)
        {
            Ajustes ajustes = new Ajustes();
            ajustes.Owner = this;
            ajustes.ShowDialog();
        }

        private void CuadroCombinadoSeleccionNumeroRondas(object sender, SelectionChangedEventArgs e)
        {

        }

        private void CuadroCombinadoSeleccionTiempoRonda(object sender, SelectionChangedEventArgs e)
        {

        }

        private void CuadroCombinadoSeleccionIdioma(object sender, SelectionChangedEventArgs e)
        {

        }

        private void CuadroCombinadoSeleccionDificultad(object sender, SelectionChangedEventArgs e)
        {

        }
    }
}
