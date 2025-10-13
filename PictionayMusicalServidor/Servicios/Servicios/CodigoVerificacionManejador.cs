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
    public class CodigoVerificacionManejador : ICodigoVerificacionManejador
    {
        private readonly CodigoVerificacionServicio _servicio;
        private readonly CodigoRecuperacionServicio _recuperacionServicio;
        private static readonly ILog Bitacora = LogManager.GetLogger(typeof(CodigoVerificacionManejador));

        public CodigoVerificacionManejador()
            : this(
                new JugadorRepositorio(),
                new UsuarioRepositorio(),
                new ClasificacionRepositorio(),
                new CorreoCodigoVerificacionNotificador())
        {
        }

        public CodigoVerificacionManejador(
            IJugadorRepositorio jugadorRepositorio,
            IUsuarioRepositorio usuarioRepositorio,
            IClasificacionRepositorio clasificacionRepositorio,
            ICodigoVerificacionNotificador notificador)
        {
            _servicio = new CodigoVerificacionServicio(
                jugadorRepositorio,
                usuarioRepositorio,
                clasificacionRepositorio,
                notificador);

            _recuperacionServicio = new CodigoRecuperacionServicio(
                usuarioRepositorio,
                notificador);
        }

        public ResultadoSolicitudCodigoDTO SolicitarCodigoVerificacion(NuevaCuentaDTO nuevaCuenta)
        {
            Bitacora.Info("Solicitud para enviar código de verificación recibida.");

            try
            {
                return _servicio.SolicitarCodigoVerificacion(nuevaCuenta);
            }
            catch (ArgumentNullException ex)
            {
                Bitacora.Warn("Los datos para solicitar el código de verificación son inválidos.", ex);
                throw FabricaFallaServicio.Crear("SOLICITUD_INVALIDA", "Los datos proporcionados no son válidos para solicitar el código.", "Solicitud inválida.");
            }
            catch (InvalidOperationException ex)
            {
                Bitacora.Error("Operación inválida al solicitar el código de verificación.", ex);
                throw FabricaFallaServicio.Crear("OPERACION_INVALIDA", "No fue posible generar el código de verificación.", "Operación inválida en la capa de datos.");
            }
            catch (DataException ex)
            {
                Bitacora.Error("Error en la base de datos al solicitar el código de verificación.", ex);
                throw FabricaFallaServicio.Crear("ERROR_BASE_DATOS", "No se pudo solicitar el código de verificación por un problema interno.", "Fallo en la base de datos.");
            }
            catch (Exception ex)
            {
                Bitacora.Fatal("Error inesperado al solicitar el código de verificación.", ex);
                throw FabricaFallaServicio.Crear("ERROR_NO_CONTROLADO", "Ocurrió un error inesperado al solicitar el código de verificación.", "Error interno del servidor.");
            }
        }

        public ResultadoRegistroCuentaDTO ConfirmarCodigoVerificacion(ConfirmarCodigoDTO confirmacion)
        {
            Bitacora.Info("Solicitud para confirmar código de verificación recibida.");

            try
            {
                return _servicio.ConfirmarCodigoVerificacion(confirmacion);
            }
            catch (ArgumentNullException ex)
            {
                Bitacora.Warn("Los datos para confirmar el código de verificación son inválidos.", ex);
                throw FabricaFallaServicio.Crear("SOLICITUD_INVALIDA", "Los datos proporcionados no son válidos para confirmar el código.", "Solicitud inválida.");
            }
            catch (InvalidOperationException ex)
            {
                Bitacora.Error("Operación inválida al confirmar el código de verificación.", ex);
                throw FabricaFallaServicio.Crear("OPERACION_INVALIDA", "No se pudo confirmar el código de verificación.", "Operación inválida en la capa de datos.");
            }
            catch (DataException ex)
            {
                Bitacora.Error("Error en la base de datos al confirmar el código de verificación.", ex);
                throw FabricaFallaServicio.Crear("ERROR_BASE_DATOS", "No se pudo confirmar el código de verificación por un problema interno.", "Fallo en la base de datos.");
            }
            catch (Exception ex)
            {
                Bitacora.Fatal("Error inesperado al confirmar el código de verificación.", ex);
                throw FabricaFallaServicio.Crear("ERROR_NO_CONTROLADO", "Ocurrió un error inesperado al confirmar el código de verificación.", "Error interno del servidor.");
            }
        }

        public ResultadoSolicitudRecuperacionDTO SolicitarCodigoRecuperacion(SolicitudRecuperarCuentaDTO solicitud)
        {
            Bitacora.Info("Solicitud para iniciar recuperación de cuenta recibida.");

            try
            {
                return _recuperacionServicio.SolicitarCodigoRecuperacion(solicitud);
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

        public ResultadoOperacionDTO ConfirmarCodigoRecuperacion(ConfirmarCodigoDTO confirmacion)
        {
            Bitacora.Info("Solicitud para confirmar código de recuperación recibida.");

            try
            {
                return _recuperacionServicio.ConfirmarCodigoRecuperacion(confirmacion);
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
    }
}
