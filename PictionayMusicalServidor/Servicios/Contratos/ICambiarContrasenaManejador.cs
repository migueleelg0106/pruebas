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
        ResultadoSolicitudCodigoDTO ReenviarCodigoRecuperacion(SolicitudReenviarCodigoRecuperacionDTO solicitud);

        [OperationContract]
        [FaultContract(typeof(ErrorDetalleServicioDTO))]
        ResultadoOperacionDTO ConfirmarCodigoRecuperacion(ConfirmarCodigoRecuperacionDTO confirmacion);

        [OperationContract]
        [FaultContract(typeof(ErrorDetalleServicioDTO))]
        ResultadoOperacionDTO ActualizarContrasena(ActualizarContrasenaDTO solicitud);
    }
}
