using System;
using System.Data;
using System.Linq;
using System.ServiceModel;
using Datos.DAL.Implementaciones;
using Datos.DAL.Interfaces;
using Datos.Modelo;
using Servicios.Contratos;
using Servicios.Contratos.DTOs;
using log4net;

namespace Servicios.Servicios
{
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.PerCall)]
    public class PerfilManejador : IPerfilManejador
    {
        private const int LongitudMaximaNombre = 50;
        private const int LongitudMaximaRedSocial = 50;

        private readonly IUsuarioRepositorio _usuarioRepositorio;
        private readonly IJugadorRepositorio _jugadorRepositorio;
        private readonly IAvatarRepositorio _avatarRepositorio;
        private static readonly ILog Bitacora = LogManager.GetLogger(typeof(PerfilManejador));

        public PerfilManejador()
            : this(new UsuarioRepositorio(), new JugadorRepositorio(), new AvatarRepositorio())
        {
        }

        public PerfilManejador(
            IUsuarioRepositorio usuarioRepositorio,
            IJugadorRepositorio jugadorRepositorio,
            IAvatarRepositorio avatarRepositorio)
        {
            _usuarioRepositorio = usuarioRepositorio ?? throw new ArgumentNullException(nameof(usuarioRepositorio));
            _jugadorRepositorio = jugadorRepositorio ?? throw new ArgumentNullException(nameof(jugadorRepositorio));
            _avatarRepositorio = avatarRepositorio ?? throw new ArgumentNullException(nameof(avatarRepositorio));
        }

        public UsuarioDTO ObtenerPerfil(int idUsuario)
        {
            Bitacora.InfoFormat("Solicitud para obtener el perfil del usuario con identificador {0}.", idUsuario);

            try
            {
                if (idUsuario <= 0)
                {
                    Bitacora.Warn("Se recibió un identificador de usuario inválido para consultar el perfil.");
                    return null;
                }

                Usuario usuario = _usuarioRepositorio.ObtenerUsuarioPorId(idUsuario);

                if (usuario == null)
                {
                    Bitacora.WarnFormat("No se encontró información de usuario para el identificador {0}.", idUsuario);
                    return null;
                }

                Jugador jugador = usuario.Jugador;
                RedSocial redSocial = jugador?.RedSocial?.FirstOrDefault();

                return new UsuarioDTO
                {
                    IdUsuario = usuario.idUsuario,
                    JugadorId = usuario.Jugador_idJugador,
                    NombreUsuario = usuario.Nombre_Usuario,
                    Nombre = jugador?.Nombre,
                    Apellido = jugador?.Apellido,
                    Correo = jugador?.Correo,
                    AvatarId = jugador?.Avatar_idAvatar ?? 0,
                    Instagram = redSocial?.Instagram,
                    Facebook = redSocial?.facebook,
                    X = redSocial?.x,
                    Discord = redSocial?.discord
                };
            }
            catch (ArgumentNullException ex)
            {
                Bitacora.Warn("La información necesaria para obtener el perfil es nula.", ex);
                throw FabricaFallaServicio.Crear("SOLICITUD_INVALIDA", "Los datos proporcionados no son válidos para obtener el perfil.", "Solicitud inválida.");
            }
            catch (InvalidOperationException ex)
            {
                Bitacora.Error("Se produjo una operación inválida al consultar el perfil.", ex);
                throw FabricaFallaServicio.Crear("OPERACION_INVALIDA", "No fue posible obtener el perfil del usuario.", "Operación inválida en la capa de datos.");
            }
            catch (DataException ex)
            {
                Bitacora.Error("Ocurrió un error en la base de datos al obtener el perfil.", ex);
                throw FabricaFallaServicio.Crear("ERROR_BASE_DATOS", "Ocurrió un problema al consultar la información del perfil.", "Fallo en la base de datos.");
            }
            catch (Exception ex)
            {
                Bitacora.Fatal("Error inesperado al obtener el perfil del usuario.", ex);
                throw FabricaFallaServicio.Crear("ERROR_NO_CONTROLADO", "Ocurrió un error inesperado al obtener el perfil.", "Error interno del servidor.");
            }
        }

        public ResultadoOperacionDTO ActualizarPerfil(ActualizarPerfilDTO solicitud)
        {
            Bitacora.InfoFormat("Solicitud para actualizar el perfil del usuario {0}.", solicitud?.UsuarioId);

            if (solicitud == null)
            {
                Bitacora.Warn("Se recibió una solicitud de actualización de perfil nula.");
                return CrearResultado(false, "La solicitud de actualización es obligatoria.");
            }

            if (solicitud.UsuarioId <= 0)
            {
                Bitacora.Warn("El identificador de usuario proporcionado es inválido.");
                return CrearResultado(false, "El identificador de usuario es inválido.");
            }

            string nombreNormalizado = solicitud.Nombre?.Trim();
            string apellidoNormalizado = solicitud.Apellido?.Trim();

            if (!EsTextoValido(nombreNormalizado))
            {
                Bitacora.Warn("El nombre proporcionado no supera las validaciones establecidas.");
                return CrearResultado(false, "El nombre es obligatorio y no debe exceder 50 caracteres.");
            }

            if (!EsTextoValido(apellidoNormalizado))
            {
                Bitacora.Warn("El apellido proporcionado no supera las validaciones establecidas.");
                return CrearResultado(false, "El apellido es obligatorio y no debe exceder 50 caracteres.");
            }

            if (solicitud.AvatarId <= 0)
            {
                Bitacora.Warn("Se recibió un avatar inválido en la solicitud de actualización.");
                return CrearResultado(false, "Selecciona un avatar válido.");
            }

            string instagram = NormalizarRedSocial(solicitud.Instagram, "Instagram", out string errorRedInstagram);
            if (!string.IsNullOrWhiteSpace(errorRedInstagram))
            {
                Bitacora.Warn("La red social Instagram no es válida.");
                return CrearResultado(false, errorRedInstagram);
            }

            string facebook = NormalizarRedSocial(solicitud.Facebook, "Facebook", out string errorRedFacebook);
            if (!string.IsNullOrWhiteSpace(errorRedFacebook))
            {
                Bitacora.Warn("La red social Facebook no es válida.");
                return CrearResultado(false, errorRedFacebook);
            }

            string x = NormalizarRedSocial(solicitud.X, "X", out string errorRedX);
            if (!string.IsNullOrWhiteSpace(errorRedX))
            {
                Bitacora.Warn("La red social X no es válida.");
                return CrearResultado(false, errorRedX);
            }

            string discord = NormalizarRedSocial(solicitud.Discord, "Discord", out string errorRedDiscord);
            if (!string.IsNullOrWhiteSpace(errorRedDiscord))
            {
                Bitacora.Warn("La red social Discord no es válida.");
                return CrearResultado(false, errorRedDiscord);
            }

            try
            {
                Usuario usuario = _usuarioRepositorio.ObtenerUsuarioPorId(solicitud.UsuarioId);

                if (usuario == null)
                {
                    Bitacora.WarnFormat("No se encontró el usuario {0} para actualizar el perfil.", solicitud.UsuarioId);
                    return CrearResultado(false, "No se encontró el usuario especificado.");
                }

                if (usuario.Jugador == null)
                {
                    Bitacora.WarnFormat("El usuario {0} no cuenta con un jugador asociado.", solicitud.UsuarioId);
                    return CrearResultado(false, "No existe un jugador asociado al usuario especificado.");
                }

                if (!_avatarRepositorio.ExisteAvatar(solicitud.AvatarId))
                {
                    Bitacora.WarnFormat("El avatar {0} no existe en el catálogo.", solicitud.AvatarId);
                    return CrearResultado(false, "El avatar seleccionado no existe.");
                }

                int jugadorId = usuario.Jugador_idJugador;

                bool actualizado = _jugadorRepositorio.ActualizarPerfil(
                    jugadorId,
                    nombreNormalizado,
                    apellidoNormalizado,
                    solicitud.AvatarId,
                    instagram,
                    facebook,
                    x,
                    discord);

                if (!actualizado)
                {
                    Bitacora.WarnFormat("No se lograron persistir los cambios del perfil para el usuario {0}.", solicitud.UsuarioId);
                    return CrearResultado(false, "No fue posible actualizar el perfil.");
                }

                Bitacora.InfoFormat("Perfil actualizado correctamente para el usuario {0}.", solicitud.UsuarioId);
                return CrearResultado(true, "Perfil actualizado correctamente.");
            }
            catch (ArgumentNullException ex)
            {
                Bitacora.Warn("Se encontraron valores nulos al actualizar el perfil.", ex);
                throw FabricaFallaServicio.Crear("SOLICITUD_INVALIDA", "Los datos proporcionados no son válidos para actualizar el perfil.", "Solicitud inválida.");
            }
            catch (InvalidOperationException ex)
            {
                Bitacora.Error("Se produjo una operación inválida al actualizar el perfil.", ex);
                throw FabricaFallaServicio.Crear("OPERACION_INVALIDA", "No fue posible actualizar el perfil del usuario.", "Operación inválida en la capa de datos.");
            }
            catch (DataException ex)
            {
                Bitacora.Error("Error de base de datos al intentar actualizar el perfil.", ex);
                throw FabricaFallaServicio.Crear("ERROR_BASE_DATOS", "Ocurrió un problema con la base de datos al actualizar el perfil.", "Fallo en la base de datos.");
            }
            catch (Exception ex)
            {
                Bitacora.Fatal("Error inesperado al actualizar el perfil.", ex);
                throw FabricaFallaServicio.Crear("ERROR_NO_CONTROLADO", "Ocurrió un error inesperado al actualizar el perfil.", "Error interno del servidor.");
            }
        }

        private static bool EsTextoValido(string valor)
        {
            return !string.IsNullOrWhiteSpace(valor) && valor.Length <= LongitudMaximaNombre;
        }

        private static string NormalizarRedSocial(string valor, string nombreRed, out string mensajeError)
        {
            mensajeError = null;

            if (string.IsNullOrWhiteSpace(valor))
            {
                return null;
            }

            string texto = valor.Trim();

            if (string.Equals(texto, "@", StringComparison.Ordinal))
            {
                return null;
            }

            if (texto.StartsWith("@", StringComparison.Ordinal))
            {
                string contenido = texto.Substring(1);

                if (string.IsNullOrWhiteSpace(contenido))
                {
                    return null;
                }
            }

            if (texto.Length > LongitudMaximaRedSocial)
            {
                mensajeError = $"El identificador de {nombreRed} no debe exceder {LongitudMaximaRedSocial} caracteres.";
                return null;
            }

            return texto;
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
