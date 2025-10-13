using Servicios.Contratos.DTOs;
using System.Collections.Generic;
using System.ServiceModel;

namespace Servicios.Contratos
{
    [ServiceContract]
    public interface IClasificacionManejador
    {
        [OperationContract]
        [FaultContract(typeof(ErrorDetalleServicioDTO))]
        List<ClasificacionUsuarioDTO> ObtenerTopJugadores();
    }
}
