using System.ServiceModel;
using Servicios.Contratos.DTOs;

namespace Servicios.Contratos
{
    [ServiceContract]
    public interface IPerfilManejador
    {
        [OperationContract]
        [FaultContract(typeof(ServiceErrorDetailDTO))]
        UsuarioDTO ObtenerPerfil(int idUsuario);

        [OperationContract]
        [FaultContract(typeof(ServiceErrorDetailDTO))]
        ResultadoOperacionDTO ActualizarPerfil(ActualizarPerfilDTO solicitud);
    }
}
