using System;
using PictionaryMusicalCliente.Modelo;

namespace PictionaryMusicalCliente.Sesiones
{
    public sealed class SesionUsuarioActual
    {
        private static readonly Lazy<SesionUsuarioActual> InstanciaUnica =
            new Lazy<SesionUsuarioActual>(() => new SesionUsuarioActual());

        private UsuarioAutenticado _usuario;

        private SesionUsuarioActual()
        {
        }

        public static SesionUsuarioActual Instancia => InstanciaUnica.Value;

        public event EventHandler SesionActualizada;

        public UsuarioAutenticado Usuario => _usuario;

        public void EstablecerUsuario(UsuarioAutenticado usuario)
        {
            _usuario = usuario;
            NotificarCambio();
        }

        public void ActualizarDatosPersonales(
            string nombre,
            string apellido,
            int avatarId,
            string instagram,
            string facebook,
            string x,
            string discord)
        {
            if (_usuario == null)
            {
                return;
            }

            _usuario.Nombre = nombre;
            _usuario.Apellido = apellido;
            _usuario.AvatarId = avatarId;
            _usuario.Instagram = instagram;
            _usuario.Facebook = facebook;
            _usuario.X = x;
            _usuario.Discord = discord;
            NotificarCambio();
        }

        public void Limpiar()
        {
            _usuario = null;
            NotificarCambio();
        }

        private void NotificarCambio()
        {
            SesionActualizada?.Invoke(this, EventArgs.Empty);
        }
    }
}
