using System.Threading.Tasks;
using PictionaryMusicalCliente.Modelo;

namespace PictionaryMusicalCliente.Servicios.Abstracciones
{
    public interface ICambioContrasenaService
    {
        Task<ResultadoOperacion> ActualizarContrasenaAsync(string tokenCodigo, string nuevaContrasena);
    }
}
