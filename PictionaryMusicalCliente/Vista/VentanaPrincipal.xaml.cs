using System;
using System.Windows;
using PictionaryMusicalCliente.Servicios.Abstracciones;
using PictionaryMusicalCliente.Servicios.Dialogos;
using PictionaryMusicalCliente.VistaModelo.Cuentas;

namespace PictionaryMusicalCliente
{
    public partial class VentanaPrincipal : Window
    {
        private readonly VentanaPrincipalVistaModelo _vistaModelo;

        public VentanaPrincipal()
        {
            InitializeComponent();

            IDialogService dialogService = new DialogService();

            _vistaModelo = new VentanaPrincipalVistaModelo(dialogService);
            _vistaModelo.SolicitarAbrirPerfil += VistaModelo_SolicitarAbrirPerfil;
            _vistaModelo.SolicitarAbrirAjustes += VistaModelo_SolicitarAbrirAjustes;
            _vistaModelo.SolicitarAbrirComoJugar += VistaModelo_SolicitarAbrirComoJugar;
            _vistaModelo.SolicitarAbrirClasificacion += VistaModelo_SolicitarAbrirClasificacion;
            _vistaModelo.SolicitarAbrirInvitaciones += VistaModelo_SolicitarAbrirInvitaciones;
            _vistaModelo.SolicitarAbrirSolicitudes += VistaModelo_SolicitarAbrirSolicitudes;
            _vistaModelo.SolicitarAbrirEliminarAmigo += VistaModelo_SolicitarAbrirEliminarAmigo;
            _vistaModelo.SolicitarAbrirBuscarAmigo += VistaModelo_SolicitarAbrirBuscarAmigo;
            _vistaModelo.SolicitarIniciarJuego += VistaModelo_SolicitarIniciarJuego;
            _vistaModelo.SolicitarUnirseSala += VistaModelo_SolicitarUnirseSala;

            DataContext = _vistaModelo;

            Closed += VentanaPrincipal_Closed;
        }

        private void VistaModelo_SolicitarAbrirPerfil(object sender, EventArgs e)
        {
            var perfil = new Perfil();
            perfil.ShowDialog();
        }

        private void VistaModelo_SolicitarAbrirAjustes(object sender, EventArgs e)
        {
            var ajustes = new Ajustes
            {
                Owner = this
            };
            ajustes.ShowDialog();
        }

        private void VistaModelo_SolicitarAbrirComoJugar(object sender, EventArgs e)
        {
            var comoJugar = new ComoJugar();
            comoJugar.ShowDialog();
        }

        private void VistaModelo_SolicitarAbrirClasificacion(object sender, EventArgs e)
        {
            var clasificacion = new Clasificacion();
            clasificacion.ShowDialog();
        }

        private void VistaModelo_SolicitarAbrirInvitaciones(object sender, EventArgs e)
        {
            var invitaciones = new Invitaciones();
            invitaciones.ShowDialog();
        }

        private void VistaModelo_SolicitarAbrirSolicitudes(object sender, EventArgs e)
        {
            var solicitudes = new Solicitudes();
            solicitudes.ShowDialog();
        }

        private void VistaModelo_SolicitarAbrirEliminarAmigo(object sender, EventArgs e)
        {
            var eliminarAmigo = new EliminarAmigo();
            eliminarAmigo.ShowDialog();
        }

        private void VistaModelo_SolicitarAbrirBuscarAmigo(object sender, EventArgs e)
        {
            var buscarAmigo = new BuscarAmigo();
            buscarAmigo.ShowDialog();
        }

        private void VistaModelo_SolicitarIniciarJuego(object sender, VentanaPrincipalVistaModelo.IniciarJuegoEventArgs e)
        {
            var ventanaJuego = new VentanaJuego();
            ventanaJuego.Show();
            Close();
        }

        private void VistaModelo_SolicitarUnirseSala(object sender, VentanaPrincipalVistaModelo.UnirseSalaEventArgs e)
        {
            // La lógica para unirse a una sala se implementará posteriormente.
        }

        private void VentanaPrincipal_Closed(object sender, EventArgs e)
        {
            Closed -= VentanaPrincipal_Closed;

            if (_vistaModelo != null)
            {
                _vistaModelo.SolicitarAbrirPerfil -= VistaModelo_SolicitarAbrirPerfil;
                _vistaModelo.SolicitarAbrirAjustes -= VistaModelo_SolicitarAbrirAjustes;
                _vistaModelo.SolicitarAbrirComoJugar -= VistaModelo_SolicitarAbrirComoJugar;
                _vistaModelo.SolicitarAbrirClasificacion -= VistaModelo_SolicitarAbrirClasificacion;
                _vistaModelo.SolicitarAbrirInvitaciones -= VistaModelo_SolicitarAbrirInvitaciones;
                _vistaModelo.SolicitarAbrirSolicitudes -= VistaModelo_SolicitarAbrirSolicitudes;
                _vistaModelo.SolicitarAbrirEliminarAmigo -= VistaModelo_SolicitarAbrirEliminarAmigo;
                _vistaModelo.SolicitarAbrirBuscarAmigo -= VistaModelo_SolicitarAbrirBuscarAmigo;
                _vistaModelo.SolicitarIniciarJuego -= VistaModelo_SolicitarIniciarJuego;
                _vistaModelo.SolicitarUnirseSala -= VistaModelo_SolicitarUnirseSala;
                _vistaModelo.Dispose();
            }
        }
    }
}
