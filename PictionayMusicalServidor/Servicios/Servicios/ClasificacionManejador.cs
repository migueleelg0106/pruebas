using System;
using System.Collections.Generic;
using System.Data;
using Datos.DAL.Implementaciones;
using Datos.DAL.Interfaces;
using Datos.Modelo;
using Servicios.Contratos;
using Servicios.Contratos.DTOs;
using log4net;

namespace Servicios.Servicios
{
    public class ClasificacionManejador : IClasificacionManejador
    {
        private readonly IClasificacionRepositorio _repositorio;
        private const int MaximoElementos = 10;
        private static readonly ILog Bitacora = LogManager.GetLogger(typeof(ClasificacionManejador));

        public ClasificacionManejador()
            : this(new ClasificacionRepositorio())
        {
        }

        public ClasificacionManejador(IClasificacionRepositorio repositorio)
        {
            _repositorio = repositorio ?? throw new ArgumentNullException(nameof(repositorio));
        }

        public List<ClasificacionUsuarioDTO> ObtenerTopJugadores()
        {
            Bitacora.Info("Solicitud para obtener la clasificación de jugadores recibida.");

            try
            {
                IList<Usuario> jugadores = _repositorio.ObtenerTopJugadores(MaximoElementos);

                var resultado = new List<ClasificacionUsuarioDTO>(jugadores.Count);
                foreach (var jugador in jugadores)
                {
                    var clasificacion = jugador.Jugador?.Clasificacion;

                    resultado.Add(new ClasificacionUsuarioDTO
                    {
                        Usuario = jugador.Nombre_Usuario,
                        Puntos = clasificacion?.Puntos_Ganados ?? 0,
                        RondasGanadas = clasificacion?.Rondas_Ganadas ?? 0
                    });
                }

                return resultado;
            }
            catch (ArgumentNullException ex)
            {
                Bitacora.Warn("Se recibió información inválida al calcular la clasificación.", ex);
                throw FabricaFallaServicio.Crear("SOLICITUD_INVALIDA", "No fue posible obtener la clasificación solicitada.", "Solicitud inválida.");
            }
            catch (InvalidOperationException ex)
            {
                Bitacora.Error("Operación inválida al consultar la clasificación de jugadores.", ex);
                throw FabricaFallaServicio.Crear("OPERACION_INVALIDA", "No fue posible recuperar la clasificación de jugadores.", "Operación inválida en la capa de datos.");
            }
            catch (DataException ex)
            {
                Bitacora.Error("Error en la base de datos al consultar la clasificación de jugadores.", ex);
                throw FabricaFallaServicio.Crear("ERROR_BASE_DATOS", "Ocurrió un problema al obtener la clasificación de jugadores.", "Fallo en la base de datos.");
            }
            catch (Exception ex)
            {
                Bitacora.Fatal("Error inesperado al obtener la clasificación de jugadores.", ex);
                throw FabricaFallaServicio.Crear("ERROR_NO_CONTROLADO", "Ocurrió un error inesperado al obtener la clasificación.", "Error interno del servidor.");
            }
        }
    }
}
