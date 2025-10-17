using System;
using System.Threading.Tasks;

namespace PictionaryMusicalCliente.Servicios.Abstracciones
{
    public interface IRecuperacionCuentaDialogService
    {
        Task<bool> MostrarDialogoVerificacionAsync(
            string tokenCodigo,
            string correoDestino,
            FuncionesVerificarCodigo funcionesVerificacion,
            string descripcionPersonalizada);

        Task<ResultadoCambioContrasena> MostrarDialogoCambioContrasenaAsync(string tokenCodigo, string identificador);
    }

    public sealed class FuncionesVerificarCodigo
    {
        public FuncionesVerificarCodigo(
            Func<string, Task<global::PictionaryMusicalCliente.VerificarCodigo.ConfirmacionResultado>> confirmarCodigo,
            Func<Task<global::PictionaryMusicalCliente.VerificarCodigo.ReenvioResultado>> reenviarCodigo)
        {
            ConfirmarCodigo = confirmarCodigo;
            ReenviarCodigo = reenviarCodigo;
        }

        public Func<string, Task<global::PictionaryMusicalCliente.VerificarCodigo.ConfirmacionResultado>> ConfirmarCodigo { get; }

        public Func<Task<global::PictionaryMusicalCliente.VerificarCodigo.ReenvioResultado>> ReenviarCodigo { get; }
    }

    public sealed class ResultadoCambioContrasena
    {
        public ResultadoCambioContrasena(bool? dialogResult, bool contrasenaActualizada)
        {
            DialogResult = dialogResult;
            ContrasenaActualizada = contrasenaActualizada;
        }

        public bool? DialogResult { get; }

        public bool ContrasenaActualizada { get; }
    }
}
