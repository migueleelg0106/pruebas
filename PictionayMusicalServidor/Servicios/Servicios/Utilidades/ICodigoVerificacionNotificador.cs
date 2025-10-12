using System.Threading.Tasks;

namespace Servicios.Servicios.Utilidades
{
    public interface ICodigoVerificacionNotificador
    {
        Task EnviarCodigoAsync(string correoDestino, string codigoGenerado);
    }
}
