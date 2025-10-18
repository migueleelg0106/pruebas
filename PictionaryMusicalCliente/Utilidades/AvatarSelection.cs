using System;
using System.Threading.Tasks;
using System.Windows.Media;
using PictionaryMusicalCliente.Modelo;
using PictionaryMusicalCliente.Servicios.Abstracciones;

namespace PictionaryMusicalCliente.Utilidades
{
    public sealed class AvatarSelection
    {
        private readonly Func<ObjetoAvatar, ObjetoAvatar> _sincronizar;

        public AvatarSelection(Func<ObjetoAvatar, ObjetoAvatar> sincronizar = null)
        {
            _sincronizar = sincronizar;
        }

        public ObjetoAvatar Avatar { get; private set; }

        public ImageSource Imagen { get; private set; }

        public string Nombre => Avatar?.Nombre ?? string.Empty;

        public void Set(ObjetoAvatar avatar)
        {
            if (_sincronizar != null)
            {
                avatar = _sincronizar(avatar);
            }

            Avatar = avatar;
            Imagen = AvatarImagenHelper.CrearImagen(Avatar);
        }

        public Task<int?> ObtenerIdAsync(IAvatarService svc)
        {
            return AvatarIdResolver.ResolverIdAsync(Avatar, svc);
        }
    }
}
