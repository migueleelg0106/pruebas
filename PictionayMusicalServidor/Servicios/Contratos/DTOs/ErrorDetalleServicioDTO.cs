using System.Runtime.Serialization;

namespace Servicios.Contratos.DTOs
{
    [DataContract]
    public class ErrorDetalleServicioDTO
    {
        [DataMember]
        public string CodigoError { get; set; }

        [DataMember]
        public string Mensaje { get; set; }
    }
}
