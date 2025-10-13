using Datos.Modelo;

namespace Datos.DAL.Interfaces
{
    public interface IJugadorRepositorio
    {
        bool ExisteCorreo(string correo);

        Jugador CrearJugador(
            string nombre,
            string apellido,
            string correo,
            int avatarId,
            int clasificacionId);

        Jugador ObtenerPorId(int jugadorId);

        bool ActualizarPerfil(
            int jugadorId,
            string nombre,
            string apellido,
            int avatarId);
    }
}
