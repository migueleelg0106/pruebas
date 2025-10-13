using System;
using System.ServiceModel;
using PictionaryMusicalCliente.Servicios;

namespace PictionaryMusicalCliente.Utilidades
{
    public static class ErrorServicioHelper
    {
        public static string ObtenerMensaje(
            FaultException<ServidorProxy.ErrorDetalleServicio> excepcion,
            string mensajePredeterminado)
        {
            if (excepcion?.Detail != null)
            {
                if (!string.IsNullOrWhiteSpace(excepcion.Detail.Mensaje))
                {
                    return excepcion.Detail.Mensaje.Trim();
                }

                if (!string.IsNullOrWhiteSpace(excepcion.Detail.Razon))
                {
                    return excepcion.Detail.Razon.Trim();
                }
            }

            if (!string.IsNullOrWhiteSpace(excepcion?.Message))
            {
                return excepcion.Message.Trim();
            }

            return string.IsNullOrWhiteSpace(mensajePredeterminado)
                ? "Ocurrió un error al procesar la solicitud."
                : mensajePredeterminado;
        }
    }
}
