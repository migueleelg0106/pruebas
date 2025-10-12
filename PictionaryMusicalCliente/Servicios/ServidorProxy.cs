using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Threading.Tasks;
using PictionaryMusicalCliente.Modelo;

// alias de tus connected services
using SrvAv = PictionaryMusicalCliente.PictionaryServidorServicioAvatares;
using SrvCod = PictionaryMusicalCliente.PictionaryServidorServicioCodigoVerificacion;
using SrvCta = PictionaryMusicalCliente.PictionaryServidorServicioCuenta;
using SrvReenv = PictionaryMusicalCliente.PictionaryServidorServicioReenvioCodigoVerificacion;

namespace PictionaryMusicalCliente.Servicios
{
    public sealed partial class ServidorProxy : IDisposable
    {
        private readonly SrvAv.CatalogoAvataresClient _avatares;
        private readonly SrvCta.CuentaManejadorClient _cuentas;
        private readonly SrvCod.CodigoVerificacionManejadorClient _codigoVerificacion;
        private readonly SrvReenv.ReenviarCodigoVerificacionManejadorClient _reenviarCodigo;
        private readonly ChannelFactory<IInicioSesionManejadorContract> _inicioSesionFactory;
        private readonly ChannelFactory<ICambiarContrasenaManejadorContract> _cambiarContrasenaFactory;
        private readonly ChannelFactory<IClasificacionManejadorContract> _clasificacionFactory;
        private readonly ChannelFactory<IPerfilManejadorContract> _perfilFactory;

        private const string BaseImagenesRemotas = "http://localhost:8086/";
        private const string InicioSesionEndpoint = "http://localhost:8086/Pictionary/InicioSesion/InicioSesion";
        private const string CambiarContrasenaEndpoint = "http://localhost:8086/Pictionary/CambiarContrasena/CambiarContrasena";
        private const string ClasificacionEndpoint = "http://localhost:8086/Pictionary/Clasificacion/Clasificacion";
        private const string PerfilEndpoint = "http://localhost:8086/Pictionary/Perfil/Perfil";

        public ServidorProxy()
        {
            _avatares = new SrvAv.CatalogoAvataresClient("BasicHttpBinding_ICatalogoAvatares");
            _cuentas = new SrvCta.CuentaManejadorClient("BasicHttpBinding_ICuentaManejador");
            _codigoVerificacion = new SrvCod.CodigoVerificacionManejadorClient("BasicHttpBinding_ICodigoVerificacionManejador");
            _reenviarCodigo = new SrvReenv.ReenviarCodigoVerificacionManejadorClient("BasicHttpBinding_IReenviarCodigoVerificacionManejador");
            _inicioSesionFactory = new ChannelFactory<IInicioSesionManejadorContract>(new BasicHttpBinding(), new EndpointAddress(InicioSesionEndpoint));
            _cambiarContrasenaFactory = new ChannelFactory<ICambiarContrasenaManejadorContract>(new BasicHttpBinding(), new EndpointAddress(CambiarContrasenaEndpoint));
            _clasificacionFactory = new ChannelFactory<IClasificacionManejadorContract>(new BasicHttpBinding(), new EndpointAddress(ClasificacionEndpoint));
            _perfilFactory = new ChannelFactory<IPerfilManejadorContract>(new BasicHttpBinding(), new EndpointAddress(PerfilEndpoint));
        }

        public async Task<List<ObjetoAvatar>> ObtenerAvataresAsync()
        {
            var dtos = await _avatares.ObtenerAvataresDisponiblesAsync();
            return dtos.Select(d => new ObjetoAvatar
            {
                Id = d.Id,
                Nombre = d.Nombre,
                RutaRelativa = d.RutaRelativa,
                ImagenUriAbsoluta = ObtenerRutaAbsoluta(d.RutaRelativa)
            }).ToList();
        }

        public async Task<ResultadoSolicitudCodigo> SolicitarCodigoVerificacionAsync(SolicitudRegistrarUsuario solicitud)
        {
            var dto = CrearNuevaCuentaDtoVerificacion(solicitud);
            SrvCod.ResultadoSolicitudCodigoDTO resultadoDto = await _codigoVerificacion.SolicitarCodigoVerificacionAsync(dto);
            return ConvertirResultadoSolicitudCodigo(resultadoDto);
        }

