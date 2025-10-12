using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PictionaryMusicalCliente.Modelo
{
    public class ObjetoAvatar
    {
        public int Id { get; set; }
        public string Nombre { get; set; }
        public string RutaRelativa { get; set; }
        public string ImagenUriAbsoluta { get; set; } // útil para Image.Source
    }
}
