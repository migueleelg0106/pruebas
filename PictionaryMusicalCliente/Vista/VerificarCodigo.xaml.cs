﻿using System;
using System.ServiceModel;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;
using PictionaryMusicalCliente.Modelo;
using PictionaryMusicalCliente.Servicios;
using PictionaryMusicalCliente.Utilidades;
using LangResources = PictionaryMusicalCliente.Properties.Langs;

namespace PictionaryMusicalCliente
{
    public partial class VerificarCodigo : Window
    {
        private string _tokenVerificacion;
        private readonly string _correoDestino;
        private readonly string _textoOriginalReenviar;
        private readonly DispatcherTimer _temporizador;
        private readonly Func<string, Task<ResultadoOperacion>> _confirmarCodigoFunc;
        private readonly Func<Task<ResultadoSolicitudCodigo>> _solicitarReenvioFunc;
        private readonly string _descripcionPersonalizada;
        private DateTime _siguienteReenvioPermitido;

        public bool OperacionCompletada { get; private set; }
        public bool RegistroCompletado => OperacionCompletada;

        public VerificarCodigo(
            string tokenVerificacion,
            string correoDestino,
            Func<string, Task<ResultadoOperacion>> confirmarCodigoAsync = null,
            Func<Task<ResultadoSolicitudCodigo>> reenviarCodigoAsync = null,
            string descripcionPersonalizada = null)
        {
            if (string.IsNullOrWhiteSpace(tokenVerificacion))
            {
                throw new ArgumentException(LangResources.Lang.errorTextoTokenVerificacionObligatorio, nameof(tokenVerificacion));
            }

            InitializeComponent();

            _tokenVerificacion = tokenVerificacion;
            _correoDestino = correoDestino ?? string.Empty;
            _textoOriginalReenviar = botonReenviarCodigo.Content?.ToString() ?? LangResources.Lang.cambiarContraseñaTextoReenviarCodigo;
            _temporizador = new DispatcherTimer { Interval = TimeSpan.FromSeconds(1) };
            _temporizador.Tick += TemporizadorTick;

            _confirmarCodigoFunc = confirmarCodigoAsync ?? ConfirmarCodigoRegistroAsync;
            _solicitarReenvioFunc = reenviarCodigoAsync ?? ReenviarCodigoRegistroAsync;
            _descripcionPersonalizada = descripcionPersonalizada;
            OperacionCompletada = false;
            _siguienteReenvioPermitido = DateTime.UtcNow.AddMinutes(1);

            if (!string.IsNullOrWhiteSpace(_descripcionPersonalizada))
            {
                textoDescripcion.Text = _descripcionPersonalizada;
            }
            else
            {
                textoDescripcion.Text = string.IsNullOrWhiteSpace(_correoDestino)
                    ? LangResources.Lang.avisoTextoCodigoDescripcionGenerica
                    : string.Format(LangResources.Lang.avisoTextoCodigoDescripcionCorreo, _correoDestino);
            }

            if (_solicitarReenvioFunc == null)
            {
                botonReenviarCodigo.Visibility = Visibility.Collapsed;
            }

            ActualizarEstadoReenvio();
        }

        private async void BotonVerificarCodigo(object sender, RoutedEventArgs e)
        {
            string codigoIngresado = bloqueTextoCodigoVerificacion.Text?.Trim();

            if (string.IsNullOrWhiteSpace(codigoIngresado))
            {
                AvisoHelper.Mostrar(LangResources.Lang.errorTextoCodigoVerificacionRequerido);
                bloqueTextoCodigoVerificacion.Focus();
                return;
            }

            botonVerificarCodigo.IsEnabled = false;

            try
            {
                if (_confirmarCodigoFunc == null)
                {
                    AvisoHelper.Mostrar(LangResources.Lang.errorTextoValidacionCodigoNoDisponible);
                    return;
                }

                ResultadoOperacion resultado = await _confirmarCodigoFunc(codigoIngresado);

                if (resultado == null)
                {
                    AvisoHelper.Mostrar(LangResources.Lang.errorTextoVerificarCodigo);
                    return;
                }

                if (resultado.OperacionExitosa)
                {
                    string mensaje = MensajeServidorHelper.Localizar(
                        resultado.Mensaje,
                        LangResources.Lang.avisoTextoCodigoVerificadoCorrecto);
                    AvisoHelper.Mostrar(mensaje);
                    OperacionCompletada = true;
                    Close();
                    return;
                }

                string mensajeError = MensajeServidorHelper.Localizar(
                    resultado.Mensaje,
                    LangResources.Lang.errorTextoCodigoIncorrectoExpirado);

                AvisoHelper.Mostrar(mensajeError);
            }
            catch (FaultException<ServidorProxy.ErrorDetalleServicio> ex)
            {
                string mensaje = ErrorServicioHelper.ObtenerMensaje(
                    ex,
                    LangResources.Lang.errorTextoServidorValidarCodigo);
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
                AvisoHelper.Mostrar(LangResources.Lang.errorTextoComunicacionServidorMasTarde);
            }
            catch (InvalidOperationException)
            {
                AvisoHelper.Mostrar(LangResources.Lang.errorTextoSolicitudVerificacionInvalida);
            }
            finally
            {
                if (!OperacionCompletada)
                {
                    botonVerificarCodigo.IsEnabled = true;
                }
            }
        }