        public async Task<ResultadoSolicitudCodigo> ReenviarCodigoVerificacionAsync(SolicitudReenviarCodigo solicitud)
        {
            var dto = new SrvReenv.ReenviarCodigoVerificacionDTO
            {
                TokenVerificacion = solicitud.TokenVerificacion
            };

            SrvReenv.ResultadoSolicitudCodigoDTO resultadoDto = await _reenviarCodigo.ReenviarCodigoVerificacionAsync(dto);
            return ConvertirResultadoSolicitudCodigo(resultadoDto);
        }

        public async Task<ResultadoRegistroCuenta> ConfirmarCodigoVerificacionAsync(SolicitudConfirmarCodigo solicitud)
        {
            var dto = new SrvCod.ConfirmarCodigoVerificacionDTO
            {
                TokenVerificacion = solicitud.TokenVerificacion,
                CodigoIngresado = solicitud.Codigo
            };

            SrvCod.ResultadoRegistroCuentaDTO resultadoDto = await _codigoVerificacion.ConfirmarCodigoVerificacionAsync(dto);
            return ConvertirResultadoRegistroCuenta(resultadoDto);
        }

        public async Task<ResultadoRegistroCuenta> RegistrarCuentaAsync(SolicitudRegistrarUsuario solicitud)
        {
            var dto = CrearNuevaCuentaDtoCuenta(solicitud);
            SrvCta.ResultadoRegistroCuentaDTO resultadoDto = await _cuentas.RegistrarCuentaAsync(dto);
            return ConvertirResultadoRegistroCuenta(resultadoDto);
        }

        public async Task<ResultadoInicioSesion> IniciarSesionAsync(SolicitudInicioSesion solicitud)
        {
            if (_inicioSesionFactory == null)
            {
                throw new InvalidOperationException("El canal de inicio de sesión no está disponible.");
            }

            CredencialesInicioSesionDto dto = CrearCredencialesInicioSesionDto(solicitud);

            if (dto == null)
            {
                return null;
            }

            IInicioSesionManejadorContract canal = _inicioSesionFactory.CreateChannel();
            var comunicacion = canal as ICommunicationObject;

            try
            {
                ResultadoInicioSesionDto resultadoDto = await Task.Run(() => canal.IniciarSesion(dto));
                comunicacion?.Close();
                return ConvertirResultadoInicioSesion(resultadoDto);
            }
            catch (CommunicationException)
            {
                comunicacion?.Abort();
                throw;
            }
            catch (TimeoutException)
            {
                comunicacion?.Abort();
                throw;
            }
            catch (InvalidOperationException)
            {
                comunicacion?.Abort();
                throw;
            }
        }

        public async Task<ResultadoSolicitudRecuperacion> SolicitarCodigoRecuperacionAsync(SolicitudRecuperarCuenta solicitud)
        {
            if (_cambiarContrasenaFactory == null)
            {
                throw new InvalidOperationException("El canal de recuperación de contraseña no está disponible.");
            }

            SolicitudRecuperacionDto dto = CrearSolicitudRecuperacionDto(solicitud);

            if (dto == null)
            {
                return null;
            }

            ICambiarContrasenaManejadorContract canal = _cambiarContrasenaFactory.CreateChannel();
            var comunicacion = canal as ICommunicationObject;

            try
            {
                ResultadoSolicitudRecuperacionDto resultadoDto = await Task.Run(() => canal.SolicitarCodigoRecuperacion(dto));
                comunicacion?.Close();
                return ConvertirResultadoSolicitudRecuperacion(resultadoDto);
            }
            catch (CommunicationException)
            {
                comunicacion?.Abort();
                throw;
            }
            catch (TimeoutException)
            {
                comunicacion?.Abort();
                throw;
            }
            catch (InvalidOperationException)
            {
                comunicacion?.Abort();
                throw;
            }
        }

        public async Task<ResultadoSolicitudCodigo> ReenviarCodigoRecuperacionAsync(SolicitudReenviarCodigoRecuperacion solicitud)
        {
            if (_cambiarContrasenaFactory == null)
            {
                throw new InvalidOperationException("El canal de recuperación de contraseña no está disponible.");
            }

            SolicitudReenviarCodigoRecuperacionDto dto = CrearSolicitudReenviarCodigoRecuperacionDto(solicitud);

            if (dto == null)
            {
                return null;
            }

            ICambiarContrasenaManejadorContract canal = _cambiarContrasenaFactory.CreateChannel();
            var comunicacion = canal as ICommunicationObject;

            try
            {
                ResultadoSolicitudCodigoDto resultadoDto = await Task.Run(() => canal.ReenviarCodigoRecuperacion(dto));
                comunicacion?.Close();
                return ConvertirResultadoSolicitudCodigo(resultadoDto);
            }
            catch (CommunicationException)
            {
                comunicacion?.Abort();
                throw;
            }
            catch (TimeoutException)
            {
                comunicacion?.Abort();
                throw;
            }
            catch (InvalidOperationException)
            {
                comunicacion?.Abort();
                throw;
            }
        }

