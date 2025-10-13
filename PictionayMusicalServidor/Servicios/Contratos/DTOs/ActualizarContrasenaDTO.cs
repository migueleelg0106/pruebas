using System.Runtime.Serialization;

namespace Servicios.Contratos.DTOs
{
    [DataContract]
    public class ActualizarContrasenaDTO
    {
        [DataMember]
        public string TokenCodigo { get; set; }

        [DataMember]
        public string NuevaContrasena { get; set; }
    }
}
