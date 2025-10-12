using System.Collections.Generic;

namespace Datos.DAL.Interfaces
{
    public interface IClasificacionRepositorio
    {
        IList<ClasificacionJugadorInfo> ObtenerTopJugadores(int limite);
    }
}
