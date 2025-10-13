using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PictionaryMusicalCliente.Utilidades;

namespace PictionaryMusicalCliente.Servicios
{
    public interface IDialogService
    {
        void Aviso(string mensaje);
    }

    public class DialogService : IDialogService
    {
        public void Aviso(string mensaje) => AvisoHelper.Mostrar(mensaje);
    }
}
