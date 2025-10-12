using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Servicios.Contratos.DTOs
{
    [DataContract]
    public class NuevaCuentaDTO
    {
        [DataMember] 
        public string Correo { get; set; }
        [DataMember] 
        public string Contrasena { get; set; }
        [DataMember] 
        public string Usuario { get; set; }
        [DataMember] 
        public string Nombre { get; set; }
        [DataMember] 
        public string Apellido { get; set; }
        [DataMember] 
        public int AvatarId { get; set; }
    }
}
