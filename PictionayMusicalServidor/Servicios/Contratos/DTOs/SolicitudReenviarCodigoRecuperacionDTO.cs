using System.Runtime.Serialization;

namespace Servicios.Contratos.DTOs
{
    [DataContract]
    public class SolicitudReenviarCodigoRecuperacionDTO
    {
        [DataMember]
        public string TokenRecuperacion { get; set; }
    }
}
