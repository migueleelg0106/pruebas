using System.Runtime.Serialization;

namespace Servicios.Contratos.DTOs
{
    [DataContract]
    public class ActualizarPerfilDTO
    {
        [DataMember]
        public int UsuarioId { get; set; }

        [DataMember]
        public string Nombre { get; set; }

        [DataMember]
        public string Apellido { get; set; }

        [DataMember]
        public int AvatarId { get; set; }
    }
}