        public async Task<ResultadoOperacion> ConfirmarCodigoRecuperacionAsync(SolicitudConfirmarCodigoRecuperacion solicitud)
        {
            if (_cambiarContrasenaFactory == null)
            {
                throw new InvalidOperationException("El canal de recuperación de contraseña no está disponible.");
            }

            ConfirmarCodigoRecuperacionDto dto = CrearConfirmarCodigoRecuperacionDto(solicitud);

            if (dto == null)
            {
                return null;
            }

            ICambiarContrasenaManejadorContract canal = _cambiarContrasenaFactory.CreateChannel();
            var comunicacion = canal as ICommunicationObject;

            try
            {
                ResultadoOperacionDto resultadoDto = await Task.Run(() => canal.ConfirmarCodigoRecuperacion(dto));
                comunicacion?.Close();
                return ConvertirResultadoOperacion(resultadoDto);
            }
            catch (CommunicationException)
            {
                comunicacion?.Abort();
                throw;
            }
            catch (TimeoutException)
            {
                comunicacion?.Abort();
                throw;
            }
            catch (InvalidOperationException)
            {
                comunicacion?.Abort();
                throw;
            }
        }

        public async Task<ResultadoOperacion> ActualizarContrasenaAsync(SolicitudActualizarContrasena solicitud)
        {
            if (_cambiarContrasenaFactory == null)
            {
                throw new InvalidOperationException("El canal de recuperación de contraseña no está disponible.");
            }

            ActualizarContrasenaDto dto = CrearActualizarContrasenaDto(solicitud);

            if (dto == null)
            {
                return null;
            }

            ICambiarContrasenaManejadorContract canal = _cambiarContrasenaFactory.CreateChannel();
            var comunicacion = canal as ICommunicationObject;

            try
            {
                ResultadoOperacionDto resultadoDto = await Task.Run(() => canal.ActualizarContrasena(dto));
                comunicacion?.Close();
                return ConvertirResultadoOperacion(resultadoDto);
            }
            catch (CommunicationException)
            {
                comunicacion?.Abort();
                throw;
            }
            catch (TimeoutException)
            {
                comunicacion?.Abort();
                throw;
            }
            catch (InvalidOperationException)
            {
                comunicacion?.Abort();
                throw;
            }
        }

        public async Task<List<EntradaClasificacion>> ObtenerClasificacionAsync()
        {
            if (_clasificacionFactory == null)
            {
                throw new InvalidOperationException("El canal de clasificación no está disponible.");
            }

            IClasificacionManejadorContract canal = _clasificacionFactory.CreateChannel();
            var comunicacion = canal as ICommunicationObject;

            try
            {
                ClasificacionUsuarioDto[] resultadoDto = await Task.Run(() => canal.ObtenerTopJugadores());
                comunicacion?.Close();
                return ConvertirClasificacion(resultadoDto);
            }
            catch (CommunicationException)
            {
                comunicacion?.Abort();
                throw;
            }
            catch (TimeoutException)
            {
                comunicacion?.Abort();
                throw;
            }
            catch (InvalidOperationException)
            {
                comunicacion?.Abort();
                throw;
            }
        }

        public async Task<UsuarioAutenticado> ObtenerPerfilAsync(int idUsuario)
        {
            if (_perfilFactory == null)
            {
                throw new InvalidOperationException("El canal de perfil no está disponible.");
            }

            if (idUsuario <= 0)
            {
                return null;
            }

            IPerfilManejadorContract canal = _perfilFactory.CreateChannel();
            var comunicacion = canal as ICommunicationObject;

            try
            {
                UsuarioDto resultadoDto = await Task.Run(() => canal.ObtenerPerfil(idUsuario));
                comunicacion?.Close();
                return ConvertirUsuario(resultadoDto);
            }
            catch (CommunicationException)
            {
                comunicacion?.Abort();
                throw;
            }
            catch (TimeoutException)
            {
                comunicacion?.Abort();
                throw;
            }
            catch (InvalidOperationException)
            {
                comunicacion?.Abort();
                throw;
            }
        }