        private async void BotonReenviarCodigo(object sender, RoutedEventArgs e)
        {
            await ManejarReenvioCodigoAsync();
        }

        private void BotonCancelarCodigo(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private async Task<ResultadoOperacion> ConfirmarCodigoRegistroAsync(string codigo)
        {
            ResultadoRegistroCuenta resultado = await ConfirmarCodigoRegistroCoreAsync(codigo);

            if (resultado == null)
            {
                return null;
            }

            return new ResultadoOperacion
            {
                OperacionExitosa = resultado.RegistroExitoso,
                Mensaje = MensajeServidorHelper.Localizar(
                    resultado.Mensaje,
                    resultado.RegistroExitoso ? LangResources.Lang.avisoTextoRegistroCompletado : null)
            };
        }

        private async Task<ResultadoRegistroCuenta> ConfirmarCodigoRegistroCoreAsync(string codigo)
        {
            using (var proxy = new ServidorProxy())
            {
                var solicitud = new SolicitudConfirmarCodigo
                {
                    TokenVerificacion = _tokenVerificacion,
                    Codigo = codigo
                };

                return await proxy.ConfirmarCodigoVerificacionAsync(solicitud);
            }
        }

        private async Task<ResultadoSolicitudCodigo> ReenviarCodigoRegistroAsync()
        {
            using (var proxy = new ServidorProxy())
            {
                var solicitud = new SolicitudReenviarCodigo
                {
                    TokenVerificacion = _tokenVerificacion
                };

                return await proxy.ReenviarCodigoVerificacionAsync(solicitud);
            }
        }

        private async Task ManejarReenvioCodigoAsync()
        {
            if (!botonReenviarCodigo.IsEnabled || _solicitarReenvioFunc == null)
            {
                return;
            }

            botonReenviarCodigo.IsEnabled = false;

            try
            {
                ResultadoSolicitudCodigo resultado = await _solicitarReenvioFunc();

                if (resultado == null)
                {
                    AvisoHelper.Mostrar(LangResources.Lang.errorTextoSolicitarNuevoCodigo);
                    return;
                }

                if (resultado.CodigoEnviado)
                {
                    if (!string.IsNullOrWhiteSpace(resultado.TokenVerificacion))
                    {
                        _tokenVerificacion = resultado.TokenVerificacion;
                    }

                    _siguienteReenvioPermitido = DateTime.UtcNow.AddMinutes(1);
                    string mensaje = MensajeServidorHelper.Localizar(
                        resultado.Mensaje,
                        LangResources.Lang.avisoTextoCodigoReenviado);
                    AvisoHelper.Mostrar(mensaje);
                    return;
                }

                string mensajeError = MensajeServidorHelper.Localizar(
                    resultado.Mensaje,
                    LangResources.Lang.avisoTextoReenvioCodigoNoDisponible);

                AvisoHelper.Mostrar(mensajeError);
            }
            catch (FaultException<ServidorProxy.ErrorDetalleServicio> ex)
            {
                string mensaje = ErrorServicioHelper.ObtenerMensaje(
                    ex,
                    LangResources.Lang.errorTextoServidorReenviarCodigo);
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
                AvisoHelper.Mostrar(LangResources.Lang.errorTextoComunicacionServidorMasTarde);
            }
            catch (InvalidOperationException)
            {
                AvisoHelper.Mostrar(LangResources.Lang.errorTextoProcesarSolicitudReenvio);
            }
            finally
            {
                ActualizarEstadoReenvio();
            }
        }

        private void TemporizadorTick(object sender, EventArgs e)
        {
            ActualizarEstadoReenvio();
        }

        private void ActualizarEstadoReenvio()
        {
            if (_solicitarReenvioFunc == null)
            {
                botonReenviarCodigo.IsEnabled = false;
                botonReenviarCodigo.Content = _textoOriginalReenviar;
                if (_temporizador.IsEnabled)
                {
                    _temporizador.Stop();
                }
                return;
            }

            DateTime ahora = DateTime.UtcNow;
            if (ahora >= _siguienteReenvioPermitido)
            {
                botonReenviarCodigo.IsEnabled = true;
                botonReenviarCodigo.Content = _textoOriginalReenviar;
                if (_temporizador.IsEnabled)
                {
                    _temporizador.Stop();
                }
                return;
            }

            TimeSpan restante = _siguienteReenvioPermitido - ahora;
            int segundos = Math.Max(1, (int)Math.Ceiling(restante.TotalSeconds));
            botonReenviarCodigo.IsEnabled = false;
            botonReenviarCodigo.Content = $"{_textoOriginalReenviar} ({segundos}s)";

            if (!_temporizador.IsEnabled)
            {
                _temporizador.Start();
            }
        }
    }
}
