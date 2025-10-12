using System.Collections.Generic;

namespace Datos.DAL.Interfaces
{
    public class ClasificacionJugadorInfo
    {
        public string Usuario { get; set; }
        public int Puntos { get; set; }
        public int Rondas { get; set; }
    }

    public interface IClasificacionRepositorio
    {
        IList<ClasificacionJugadorInfo> ObtenerTopJugadores(int limite);
    }
}
