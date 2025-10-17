using System.Threading.Tasks;
using PictionaryMusicalCliente.Servicios.Abstracciones;

namespace PictionaryMusicalCliente.Servicios.Dialogos
{
    public class VerificarCodigoDialogService : IVerificarCodigoDialogService
    {
        public Task<bool> MostrarDialogoAsync(string tokenCodigo, string correoDestino)
        {
            var ventanaVerificacion = new global::PictionaryMusicalCliente.VerificarCodigo(tokenCodigo, correoDestino);
            bool? resultado = ventanaVerificacion.ShowDialog();
            return Task.FromResult(resultado == true && ventanaVerificacion.RegistroCompletado);
        }
    }
}
