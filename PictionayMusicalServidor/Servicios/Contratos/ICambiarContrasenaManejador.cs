using System.ServiceModel;
using Servicios.Contratos.DTOs;

namespace Servicios.Contratos
{
    [ServiceContract]
    public interface ICambiarContrasenaManejador
    {
        [OperationContract]
        ResultadoSolicitudRecuperacionDTO SolicitarCodigoRecuperacion(SolicitudRecuperarCuentaDTO solicitud);

        [OperationContract]
        ResultadoSolicitudCodigoDTO ReenviarCodigoRecuperacion(SolicitudReenviarCodigoRecuperacionDTO solicitud);

        [OperationContract]
        ResultadoOperacionDTO ConfirmarCodigoRecuperacion(ConfirmarCodigoRecuperacionDTO confirmacion);

        [OperationContract]
        ResultadoOperacionDTO ActualizarContrasena(ActualizarContrasenaDTO solicitud);
    }
}
