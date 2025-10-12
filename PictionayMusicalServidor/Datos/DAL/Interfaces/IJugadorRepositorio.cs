using Datos.Modelo;

namespace Datos.DAL.Interfaces
{
    public interface IJugadorRepositorio
    {
        Jugador ObtenerPorId(int jugadorId);

        bool ActualizarPerfil(int jugadorId, string nombre, string apellido, int avatarId);
    }
}
