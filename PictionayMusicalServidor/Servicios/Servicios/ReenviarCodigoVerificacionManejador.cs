using Datos.DAL.Implementaciones;
using Datos.DAL.Interfaces;
using Servicios.Contratos;
using Servicios.Contratos.DTOs;
using Servicios.Servicios.Utilidades;

namespace Servicios.Servicios
{
    public class ReenviarCodigoVerificacionManejador : IReenviarCodigoVerificacionManejador
    {
        private readonly CodigoVerificacionServicio _servicio;

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
            return _servicio.ReenviarCodigoVerificacion(solicitud);
        }
    }
}
