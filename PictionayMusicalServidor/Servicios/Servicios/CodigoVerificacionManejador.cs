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
        private static readonly ILog Bitacora = LogManager.GetLogger(typeof(CodigoVerificacionManejador));

        public CodigoVerificacionManejador()
            : this(new CuentaRepositorio(), new CorreoCodigoVerificacionNotificador())
        {
        }

        public CodigoVerificacionManejador(ICuentaRepositorio repositorioCuenta, ICodigoVerificacionNotificador notificador)
        {
            _servicio = new CodigoVerificacionServicio(repositorioCuenta, notificador);
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

        public ResultadoRegistroCuentaDTO ConfirmarCodigoVerificacion(ConfirmarCodigoVerificacionDTO confirmacion)
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
    }
}