        public async Task<ResultadoOperacion> ActualizarPerfilAsync(SolicitudActualizarPerfil solicitud)
        {
            if (_perfilFactory == null)
            {
                throw new InvalidOperationException("El canal de perfil no está disponible.");
            }

            ActualizarPerfilDto dto = CrearActualizarPerfilDto(solicitud);

            if (dto == null)
            {
                return null;
            }

            IPerfilManejadorContract canal = _perfilFactory.CreateChannel();
            var comunicacion = canal as ICommunicationObject;

            try
            {
                ResultadoOperacionDto resultadoDto = await Task.Run(() => canal.ActualizarPerfil(dto));
                comunicacion?.Close();
                return ConvertirResultadoOperacion(resultadoDto);
            }
            catch (CommunicationException)
            {
                comunicacion?.Abort();
                throw;
            }
            catch (TimeoutException)
            {
                comunicacion?.Abort();
                throw;
            }
            catch (InvalidOperationException)
            {
                comunicacion?.Abort();
                throw;
            }
        }

        private static string ObtenerRutaAbsoluta(string rutaRelativa)
        {
            if (string.IsNullOrWhiteSpace(rutaRelativa))
            {
                return null;
            }

            if (Uri.TryCreate(rutaRelativa, UriKind.Absolute, out Uri uriAbsoluta))
            {
                return uriAbsoluta.ToString();
            }

            string rutaNormalizada = rutaRelativa.TrimStart('/');

            if (!string.IsNullOrEmpty(BaseImagenesRemotas)
                && Uri.TryCreate(BaseImagenesRemotas, UriKind.Absolute, out Uri baseUri))
            {
                return new Uri(baseUri, rutaNormalizada).ToString();
            }

            return null;
        }

        public void Dispose()
        {
            CerrarCliente(_avatares);
            CerrarCliente(_cuentas);
            CerrarCliente(_codigoVerificacion);
            CerrarCliente(_reenviarCodigo);
            CerrarCliente(_inicioSesionFactory);
            CerrarCliente(_cambiarContrasenaFactory);
            CerrarCliente(_clasificacionFactory);
            CerrarCliente(_perfilFactory);
        }

        private static SrvCod.NuevaCuentaDTO CrearNuevaCuentaDtoVerificacion(SolicitudRegistrarUsuario solicitud)
        {
            if (solicitud == null)
            {
                return null;
            }

            return new SrvCod.NuevaCuentaDTO
            {
                Correo = solicitud.Correo,
                Contrasena = solicitud.ContrasenaPlano,
                Usuario = solicitud.Usuario,
                Nombre = solicitud.Nombre,
                Apellido = solicitud.Apellido,
                AvatarId = solicitud.AvatarId
            };
        }

        private static SrvCta.NuevaCuentaDTO CrearNuevaCuentaDtoCuenta(SolicitudRegistrarUsuario solicitud)
        {
            if (solicitud == null)
            {
                return null;
            }

            return new SrvCta.NuevaCuentaDTO
            {
                Correo = solicitud.Correo,
                Contrasena = solicitud.ContrasenaPlano,
                Usuario = solicitud.Usuario,
                Nombre = solicitud.Nombre,
                Apellido = solicitud.Apellido,
                AvatarId = solicitud.AvatarId
            };
        }

        private static SolicitudRecuperacionDto CrearSolicitudRecuperacionDto(SolicitudRecuperarCuenta solicitud)
        {
            if (solicitud == null || string.IsNullOrWhiteSpace(solicitud.Identificador))
            {
                return null;
            }

            return new SolicitudRecuperacionDto
            {
                Identificador = solicitud.Identificador
            };
        }

        private static SolicitudReenviarCodigoRecuperacionDto CrearSolicitudReenviarCodigoRecuperacionDto(SolicitudReenviarCodigoRecuperacion solicitud)
        {
            if (solicitud == null || string.IsNullOrWhiteSpace(solicitud.TokenRecuperacion))
            {
                return null;
            }

            return new SolicitudReenviarCodigoRecuperacionDto
            {
                TokenRecuperacion = solicitud.TokenRecuperacion
            };
        }

