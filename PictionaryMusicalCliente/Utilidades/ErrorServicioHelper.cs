using System;
using System.ServiceModel;
using PictionaryMusicalCliente.Servicios;
using LangResources = PictionaryMusicalCliente.Properties.Langs;

namespace PictionaryMusicalCliente.Utilidades
{
    public static class ErrorServicioHelper
    {
        public static string ObtenerMensaje(
            FaultException<ServidorProxy.ErrorDetalleServicio> excepcion,
            string mensajePredeterminado)
        {
            if (excepcion?.Detail != null &&
                !string.IsNullOrWhiteSpace(excepcion.Detail.Mensaje))
            {
                return MensajeServidorHelper.Localizar(
                    excepcion.Detail.Mensaje,
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
    }
}
