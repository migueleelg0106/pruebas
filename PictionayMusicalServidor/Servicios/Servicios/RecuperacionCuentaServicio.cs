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
    internal class RecuperacionCuentaServicio
    {
        private readonly ICodigoVerificacionNotificador _notificador;
        private readonly ICuentaRepositorio _cuentaRepositorio;
        private readonly RecuperacionCuentaStore _store;
        private static readonly TimeSpan DuracionCodigo = TimeSpan.FromMinutes(5);
        private static readonly TimeSpan TiempoEsperaReenvio = TimeSpan.FromMinutes(1);
        private static readonly TimeSpan DuracionTokenConfirmado = TimeSpan.FromMinutes(10);

        public RecuperacionCuentaServicio(ICuentaRepositorio cuentaRepositorio, ICodigoVerificacionNotificador notificador)
        {
            _cuentaRepositorio = cuentaRepositorio ?? throw new ArgumentNullException(nameof(cuentaRepositorio));
            _notificador = notificador ?? throw new ArgumentNullException(nameof(notificador));
            _store = RecuperacionCuentaStore.Instancia;
        }

        public ResultadoSolicitudRecuperacionDTO SolicitarCodigoRecuperacion(SolicitudRecuperarCuentaDTO solicitud)
        {
            var resultado = new ResultadoSolicitudRecuperacionDTO
            {
                CodigoEnviado = false,
                CuentaEncontrada = false,
                Mensaje = "Debe proporcionar el usuario o correo registrado."
            };

            if (solicitud == null || string.IsNullOrWhiteSpace(solicitud.Identificador))
            {
                return resultado;
            }

            DateTime ahora = DateTime.UtcNow;
            _store.LimpiarExpirados(ahora);

            if (!_cuentaRepositorio.TryObtenerCuentaPorIdentificador(solicitud.Identificador, out int idUsuario, out string correo))
            {
                resultado.Mensaje = "No se encontró una cuenta con el usuario o correo proporcionado.";
                return resultado;
            }

            resultado.CuentaEncontrada = true;
            resultado.CorreoDestino = correo;

            _store.RemoverPorUsuario(idUsuario);

            string codigo = GenerarCodigo();
            var registro = RecuperacionCuentaPendiente.Crear(idUsuario, correo, codigo, DuracionCodigo, ahora);

            if (!_store.TryAdd(registro))
            {
                resultado.Mensaje = "No fue posible iniciar la recuperación de la cuenta.";
                return resultado;
            }

            try
            {
                EjecutarEnvioCodigoAsync(registro.Correo, codigo).GetAwaiter().GetResult();
            }
            catch (SmtpException ex)
            {
                _store.TryRemove(registro.Token);
                resultado.Mensaje = string.IsNullOrWhiteSpace(ex.Message)
                    ? "No se pudo enviar el código de verificación."
                    : ex.Message;
                return resultado;
            }
            catch (InvalidOperationException ex)
            {
                _store.TryRemove(registro.Token);
                resultado.Mensaje = string.IsNullOrWhiteSpace(ex.Message)
                    ? "No se pudo enviar el código de verificación."
                    : ex.Message;
                return resultado;
            }
            catch (FormatException ex)
            {
                _store.TryRemove(registro.Token);
                resultado.Mensaje = string.IsNullOrWhiteSpace(ex.Message)
                    ? "No se pudo enviar el código de verificación."
                    : ex.Message;
                return resultado;
            }
            catch (ConfigurationErrorsException ex)
            {
                _store.TryRemove(registro.Token);
                resultado.Mensaje = string.IsNullOrWhiteSpace(ex.Message)
                    ? "No se pudo enviar el código de verificación."
                    : ex.Message;
                return resultado;
            }

            resultado.CodigoEnviado = true;
            resultado.TokenRecuperacion = registro.Token;
            resultado.Mensaje = "Se envió un código de verificación al correo registrado.";
            return resultado;
        }

        public ResultadoSolicitudCodigoDTO ReenviarCodigoRecuperacion(SolicitudReenviarCodigoRecuperacionDTO solicitud)
        {
            var resultado = new ResultadoSolicitudCodigoDTO
            {
                CodigoEnviado = false,
                Mensaje = "La solicitud de recuperación no es válida."
            };

            if (solicitud == null || string.IsNullOrWhiteSpace(solicitud.TokenRecuperacion))
            {
                return resultado;
            }

            DateTime ahora = DateTime.UtcNow;
            _store.LimpiarExpirados(ahora);

            if (!_store.TryGet(solicitud.TokenRecuperacion, out RecuperacionCuentaPendiente registro))
            {
                resultado.Mensaje = "No se encontró una solicitud de recuperación activa.";
                return resultado;
            }

            if (registro.EstaExpiradoParaVerificacion(ahora))
            {
                _store.TryRemove(solicitud.TokenRecuperacion);
                resultado.Mensaje = "El código de verificación ha expirado. Solicite uno nuevo.";
                return resultado;
            }

            if (!registro.PuedeReenviar(ahora, TiempoEsperaReenvio, out int segundosRestantes))
            {
                resultado.Mensaje = $"Debe esperar {segundosRestantes} segundos para solicitar un nuevo código.";
                return resultado;
            }

            string nuevoCodigo = GenerarCodigo();
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
            resultado.TokenVerificacion = registro.Token;
            resultado.TokenRecuperacion = registro.Token;
            resultado.Mensaje = "Se envió un nuevo código de verificación.";
            return resultado;
        }

        public ResultadoOperacionDTO ConfirmarCodigoRecuperacion(ConfirmarCodigoRecuperacionDTO confirmacion)
        {
            var resultado = new ResultadoOperacionDTO
            {
                OperacionExitosa = false,
                Mensaje = "El código de verificación es inválido."
            };

            if (confirmacion == null || string.IsNullOrWhiteSpace(confirmacion.TokenRecuperacion) || string.IsNullOrWhiteSpace(confirmacion.CodigoIngresado))
            {
                return resultado;
            }

            DateTime ahora = DateTime.UtcNow;
            _store.LimpiarExpirados(ahora);

            if (!_store.TryGet(confirmacion.TokenRecuperacion, out RecuperacionCuentaPendiente registro))
            {
                resultado.Mensaje = "No hay una solicitud de recuperación vigente.";
                return resultado;
            }

            if (registro.EstaExpiradoParaVerificacion(ahora))
            {
                _store.TryRemove(confirmacion.TokenRecuperacion);
                resultado.Mensaje = "El código de verificación ha expirado.";
                return resultado;
            }

            if (!registro.CodigoCoincide(confirmacion.CodigoIngresado))
            {
                resultado.Mensaje = "El código ingresado no es correcto.";
                return resultado;
            }

            registro.MarcarCodigoVerificado(ahora, DuracionTokenConfirmado);
            resultado.OperacionExitosa = true;
            resultado.Mensaje = "Código verificado correctamente. Continúe con el cambio de contraseña.";
            return resultado;
        }

        public ResultadoOperacionDTO ActualizarContrasena(ActualizarContrasenaDTO solicitud)
        {
            var resultado = new ResultadoOperacionDTO
            {
                OperacionExitosa = false,
                Mensaje = "La solicitud de actualización de contraseña no es válida."
            };

            if (solicitud == null || string.IsNullOrWhiteSpace(solicitud.TokenRecuperacion) || string.IsNullOrWhiteSpace(solicitud.NuevaContrasena))
            {
                return resultado;
            }

            DateTime ahora = DateTime.UtcNow;
            _store.LimpiarExpirados(ahora);

            if (!_store.TryGet(solicitud.TokenRecuperacion, out RecuperacionCuentaPendiente registro))
            {
                resultado.Mensaje = "No se encontró una solicitud de recuperación activa.";
                return resultado;
            }

            if (!registro.PuedeActualizar(ahora))
            {
                _store.TryRemove(solicitud.TokenRecuperacion);
                resultado.Mensaje = "El código de verificación ha expirado. Solicite uno nuevo.";
                return resultado;
            }

            string contrasenaHash = BCrypt.Net.BCrypt.HashPassword(solicitud.NuevaContrasena);

            if (!_cuentaRepositorio.ActualizarContrasena(registro.UsuarioId, contrasenaHash))
            {
                resultado.Mensaje = "No fue posible actualizar la contraseña.";
                return resultado;
            }

            _store.TryRemove(solicitud.TokenRecuperacion);

            resultado.OperacionExitosa = true;
            resultado.Mensaje = "La contraseña se actualizó correctamente.";
            return resultado;
        }

        private static string GenerarCodigo()
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