        private static ConfirmarCodigoRecuperacionDto CrearConfirmarCodigoRecuperacionDto(SolicitudConfirmarCodigoRecuperacion solicitud)
        {
            if (solicitud == null || string.IsNullOrWhiteSpace(solicitud.TokenRecuperacion) || string.IsNullOrWhiteSpace(solicitud.Codigo))
            {
                return null;
            }

            return new ConfirmarCodigoRecuperacionDto
            {
                TokenRecuperacion = solicitud.TokenRecuperacion,
                CodigoIngresado = solicitud.Codigo
            };
        }

        private static ActualizarContrasenaDto CrearActualizarContrasenaDto(SolicitudActualizarContrasena solicitud)
        {
            if (solicitud == null || string.IsNullOrWhiteSpace(solicitud.TokenRecuperacion) || string.IsNullOrWhiteSpace(solicitud.NuevaContrasena))
            {
                return null;
            }

            return new ActualizarContrasenaDto
            {
                TokenRecuperacion = solicitud.TokenRecuperacion,
                NuevaContrasena = solicitud.NuevaContrasena
            };
        }

        private static ResultadoSolicitudCodigo ConvertirResultadoSolicitudCodigo(SrvCod.ResultadoSolicitudCodigoDTO resultadoDto)
        {
            return resultadoDto == null
                ? null
                : CrearResultadoSolicitudCodigo(
                    resultadoDto.CodigoEnviado,
                    resultadoDto.Mensaje,
                    resultadoDto.TokenVerificacion,
                    resultadoDto.CorreoYaRegistrado,
                    resultadoDto.UsuarioYaRegistrado,
                    null);
        }

        private static ResultadoSolicitudCodigo ConvertirResultadoSolicitudCodigo(SrvReenv.ResultadoSolicitudCodigoDTO resultadoDto)
        {
            return resultadoDto == null
                ? null
                : CrearResultadoSolicitudCodigo(
                    resultadoDto.CodigoEnviado,
                    resultadoDto.Mensaje,
                    resultadoDto.TokenVerificacion,
                    resultadoDto.CorreoYaRegistrado,
                    resultadoDto.UsuarioYaRegistrado,
                    null);
        }

        private static ResultadoSolicitudCodigo ConvertirResultadoSolicitudCodigo(ResultadoSolicitudCodigoDto resultadoDto)
        {
            return resultadoDto == null
                ? null
                : CrearResultadoSolicitudCodigo(
                    resultadoDto.CodigoEnviado,
                    resultadoDto.Mensaje,
                    resultadoDto.TokenRecuperacion ?? resultadoDto.TokenVerificacion,
                    false,
                    false,
                    resultadoDto.TokenRecuperacion);
        }

        private static ResultadoSolicitudRecuperacion ConvertirResultadoSolicitudRecuperacion(ResultadoSolicitudRecuperacionDto resultadoDto)
        {
            return resultadoDto == null
                ? null
                : new ResultadoSolicitudRecuperacion
                {
                    CodigoEnviado = resultadoDto.CodigoEnviado,
                    CuentaEncontrada = resultadoDto.CuentaEncontrada,
                    Mensaje = resultadoDto.Mensaje,
                    TokenRecuperacion = resultadoDto.TokenRecuperacion,
                    CorreoDestino = resultadoDto.CorreoDestino
                };
        }

        private static ResultadoOperacion ConvertirResultadoOperacion(ResultadoOperacionDto resultadoDto)
        {
            return resultadoDto == null
                ? null
                : new ResultadoOperacion
                {
                    OperacionExitosa = resultadoDto.OperacionExitosa,
                    Mensaje = resultadoDto.Mensaje
                };
        }

        private static List<EntradaClasificacion> ConvertirClasificacion(ClasificacionUsuarioDto[] clasificacionDto)
        {
            if (clasificacionDto == null || clasificacionDto.Length == 0)
            {
                return new List<EntradaClasificacion>();
            }

            return clasificacionDto
                .Select(entrada => new EntradaClasificacion
                {
                    Usuario = entrada.Usuario,
                    Puntos = entrada.Puntos,
                    RondasGanadas = entrada.RondasGanadas
                })
                .ToList();
        }

        private static ResultadoSolicitudCodigo CrearResultadoSolicitudCodigo(bool codigoEnviado, string mensaje, string token, bool correoYaRegistrado, bool usuarioYaRegistrado, string tokenRecuperacion)
        {
            return new ResultadoSolicitudCodigo
            {
                CodigoEnviado = codigoEnviado,
                Mensaje = mensaje,
                TokenVerificacion = token,
                CorreoYaRegistrado = correoYaRegistrado,
                UsuarioYaRegistrado = usuarioYaRegistrado,
                TokenRecuperacion = tokenRecuperacion
            };
        }

