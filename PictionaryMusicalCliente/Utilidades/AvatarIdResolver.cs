using System.Threading.Tasks;
using PictionaryMusicalCliente.Modelo;
using PictionaryMusicalCliente.Servicios.Abstracciones;
using PictionaryMusicalCliente.Servicios.Wcf.Helpers;

namespace PictionaryMusicalCliente.Utilidades
{
    public static class AvatarIdResolver
    {
        public static async Task<int?> ResolverIdAsync(ObjetoAvatar avatar, IAvatarService svc)
        {
            if (avatar == null || svc == null)
            {
                return null;
            }

            if (avatar.Id > 0)
            {
                return avatar.Id;
            }

            string rutaNormalizada = AvatarRutaHelper.NormalizarRutaRelativa(avatar.RutaRelativa);
            if (string.IsNullOrWhiteSpace(rutaNormalizada))
            {
                return null;
            }

            return await svc.ObtenerIdPorRutaAsync(rutaNormalizada).ConfigureAwait(true);
        }
    }
}
