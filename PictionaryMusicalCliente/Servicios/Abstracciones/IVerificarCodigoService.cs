using System.Threading.Tasks;
using PictionaryMusicalCliente.Modelo.Cuentas;

namespace PictionaryMusicalCliente.Servicios.Abstracciones
{
    public interface IVerificarCodigoService
    {
        Task<ConfirmacionCodigoResultado> ConfirmarCodigoRegistroAsync(string tokenCodigo, string codigoIngresado);

        Task<ReenvioCodigoResultado> ReenviarCodigoRegistroAsync(string tokenCodigo);
    }
}
