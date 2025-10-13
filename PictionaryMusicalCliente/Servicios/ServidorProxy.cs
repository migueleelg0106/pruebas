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
            var dto = new SrvReenv.ReenviarCodigoDTO
            {
                TokenCodigo = solicitud.TokenCodigo
            };

            SrvReenv.ResultadoSolicitudCodigoDTO resultadoDto = await _reenviarCodigo.ReenviarCodigoVerificacionAsync(dto);
            return ConvertirResultadoSolicitudCodigo(resultadoDto);
        }

        public async Task<ResultadoRegistroCuenta> ConfirmarCodigoVerificacionAsync(SolicitudConfirmarCodigo solicitud)
        {
            var dto = new SrvCod.ConfirmarCodigoDTO
            {
                TokenCodigo = solicitud.TokenCodigo,
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
            catch (FaultException<ErrorDetalleServicio>)
            {
                comunicacion?.Abort();
                throw;
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
            var dto = CrearSolicitudRecuperacionDto(solicitud);

            if (dto == null)
            {
                return null;
            }

            SrvCod.ResultadoSolicitudRecuperacionDTO resultadoDto = await _codigoVerificacion.SolicitarCodigoRecuperacionAsync(dto);
            return ConvertirResultadoSolicitudRecuperacion(resultadoDto);
        }

        public async Task<ResultadoSolicitudCodigo> ReenviarCodigoRecuperacionAsync(SolicitudReenviarCodigo solicitud)
        {
            SrvReenv.ReenviarCodigoDTO dto = CrearSolicitudReenviarCodigoDto(solicitud);

            if (dto == null)
            {
                return null;
            }

            SrvReenv.ResultadoSolicitudCodigoDTO resultadoDto = await _reenviarCodigo.ReenviarCodigoRecuperacionAsync(dto);
            return ConvertirResultadoSolicitudCodigo(resultadoDto);
        }

        public async Task<ResultadoOperacion> ConfirmarCodigoRecuperacionAsync(SolicitudConfirmarCodigo solicitud)
        {
            SrvCod.ConfirmarCodigoDTO dto = CrearConfirmarCodigoDto(solicitud);

            if (dto == null)
            {
                return null;
            }

            SrvCod.ResultadoOperacionDTO resultadoDto = await _codigoVerificacion.ConfirmarCodigoRecuperacionAsync(dto);
            return ConvertirResultadoOperacion(resultadoDto);
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
            catch (FaultException<ErrorDetalleServicio>)
            {
                comunicacion?.Abort();
                throw;
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
            catch (FaultException<ErrorDetalleServicio>)
            {
                comunicacion?.Abort();
                throw;
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
            catch (FaultException<ErrorDetalleServicio>)
            {
                comunicacion?.Abort();
                throw;
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
            catch (FaultException<ErrorDetalleServicio>)
            {
                comunicacion?.Abort();
                throw;
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

        private static SrvCod.SolicitudRecuperarCuentaDTO CrearSolicitudRecuperacionDto(SolicitudRecuperarCuenta solicitud)
        {
            if (solicitud == null || string.IsNullOrWhiteSpace(solicitud.Identificador))
            {
                return null;
            }

            return new SrvCod.SolicitudRecuperarCuentaDTO
            {
                Identificador = solicitud.Identificador
            };
        }

        private static SrvReenv.ReenviarCodigoDTO CrearSolicitudReenviarCodigoDto(SolicitudReenviarCodigo solicitud)
        {
            if (solicitud == null || string.IsNullOrWhiteSpace(solicitud.TokenCodigo))
            {
                return null;
            }

            return new SrvReenv.ReenviarCodigoDTO
            {
                TokenCodigo = solicitud.TokenCodigo
            };
        }

        private static SrvCod.ConfirmarCodigoDTO CrearConfirmarCodigoDto(SolicitudConfirmarCodigo solicitud)
        {
            if (solicitud == null || string.IsNullOrWhiteSpace(solicitud.TokenCodigo) || string.IsNullOrWhiteSpace(solicitud.Codigo))
            {
                return null;
            }

            return new SrvCod.ConfirmarCodigoDTO
            {
                TokenCodigo = solicitud.TokenCodigo,
                CodigoIngresado = solicitud.Codigo
            };
        }

        private static ActualizarContrasenaDto CrearActualizarContrasenaDto(SolicitudActualizarContrasena solicitud)
        {
            if (solicitud == null || string.IsNullOrWhiteSpace(solicitud.TokenCodigo) || string.IsNullOrWhiteSpace(solicitud.NuevaContrasena))
            {
                return null;
            }

            return new ActualizarContrasenaDto
            {
                TokenCodigo = solicitud.TokenCodigo,
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
                    resultadoDto.TokenCodigo,
                    resultadoDto.CorreoYaRegistrado,
                    resultadoDto.UsuarioYaRegistrado);
        }

        private static ResultadoSolicitudCodigo ConvertirResultadoSolicitudCodigo(SrvReenv.ResultadoSolicitudCodigoDTO resultadoDto)
        {
            return resultadoDto == null
                ? null
                : CrearResultadoSolicitudCodigo(
                    resultadoDto.CodigoEnviado,
                    resultadoDto.Mensaje,
                    resultadoDto.TokenCodigo,
                    resultadoDto.CorreoYaRegistrado,
                    resultadoDto.UsuarioYaRegistrado);
        }

        private static ResultadoSolicitudRecuperacion ConvertirResultadoSolicitudRecuperacion(SrvCod.ResultadoSolicitudRecuperacionDTO resultadoDto)
        {
            return resultadoDto == null
                ? null
                : new ResultadoSolicitudRecuperacion
                {
                    CodigoEnviado = resultadoDto.CodigoEnviado,
                    CuentaEncontrada = resultadoDto.CuentaEncontrada,
                    Mensaje = resultadoDto.Mensaje,
                    TokenCodigo = resultadoDto.TokenCodigo,
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

        private static ResultadoOperacion ConvertirResultadoOperacion(SrvCod.ResultadoOperacionDTO resultadoDto)
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

        private static ResultadoSolicitudCodigo CrearResultadoSolicitudCodigo(bool codigoEnviado, string mensaje, string token, bool correoYaRegistrado, bool usuarioYaRegistrado)
        {
            return new ResultadoSolicitudCodigo
            {
                CodigoEnviado = codigoEnviado,
                Mensaje = mensaje,
                TokenCodigo = token,
                CorreoYaRegistrado = correoYaRegistrado,
                UsuarioYaRegistrado = usuarioYaRegistrado
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
