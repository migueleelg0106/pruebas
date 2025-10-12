using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Servicios.Contratos.DTOs
{
    [DataContract]
    public class AvatarDTO
    {
        [DataMember] public int Id { get; set; }
        [DataMember] public string Nombre { get; set; }
        [DataMember] public string RutaRelativa { get; set; }
    }
}
