using PictionaryMusicalCliente.Servicios.Abstracciones;
using PictionaryMusicalCliente.Utilidades;

namespace PictionaryMusicalCliente.Servicios.Dialogos
{
    public class DialogService : IDialogService
    {
        public void Aviso(string mensaje) => AvisoHelper.Mostrar(mensaje);
    }
}
