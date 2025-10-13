using Servicios.Contratos.DTOs;
using System.ServiceModel;

namespace Servicios.Contratos
{
    [ServiceContract]
    public interface IInicioSesionManejador
    {
        [OperationContract]
        [FaultContract(typeof(ErrorDetalleServicioDTO))]
        ResultadoInicioSesionDTO IniciarSesion(CredencialesInicioSesionDTO credenciales);
    }
}
