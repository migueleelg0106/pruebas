using System.Runtime.Serialization;

namespace Servicios.Contratos.DTOs
{
    [DataContract]
    public class ConfirmarCodigoDTO
    {
        [DataMember]
        public string TokenCodigo { get; set; }

        [DataMember]
        public string CodigoIngresado { get; set; }
    }
}
