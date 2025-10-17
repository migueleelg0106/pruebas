using System.Threading.Tasks;
using PictionaryMusicalCliente.Servicios.Abstracciones;

namespace PictionaryMusicalCliente.Servicios.Dialogos
{
    public class RecuperacionCuentaDialogService : IRecuperacionCuentaDialogService
    {
        public Task<bool> MostrarDialogoVerificacionAsync(
            string tokenCodigo,
            string correoDestino,
            FuncionesVerificarCodigo funcionesVerificacion,
            string descripcionPersonalizada)
        {
            var ventanaVerificacion = new global::PictionaryMusicalCliente.VerificarCodigo(
                tokenCodigo,
                correoDestino,
                funcionesVerificacion?.ConfirmarCodigo,
                funcionesVerificacion?.ReenviarCodigo,
                descripcionPersonalizada);

            bool? resultado = ventanaVerificacion.ShowDialog();
            return Task.FromResult(resultado == true && ventanaVerificacion.OperacionCompletada);
        }

        public Task<ResultadoCambioContrasena> MostrarDialogoCambioContrasenaAsync(string tokenCodigo, string identificador)
        {
            var ventanaCambio = new global::PictionaryMusicalCliente.CambioContrasena(tokenCodigo, identificador);
            bool? resultado = ventanaCambio.ShowDialog();
            return Task.FromResult(new ResultadoCambioContrasena(resultado, ventanaCambio.ContrasenaActualizada));
        }
    }
}
