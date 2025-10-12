namespace Datos.DAL.Interfaces
{
    using Datos.Modelo;

    public interface IUsuarioRepositorio
    {
        Usuario ObtenerUsuarioPorIdentificador(string identificador);

        Usuario ObtenerUsuarioPorId(int idUsuario);
    }
}