        private static ResultadoRegistroCuenta ConvertirResultadoRegistroCuenta(SrvCod.ResultadoRegistroCuentaDTO resultadoDto)
        {
            return resultadoDto == null
                ? null
                : CrearResultadoRegistroCuenta(
                    resultadoDto.RegistroExitoso,
                    resultadoDto.Mensaje,
                    resultadoDto.CorreoYaRegistrado,
                    resultadoDto.UsuarioYaRegistrado);
        }

        private static ResultadoRegistroCuenta ConvertirResultadoRegistroCuenta(SrvCta.ResultadoRegistroCuentaDTO resultadoDto)
        {
            return resultadoDto == null
                ? null
                : CrearResultadoRegistroCuenta(
                    resultadoDto.RegistroExitoso,
                    resultadoDto.Mensaje,
                    resultadoDto.CorreoYaRegistrado,
                    resultadoDto.UsuarioYaRegistrado);
        }

        private static ResultadoRegistroCuenta CrearResultadoRegistroCuenta(bool registroExitoso, string mensaje, bool correoYaRegistrado, bool usuarioYaRegistrado)
        {
            return new ResultadoRegistroCuenta
            {
                RegistroExitoso = registroExitoso,
                Mensaje = mensaje,
                CorreoYaRegistrado = correoYaRegistrado,
                UsuarioYaRegistrado = usuarioYaRegistrado
            };
        }

        private static ResultadoInicioSesion ConvertirResultadoInicioSesion(ResultadoInicioSesionDto resultadoDto)
        {
            return resultadoDto == null
                ? null
                : new ResultadoInicioSesion
                {
                    InicioSesionExitoso = resultadoDto.InicioSesionExitoso,
                    Mensaje = resultadoDto.Mensaje,
                    CuentaNoEncontrada = resultadoDto.CuentaNoEncontrada,
                    ContrasenaIncorrecta = resultadoDto.ContrasenaIncorrecta,
                    Usuario = ConvertirUsuario(resultadoDto.Usuario)
                };
        }

        private static UsuarioAutenticado ConvertirUsuario(UsuarioDto usuarioDto)
        {
            return usuarioDto == null
                ? null
                : new UsuarioAutenticado
                {
                    IdUsuario = usuarioDto.IdUsuario,
                    JugadorId = usuarioDto.JugadorId,
                    NombreUsuario = usuarioDto.NombreUsuario,
                    Nombre = usuarioDto.Nombre,
                    Apellido = usuarioDto.Apellido,
                    Correo = usuarioDto.Correo,
                    AvatarId = usuarioDto.AvatarId,
                    Instagram = usuarioDto.Instagram,
                    Facebook = usuarioDto.Facebook,
                    X = usuarioDto.X,
                    Discord = usuarioDto.Discord
                };
        }

        private static CredencialesInicioSesionDto CrearCredencialesInicioSesionDto(SolicitudInicioSesion solicitud)
        {
            if (solicitud == null)
            {
                return null;
            }

            return new CredencialesInicioSesionDto
            {
                Identificador = solicitud.Identificador,
                Contrasena = solicitud.Contrasena
            };
        }

        private static ActualizarPerfilDto CrearActualizarPerfilDto(SolicitudActualizarPerfil solicitud)
        {
            if (solicitud == null)
            {
                return null;
            }

            return new ActualizarPerfilDto
            {
                UsuarioId = solicitud.UsuarioId,
                Nombre = solicitud.Nombre,
                Apellido = solicitud.Apellido,
                AvatarId = solicitud.AvatarId,
                Instagram = solicitud.Instagram,
                Facebook = solicitud.Facebook,
                X = solicitud.X,
                Discord = solicitud.Discord
            };
        }

        private static void CerrarCliente(ICommunicationObject cliente)
        {
            if (cliente == null)
            {
                return;
            }

            try
            {
                if (cliente.State == CommunicationState.Opened)
                {
                    cliente.Close();
                }
                else
                {
                    cliente.Abort();
                }
            }
            catch (CommunicationException)
            {
                cliente.Abort();
            }
            catch (TimeoutException)
            {
                cliente.Abort();
            }
            catch (InvalidOperationException)
            {
                cliente.Abort();
            }
        }
    }
}
