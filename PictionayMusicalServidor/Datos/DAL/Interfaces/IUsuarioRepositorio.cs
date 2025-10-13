namespace Datos.DAL.Interfaces
{
    using Datos.Modelo;

    public interface IUsuarioRepositorio
    {
        bool ExisteUsuario(string usuario);

        Usuario CrearUsuario(int jugadorId, string usuario, string contrasenaHash);

        Usuario ObtenerUsuarioPorIdentificador(string identificador);

        Usuario ObtenerUsuarioPorId(int idUsuario);

        bool ActualizarContrasena(int idUsuario, string contrasenaHash);
    }
}
