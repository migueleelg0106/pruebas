using System.Threading.Tasks;

namespace PictionaryMusicalCliente.Servicios.Abstracciones
{
    public interface IVerificarCodigoDialogService
    {
        Task<bool> MostrarDialogoAsync(string tokenCodigo, string correoDestino);
    }
}
