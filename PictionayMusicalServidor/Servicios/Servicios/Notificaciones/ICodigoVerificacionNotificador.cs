using System.Threading.Tasks;

namespace Servicios.Servicios.Notificaciones
{
    public interface ICodigoVerificacionNotificador
    {
        Task EnviarCodigoAsync(string correoDestino, string codigoGenerado);
    }
}
