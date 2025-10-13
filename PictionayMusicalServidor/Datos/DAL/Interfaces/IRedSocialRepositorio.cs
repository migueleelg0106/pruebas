using Datos.Modelo;

namespace Datos.DAL.Interfaces
{
    public interface IRedSocialRepositorio
    {
        RedSocial ObtenerPorJugadorId(int jugadorId);

        bool GuardarRedSocial(
            int jugadorId,
            string instagram,
            string facebook,
            string x,
            string discord);
    }
}
