using System.Data.Entity;
using System.Linq;
using Datos.DAL.Interfaces;
using Datos.Modelo;
using Datos.Utilidades;

namespace Datos.DAL.Implementaciones
{
    public class RedSocialRepositorio : IRedSocialRepositorio
    {
        public RedSocial ObtenerPorJugadorId(int jugadorId)
        {
            using (var contexto = new BaseDatosPruebaEntities1(Conexion.ObtenerConexion()))
            {
                return contexto.RedSocial
                    .AsNoTracking()
                    .FirstOrDefault(r => r.Jugador_idJugador == jugadorId);
            }
        }

        public bool GuardarRedSocial(
            int jugadorId,
            string instagram,
            string facebook,
            string x,
            string discord)
        {
            using (var contexto = new BaseDatosPruebaEntities1(Conexion.ObtenerConexion()))
            {
                var redSocial = contexto.RedSocial
                    .FirstOrDefault(r => r.Jugador_idJugador == jugadorId);

                if (redSocial == null)
                {
                    bool jugadorExiste = contexto.Jugador.Any(j => j.idJugador == jugadorId);

                    if (!jugadorExiste)
                    {
                        return false;
                    }

                    redSocial = new RedSocial
                    {
                        Jugador_idJugador = jugadorId
                    };

                    contexto.RedSocial.Add(redSocial);
                }

                redSocial.Instagram = instagram;
                redSocial.facebook = facebook;
                redSocial.x = x;
                redSocial.discord = discord;

                contexto.SaveChanges();

                return true;
            }
        }
    }
}
