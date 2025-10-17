using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Media;
using PictionaryMusicalCliente.Modelo;
using PictionaryMusicalCliente.Servicios.Abstracciones;
using PictionaryMusicalCliente.Servicios.Dialogos;
using PictionaryMusicalCliente.VistaModelo.Cuentas;

namespace PictionaryMusicalCliente
{
    public partial class SeleccionarAvatar : Window
    {
        private readonly SeleccionarAvatarVistaModelo _vistaModelo;

        public SeleccionarAvatar(IReadOnlyCollection<ObjetoAvatar> avatares = null)
        {
            InitializeComponent();

            IDialogService dialogService = new DialogService();

            _vistaModelo = new SeleccionarAvatarVistaModelo(dialogService, avatares);
            _vistaModelo.AvatarSeleccionadoConfirmado += VistaModelo_AvatarSeleccionadoConfirmado;
            _vistaModelo.SolicitarCerrar += VistaModelo_SolicitarCerrar;

            DataContext = _vistaModelo;

            Closed += SeleccionarAvatar_Closed;
        }

        public ObjetoAvatar AvatarSeleccionado => _vistaModelo?.AvatarSeleccionadoModelo;

        public ImageSource AvatarSeleccionadoImagen => _vistaModelo?.AvatarSeleccionadoImagen;

        private void VistaModelo_AvatarSeleccionadoConfirmado(object sender, SeleccionarAvatarVistaModelo.AvatarSeleccionadoEventArgs e)
        {
            DialogResult = true;
            Close();
        }

        private void VistaModelo_SolicitarCerrar(object sender, EventArgs e)
        {
            DialogResult = false;
            Close();
        }

        private void SeleccionarAvatar_Closed(object sender, EventArgs e)
        {
            if (_vistaModelo != null)
            {
                _vistaModelo.AvatarSeleccionadoConfirmado -= VistaModelo_AvatarSeleccionadoConfirmado;
                _vistaModelo.SolicitarCerrar -= VistaModelo_SolicitarCerrar;
            }

            Closed -= SeleccionarAvatar_Closed;
        }
    }
}
