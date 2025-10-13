using Servicios.Contratos.DTOs;
using System.ServiceModel;

namespace Servicios.Contratos
{
    [ServiceContract]
    public interface ICodigoVerificacionManejador
    {
        [OperationContract]
        [FaultContract(typeof(ErrorDetalleServicioDTO))]
        ResultadoSolicitudCodigoDTO SolicitarCodigoVerificacion(NuevaCuentaDTO nuevaCuenta);

        [OperationContract]
        [FaultContract(typeof(ErrorDetalleServicioDTO))]
        ResultadoRegistroCuentaDTO ConfirmarCodigoVerificacion(ConfirmarCodigoDTO confirmacion);

        [OperationContract]
        [FaultContract(typeof(ErrorDetalleServicioDTO))]
        ResultadoSolicitudRecuperacionDTO SolicitarCodigoRecuperacion(SolicitudRecuperarCuentaDTO solicitud);

        [OperationContract]
        [FaultContract(typeof(ErrorDetalleServicioDTO))]
        ResultadoOperacionDTO ConfirmarCodigoRecuperacion(ConfirmarCodigoDTO confirmacion);
    }
}
