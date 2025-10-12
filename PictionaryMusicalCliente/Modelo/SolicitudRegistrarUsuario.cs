using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PictionaryMusicalCliente.Modelo
{
    public class SolicitudRegistrarUsuario
    {
        public string Usuario { get; set; }
        public string Correo { get; set; }
        public string Nombre { get; set; }
        public string Apellido { get; set; }
        public string ContrasenaPlano { get; set; }
        public int AvatarId { get; set; }
    }
}
