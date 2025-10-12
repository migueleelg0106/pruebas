using System;
using Datos.DAL.Implementaciones;
using Datos.DAL.Interfaces;
using Datos.Modelo;
using Servicios.Contratos;
using Servicios.Contratos.DTOs;

namespace Servicios.Servicios
{
    public class JugadoresManejador : IJugadoresManejador
    {
        private const int LongitudMaximaNombre = 50;

        private readonly IUsuarioRepositorio _usuarioRepositorio;
        private readonly IJugadorRepositorio _jugadorRepositorio;
        private readonly IAvatarRepositorio _avatarRepositorio;

        public JugadoresManejador()
            : this(new UsuarioRepositorio(), new JugadorRepositorio(), new AvatarRepositorio())
        {
        }

        public JugadoresManejador(IUsuarioRepositorio usuarioRepositorio, IJugadorRepositorio jugadorRepositorio, IAvatarRepositorio avatarRepositorio)
        {
            _usuarioRepositorio = usuarioRepositorio ?? throw new ArgumentNullException(nameof(usuarioRepositorio));
            _jugadorRepositorio = jugadorRepositorio ?? throw new ArgumentNullException(nameof(jugadorRepositorio));
            _avatarRepositorio = avatarRepositorio ?? throw new ArgumentNullException(nameof(avatarRepositorio));
        }

        public UsuarioDTO ObtenerPerfil(int idUsuario)
        {
            if (idUsuario <= 0)
            {
                return null;
            }

            Usuario usuario = _usuarioRepositorio.ObtenerUsuarioPorId(idUsuario);

            if (usuario == null)
            {
                return null;
            }

            Jugador jugador = usuario.Jugador;

            return new UsuarioDTO
            {
                IdUsuario = usuario.idUsuario,
                JugadorId = usuario.Jugador_idJugador,
                NombreUsuario = usuario.Nombre_Usuario,
                Nombre = jugador?.Nombre,
                Apellido = jugador?.Apellido,
                Correo = jugador?.Correo,
                AvatarId = jugador?.Avatar_idAvatar ?? 0
            };
        }

        public ResultadoOperacionDTO ActualizarPerfil(ActualizarPerfilDTO solicitud)
        {
            if (solicitud == null)
            {
                return CrearResultado(false, "La solicitud de actualización es obligatoria.");
            }

            if (solicitud.UsuarioId <= 0)
            {
                return CrearResultado(false, "El identificador de usuario es inválido.");
            }

            string nombreNormalizado = solicitud.Nombre?.Trim();
            string apellidoNormalizado = solicitud.Apellido?.Trim();

            if (!EsTextoValido(nombreNormalizado))
            {
                return CrearResultado(false, "El nombre es obligatorio y no debe exceder 50 caracteres.");
            }

            if (!EsTextoValido(apellidoNormalizado))
            {
                return CrearResultado(false, "El apellido es obligatorio y no debe exceder 50 caracteres.");
            }

            if (solicitud.AvatarId <= 0)
            {
                return CrearResultado(false, "Selecciona un avatar válido.");
            }

            Usuario usuario = _usuarioRepositorio.ObtenerUsuarioPorId(solicitud.UsuarioId);

            if (usuario == null)
            {
                return CrearResultado(false, "No se encontró el usuario especificado.");
            }

            if (usuario.Jugador == null)
            {
                return CrearResultado(false, "No existe un jugador asociado al usuario especificado.");
            }

            if (!_avatarRepositorio.ExisteAvatar(solicitud.AvatarId))
            {
                return CrearResultado(false, "El avatar seleccionado no existe.");
            }

            int jugadorId = usuario.Jugador_idJugador;

            bool actualizado = _jugadorRepositorio.ActualizarPerfil(jugadorId, nombreNormalizado, apellidoNormalizado, solicitud.AvatarId);

            if (!actualizado)
            {
                return CrearResultado(false, "No fue posible actualizar el perfil.");
            }

            return CrearResultado(true, "Perfil actualizado correctamente.");
        }

        private static bool EsTextoValido(string valor)
        {
            return !string.IsNullOrWhiteSpace(valor) && valor.Length <= LongitudMaximaNombre;
        }

        private static ResultadoOperacionDTO CrearResultado(bool exito, string mensaje)
        {
            return new ResultadoOperacionDTO
            {
                OperacionExitosa = exito,
                Mensaje = mensaje
            };
        }
    }
}
