using Datos.DAL.Interfaces;
using Datos.Modelo;
using Datos.Utilidades;
using System;
using System.Data.Entity;
using System.Linq;

namespace Datos.DAL.Implementaciones
{
    public class UsuarioRepositorio : IUsuarioRepositorio
    {
        public Usuario ObtenerUsuarioPorIdentificador(string identificador)
        {
            string identificadorNormalizado = identificador?.Trim();

            using (var contexto = new BaseDatosPruebaEntities1(Conexion.ObtenerConexion()))
            {
                Usuario usuario = contexto.Usuario
                    .Include(u => u.Jugador.RedSocial)
                    .FirstOrDefault(u => u.Nombre_Usuario == identificadorNormalizado);

                if (usuario != null)
                {
                    if (string.Equals(usuario.Nombre_Usuario, identificadorNormalizado, StringComparison.Ordinal))
                    {
                        return usuario;
                    }

                    return null;
                }

                Usuario usuarioPorCorreo = contexto.Usuario
                    .Include(u => u.Jugador.RedSocial)
                    .FirstOrDefault(u => u.Jugador.Correo == identificadorNormalizado);

                if (usuarioPorCorreo != null &&
                    string.Equals(usuarioPorCorreo.Jugador?.Correo, identificadorNormalizado, StringComparison.Ordinal))
                {
                    return usuarioPorCorreo;
                }

                return null;
            }
        }

        public Usuario ObtenerUsuarioPorId(int idUsuario)
        {
            if (idUsuario <= 0)
            {
                return null;
            }

            using (var contexto = new BaseDatosPruebaEntities1(Conexion.ObtenerConexion()))
            {
                return contexto.Usuario
                    .Include(u => u.Jugador.RedSocial)
                    .FirstOrDefault(u => u.idUsuario == idUsuario);
            }
        }
    }
}
