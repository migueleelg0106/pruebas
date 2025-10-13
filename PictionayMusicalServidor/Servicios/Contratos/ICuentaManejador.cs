using Servicios.Contratos.DTOs;
using System.ServiceModel;

namespace Servicios.Contratos
{
    [ServiceContract]
    public interface ICuentaManejador
    {
        [OperationContract]
        [FaultContract(typeof(ErrorDetalleServicioDTO))]
        ResultadoRegistroCuentaDTO RegistrarCuenta(NuevaCuentaDTO nuevaCuenta);
    }
}
