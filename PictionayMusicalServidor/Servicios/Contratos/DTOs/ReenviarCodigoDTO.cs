using System.Runtime.Serialization;

namespace Servicios.Contratos.DTOs
{
    [DataContract]
    public class ReenviarCodigoDTO
    {
        [DataMember]
        public string TokenCodigo { get; set; }
    }
}
