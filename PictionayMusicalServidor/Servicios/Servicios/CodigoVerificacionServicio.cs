using Datos.DAL.Interfaces;
using Servicios.Contratos.DTOs;
using Servicios.Servicios.Utilidades;
using System;
using System.Configuration;
using System.Net.Mail;
using System.Security.Cryptography;
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

        public CodigoVerificacionServicio(ICuentaRepositorio repositorioCuenta, ICodigoVerificacionNotificador notificador)
        {
            if (repositorioCuenta == null)
            {
                throw new ArgumentNullException(nameof(repositorioCuenta));
            }

            _notificador = notificador ?? throw new ArgumentNullException(nameof(notificador));
            _registroStore = RegistroCuentaPendienteStore.Instancia;
            _cuentaRegistroServicio = new CuentaRegistroServicio(repositorioCuenta);
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

            string codigo = GenerarCodigoVerificacion();
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
            resultado.TokenVerificacion = registroPendiente.Token;
            resultado.Mensaje = "Se envió un código de verificación al correo proporcionado.";

            return resultado;
        }

        public ResultadoSolicitudCodigoDTO ReenviarCodigoVerificacion(ReenviarCodigoVerificacionDTO solicitud)
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

            if (string.IsNullOrWhiteSpace(solicitud.TokenVerificacion))
            {
                return resultado;
            }

            if (!_registroStore.TryGet(solicitud.TokenVerificacion, out RegistroCuentaPendiente registro))
            {
                resultado.Mensaje = "No se encontró una solicitud de verificación activa.";
                return resultado;
            }

            if (registro.EstaExpirado(ahora))
            {
                _registroStore.TryRemove(solicitud.TokenVerificacion);
                resultado.Mensaje = "El código de verificación ha expirado. Inicie el proceso nuevamente.";
                return resultado;
            }

            if (!registro.PuedeReenviar(ahora, TiempoEsperaReenvio, out int segundosRestantes))
            {
                resultado.Mensaje = $"Debe esperar {segundosRestantes} segundos para solicitar un nuevo código.";
                return resultado;
            }

            string nuevoCodigo = GenerarCodigoVerificacion();
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
            resultado.TokenVerificacion = solicitud.TokenVerificacion;
            resultado.Mensaje = "Se envió un nuevo código de verificación.";

            return resultado;
        }

        public ResultadoRegistroCuentaDTO ConfirmarCodigoVerificacion(ConfirmarCodigoVerificacionDTO confirmacion)
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

            if (string.IsNullOrWhiteSpace(confirmacion.TokenVerificacion) || string.IsNullOrWhiteSpace(confirmacion.CodigoIngresado))
            {
                return resultado;
            }

            if (!_registroStore.TryGet(confirmacion.TokenVerificacion, out RegistroCuentaPendiente registro))
            {
                resultado.Mensaje = "No hay una solicitud de verificación vigente.";
                return resultado;
            }

            if (registro.EstaExpirado(ahora))
            {
                _registroStore.TryRemove(confirmacion.TokenVerificacion);
                resultado.Mensaje = "El código de verificación ha expirado.";
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
                _registroStore.TryRemove(confirmacion.TokenVerificacion);
            }

            return resultado;
        }

        private static string GenerarCodigoVerificacion()
        {
            var bytes = new byte[4];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(bytes);
            }

            int valor = BitConverter.ToInt32(bytes, 0) & int.MaxValue;
            int codigo = valor % 1000000;
            return codigo.ToString("D6");
        }

        private Task EjecutarEnvioCodigoAsync(string correoDestino, string codigo)
        {
            return _notificador.EnviarCodigoAsync(correoDestino, codigo);
        }
    }
}
