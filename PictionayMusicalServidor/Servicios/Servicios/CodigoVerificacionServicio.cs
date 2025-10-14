using Datos.DAL.Interfaces;
using Servicios.Contratos.DTOs;
using Servicios.Servicios.Utilidades;
using System;
using System.Configuration;
using System.Net.Mail;
using System.Threading.Tasks;

namespace Servicios.Servicios
{
    internal class CodigoVerificacionServicio
    {
        private readonly ICodigoVerificacionNotificador _notificador;
        private readonly RegistroCuentaPendienteStore _registroStore;
        private readonly CuentaRegistroServicio _cuentaRegistroServicio;
        private static readonly TimeSpan DuracionCodigo = TimeSpan.FromMinutes(5);
        private static readonly TimeSpan TiempoEsperaReenvio = TimeSpan.FromMinutes(1);

        public CodigoVerificacionServicio(
            IJugadorRepositorio jugadorRepositorio,
            IUsuarioRepositorio usuarioRepositorio,
            IClasificacionRepositorio clasificacionRepositorio,
            ICodigoVerificacionNotificador notificador)
        {
            _notificador = notificador ?? throw new ArgumentNullException(nameof(notificador));
            _registroStore = RegistroCuentaPendienteStore.Instancia;
            _cuentaRegistroServicio = new CuentaRegistroServicio(
                jugadorRepositorio ?? throw new ArgumentNullException(nameof(jugadorRepositorio)),
                usuarioRepositorio ?? throw new ArgumentNullException(nameof(usuarioRepositorio)),
                clasificacionRepositorio ?? throw new ArgumentNullException(nameof(clasificacionRepositorio)));
        }

        public ResultadoSolicitudCodigoDTO SolicitarCodigoVerificacion(NuevaCuentaDTO nuevaCuenta)
        {
            if (nuevaCuenta == null)
            {
                throw new ArgumentNullException(nameof(nuevaCuenta));
            }

            DateTime ahora = DateTime.UtcNow;
            _registroStore.LimpiarExpirados(ahora);

            var resultadoValidacion = new ResultadoRegistroCuentaDTO();
            bool informacionValida = _cuentaRegistroServicio.ValidarNuevaCuenta(nuevaCuenta, resultadoValidacion);

            var resultado = new ResultadoSolicitudCodigoDTO
            {
                CodigoEnviado = false,
                Mensaje = resultadoValidacion.Mensaje,
                CorreoYaRegistrado = resultadoValidacion.CorreoYaRegistrado,
                UsuarioYaRegistrado = resultadoValidacion.UsuarioYaRegistrado
            };

            if (!informacionValida)
            {
                return resultado;
            }

            _registroStore.RemoverPorCorreoOUsuario(nuevaCuenta.Correo, nuevaCuenta.Usuario);

            string codigo = CodigoVerificacionGenerator.GenerarCodigo();
            var registroPendiente = RegistroCuentaPendiente.Crear(nuevaCuenta, codigo, DuracionCodigo, ahora);

            if (!_registroStore.TryAdd(registroPendiente))
            {
                resultado.Mensaje = "No fue posible procesar la solicitud de verificación.";
                return resultado;
            }

            try
            {
                EjecutarEnvioCodigoAsync(registroPendiente.Correo, codigo).GetAwaiter().GetResult();
            }
            catch (SmtpException ex)
            {
                _registroStore.TryRemove(registroPendiente.Token);
                resultado.Mensaje = string.IsNullOrWhiteSpace(ex.Message)
                    ? "No se pudo enviar el código de verificación."
                    : ex.Message;
                return resultado;
            }
            catch (InvalidOperationException ex)
            {
                _registroStore.TryRemove(registroPendiente.Token);
                resultado.Mensaje = string.IsNullOrWhiteSpace(ex.Message)
                    ? "No se pudo enviar el código de verificación."
                    : ex.Message;
                return resultado;
            }
            catch (FormatException ex)
            {
                _registroStore.TryRemove(registroPendiente.Token);
                resultado.Mensaje = string.IsNullOrWhiteSpace(ex.Message)
                    ? "No se pudo enviar el código de verificación."
                    : ex.Message;
                return resultado;
            }
            catch (ConfigurationErrorsException ex)
            {
                _registroStore.TryRemove(registroPendiente.Token);
                resultado.Mensaje = string.IsNullOrWhiteSpace(ex.Message)
                    ? "No se pudo enviar el código de verificación."
                    : ex.Message;
                return resultado;
            }

            resultado.CodigoEnviado = true;
            resultado.TokenCodigo = registroPendiente.Token;
            resultado.Mensaje = "Se envió un código de verificación al correo proporcionado.";

            return resultado;
        }

