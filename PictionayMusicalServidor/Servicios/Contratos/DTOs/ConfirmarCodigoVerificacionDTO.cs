using System.Runtime.Serialization;

namespace Servicios.Contratos.DTOs
{
    [DataContract]
    public class ConfirmarCodigoVerificacionDTO
    {
        [DataMember]
        public string TokenVerificacion { get; set; }

        [DataMember]
        public string CodigoIngresado { get; set; }
    }
}
