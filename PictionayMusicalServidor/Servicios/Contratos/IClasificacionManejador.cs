using Servicios.Contratos.DTOs;
using System.Collections.Generic;
using System.ServiceModel;

namespace Servicios.Contratos
{
    [ServiceContract]
    public interface IClasificacionManejador
    {
        [OperationContract]
        List<ClasificacionUsuarioDTO> ObtenerTopJugadores();
    }
}
