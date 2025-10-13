using System.ServiceModel;
using Servicios.Contratos.DTOs;

namespace Servicios.Contratos
{
    [ServiceContract]
    public interface ICambiarContrasenaManejador
    {
        [OperationContract]
        [FaultContract(typeof(ErrorDetalleServicioDTO))]
        ResultadoSolicitudRecuperacionDTO SolicitarCodigoRecuperacion(SolicitudRecuperarCuentaDTO solicitud);

        [OperationContract]
        [FaultContract(typeof(ErrorDetalleServicioDTO))]
        ResultadoSolicitudCodigoDTO ReenviarCodigoRecuperacion(ReenviarCodigoDTO solicitud);

        [OperationContract]
        [FaultContract(typeof(ErrorDetalleServicioDTO))]
        ResultadoOperacionDTO ConfirmarCodigoRecuperacion(ConfirmarCodigoDTO confirmacion);

        [OperationContract]
        [FaultContract(typeof(ErrorDetalleServicioDTO))]
        ResultadoOperacionDTO ActualizarContrasena(ActualizarContrasenaDTO solicitud);
    }
}
