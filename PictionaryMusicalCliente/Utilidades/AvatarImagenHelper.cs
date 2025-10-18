using System;
using System.IO;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using PictionaryMusicalCliente.Modelo;
using PictionaryMusicalCliente.Servicios.Wcf.Helpers;

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

            string rutaNormalizada = AvatarRutaHelper.NormalizarRutaRelativa(avatar.RutaRelativa);
            if (string.IsNullOrWhiteSpace(rutaNormalizada))
            {
                return null;
            }

            ImageSource imagen = CrearImagenDesdePack(rutaNormalizada);
            if (imagen != null)
            {
                return imagen;
            }

            if (Uri.TryCreate($"/{rutaNormalizada}", UriKind.Relative, out Uri uriRelativa))
            {
                return CrearBitmap(uriRelativa);
            }

            return null;
        }

        private static ImageSource CrearImagenDesdePack(string rutaNormalizada)
        {
            if (string.IsNullOrWhiteSpace(rutaNormalizada))
            {
                return null;
            }

            // Primero intentamos con el nombre del ensamblado para asegurar la resolución del recurso
            string nombreEnsamblado = typeof(AvatarImagenHelper).Assembly.GetName().Name;
            if (!string.IsNullOrWhiteSpace(nombreEnsamblado))
            {
                string rutaConEnsamblado = $"pack://application:,,,/{nombreEnsamblado};component/{rutaNormalizada}";
                if (Uri.TryCreate(rutaConEnsamblado, UriKind.Absolute, out Uri uriConEnsamblado))
                {
                    ImageSource imagenConEnsamblado = CrearBitmap(uriConEnsamblado);
                    if (imagenConEnsamblado != null)
                    {
                        return imagenConEnsamblado;
                    }
                }
            }

            // Como alternativa se deja el formato original por compatibilidad
            string rutaSinEnsamblado = $"pack://application:,,,/{rutaNormalizada}";
            if (Uri.TryCreate(rutaSinEnsamblado, UriKind.Absolute, out Uri uriSinEnsamblado))
            {
                return CrearBitmap(uriSinEnsamblado);
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
