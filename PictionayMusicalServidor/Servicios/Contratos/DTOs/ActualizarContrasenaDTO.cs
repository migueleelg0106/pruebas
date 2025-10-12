using System.Runtime.Serialization;

namespace Servicios.Contratos.DTOs
{
    [DataContract]
    public class ActualizarContrasenaDTO
    {
        [DataMember]
        public string TokenRecuperacion { get; set; }

        [DataMember]
        public string NuevaContrasena { get; set; }
    }
}
