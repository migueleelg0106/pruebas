using System.Threading.Tasks;
using PictionaryMusicalCliente.Modelo;
using PictionaryMusicalCliente.Servicios.Abstracciones;

namespace PictionaryMusicalCliente.Servicios.Dialogos
{
    public class SeleccionarAvatarDialogService : ISeleccionarAvatarService
    {
        public Task<ObjetoAvatar> SeleccionarAsync()
        {
            var ventanaSeleccion = new global::PictionaryMusicalCliente.SeleccionarAvatar();
            bool? resultado = ventanaSeleccion.ShowDialog();

            if (resultado == true)
            {
                return Task.FromResult(ventanaSeleccion.AvatarSeleccionado);
            }

            return Task.FromResult<ObjetoAvatar>(null);
        }
    }
}
