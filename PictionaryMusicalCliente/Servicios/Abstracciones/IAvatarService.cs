using System.Threading.Tasks;

namespace PictionaryMusicalCliente.Servicios.Abstracciones
{
    public interface IAvatarService
    {
        Task<int?> ObtenerIdPorRutaAsync(string rutaRelativa);
    }
}
