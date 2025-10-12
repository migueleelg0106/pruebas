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

        public bool ActualizarPerfil(int jugadorId, string nombre, string apellido, int avatarId)
        {
            using (var contexto = new BaseDatosPruebaEntities1(Conexion.ObtenerConexion()))
            {
                var jugador = contexto.Jugador.FirstOrDefault(j => j.idJugador == jugadorId);

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

                if (!requiereActualizacion)
                {
                    return true;
                }

                return contexto.SaveChanges() > 0;
            }
        }
    }
}
