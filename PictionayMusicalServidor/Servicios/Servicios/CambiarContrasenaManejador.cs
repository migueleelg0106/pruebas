using Datos.DAL.Implementaciones;
using Datos.DAL.Interfaces;
using Servicios.Contratos;
using Servicios.Contratos.DTOs;
using Servicios.Servicios.Notificaciones;

namespace Servicios.Servicios
{
    public class CambiarContrasenaManejador : ICambiarContrasenaManejador
    {
        private readonly RecuperacionCuentaServicio _servicio;

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
            return _servicio.SolicitarCodigoRecuperacion(solicitud);
        }

        public ResultadoSolicitudCodigoDTO ReenviarCodigoRecuperacion(SolicitudReenviarCodigoRecuperacionDTO solicitud)
        {
            return _servicio.ReenviarCodigoRecuperacion(solicitud);
        }

        public ResultadoOperacionDTO ConfirmarCodigoRecuperacion(ConfirmarCodigoRecuperacionDTO confirmacion)
        {
            return _servicio.ConfirmarCodigoRecuperacion(confirmacion);
        }

        public ResultadoOperacionDTO ActualizarContrasena(ActualizarContrasenaDTO solicitud)
        {
            return _servicio.ActualizarContrasena(solicitud);
        }
    }
}
