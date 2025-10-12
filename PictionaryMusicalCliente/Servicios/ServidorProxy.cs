using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
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
    public class ServidorProxy : IDisposable
    {
        private readonly SrvAv.CatalogoAvataresClient _avatares;
        private readonly SrvCta.CuentaManejadorClient _cuentas;
        private readonly SrvCod.CodigoVerificacionManejadorClient _codigoVerificacion;
        private readonly SrvReenv.ReenviarCodigoVerificacionManejadorClient _reenviarCodigo;
        private readonly ChannelFactory<IInicioSesionManejadorContract> _inicioSesionFactory;
        private readonly ChannelFactory<ICambiarContrasenaManejadorContract> _cambiarContrasenaFactory;

        private const string BaseImagenesRemotas = "http://localhost:8086/";
        private const string InicioSesionEndpoint = "http://localhost:8086/Pictionary/InicioSesion/InicioSesion";
        private const string CambiarContrasenaEndpoint = "http://localhost:8086/Pictionary/CambiarContrasena/CambiarContrasena";

        public ServidorProxy()
        {
            _avatares = new SrvAv.CatalogoAvataresClient("BasicHttpBinding_ICatalogoAvatares");
            _cuentas = new SrvCta.CuentaManejadorClient("BasicHttpBinding_ICuentaManejador");
            _codigoVerificacion = new SrvCod.CodigoVerificacionManejadorClient("BasicHttpBinding_ICodigoVerificacionManejador");
            _reenviarCodigo = new SrvReenv.ReenviarCodigoVerificacionManejadorClient("BasicHttpBinding_IReenviarCodigoVerificacionManejador");
            _inicioSesionFactory = new ChannelFactory<IInicioSesionManejadorContract>(new BasicHttpBinding(), new EndpointAddress(InicioSesionEndpoint));
            _cambiarContrasenaFactory = new ChannelFactory<ICambiarContrasenaManejadorContract>(new BasicHttpBinding(), new EndpointAddress(CambiarContrasenaEndpoint));
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
            catch
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
            catch
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
            catch
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
            catch
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
            catch
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
                    AvatarId = usuarioDto.AvatarId
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
            catch
            {
                cliente.Abort();
            }
        }

        [ServiceContract(Name = "ICambiarContrasenaManejador", Namespace = "http://tempuri.org/", ConfigurationName = "ICambiarContrasenaManejador")]
        private interface ICambiarContrasenaManejadorContract
        {
            [OperationContract(Action = "http://tempuri.org/ICambiarContrasenaManejador/SolicitarCodigoRecuperacion", ReplyAction = "http://tempuri.org/ICambiarContrasenaManejador/SolicitarCodigoRecuperacionResponse")]
            ResultadoSolicitudRecuperacionDto SolicitarCodigoRecuperacion(SolicitudRecuperacionDto solicitud);

            [OperationContract(Action = "http://tempuri.org/ICambiarContrasenaManejador/ReenviarCodigoRecuperacion", ReplyAction = "http://tempuri.org/ICambiarContrasenaManejador/ReenviarCodigoRecuperacionResponse")]
            ResultadoSolicitudCodigoDto ReenviarCodigoRecuperacion(SolicitudReenviarCodigoRecuperacionDto solicitud);

            [OperationContract(Action = "http://tempuri.org/ICambiarContrasenaManejador/ConfirmarCodigoRecuperacion", ReplyAction = "http://tempuri.org/ICambiarContrasenaManejador/ConfirmarCodigoRecuperacionResponse")]
            ResultadoOperacionDto ConfirmarCodigoRecuperacion(ConfirmarCodigoRecuperacionDto confirmacion);

            [OperationContract(Action = "http://tempuri.org/ICambiarContrasenaManejador/ActualizarContrasena", ReplyAction = "http://tempuri.org/ICambiarContrasenaManejador/ActualizarContrasenaResponse")]
            ResultadoOperacionDto ActualizarContrasena(ActualizarContrasenaDto solicitud);
        }

        [DataContract(Name = "SolicitudRecuperacionDTO", Namespace = "http://schemas.datacontract.org/2004/07/Servicios.Contratos.DTOs")]
        private class SolicitudRecuperacionDto
        {
            [DataMember]
            public string Identificador { get; set; }
        }

        [DataContract(Name = "SolicitudReenviarCodigoRecuperacionDTO", Namespace = "http://schemas.datacontract.org/2004/07/Servicios.Contratos.DTOs")]
        private class SolicitudReenviarCodigoRecuperacionDto
        {
            [DataMember]
            public string TokenRecuperacion { get; set; }
        }

        [DataContract(Name = "ConfirmarCodigoRecuperacionDTO", Namespace = "http://schemas.datacontract.org/2004/07/Servicios.Contratos.DTOs")]
        private class ConfirmarCodigoRecuperacionDto
        {
            [DataMember]
            public string TokenRecuperacion { get; set; }

            [DataMember]
            public string CodigoIngresado { get; set; }
        }

        [DataContract(Name = "ActualizarContrasenaDTO", Namespace = "http://schemas.datacontract.org/2004/07/Servicios.Contratos.DTOs")]
        private class ActualizarContrasenaDto
        {
            [DataMember]
            public string TokenRecuperacion { get; set; }

            [DataMember]
            public string NuevaContrasena { get; set; }
        }

        [DataContract(Name = "ResultadoSolicitudRecuperacionDTO", Namespace = "http://schemas.datacontract.org/2004/07/Servicios.Contratos.DTOs")]
        private class ResultadoSolicitudRecuperacionDto
        {
            [DataMember]
            public bool CodigoEnviado { get; set; }

            [DataMember]
            public bool CuentaEncontrada { get; set; }

            [DataMember]
            public string Mensaje { get; set; }

            [DataMember]
            public string TokenRecuperacion { get; set; }

            [DataMember]
            public string CorreoDestino { get; set; }
        }

        [DataContract(Name = "ResultadoSolicitudCodigoDTO", Namespace = "http://schemas.datacontract.org/2004/07/Servicios.Contratos.DTOs")]
        private class ResultadoSolicitudCodigoDto
        {
            [DataMember]
            public bool CodigoEnviado { get; set; }

            [DataMember]
            public string Mensaje { get; set; }

            [DataMember]
            public string TokenVerificacion { get; set; }

            [DataMember]
            public string TokenRecuperacion { get; set; }
        }

        [DataContract(Name = "ResultadoOperacionDTO", Namespace = "http://schemas.datacontract.org/2004/07/Servicios.Contratos.DTOs")]
        private class ResultadoOperacionDto
        {
            [DataMember]
            public bool OperacionExitosa { get; set; }

            [DataMember]
            public string Mensaje { get; set; }
        }

        [ServiceContract(Name = "IInicioSesionManejador", Namespace = "http://tempuri.org/", ConfigurationName = "IInicioSesionManejador")]
        private interface IInicioSesionManejadorContract
        {
            [OperationContract(Action = "http://tempuri.org/IInicioSesionManejador/IniciarSesion", ReplyAction = "http://tempuri.org/IInicioSesionManejador/IniciarSesionResponse")]
            ResultadoInicioSesionDto IniciarSesion(CredencialesInicioSesionDto credenciales);
        }

        [DataContract(Name = "CredencialesInicioSesionDTO", Namespace = "http://schemas.datacontract.org/2004/07/Servicios.Contratos.DTOs")]
        private class CredencialesInicioSesionDto
        {
            [DataMember]
            public string Identificador { get; set; }

            [DataMember]
            public string Contrasena { get; set; }
        }

        [DataContract(Name = "ResultadoInicioSesionDTO", Namespace = "http://schemas.datacontract.org/2004/07/Servicios.Contratos.DTOs")]
        private class ResultadoInicioSesionDto
        {
            [DataMember]
            public bool InicioSesionExitoso { get; set; }

            [DataMember]
            public string Mensaje { get; set; }

            [DataMember]
            public bool CuentaNoEncontrada { get; set; }

            [DataMember]
            public bool ContrasenaIncorrecta { get; set; }

            [DataMember]
            public UsuarioDto Usuario { get; set; }
        }

        [DataContract(Name = "UsuarioDTO", Namespace = "http://schemas.datacontract.org/2004/07/Servicios.Contratos.DTOs")]
        private class UsuarioDto
        {
            [DataMember]
            public int IdUsuario { get; set; }

            [DataMember]
            public int JugadorId { get; set; }

            [DataMember]
            public string NombreUsuario { get; set; }

            [DataMember]
            public string Nombre { get; set; }

            [DataMember]
            public string Apellido { get; set; }

            [DataMember]
            public string Correo { get; set; }

            [DataMember]
            public int AvatarId { get; set; }
        }
    }
}
