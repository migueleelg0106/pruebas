using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using System.Windows.Media;
using PictionaryMusicalCliente.Comandos;
using PictionaryMusicalCliente.Modelo;
using PictionaryMusicalCliente.Modelo.Catalogos;
using PictionaryMusicalCliente.Properties.Langs;
using PictionaryMusicalCliente.Servicios.Abstracciones;
using PictionaryMusicalCliente.Utilidades;

namespace PictionaryMusicalCliente.VistaModelo.Cuentas
{
    public class SeleccionarAvatarVistaModelo : BaseVistaModelo
    {
        public sealed class AvatarItem
        {
            public AvatarItem(ObjetoAvatar avatar)
            {
                Avatar = avatar ?? throw new ArgumentNullException(nameof(avatar));
                Imagen = AvatarImagenHelper.CrearImagen(avatar);
                Nombre = avatar.Nombre;
            }

            public ObjetoAvatar Avatar { get; }

            public ImageSource Imagen { get; }

            public string Nombre { get; }
        }

        public sealed class AvatarSeleccionadoEventArgs : EventArgs
        {
            public AvatarSeleccionadoEventArgs(ObjetoAvatar avatar, ImageSource imagen)
            {
                Avatar = avatar;
                Imagen = imagen;
            }

            public ObjetoAvatar Avatar { get; }

            public ImageSource Imagen { get; }
        }

        private readonly IDialogService _dialogService;
        private readonly ObservableCollection<AvatarItem> _avatares;
        private readonly Comando _confirmarSeleccionCommand;

        private AvatarItem _avatarSeleccionado;
        private ImageSource _avatarSeleccionadoImagen;

        public SeleccionarAvatarVistaModelo(IDialogService dialogService, IEnumerable<ObjetoAvatar> avatares = null)
        {
            _dialogService = dialogService ?? throw new ArgumentNullException(nameof(dialogService));

            IEnumerable<ObjetoAvatar> origenAvatares = avatares ?? CatalogoAvataresLocales.ObtenerAvatares();
            _avatares = new ObservableCollection<AvatarItem>(
                (origenAvatares ?? Enumerable.Empty<ObjetoAvatar>())
                    .Where(avatar => avatar != null)
                    .Select(avatar => new AvatarItem(avatar)));

            _confirmarSeleccionCommand = new Comando(ConfirmarSeleccion);
            CancelarCommand = new Comando(() => SolicitarCerrar?.Invoke(this, EventArgs.Empty));
        }

        public event EventHandler<AvatarSeleccionadoEventArgs> AvatarSeleccionadoConfirmado;

        public event EventHandler SolicitarCerrar;

        public IEnumerable<AvatarItem> Avatares => _avatares;

        public AvatarItem AvatarSeleccionado
        {
            get => _avatarSeleccionado;
            set
            {
                if (EstablecerPropiedad(ref _avatarSeleccionado, value))
                {
                    AvatarSeleccionadoImagen = value?.Imagen;
                }
            }
        }

        public ImageSource AvatarSeleccionadoImagen
        {
            get => _avatarSeleccionadoImagen;
            private set => EstablecerPropiedad(ref _avatarSeleccionadoImagen, value);
        }

        public ICommand ConfirmarSeleccionCommand => _confirmarSeleccionCommand;

        public ICommand CancelarCommand { get; }

        public ObjetoAvatar AvatarSeleccionadoModelo => AvatarSeleccionado?.Avatar;

        private void ConfirmarSeleccion()
        {
            if (AvatarSeleccionado == null)
            {
                _dialogService.Aviso(Lang.globalTextoSeleccionarAvatar);
                return;
            }

            AvatarSeleccionadoConfirmado?.Invoke(
                this,
                new AvatarSeleccionadoEventArgs(
                    AvatarSeleccionado.Avatar,
                    AvatarSeleccionado.Imagen));
        }
    }
}
