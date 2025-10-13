using Datos.DAL.Implementaciones;
using Datos.DAL.Interfaces;
using Servicios.Contratos;
using Servicios.Contratos.DTOs;
using System;
using System.Data;
using log4net;

namespace Servicios.Servicios
{
    public class InicioSesionManejador : IInicioSesionManejador
    {
        private readonly InicioSesionServicio _servicio;
        private static readonly ILog Bitacora = LogManager.GetLogger(typeof(InicioSesionManejador));

        public InicioSesionManejador()
            : this(new UsuarioRepositorio())
        {
        }

        public InicioSesionManejador(IUsuarioRepositorio usuarioRepositorio)
        {
            if (usuarioRepositorio == null)
            {
                throw new ArgumentNullException(nameof(usuarioRepositorio));
            }

            _servicio = new InicioSesionServicio(usuarioRepositorio);
        }

        public ResultadoInicioSesionDTO IniciarSesion(CredencialesInicioSesionDTO credenciales)
        {
            Bitacora.Info("Solicitud para iniciar sesión recibida.");

            try
            {
                return _servicio.IniciarSesion(credenciales);
            }
            catch (ArgumentNullException ex)
            {
                Bitacora.Warn("Las credenciales proporcionadas son inválidas.", ex);
                throw FabricaFallaServicio.Crear("SOLICITUD_INVALIDA", "Los datos proporcionados no son válidos para iniciar sesión.", "Solicitud inválida.");
            }
            catch (InvalidOperationException ex)
            {
                Bitacora.Error("Operación inválida al intentar iniciar sesión.", ex);
                throw FabricaFallaServicio.Crear("OPERACION_INVALIDA", "No fue posible completar el inicio de sesión.", "Operación inválida en la capa de datos.");
            }
            catch (DataException ex)
            {
                Bitacora.Error("Error en la base de datos al iniciar sesión.", ex);
                throw FabricaFallaServicio.Crear("ERROR_BASE_DATOS", "Ocurrió un problema al iniciar sesión.", "Fallo en la base de datos.");
            }
            catch (Exception ex)
            {
                Bitacora.Fatal("Error inesperado al iniciar sesión.", ex);
                throw FabricaFallaServicio.Crear("ERROR_NO_CONTROLADO", "Ocurrió un error inesperado al iniciar sesión.", "Error interno del servidor.");
            }
        }
    }
}
