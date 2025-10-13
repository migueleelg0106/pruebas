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
        private readonly RecuperacionCuentaServicio _servicio;
        private static readonly ILog Bitacora = LogManager.GetLogger(typeof(CambiarContrasenaManejador));

        public CambiarContrasenaManejador()
            : this(new CuentaRepositorio(), new CorreoCodigoVerificacionNotificador())
        {
        }

        public CambiarContrasenaManejador(ICuentaRepositorio cuentaRepositorio, ICodigoVerificacionNotificador notificador)
        {
            _servicio = new RecuperacionCuentaServicio(cuentaRepositorio, notificador);
        }

        public ResultadoSolicitudRecuperacionDTO SolicitarCodigoRecuperacion(SolicitudRecuperarCuentaDTO solicitud)
        {
            Bitacora.Info("Solicitud para iniciar recuperación de cuenta recibida.");

            try
            {
                return _servicio.SolicitarCodigoRecuperacion(solicitud);
            }
            catch (ArgumentNullException ex)
            {
                Bitacora.Warn("Los datos para solicitar la recuperación de cuenta son inválidos.", ex);
                throw FabricaFallaServicio.Crear("SOLICITUD_INVALIDA", "Los datos proporcionados no son válidos para recuperar la cuenta.", "Solicitud inválida.");
            }
            catch (InvalidOperationException ex)
            {
                Bitacora.Error("Operación inválida al solicitar la recuperación de cuenta.", ex);
                throw FabricaFallaServicio.Crear("OPERACION_INVALIDA", "No fue posible procesar la recuperación de la cuenta.", "Operación inválida en la capa de datos.");
            }
            catch (DataException ex)
            {
                Bitacora.Error("Error en la base de datos al solicitar la recuperación de cuenta.", ex);
                throw FabricaFallaServicio.Crear("ERROR_BASE_DATOS", "Ocurrió un problema al iniciar la recuperación de la cuenta.", "Fallo en la base de datos.");
            }
            catch (Exception ex)
            {
                Bitacora.Fatal("Error inesperado al solicitar la recuperación de cuenta.", ex);
                throw FabricaFallaServicio.Crear("ERROR_NO_CONTROLADO", "Ocurrió un error inesperado al solicitar la recuperación de la cuenta.", "Error interno del servidor.");
            }
        }

        public ResultadoSolicitudCodigoDTO ReenviarCodigoRecuperacion(SolicitudReenviarCodigoRecuperacionDTO solicitud)
        {
            Bitacora.Info("Solicitud para reenviar código de recuperación recibida.");

            try
            {
                return _servicio.ReenviarCodigoRecuperacion(solicitud);
            }
            catch (ArgumentNullException ex)
            {
                Bitacora.Warn("Los datos para reenviar el código de recuperación son inválidos.", ex);
                throw FabricaFallaServicio.Crear("SOLICITUD_INVALIDA", "Los datos proporcionados no son válidos para reenviar el código.", "Solicitud inválida.");
            }
            catch (InvalidOperationException ex)
            {
                Bitacora.Error("Operación inválida al reenviar el código de recuperación.", ex);
                throw FabricaFallaServicio.Crear("OPERACION_INVALIDA", "No fue posible reenviar el código de recuperación.", "Operación inválida en la capa de datos.");
            }
            catch (DataException ex)
            {
                Bitacora.Error("Error en la base de datos al reenviar el código de recuperación.", ex);
                throw FabricaFallaServicio.Crear("ERROR_BASE_DATOS", "Ocurrió un problema al reenviar el código de recuperación.", "Fallo en la base de datos.");
            }
            catch (Exception ex)
            {
                Bitacora.Fatal("Error inesperado al reenviar el código de recuperación.", ex);
                throw FabricaFallaServicio.Crear("ERROR_NO_CONTROLADO", "Ocurrió un error inesperado al reenviar el código de recuperación.", "Error interno del servidor.");
            }
        }

        public ResultadoOperacionDTO ConfirmarCodigoRecuperacion(ConfirmarCodigoRecuperacionDTO confirmacion)
        {
            Bitacora.Info("Solicitud para confirmar código de recuperación recibida.");

            try
            {
                return _servicio.ConfirmarCodigoRecuperacion(confirmacion);
            }
            catch (ArgumentNullException ex)
            {
                Bitacora.Warn("Los datos para confirmar el código de recuperación son inválidos.", ex);
                throw FabricaFallaServicio.Crear("SOLICITUD_INVALIDA", "Los datos proporcionados no son válidos para confirmar el código.", "Solicitud inválida.");
            }
            catch (InvalidOperationException ex)
            {
                Bitacora.Error("Operación inválida al confirmar el código de recuperación.", ex);
                throw FabricaFallaServicio.Crear("OPERACION_INVALIDA", "No fue posible confirmar el código de recuperación.", "Operación inválida en la capa de datos.");
            }
            catch (DataException ex)
            {
                Bitacora.Error("Error en la base de datos al confirmar el código de recuperación.", ex);
                throw FabricaFallaServicio.Crear("ERROR_BASE_DATOS", "Ocurrió un problema al confirmar el código de recuperación.", "Fallo en la base de datos.");
            }
            catch (Exception ex)
            {
                Bitacora.Fatal("Error inesperado al confirmar el código de recuperación.", ex);
                throw FabricaFallaServicio.Crear("ERROR_NO_CONTROLADO", "Ocurrió un error inesperado al confirmar el código de recuperación.", "Error interno del servidor.");
            }
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
