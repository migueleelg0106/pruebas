using System;
using System.ServiceModel;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using PictionaryMusicalCliente.Sesiones;
using PictionaryMusicalCliente.Utilidades;
using LangResources = PictionaryMusicalCliente.Properties.Langs;
using CodigoVerificacionSrv = PictionaryMusicalCliente.PictionaryServidorServicioCodigoVerificacion;
using ReenvioSrv = PictionaryMusicalCliente.PictionaryServidorServicioReenvioCodigoVerificacion;

namespace PictionaryMusicalCliente
{
    public partial class Perfil
    {
        private async void BotonCambiarContraseña(object sender, RoutedEventArgs e)
        {
            _usuarioSesion = _usuarioSesion ?? SesionUsuarioActual.Instancia.Usuario;

            if (_usuarioSesion == null)
            {
                AvisoHelper.Mostrar(LangResources.Lang.errorTextoCuentaNoEncontradaSesion);
                Close();
                return;
            }

            string identificador = !string.IsNullOrWhiteSpace(_usuarioSesion.Correo)
                ? _usuarioSesion.Correo
                : _usuarioSesion.NombreUsuario;

            if (string.IsNullOrWhiteSpace(identificador))
            {
                AvisoHelper.Mostrar(LangResources.Lang.errorTextoDeterminarUsuarioCambioContrasena);
                return;
            }

            Button boton = sender as Button;
            if (boton != null)
            {
                boton.IsEnabled = false;
            }

            Mouse.OverrideCursor = Cursors.Wait;

            try
            {
                CambioContrasenaContexto contexto = await SolicitarCambioContrasenaAsync(identificador);

                if (contexto == null)
                {
                    return;
                }

                Mouse.OverrideCursor = null;

                bool contrasenaActualizada = await EjecutarDialogosCambioContrasenaAsync(contexto, identificador);

                if (contrasenaActualizada)
                {
                    AvisoHelper.Mostrar(LangResources.Lang.avisoTextoContrasenaActualizada);
                }
            }
            catch (FaultException ex)
            {
                string mensaje = ErrorServicioHelper.ObtenerMensaje(
                    ex,
                    LangResources.Lang.errorTextoServidorSolicitudCambioContrasena);
                AvisoHelper.Mostrar(mensaje);
            }
            catch (EndpointNotFoundException)
            {
                AvisoHelper.Mostrar(LangResources.Lang.errorTextoServidorNoDisponible);
            }
            catch (TimeoutException)
            {
                AvisoHelper.Mostrar(LangResources.Lang.errorTextoServidorTiempoAgotado);
            }
            catch (CommunicationException)
            {
                AvisoHelper.Mostrar(LangResources.Lang.errorTextoServidorNoDisponible);
            }
            catch (InvalidOperationException)
            {
                AvisoHelper.Mostrar(LangResources.Lang.errorTextoErrorProcesarSolicitud);
            }
            finally
            {
                if (boton != null)
                {
                    boton.IsEnabled = true;
                }

                Mouse.OverrideCursor = null;
            }
        }

        private async Task<CambioContrasenaContexto> SolicitarCambioContrasenaAsync(string identificador)
        {
            CodigoVerificacionSrv.ResultadoSolicitudRecuperacionDTO resultado = await CodigoVerificacionServicioHelper.SolicitarCodigoRecuperacionAsync(identificador);

            if (resultado == null)
            {
                AvisoHelper.Mostrar(LangResources.Lang.errorTextoIniciarCambioContrasena);
                return null;
            }

            if (!resultado.CuentaEncontrada)
            {
                string mensajeCuenta = MensajeServidorHelper.Localizar(
                    resultado.Mensaje,
                    LangResources.Lang.errorTextoCuentaNoEncontradaSesion);
                AvisoHelper.Mostrar(mensajeCuenta);
                return null;
            }

            if (!resultado.CodigoEnviado || string.IsNullOrWhiteSpace(resultado.TokenCodigo))
            {
                string mensajeCodigo = MensajeServidorHelper.Localizar(
                    resultado.Mensaje,
                    LangResources.Lang.errorTextoEnvioCodigoVerificacionMasTarde);
                AvisoHelper.Mostrar(mensajeCodigo);
                return null;
            }

            return new CambioContrasenaContexto(resultado.TokenCodigo, resultado.CorreoDestino);
        }

        private async Task<bool> EjecutarDialogosCambioContrasenaAsync(CambioContrasenaContexto contexto, string identificador)
        {
            if (contexto == null)
            {
                return false;
            }

            string tokenCodigo = contexto.TokenCodigo;
            string correoDestino = contexto.CorreoDestino;

            async Task<VerificarCodigo.ConfirmacionResultado> ConfirmarCodigoAsync(string codigo)
            {
                CodigoVerificacionSrv.ResultadoOperacionDTO resultadoConfirmacion = await CodigoVerificacionServicioHelper.ConfirmarCodigoRecuperacionAsync(
                    tokenCodigo,
                    codigo);

                if (resultadoConfirmacion == null)
                {
                    return null;
                }

                return new VerificarCodigo.ConfirmacionResultado(
                    resultadoConfirmacion.OperacionExitosa,
                    resultadoConfirmacion.Mensaje);
            }

            async Task<VerificarCodigo.ReenvioResultado> ReenviarCodigoAsync()
            {
                ReenvioSrv.ResultadoSolicitudCodigoDTO resultadoReenvio = await CodigoVerificacionServicioHelper.ReenviarCodigoRecuperacionAsync(tokenCodigo);

                if (resultadoReenvio != null && !string.IsNullOrWhiteSpace(resultadoReenvio.TokenCodigo))
                {
                    tokenCodigo = resultadoReenvio.TokenCodigo;
                }

                return resultadoReenvio == null
                    ? null
                    : new VerificarCodigo.ReenvioResultado(
                        resultadoReenvio.CodigoEnviado,
                        resultadoReenvio.Mensaje,
                        resultadoReenvio.TokenCodigo);
            }

            var ventanaVerificacion = new VerificarCodigo(
                tokenCodigo,
                correoDestino,
                ConfirmarCodigoAsync,
                ReenviarCodigoAsync,
                LangResources.Lang.avisoTextoCodigoDescripcionCambio);

            ventanaVerificacion.ShowDialog();

            if (!ventanaVerificacion.OperacionCompletada)
            {
                return false;
            }

            var ventanaCambio = new CambioContrasena(tokenCodigo, identificador);
            bool? resultadoCambio = ventanaCambio.ShowDialog();

            return ventanaCambio.ContrasenaActualizada || resultadoCambio == true;
        }

        private sealed class CambioContrasenaContexto
        {
            public CambioContrasenaContexto(string tokenCodigo, string correoDestino)
            {
                TokenCodigo = tokenCodigo;
                CorreoDestino = correoDestino;
            }

            public string TokenCodigo { get; }

            public string CorreoDestino { get; }
        }
    }
}
