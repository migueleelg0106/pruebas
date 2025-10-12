using System.Runtime.Serialization;

namespace Servicios.Contratos.DTOs
{
    [DataContract]
    public class CredencialesInicioSesionDTO
    {
        [DataMember]
        public string Identificador { get; set; }

        [DataMember]
        public string Contrasena { get; set; }
    }
}
