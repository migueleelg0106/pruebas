using System.Runtime.Serialization;

namespace Servicios.Contratos.DTOs
{
    [DataContract]
    public class ConfirmarCodigoRecuperacionDTO
    {
        [DataMember]
        public string TokenRecuperacion { get; set; }

        [DataMember]
        public string CodigoIngresado { get; set; }
    }
}
