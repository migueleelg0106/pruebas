using System.Runtime.Serialization;

namespace Servicios.Contratos.DTOs
{
    [DataContract]
    public class ReenviarCodigoVerificacionDTO
    {
        [DataMember]
        public string TokenVerificacion { get; set; }
    }
}
