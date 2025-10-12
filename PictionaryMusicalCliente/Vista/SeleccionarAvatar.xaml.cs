using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using PictionaryMusicalCliente.Modelo;
using PictionaryMusicalCliente.Properties.Langs;
using PictionaryMusicalCliente.Modelo.Catalogos;
using PictionaryMusicalCliente.Utilidades;

namespace PictionaryMusicalCliente
{
    public partial class SeleccionarAvatar : Window
    {
        private readonly IReadOnlyCollection<ObjetoAvatar> _avataresDisponibles;

        public ObjetoAvatar AvatarSeleccionado { get; private set; }
        public ImageSource AvatarSeleccionadoImagen { get; private set; }

        public SeleccionarAvatar(IReadOnlyCollection<ObjetoAvatar> avatares = null)
        {
            InitializeComponent();
            _avataresDisponibles = avatares;
            Loaded += SeleccionarAvatar_Loaded;
        }

        private void SeleccionarAvatar_Loaded(object sender, RoutedEventArgs e) =>
            MostrarAvataresEnLista(_avataresDisponibles ?? CatalogoAvataresLocales.ObtenerAvatares());

        private void MostrarAvataresEnLista(IReadOnlyCollection<ObjetoAvatar> avatares)
        {
            listaAvatares.Items.Clear();

            if (avatares == null || avatares.Count == 0)
            {
                return;
            }

            foreach (ObjetoAvatar avatar in avatares)
            {
                ImageSource imagenAvatar = AvatarImagenHelper.CrearImagen(avatar);

                var imagenControl = new Image
                {
                    Width = 72,
                    Height = 72,
                    Margin = new Thickness(5),
                    Cursor = Cursors.Hand,
                    Source = imagenAvatar
                };

                var itemLista = new ListBoxItem
                {
                    Content = imagenControl,
                    Tag = avatar,
                    ToolTip = avatar.Nombre
                };

                listaAvatares.Items.Add(itemLista);
            }
        }

        private void BotonAceptarSeleccionAvatar(object sender, RoutedEventArgs e)
        {
            if (listaAvatares.SelectedItem is ListBoxItem itemSeleccionado && itemSeleccionado.Tag is ObjetoAvatar avatar)
            {
                AvatarSeleccionado = avatar;
                AvatarSeleccionadoImagen = AvatarImagenHelper.CrearImagen(avatar);
                DialogResult = true;
                Close();
                return;
            }

            new Avisos(Lang.globalTextoSeleccionarAvatar).ShowDialog();
        }
    }
}
