using System.ServiceModel;
using Servicios.Contratos.DTOs;

namespace Servicios.Servicios
{
    internal static class FabricaFallaServicio
    {
        public static FaultException<ErrorDetalleServicioDTO> Crear(string codigoError, string mensajeUsuario, string razon)
        {
            var detalle = new ErrorDetalleServicioDTO
            {
                CodigoError = codigoError,
                Mensaje = mensajeUsuario
            };

            return new FaultException<ErrorDetalleServicioDTO>(detalle, new FaultReason(razon));
        }
    }
}