        public ResultadoSolicitudCodigoDTO ReenviarCodigoVerificacion(ReenviarCodigoDTO solicitud)
        {
            if (solicitud == null)
            {
                throw new ArgumentNullException(nameof(solicitud));
            }

            DateTime ahora = DateTime.UtcNow;
            _registroStore.LimpiarExpirados(ahora);

            var resultado = new ResultadoSolicitudCodigoDTO
            {
                CodigoEnviado = false,
                Mensaje = "La solicitud de verificación no es válida."
            };

            if (!TryObtenerRegistro(solicitud.TokenCodigo, ahora, resultado, out RegistroCuentaPendiente registro))
            {
                return resultado;
            }

            if (!registro.PuedeReenviar(ahora, TiempoEsperaReenvio, out int segundosRestantes))
            {
                resultado.Mensaje = $"Debe esperar {segundosRestantes} segundos para solicitar un nuevo código.";
                return resultado;
            }

            string nuevoCodigo = CodigoVerificacionGenerator.GenerarCodigo();
            registro.ActualizarCodigo(nuevoCodigo, DuracionCodigo, ahora);

            try
            {
                EjecutarEnvioCodigoAsync(registro.Correo, nuevoCodigo).GetAwaiter().GetResult();
            }
            catch (SmtpException ex)
            {
                resultado.Mensaje = string.IsNullOrWhiteSpace(ex.Message)
                    ? "No se pudo reenviar el código de verificación."
                    : ex.Message;
                return resultado;
            }
            catch (InvalidOperationException ex)
            {
                resultado.Mensaje = string.IsNullOrWhiteSpace(ex.Message)
                    ? "No se pudo reenviar el código de verificación."
                    : ex.Message;
                return resultado;
            }
            catch (FormatException ex)
            {
                resultado.Mensaje = string.IsNullOrWhiteSpace(ex.Message)
                    ? "No se pudo reenviar el código de verificación."
                    : ex.Message;
                return resultado;
            }
            catch (ConfigurationErrorsException ex)
            {
                resultado.Mensaje = string.IsNullOrWhiteSpace(ex.Message)
                    ? "No se pudo reenviar el código de verificación."
                    : ex.Message;
                return resultado;
            }

            resultado.CodigoEnviado = true;
            resultado.TokenCodigo = solicitud.TokenCodigo;
            resultado.Mensaje = "Se envió un nuevo código de verificación.";

            return resultado;
        }

        public ResultadoRegistroCuentaDTO ConfirmarCodigoVerificacion(ConfirmarCodigoDTO confirmacion)
        {
            if (confirmacion == null)
            {
                throw new ArgumentNullException(nameof(confirmacion));
            }

            DateTime ahora = DateTime.UtcNow;
            _registroStore.LimpiarExpirados(ahora);

            var resultado = new ResultadoRegistroCuentaDTO
            {
                RegistroExitoso = false,
                Mensaje = "El código de verificación es inválido."
            };

            if (!TryObtenerRegistro(confirmacion.TokenCodigo, ahora, null, out RegistroCuentaPendiente registro))
            {
                return resultado;
            }

            if (string.IsNullOrWhiteSpace(confirmacion.CodigoIngresado))
            {
                return resultado;
            }

            if (!registro.CodigoCoincide(confirmacion.CodigoIngresado))
            {
                resultado.Mensaje = "El código ingresado no es correcto.";
                return resultado;
            }

            resultado = _cuentaRegistroServicio.RegistrarCuentaPendiente(registro);

            if (resultado.RegistroExitoso)
            {
                _registroStore.TryRemove(confirmacion.TokenCodigo);
            }

            return resultado;
        }

        private Task EjecutarEnvioCodigoAsync(string correoDestino, string codigo)
        {
            return _notificador.EnviarCodigoAsync(correoDestino, codigo);
        }

        private bool TryObtenerRegistro(string token, DateTime ahora, ResultadoSolicitudCodigoDTO resultado, out RegistroCuentaPendiente registro)
        {
            registro = null;

            if (string.IsNullOrWhiteSpace(token))
            {
                return false;
            }

            if (!_registroStore.TryGet(token, out registro))
            {
                if (resultado != null)
                {
                    resultado.Mensaje = "No se encontró una solicitud de verificación activa.";
                }
                return false;
            }

            if (registro.EstaExpirado(ahora))
            {
                _registroStore.TryRemove(token);
                if (resultado != null)
                {
                    resultado.Mensaje = "El código de verificación ha expirado. Inicie el proceso nuevamente.";
                }
                return false;
            }

            return true;
        }
    }
}
