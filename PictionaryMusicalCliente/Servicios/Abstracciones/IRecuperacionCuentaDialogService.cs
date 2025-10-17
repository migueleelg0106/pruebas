using System;
using System.Threading.Tasks;
using PictionaryMusicalCliente.Modelo.Cuentas;

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
            Func<string, Task<PictionaryMusicalCliente.Modelo.Cuentas.ConfirmacionCodigoResultado>> confirmarCodigo,
            Func<Task<PictionaryMusicalCliente.Modelo.Cuentas.ReenvioCodigoResultado>> reenviarCodigo)
        {
            ConfirmarCodigo = confirmarCodigo;
            ReenviarCodigo = reenviarCodigo;
        }

        public Func<string, Task<ConfirmacionCodigoResultado>> ConfirmarCodigo { get; }

        public Func<Task<ReenvioCodigoResultado>> ReenviarCodigo { get; }
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
