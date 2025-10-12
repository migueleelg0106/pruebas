using System;
using System.Data.Entity;
using System.Linq;
using Datos.DAL.Interfaces;
using Datos.Modelo;
using Datos.Utilidades;

namespace Datos.DAL.Implementaciones
{
    public class JugadorRepositorio : IJugadorRepositorio
    {
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

                bool requiereActualizacion = false;

                if (!string.Equals(jugador.Nombre, nombre, StringComparison.Ordinal))
                {
                    jugador.Nombre = nombre;
                    requiereActualizacion = true;
                }

                if (!string.Equals(jugador.Apellido, apellido, StringComparison.Ordinal))
                {
                    jugador.Apellido = apellido;
                    requiereActualizacion = true;
                }

                if (jugador.Avatar_idAvatar != avatarId)
                {
                    jugador.Avatar_idAvatar = avatarId;
                    requiereActualizacion = true;
                }

                RedSocial redSocial = jugador.RedSocial?.FirstOrDefault();

                if (redSocial == null)
                {
                    redSocial = new RedSocial
                    {
                        Jugador_idJugador = jugadorId,
                        Jugador = jugador
                    };
                    contexto.RedSocial.Add(redSocial);
                    requiereActualizacion = true;
                }

                if (!string.Equals(redSocial.Instagram, instagram, StringComparison.Ordinal))
                {
                    redSocial.Instagram = instagram;
                    requiereActualizacion = true;
                }

                if (!string.Equals(redSocial.facebook, facebook, StringComparison.Ordinal))
                {
                    redSocial.facebook = facebook;
                    requiereActualizacion = true;
                }

                if (!string.Equals(redSocial.x, x, StringComparison.Ordinal))
                {
                    redSocial.x = x;
                    requiereActualizacion = true;
                }

                if (!string.Equals(redSocial.discord, discord, StringComparison.Ordinal))
                {
                    redSocial.discord = discord;
                    requiereActualizacion = true;
                }

                if (!requiereActualizacion)
                {
                    return true;
                }

                return contexto.SaveChanges() > 0;
            }
        }
    }
}
