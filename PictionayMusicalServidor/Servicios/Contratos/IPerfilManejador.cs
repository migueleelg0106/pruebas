using System.ServiceModel;
using Servicios.Contratos.DTOs;

namespace Servicios.Contratos
{
    [ServiceContract]
    public interface IPerfilManejador
    {
        [OperationContract]
        [FaultContract(typeof(ErrorDetalleServicioDTO))]
        UsuarioDTO ObtenerPerfil(int idUsuario);

        [OperationContract]
        [FaultContract(typeof(ErrorDetalleServicioDTO))]
        ResultadoOperacionDTO ActualizarPerfil(ActualizarPerfilDTO solicitud);
    }
}
