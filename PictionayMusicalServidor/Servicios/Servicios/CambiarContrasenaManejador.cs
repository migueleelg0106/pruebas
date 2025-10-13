using Datos.DAL.Implementaciones;
using Datos.DAL.Interfaces;
using Servicios.Contratos;
using Servicios.Contratos.DTOs;
using Servicios.Servicios.Utilidades;
using System;
using System.Data;
using log4net;

namespace Servicios.Servicios
{
    public class CambiarContrasenaManejador : ICambiarContrasenaManejador
    {
        private readonly CodigoRecuperacionServicio _servicio;
        private static readonly ILog Bitacora = LogManager.GetLogger(typeof(CambiarContrasenaManejador));

        public CambiarContrasenaManejador()
            : this(new UsuarioRepositorio(), new CorreoCodigoVerificacionNotificador())
        {
        }

        public CambiarContrasenaManejador(IUsuarioRepositorio usuarioRepositorio, ICodigoVerificacionNotificador notificador)
        {
            _servicio = new CodigoRecuperacionServicio(usuarioRepositorio, notificador);
        }

        public ResultadoOperacionDTO ActualizarContrasena(ActualizarContrasenaDTO solicitud)
        {
            Bitacora.Info("Solicitud para actualizar contraseña recibida.");

            try
            {
                return _servicio.ActualizarContrasena(solicitud);
            }
            catch (ArgumentNullException ex)
            {
                Bitacora.Warn("Los datos para actualizar la contraseña son inválidos.", ex);
                throw FabricaFallaServicio.Crear("SOLICITUD_INVALIDA", "Los datos proporcionados no son válidos para actualizar la contraseña.", "Solicitud inválida.");
            }
            catch (InvalidOperationException ex)
            {
                Bitacora.Error("Operación inválida al actualizar la contraseña.", ex);
                throw FabricaFallaServicio.Crear("OPERACION_INVALIDA", "No fue posible actualizar la contraseña.", "Operación inválida en la capa de datos.");
            }
            catch (DataException ex)
            {
                Bitacora.Error("Error en la base de datos al actualizar la contraseña.", ex);
                throw FabricaFallaServicio.Crear("ERROR_BASE_DATOS", "Ocurrió un problema al actualizar la contraseña.", "Fallo en la base de datos.");
            }
            catch (Exception ex)
            {
                Bitacora.Fatal("Error inesperado al actualizar la contraseña.", ex);
                throw FabricaFallaServicio.Crear("ERROR_NO_CONTROLADO", "Ocurrió un error inesperado al actualizar la contraseña.", "Error interno del servidor.");
            }
        }
    }
}
