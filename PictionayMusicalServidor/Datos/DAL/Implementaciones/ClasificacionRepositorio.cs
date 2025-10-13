using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using Datos.DAL.Interfaces;
using Datos.Modelo;
using Datos.Utilidades;

namespace Datos.DAL.Implementaciones
{
    public class ClasificacionRepositorio : IClasificacionRepositorio
    {
        public Clasificacion CrearClasificacionInicial()
        {
            using (var contexto = new BaseDatosPruebaEntities1(Conexion.ObtenerConexion()))
            {
                var clasificacion = new Clasificacion
                {
                    Puntos_Ganados = 0,
                    Rondas_Ganadas = 0
                };

                contexto.Clasificacion.Add(clasificacion);
                contexto.SaveChanges();

                return clasificacion;
            }
        }

        public IList<ClasificacionJugadorInfo> ObtenerTopJugadores(int limite)
        {
            using (var contexto = new BaseDatosPruebaEntities1(Conexion.ObtenerConexion()))
            {
                return contexto.Usuario
                    .Include(u => u.Jugador.Clasificacion)
                    .Where(u => u.Jugador != null && u.Jugador.Clasificacion != null)
                    .Select(u => new ClasificacionJugadorInfo
                    {
                        Usuario = u.Nombre_Usuario,
                        Puntos = u.Jugador.Clasificacion.Puntos_Ganados ?? 0,
                        Rondas = u.Jugador.Clasificacion.Rondas_Ganadas ?? 0
                    })
                    .OrderByDescending(c => c.Puntos)
                    .ThenByDescending(c => c.Rondas)
                    .ThenBy(c => c.Usuario)
                    .Take(limite)
                    .ToList();
            }
        }
    }
}
