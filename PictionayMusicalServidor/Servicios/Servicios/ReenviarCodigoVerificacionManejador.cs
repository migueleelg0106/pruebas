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
    public class ReenviarCodigoVerificacionManejador : IReenviarCodigoVerificacionManejador
    {
        private readonly CodigoVerificacionServicio _servicio;
        private static readonly ILog Bitacora = LogManager.GetLogger(typeof(ReenviarCodigoVerificacionManejador));

        public ReenviarCodigoVerificacionManejador()
            : this(new CuentaRepositorio(), new CorreoCodigoVerificacionNotificador())
        {
        }

        public ReenviarCodigoVerificacionManejador(ICuentaRepositorio repositorioCuenta, ICodigoVerificacionNotificador notificador)
        {
            _servicio = new CodigoVerificacionServicio(repositorioCuenta, notificador);
        }

        public ResultadoSolicitudCodigoDTO ReenviarCodigoVerificacion(ReenviarCodigoVerificacionDTO solicitud)
        {
            Bitacora.Info("Solicitud para reenviar código de verificación recibida.");

            try
            {
                return _servicio.ReenviarCodigoVerificacion(solicitud);
            }
            catch (ArgumentNullException ex)
            {
                Bitacora.Warn("Los datos para reenviar el código de verificación son inválidos.", ex);
                throw FabricaFallaServicio.Crear("SOLICITUD_INVALIDA", "Los datos proporcionados no son válidos para reenviar el código.", "Solicitud inválida.");
            }
            catch (InvalidOperationException ex)
            {
                Bitacora.Error("Operación inválida al reenviar el código de verificación.", ex);
                throw FabricaFallaServicio.Crear("OPERACION_INVALIDA", "No fue posible reenviar el código de verificación.", "Operación inválida en la capa de datos.");
            }
            catch (DataException ex)
            {
                Bitacora.Error("Error en la base de datos al reenviar el código de verificación.", ex);
                throw FabricaFallaServicio.Crear("ERROR_BASE_DATOS", "Ocurrió un problema al reenviar el código de verificación.", "Fallo en la base de datos.");
            }
            catch (Exception ex)
            {
                Bitacora.Fatal("Error inesperado al reenviar el código de verificación.", ex);
                throw FabricaFallaServicio.Crear("ERROR_NO_CONTROLADO", "Ocurrió un error inesperado al reenviar el código de verificación.", "Error interno del servidor.");
            }
        }
    }
}
