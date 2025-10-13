using Servicios.Contratos.DTOs;
using System.ServiceModel;

namespace Servicios.Contratos
{
    [ServiceContract]
    public interface IReenviarCodigoVerificacionManejador
    {
        [OperationContract]
        [FaultContract(typeof(ErrorDetalleServicioDTO))]
        ResultadoSolicitudCodigoDTO ReenviarCodigoVerificacion(ReenviarCodigoDTO solicitud);

        [OperationContract]
        [FaultContract(typeof(ErrorDetalleServicioDTO))]
        ResultadoSolicitudCodigoDTO ReenviarCodigoRecuperacion(ReenviarCodigoDTO solicitud);
    }
}
