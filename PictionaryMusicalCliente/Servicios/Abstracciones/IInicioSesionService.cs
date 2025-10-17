using System.Threading.Tasks;
using InicioSesionSrv = PictionaryMusicalCliente.PictionaryServidorServicioInicioSesion;

namespace PictionaryMusicalCliente.Servicios.Abstracciones
{
    public interface IInicioSesionService
    {
        Task<InicioSesionSrv.ResultadoInicioSesionDTO> IniciarSesionAsync(InicioSesionSrv.CredencialesInicioSesionDTO credenciales);
    }
}
