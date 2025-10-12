using System;
using System.Collections.Generic;
using Datos.DAL.Implementaciones;
using Datos.DAL.Interfaces;
using Servicios.Contratos;
using Servicios.Contratos.DTOs;

namespace Servicios.Servicios
{
    public class ClasificacionManejador : IClasificacionManejador
    {
        private readonly IClasificacionRepositorio _repositorio;
        private const int MaximoElementos = 10;

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
            IList<ClasificacionJugadorInfo> jugadores = _repositorio.ObtenerTopJugadores(MaximoElementos);

            var resultado = new List<ClasificacionUsuarioDTO>(jugadores.Count);
            foreach (var jugador in jugadores)
            {
                resultado.Add(new ClasificacionUsuarioDTO
                {
                    Usuario = jugador.Usuario,
                    Puntos = jugador.Puntos,
                    RondasGanadas = jugador.Rondas
                });
            }

            return resultado;
        }
    }
}
