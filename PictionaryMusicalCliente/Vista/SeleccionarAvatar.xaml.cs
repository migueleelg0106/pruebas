using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using PictionaryMusicalCliente.Modelo;
using PictionaryMusicalCliente.Properties.Langs;
using PictionaryMusicalCliente.Modelo.Catalogos;

namespace PictionaryMusicalCliente
{
    public partial class SeleccionarAvatar : Window
    {
        public ObjetoAvatar AvatarSeleccionado { get; private set; }
        public ImageSource AvatarSeleccionadoImagen { get; private set; }

        public SeleccionarAvatar()
        {
            InitializeComponent();
            Loaded += SeleccionarAvatar_Loaded;
        }

        private void SeleccionarAvatar_Loaded(object sender, RoutedEventArgs e) =>
            MostrarAvataresEnLista(CatalogoAvataresLocales.ObtenerAvatares());

        private void MostrarAvataresEnLista(IReadOnlyCollection<ObjetoAvatar> avatares)
        {
            listaAvatares.Items.Clear();

            if (avatares == null || avatares.Count == 0)
            {
                return;
            }

            foreach (ObjetoAvatar avatar in avatares)
            {
                ImageSource imagenAvatar = CrearImagenParaAvatar(avatar);

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

        private static ImageSource CrearImagenParaAvatar(ObjetoAvatar avatar)
        {
            if (avatar == null)
            {
                return null;
            }

            if (!string.IsNullOrWhiteSpace(avatar.ImagenUriAbsoluta)
                && Uri.TryCreate(avatar.ImagenUriAbsoluta, UriKind.Absolute, out Uri uriRemota))
            {
                return new BitmapImage(uriRemota);
            }

            if (!string.IsNullOrWhiteSpace(avatar.RutaRelativa))
            {
                string rutaNormalizada = NormalizarRutaLocal(avatar.RutaRelativa);

                if (Uri.TryCreate($"pack://application:,,,/{rutaNormalizada}", UriKind.Absolute, out Uri uriRecurso))
                {
                    try
                    {
                        return new BitmapImage(uriRecurso);
                    }
                    catch
                    {
                        // Ignorado para intentar con la ruta relativa simple.
                    }
                }

                try
                {
                    return new BitmapImage(new Uri($"/{rutaNormalizada}", UriKind.Relative));
                }
                catch
                {
                    return null;
                }
            }

            return null;
        }

        private static string NormalizarRutaLocal(string ruta)
        {
            if (string.IsNullOrWhiteSpace(ruta))
            {
                return null;
            }

            string rutaNormalizada = ruta
                .TrimStart('/')
                .Replace('\\', '/');

            return rutaNormalizada;
        }

        private void BotonAceptarSeleccionAvatar(object sender, RoutedEventArgs e)
        {
            if (listaAvatares.SelectedItem is ListBoxItem itemSeleccionado && itemSeleccionado.Tag is ObjetoAvatar avatar)
            {
                AvatarSeleccionado = avatar;
                AvatarSeleccionadoImagen = CrearImagenParaAvatar(avatar);
                DialogResult = true;
                Close();
                return;
            }

            new Avisos(Lang.globalTextoSeleccionarAvatar).ShowDialog();
        }
    }
}
