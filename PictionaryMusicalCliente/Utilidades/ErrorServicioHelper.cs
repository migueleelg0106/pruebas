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
                return excepcion.Detail.Mensaje.Trim();
            }


            if (!string.IsNullOrWhiteSpace(excepcion?.Message))
            {
                return excepcion.Message.Trim();
            }

            return string.IsNullOrWhiteSpace(mensajePredeterminado)
                ? LangResources.Lang.errorTextoErrorProcesarSolicitud
                : mensajePredeterminado;
        }
    }
}
