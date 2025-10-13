using System.Collections.Generic;
using Datos.Modelo;

namespace Datos.DAL.Interfaces
{
    public interface IClasificacionRepositorio
    {
        Clasificacion CrearClasificacionInicial();

        IList<ClasificacionJugadorInfo> ObtenerTopJugadores(int limite);
    }
}
