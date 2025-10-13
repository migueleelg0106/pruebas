using System.Data.Entity;
using System.Linq;
using Datos.DAL.Interfaces;
using Datos.Modelo;
using Datos.Utilidades;

namespace Datos.DAL.Implementaciones
{
    public class JugadorRepositorio : IJugadorRepositorio
    {
        public bool ExisteCorreo(string correo)
        {
            using (var contexto = new BaseDatosPruebaEntities1(Conexion.ObtenerConexion()))
            {
                return contexto.Jugador.Any(j => j.Correo == correo);
            }
        }

        public Jugador CrearJugador(
            string nombre,
            string apellido,
            string correo,
            int avatarId,
            int clasificacionId)
        {
            using (var contexto = new BaseDatosPruebaEntities1(Conexion.ObtenerConexion()))
            {
                var jugador = new Jugador
                {
                    Nombre = nombre,
                    Apellido = apellido,
                    Correo = correo,
                    Avatar_idAvatar = avatarId,
                    Clasificacion_idClasificacion = clasificacionId
                };

                contexto.Jugador.Add(jugador);
                contexto.SaveChanges();

                return jugador;
            }
        }

        public Jugador ObtenerPorId(int jugadorId)
        {
            using (var contexto = new BaseDatosPruebaEntities1(Conexion.ObtenerConexion()))
            {
                return contexto.Jugador
                    .AsNoTracking()
                    .FirstOrDefault(j => j.idJugador == jugadorId);
            }
        }

        public bool ActualizarPerfil(
            int jugadorId,
            string nombre,
            string apellido,
            int avatarId,
            string instagram,
            string facebook,
            string x,
            string discord)
        {
            using (var contexto = new BaseDatosPruebaEntities1(Conexion.ObtenerConexion()))
            {
                var jugador = contexto.Jugador
                    .Include(j => j.RedSocial)
                    .FirstOrDefault(j => j.idJugador == jugadorId);

                if (jugador == null)
                {
                    return false;
                }

                jugador.Nombre = nombre;
                jugador.Apellido = apellido;
                jugador.Avatar_idAvatar = avatarId;

                RedSocial redSocial = jugador.RedSocial?.FirstOrDefault();

                if (redSocial == null)
                {
                    redSocial = new RedSocial
                    {
                        Jugador_idJugador = jugadorId,
                        Jugador = jugador
                    };

                    contexto.RedSocial.Add(redSocial);
                }

                redSocial.Instagram = instagram;
                redSocial.facebook = facebook;
                redSocial.x = x;
                redSocial.discord = discord;

                return contexto.SaveChanges() > 0;
            }
        }
    }
}
