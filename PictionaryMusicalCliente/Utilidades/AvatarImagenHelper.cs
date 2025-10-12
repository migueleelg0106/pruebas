using System;
using System.IO;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using PictionaryMusicalCliente.Modelo;

namespace PictionaryMusicalCliente.Utilidades
{
    public static class AvatarImagenHelper
    {
        public static ImageSource CrearImagen(ObjetoAvatar avatar)
        {
            if (avatar == null)
            {
                return null;
            }

            if (!string.IsNullOrWhiteSpace(avatar.ImagenUriAbsoluta)
                && Uri.TryCreate(avatar.ImagenUriAbsoluta, UriKind.Absolute, out Uri uriRemota))
            {
                ImageSource imagenRemota = CrearBitmap(uriRemota);

                if (imagenRemota != null)
                {
                    return imagenRemota;
                }
            }

            if (string.IsNullOrWhiteSpace(avatar.RutaRelativa))
            {
                return null;
            }

            string rutaNormalizada = avatar.RutaRelativa.TrimStart('/').Replace('\\', '/');

            if (Uri.TryCreate($"pack://application:,,,/{rutaNormalizada}", UriKind.Absolute, out Uri uriRecurso))
            {
                ImageSource imagenRecurso = CrearBitmap(uriRecurso);

                if (imagenRecurso != null)
                {
                    return imagenRecurso;
                }
            }

            if (Uri.TryCreate($"/{rutaNormalizada}", UriKind.Relative, out Uri uriRelativa))
            {
                return CrearBitmap(uriRelativa);
            }

            return null;
        }

        private static ImageSource CrearBitmap(Uri origen)
        {
            try
            {
                var imagen = new BitmapImage();
                imagen.BeginInit();
                imagen.UriSource = origen;
                imagen.CacheOption = BitmapCacheOption.OnLoad;
                imagen.EndInit();

                if (imagen.CanFreeze)
                {
                    imagen.Freeze();
                }
                return imagen;
            }
            catch (IOException)
            {
                return null;
            }
            catch (NotSupportedException)
            {
                return null;
            }
            catch (ArgumentException)
            {
                return null;
            }
            catch (UriFormatException)
            {
                return null;
            }
        }
    }
}
