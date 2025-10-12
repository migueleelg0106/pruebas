using System.ServiceModel;
using Servicios.Contratos.DTOs;

namespace Servicios.Contratos
{
    [ServiceContract]
    public interface IJugadoresManejador
    {
        [OperationContract]
        UsuarioDTO ObtenerPerfil(int idUsuario);

        [OperationContract]
        ResultadoOperacionDTO ActualizarPerfil(ActualizarPerfilDTO solicitud);
    }
}
