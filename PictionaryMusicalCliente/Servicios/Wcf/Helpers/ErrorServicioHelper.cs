using System;
using System.Reflection;
using System.ServiceModel;
using LangResources = PictionaryMusicalCliente.Properties.Langs;

namespace PictionaryMusicalCliente.Servicios.Wcf.Helpers
{
    public static class ErrorServicioHelper
    {
        public static string ObtenerMensaje(
            FaultException excepcion,
            string mensajePredeterminado)
        {
            string mensajeDetalle = ObtenerMensajeDetalle(excepcion);

            if (!string.IsNullOrWhiteSpace(mensajeDetalle))
            {
                return MensajeServidorHelper.Localizar(
                    mensajeDetalle,
                    mensajePredeterminado);
            }

            if (!string.IsNullOrWhiteSpace(excepcion?.Message))
            {
                return MensajeServidorHelper.Localizar(
                    excepcion.Message,
                    mensajePredeterminado);
            }

            return MensajeServidorHelper.Localizar(null, mensajePredeterminado);
        }

        private static string ObtenerMensajeDetalle(FaultException excepcion)
        {
            if (excepcion == null)
            {
                return null;
            }

            Type tipoExcepcion = excepcion.GetType();

            if (!tipoExcepcion.GetTypeInfo().IsGenericType)
            {
                return null;
            }

            if (tipoExcepcion.GetGenericTypeDefinition() != typeof(FaultException<>))
            {
                return null;
            }

            PropertyInfo detallePropiedad = tipoExcepcion.GetRuntimeProperty("Detail");
            object detalle = detallePropiedad?.GetValue(excepcion);

            if (detalle == null)
            {
                return null;
            }

            PropertyInfo mensajePropiedad = detalle.GetType().GetRuntimeProperty("Mensaje");
            object mensaje = mensajePropiedad?.GetValue(detalle);
            return mensaje as string;
        }
    }
}
