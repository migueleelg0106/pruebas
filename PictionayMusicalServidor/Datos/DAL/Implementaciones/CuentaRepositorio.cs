using Datos.DAL.Interfaces;
using Datos.Modelo;
using Datos.Utilidades;
using System;
using System.Data.Entity;
using System.Linq;

namespace Datos.DAL.Implementaciones
{
    public class CuentaRepositorio : ICuentaRepositorio
    {
        public bool CreateAccount(string correo, string contrasenaHash, string usuario, string nombre, string apellido, int avatarId)
        {
            using (var contexto = new BaseDatosPruebaEntities1(Conexion.ObtenerConexion()))
            {
                var clasificacion = new Clasificacion { Puntos_Ganados = 0, Rondas_Ganadas = 0 };
                contexto.Clasificacion.Add(clasificacion);
                contexto.SaveChanges();

                var jugador = new Jugador
                {
                    Nombre = nombre,
                    Apellido = apellido,
                    Correo = correo,
                    Avatar_idAvatar = avatarId,
                    Clasificacion_idClasificacion = clasificacion.idClasificacion
                };
                contexto.Jugador.Add(jugador);
                contexto.SaveChanges();

                var usuarioEntidad = new Usuario
                {
                    Nombre_Usuario = usuario,
                    Contrasena = contrasenaHash,
                    Jugador_idJugador = jugador.idJugador
                };
                contexto.Usuario.Add(usuarioEntidad);
                contexto.SaveChanges();

                return true;
            }
        }

        public bool ExisteCorreo(string correo)
        {
            using (var contexto = new BaseDatosPruebaEntities1(Conexion.ObtenerConexion()))
            {
                return contexto.Jugador.Any(j => j.Correo == correo);
            }
        }

        public bool ExisteUsuario(string usuario)
        {
            using (var contexto = new BaseDatosPruebaEntities1(Conexion.ObtenerConexion()))
            {
                return contexto.Usuario.Any(u => u.Nombre_Usuario == usuario);
            }
        }

        public bool TryObtenerCuentaPorIdentificador(string identificador, out int idUsuario, out string correo)
        {
            idUsuario = 0;
            correo = null;

            if (string.IsNullOrWhiteSpace(identificador))
            {
                return false;
            }

            string filtro = identificador.Trim();

            using (var contexto = new BaseDatosPruebaEntities1(Conexion.ObtenerConexion()))
            {
                var usuario = contexto.Usuario
                    .Include(u => u.Jugador)
                    .FirstOrDefault(u => u.Nombre_Usuario == filtro || u.Jugador.Correo == filtro);

                if (usuario == null)
                {
                    return false;
                }

                bool coincideUsuario = string.Equals(usuario.Nombre_Usuario, filtro, StringComparison.Ordinal);
                bool coincideCorreo = string.Equals(usuario.Jugador?.Correo, filtro, StringComparison.Ordinal);

                if (!coincideUsuario && !coincideCorreo)
                {
                    return false;
                }

                idUsuario = usuario.idUsuario;
                correo = usuario.Jugador?.Correo;
                return true;
            }
        }

        public bool ActualizarContrasena(int idUsuario, string contrasenaHash)
        {
            if (idUsuario <= 0 || string.IsNullOrWhiteSpace(contrasenaHash))
            {
                return false;
            }

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
