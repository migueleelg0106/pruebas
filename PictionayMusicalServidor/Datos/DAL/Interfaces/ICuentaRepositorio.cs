namespace Datos.DAL.Interfaces
{
    public interface ICuentaRepositorio
    {
        bool CreateAccount(string correo, string contrasenaHash,
                           string usuario, string nombre, string apellido,
                           int avatarId);

        bool ExisteCorreo(string correo);

        bool ExisteUsuario(string usuario);

        bool TryObtenerCuentaPorIdentificador(string identificador, out int idUsuario, out string correo);

        bool ActualizarContrasena(int idUsuario, string contrasenaHash);
    }
}
