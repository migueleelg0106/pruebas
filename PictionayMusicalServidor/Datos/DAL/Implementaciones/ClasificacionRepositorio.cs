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
        private const int MaximoPorDefecto = 10;

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

        public IList<Usuario> ObtenerTopJugadores(int limite)
        {
            int cantidadSolicitada = limite <= 0 ? MaximoPorDefecto : limite;

            using (var contexto = new BaseDatosPruebaEntities1(Conexion.ObtenerConexion()))
            {
                return contexto.Usuario
                    .Include(u => u.Jugador.Clasificacion)
                    .Where(u => u.Jugador != null && u.Jugador.Clasificacion != null)
                    .OrderByDescending(u => u.Jugador.Clasificacion.Puntos_Ganados ?? 0)
                    .ThenByDescending(u => u.Jugador.Clasificacion.Rondas_Ganadas ?? 0)
                    .ThenBy(u => u.Nombre_Usuario)
                    .Take(cantidadSolicitada)
                    .ToList();
            }
        }
    }
}