using Datos.DAL.Implementaciones;
using Datos.DAL.Interfaces;
using Servicios.Contratos;
using Servicios.Contratos.DTOs;
using Servicios.Servicios.Notificaciones;

namespace Servicios.Servicios
{
    public class CodigoVerificacionManejador : ICodigoVerificacionManejador
    {
        private readonly CodigoVerificacionServicio _servicio;

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
            return _servicio.SolicitarCodigoVerificacion(nuevaCuenta);
        }

        public ResultadoRegistroCuentaDTO ConfirmarCodigoVerificacion(ConfirmarCodigoVerificacionDTO confirmacion)
        {
            return _servicio.ConfirmarCodigoVerificacion(confirmacion);
        }
    }
}
