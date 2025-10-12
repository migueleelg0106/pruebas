using System.Collections.Generic;
using System.Linq;

namespace PictionaryMusicalCliente.Modelo.Catalogos
{
    public static class CatalogoImagenesPerfilLocales
    {
        private static readonly IReadOnlyList<RedSocialPerfil> RedesSociales = new List<RedSocialPerfil>
        {
            Crear("Discord", "/Recursos/discord.png"),
            Crear("Facebook", "/Recursos/facebook.png"),
            Crear("Instagram", "/Recursos/instagram.png"),
            Crear("X", "/Recursos/x_logo.png")
        };

        public static IReadOnlyList<RedSocialPerfil> ObtenerRedesSociales()
        {
            return RedesSociales.Select(red => red.Clonar()).ToList();
        }

        private static RedSocialPerfil Crear(string nombre, string rutaIcono)
        {
            return new RedSocialPerfil
            {
                Nombre = nombre,
                RutaIcono = rutaIcono
            };
        }
    }
}
