using System.Collections.Generic;
using System.Linq;

namespace PictionaryMusicalCliente.Modelo.Catalogos
{
    public static class CatalogoImagenesPerfilLocales
    {
        private static readonly IReadOnlyList<RedSocialPerfil> RedesSociales = new List<RedSocialPerfil>
        {
            Crear("Discord", "discord", "/Recursos/discord.png"),
            Crear("Facebook", "facebook", "/Recursos/facebook.png"),
            Crear("Instagram", "instagram", "/Recursos/instagram.png"),
            Crear("X", "x", "/Recursos/x_logo.png")
        };

        public static IReadOnlyList<RedSocialPerfil> ObtenerRedesSociales()
        {
            return RedesSociales.Select(red => red.Clonar()).ToList();
        }

        private static RedSocialPerfil Crear(string nombre, string clave, string rutaIcono)
        {
            return new RedSocialPerfil
            {
                Nombre = nombre,
                Clave = clave,
                RutaIcono = rutaIcono
            };
        }
    }
}
