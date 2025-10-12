using System.Runtime.Serialization;

namespace Servicios.Contratos.DTOs
{
    [DataContract]
    public class SolicitudRecuperarCuentaDTO
    {
        [DataMember]
        public string Identificador { get; set; }
    }
}
