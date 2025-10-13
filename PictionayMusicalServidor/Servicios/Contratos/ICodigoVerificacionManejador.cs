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
        ResultadoRegistroCuentaDTO ConfirmarCodigoVerificacion(ConfirmarCodigoVerificacionDTO confirmacion);
    }
}
