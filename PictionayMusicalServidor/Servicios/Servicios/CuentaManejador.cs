using Datos.DAL.Implementaciones;
using Datos.DAL.Interfaces;
using Servicios.Contratos;
using Servicios.Contratos.DTOs;
using System;
using System.Data;
using log4net;

namespace Servicios.Servicios
{
    public class CuentaManejador : ICuentaManejador
    {
        private readonly CuentaRegistroServicio _registroServicio;
        private static readonly ILog Bitacora = LogManager.GetLogger(typeof(CuentaManejador));

        public CuentaManejador()
            : this(new CuentaRepositorio())
        {
        }

        public CuentaManejador(ICuentaRepositorio repositorioCuenta)
        {
            if (repositorioCuenta == null)
            {
                throw new ArgumentNullException(nameof(repositorioCuenta));
            }

            _registroServicio = new CuentaRegistroServicio(repositorioCuenta);
        }

        public ResultadoRegistroCuentaDTO RegistrarCuenta(NuevaCuentaDTO nuevaCuenta)
        {
            Bitacora.Info("Solicitud para registrar una nueva cuenta recibida.");

            try
            {
                return _registroServicio.RegistrarCuenta(nuevaCuenta);
            }
            catch (ArgumentNullException ex)
            {
                Bitacora.Warn("La información enviada para registrar la cuenta es inválida.", ex);
                throw FabricaFallaServicio.Crear("SOLICITUD_INVALIDA", "Los datos de la cuenta no son válidos.", "Solicitud inválida.");
            }
            catch (InvalidOperationException ex)
            {
                Bitacora.Error("Operación inválida detectada al registrar la cuenta.", ex);
                throw FabricaFallaServicio.Crear("OPERACION_INVALIDA", "No fue posible completar el registro de la cuenta.", "Operación inválida en la capa de datos.");
            }
            catch (DataException ex)
            {
                Bitacora.Error("Error en la base de datos al registrar la cuenta.", ex);
                throw FabricaFallaServicio.Crear("ERROR_BASE_DATOS", "Ocurrió un problema al registrar la cuenta.", "Fallo en la base de datos.");
            }
            catch (Exception ex)
            {
                Bitacora.Fatal("Error inesperado al registrar la cuenta.", ex);
                throw FabricaFallaServicio.Crear("ERROR_NO_CONTROLADO", "Ocurrió un error inesperado al registrar la cuenta.", "Error interno del servidor.");
            }
        }
    }
}
