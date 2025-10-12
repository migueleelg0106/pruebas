using System.Runtime.Serialization;

namespace Servicios.Contratos.DTOs
{
    [DataContract]
    public class ResultadoSolicitudRecuperacionDTO
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
}
