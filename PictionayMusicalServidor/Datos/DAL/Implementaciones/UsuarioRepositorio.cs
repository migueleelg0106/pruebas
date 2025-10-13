using Datos.DAL.Interfaces;
using Datos.Modelo;
using Datos.Utilidades;
using System.Data.Entity;
using System.Linq;

namespace Datos.DAL.Implementaciones
{
    public class UsuarioRepositorio : IUsuarioRepositorio
    {
        public bool ExisteUsuario(string usuario)
        {
            using (var contexto = new BaseDatosPruebaEntities1(Conexion.ObtenerConexion()))
            {
                return contexto.Usuario.Any(u => u.Nombre_Usuario == usuario);
            }
        }

        public Usuario CrearUsuario(int jugadorId, string usuario, string contrasenaHash)
        {
            using (var contexto = new BaseDatosPruebaEntities1(Conexion.ObtenerConexion()))
            {
                var entidad = new Usuario
                {
                    Nombre_Usuario = usuario,
                    Contrasena = contrasenaHash,
                    Jugador_idJugador = jugadorId
                };

                contexto.Usuario.Add(entidad);
                contexto.SaveChanges();

                return entidad;
            }
        }

        public Usuario ObtenerUsuarioPorIdentificador(string identificador)
        {
            using (var contexto = new BaseDatosPruebaEntities1(Conexion.ObtenerConexion()))
            {
                return contexto.Usuario
                    .Include(u => u.Jugador)
                    .FirstOrDefault(u =>
                        u.Nombre_Usuario == identificador ||
                        (u.Jugador != null && u.Jugador.Correo == identificador));
            }
        }

        public Usuario ObtenerUsuarioPorId(int idUsuario)
        {
            using (var contexto = new BaseDatosPruebaEntities1(Conexion.ObtenerConexion()))
            {
                return contexto.Usuario
                    .Include(u => u.Jugador)
                    .FirstOrDefault(u => u.idUsuario == idUsuario);
            }
        }

        public bool ActualizarContrasena(int idUsuario, string contrasenaHash)
        {
            using (var contexto = new BaseDatosPruebaEntities1(Conexion.ObtenerConexion()))
            {
                var usuario = contexto.Usuario.FirstOrDefault(u => u.idUsuario == idUsuario);

                if (usuario == null)
                {
                    return false;
                }

                usuario.Contrasena = contrasenaHash;
                return contexto.SaveChanges() > 0;
            }
        }
    }
}
