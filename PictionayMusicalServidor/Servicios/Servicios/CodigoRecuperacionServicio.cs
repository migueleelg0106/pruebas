using Datos.DAL.Interfaces;
using Datos.Modelo;
using Servicios.Contratos.DTOs;
using Servicios.Servicios.Utilidades;
using System;
using System.Configuration;
using System.Net.Mail;
using System.Threading.Tasks;

namespace Servicios.Servicios
{
    internal class CodigoRecuperacionServicio
    {
        private readonly ICodigoVerificacionNotificador _notificador;
        private readonly IUsuarioRepositorio _usuarioRepositorio;
        private readonly RecuperacionCuentaStore _store;
        private static readonly TimeSpan DuracionCodigo = TimeSpan.FromMinutes(5);
        private static readonly TimeSpan TiempoEsperaReenvio = TimeSpan.FromMinutes(1);
        private static readonly TimeSpan DuracionTokenConfirmado = TimeSpan.FromMinutes(10);

        public CodigoRecuperacionServicio(IUsuarioRepositorio usuarioRepositorio, ICodigoVerificacionNotificador notificador)
        {
            _usuarioRepositorio = usuarioRepositorio ?? throw new ArgumentNullException(nameof(usuarioRepositorio));
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

            Usuario usuario = _usuarioRepositorio.ObtenerUsuarioPorIdentificador(solicitud.Identificador);

            if (usuario == null)
            {
                resultado.Mensaje = "No se encontró una cuenta con el usuario o correo proporcionado.";
                return resultado;
            }

            int idUsuario = usuario.idUsuario;
            string correo = usuario.Jugador?.Correo;

            if (string.IsNullOrWhiteSpace(correo))
            {
                resultado.Mensaje = "No se encontró un correo electrónico asociado a la cuenta.";
                return resultado;
            }

            resultado.CuentaEncontrada = true;
            resultado.CorreoDestino = correo;

            _store.RemoverPorUsuario(idUsuario);

            string codigo = CodigoVerificacionGenerator.GenerarCodigo();
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
            resultado.TokenCodigo = registro.Token;
            resultado.Mensaje = "Se envió un código de verificación al correo registrado.";
            return resultado;
        }

        public ResultadoSolicitudCodigoDTO ReenviarCodigoRecuperacion(ReenviarCodigoDTO solicitud)
        {
            var resultado = new ResultadoSolicitudCodigoDTO
            {
                CodigoEnviado = false,
                Mensaje = "La solicitud de recuperación no es válida."
            };

            DateTime ahora = DateTime.UtcNow;
            _store.LimpiarExpirados(ahora);

            if (!TryObtenerRegistro(solicitud?.TokenCodigo, ahora, resultado, out RecuperacionCuentaPendiente registro))
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
            resultado.TokenCodigo = registro.Token;
            resultado.Mensaje = "Se envió un nuevo código de verificación.";
            return resultado;
        }

        public ResultadoOperacionDTO ConfirmarCodigoRecuperacion(ConfirmarCodigoDTO confirmacion)
        {
            var resultado = new ResultadoOperacionDTO
            {
                OperacionExitosa = false,
                Mensaje = "El código de verificación es inválido."
            };

            if (confirmacion == null || string.IsNullOrWhiteSpace(confirmacion.TokenCodigo) || string.IsNullOrWhiteSpace(confirmacion.CodigoIngresado))
            {
                return resultado;
            }

            DateTime ahora = DateTime.UtcNow;
            _store.LimpiarExpirados(ahora);

            if (!_store.TryGet(confirmacion.TokenCodigo, out RecuperacionCuentaPendiente registro))
            {
                resultado.Mensaje = "No hay una solicitud de recuperación vigente.";
                return resultado;
            }

            if (registro.EstaExpiradoParaVerificacion(ahora))
            {
                _store.TryRemove(confirmacion.TokenCodigo);
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

            if (solicitud == null || string.IsNullOrWhiteSpace(solicitud.TokenCodigo) || string.IsNullOrWhiteSpace(solicitud.NuevaContrasena))
            {
                return resultado;
            }

            DateTime ahora = DateTime.UtcNow;
            _store.LimpiarExpirados(ahora);

            if (!_store.TryGet(solicitud.TokenCodigo, out RecuperacionCuentaPendiente registro))
            {
                resultado.Mensaje = "No se encontró una solicitud de recuperación activa.";
                return resultado;
            }

            if (!registro.PuedeActualizar(ahora))
            {
                _store.TryRemove(solicitud.TokenCodigo);
                resultado.Mensaje = "El código de verificación ha expirado. Solicite uno nuevo.";
                return resultado;
            }

            string contrasenaHash = BCrypt.Net.BCrypt.HashPassword(solicitud.NuevaContrasena);

            if (!_usuarioRepositorio.ActualizarContrasena(registro.UsuarioId, contrasenaHash))
            {
                resultado.Mensaje = "No fue posible actualizar la contraseña.";
                return resultado;
            }

            _store.TryRemove(solicitud.TokenCodigo);

            resultado.OperacionExitosa = true;
            resultado.Mensaje = "La contraseña se actualizó correctamente.";
            return resultado;
        }

        private Task EjecutarEnvioCodigoAsync(string correoDestino, string codigo)
        {
            return _notificador.EnviarCodigoAsync(correoDestino, codigo);
        }

        private bool TryObtenerRegistro(string token, DateTime ahora, ResultadoSolicitudCodigoDTO resultado, out RecuperacionCuentaPendiente registro)
        {
            registro = null;

            if (string.IsNullOrWhiteSpace(token))
            {
                return false;
            }

            if (!_store.TryGet(token, out registro))
            {
                resultado.Mensaje = "No se encontró una solicitud de recuperación activa.";
                return false;
            }

            if (registro.EstaExpiradoParaVerificacion(ahora))
            {
                _store.TryRemove(token);
                resultado.Mensaje = "El código de verificación ha expirado. Solicite uno nuevo.";
                return false;
            }

            return true;
        }
    }
}
