using System.Runtime.Serialization;
using System.ServiceModel;

namespace PictionaryMusicalCliente.Servicios
{
    public sealed partial class ServidorProxy
    {
        [ServiceContract(Name = "ICambiarContrasenaManejador", Namespace = "http://tempuri.org/", ConfigurationName = "ICambiarContrasenaManejador")]
        private interface ICambiarContrasenaManejadorContract
        {
            [OperationContract(Action = "http://tempuri.org/ICambiarContrasenaManejador/SolicitarCodigoRecuperacion", ReplyAction = "http://tempuri.org/ICambiarContrasenaManejador/SolicitarCodigoRecuperacionResponse")]
            [FaultContract(typeof(ErrorDetalleServicio), Action = "http://tempuri.org/ICambiarContrasenaManejador/SolicitarCodigoRecuperacionErrorDetalleServicioDTOFault", Name = "ErrorDetalleServicioDTO")]
            ResultadoSolicitudRecuperacionDto SolicitarCodigoRecuperacion(SolicitudRecuperacionDto solicitud);

            [OperationContract(Action = "http://tempuri.org/ICambiarContrasenaManejador/ReenviarCodigoRecuperacion", ReplyAction = "http://tempuri.org/ICambiarContrasenaManejador/ReenviarCodigoRecuperacionResponse")]
            [FaultContract(typeof(ErrorDetalleServicio), Action = "http://tempuri.org/ICambiarContrasenaManejador/ReenviarCodigoRecuperacionErrorDetalleServicioDTOFault", Name = "ErrorDetalleServicioDTO")]
            ResultadoSolicitudCodigoDto ReenviarCodigoRecuperacion(SolicitudReenviarCodigoRecuperacionDto solicitud);

            [OperationContract(Action = "http://tempuri.org/ICambiarContrasenaManejador/ConfirmarCodigoRecuperacion", ReplyAction = "http://tempuri.org/ICambiarContrasenaManejador/ConfirmarCodigoRecuperacionResponse")]
            [FaultContract(typeof(ErrorDetalleServicio), Action = "http://tempuri.org/ICambiarContrasenaManejador/ConfirmarCodigoRecuperacionErrorDetalleServicioDTOFault", Name = "ErrorDetalleServicioDTO")]
            ResultadoOperacionDto ConfirmarCodigoRecuperacion(ConfirmarCodigoRecuperacionDto confirmacion);

            [OperationContract(Action = "http://tempuri.org/ICambiarContrasenaManejador/ActualizarContrasena", ReplyAction = "http://tempuri.org/ICambiarContrasenaManejador/ActualizarContrasenaResponse")]
            [FaultContract(typeof(ErrorDetalleServicio), Action = "http://tempuri.org/ICambiarContrasenaManejador/ActualizarContrasenaErrorDetalleServicioDTOFault", Name = "ErrorDetalleServicioDTO")]
            ResultadoOperacionDto ActualizarContrasena(ActualizarContrasenaDto solicitud);
        }

        [ServiceContract(Name = "IPerfilManejador", Namespace = "http://tempuri.org/", ConfigurationName = "IPerfilManejador")]
        private interface IPerfilManejadorContract
        {
            [OperationContract(Action = "http://tempuri.org/IPerfilManejador/ObtenerPerfil", ReplyAction = "http://tempuri.org/IPerfilManejador/ObtenerPerfilResponse")]
            [FaultContract(typeof(ErrorDetalleServicio), Action = "http://tempuri.org/IPerfilManejador/ObtenerPerfilErrorDetalleServicioDTOFault", Name = "ErrorDetalleServicioDTO")]
            UsuarioDto ObtenerPerfil(int idUsuario);

            [OperationContract(Action = "http://tempuri.org/IPerfilManejador/ActualizarPerfil", ReplyAction = "http://tempuri.org/IPerfilManejador/ActualizarPerfilResponse")]
            [FaultContract(typeof(ErrorDetalleServicio), Action = "http://tempuri.org/IPerfilManejador/ActualizarPerfilErrorDetalleServicioDTOFault", Name = "ErrorDetalleServicioDTO")]
            ResultadoOperacionDto ActualizarPerfil(ActualizarPerfilDto solicitud);
        }

        [ServiceContract(Name = "IClasificacionManejador", Namespace = "http://tempuri.org/", ConfigurationName = "IClasificacionManejador")]
        private interface IClasificacionManejadorContract
        {
            [OperationContract(Action = "http://tempuri.org/IClasificacionManejador/ObtenerTopJugadores", ReplyAction = "http://tempuri.org/IClasificacionManejador/ObtenerTopJugadoresResponse")]
            [FaultContract(typeof(ErrorDetalleServicio), Action = "http://tempuri.org/IClasificacionManejador/ObtenerTopJugadoresErrorDetalleServicioDTOFault", Name = "ErrorDetalleServicioDTO")]
            ClasificacionUsuarioDto[] ObtenerTopJugadores();
        }

        [ServiceContract(Name = "IInicioSesionManejador", Namespace = "http://tempuri.org/", ConfigurationName = "IInicioSesionManejador")]
        private interface IInicioSesionManejadorContract
        {
            [OperationContract(Action = "http://tempuri.org/IInicioSesionManejador/IniciarSesion", ReplyAction = "http://tempuri.org/IInicioSesionManejador/IniciarSesionResponse")]
            [FaultContract(typeof(ErrorDetalleServicio), Action = "http://tempuri.org/IInicioSesionManejador/IniciarSesionErrorDetalleServicioDTOFault", Name = "ErrorDetalleServicioDTO")]
            ResultadoInicioSesionDto IniciarSesion(CredencialesInicioSesionDto credenciales);
        }

        [DataContract(Name = "ErrorDetalleServicioDTO", Namespace = "http://schemas.datacontract.org/2004/07/Servicios.Contratos.DTOs")]
        public class ErrorDetalleServicio
        {
            [DataMember]
            public string CodigoError { get; set; }

            [DataMember]
            public string Mensaje { get; set; }
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

        [DataContract(Name = "ActualizarPerfilDTO", Namespace = "http://schemas.datacontract.org/2004/07/Servicios.Contratos.DTOs")]
        private class ActualizarPerfilDto
        {
            [DataMember]
            public int UsuarioId { get; set; }

            [DataMember]
            public string Nombre { get; set; }

            [DataMember]
            public string Apellido { get; set; }

            [DataMember]
            public int AvatarId { get; set; }

            [DataMember]
            public string Instagram { get; set; }

            [DataMember]
            public string Facebook { get; set; }

            [DataMember]
            public string X { get; set; }

            [DataMember]
            public string Discord { get; set; }
        }

        [DataContract(Name = "ClasificacionUsuarioDTO", Namespace = "http://schemas.datacontract.org/2004/07/Servicios.Contratos.DTOs")]
        private class ClasificacionUsuarioDto
        {
            [DataMember]
            public string Usuario { get; set; }

            [DataMember]
            public int Puntos { get; set; }

            [DataMember]
            public int RondasGanadas { get; set; }
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

            [DataMember]
            public string Instagram { get; set; }

            [DataMember]
            public string Facebook { get; set; }

            [DataMember]
            public string X { get; set; }

            [DataMember]
            public string Discord { get; set; }
        }
    }
}
