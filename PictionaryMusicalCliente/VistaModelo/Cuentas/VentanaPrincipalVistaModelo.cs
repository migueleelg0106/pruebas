using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using PictionaryMusicalCliente.Comandos;
using PictionaryMusicalCliente.Properties.Langs;
using PictionaryMusicalCliente.Servicios.Abstracciones;
using PictionaryMusicalCliente.Sesiones;

namespace PictionaryMusicalCliente.VistaModelo.Cuentas
{
    public class VentanaPrincipalVistaModelo : BaseVistaModelo, IDisposable
    {
        public sealed class Opcion<T>
        {
            public Opcion(T valor, string descripcion)
            {
                Valor = valor;
                Descripcion = descripcion;
            }

            public T Valor { get; }

            public string Descripcion { get; }
        }

        public sealed class UnirseSalaEventArgs : EventArgs
        {
            public UnirseSalaEventArgs(string codigoSala)
            {
                CodigoSala = codigoSala;
            }

            public string CodigoSala { get; }
        }

        public sealed class IniciarJuegoEventArgs : EventArgs
        {
            public IniciarJuegoEventArgs(int numeroRondas, int tiempoRonda, string idioma, string dificultad)
            {
                NumeroRondas = numeroRondas;
                TiempoRonda = tiempoRonda;
                Idioma = idioma;
                Dificultad = dificultad;
            }

            public int NumeroRondas { get; }

            public int TiempoRonda { get; }

            public string Idioma { get; }

            public string Dificultad { get; }
        }

        private readonly IDialogService _dialogService;
        private readonly SesionUsuarioActual _sesionUsuarioActual;

        private readonly IReadOnlyList<Opcion<int>> _opcionesNumeroRondas;
        private readonly IReadOnlyList<Opcion<int>> _opcionesTiempoRonda;
        private readonly IReadOnlyList<Opcion<string>> _opcionesIdioma;
        private readonly IReadOnlyList<Opcion<string>> _opcionesDificultad;
        private readonly ObservableCollection<string> _amigos;

        private readonly Comando _abrirPerfilCommand;
        private readonly Comando _abrirAjustesCommand;
        private readonly Comando _unirseSalaCommand;
        private readonly Comando _abrirComoJugarCommand;
        private readonly Comando _abrirClasificacionCommand;
        private readonly Comando _iniciarJuegoCommand;
        private readonly Comando _abrirInvitacionesCommand;
        private readonly Comando _abrirSolicitudesCommand;
        private readonly Comando _abrirEliminarAmigoCommand;
        private readonly Comando _abrirBuscarAmigoCommand;

        private string _nombreUsuario;
        private string _codigoSala;
        private Opcion<int> _numeroRondasSeleccionada;
        private Opcion<int> _tiempoRondaSeleccionada;
        private Opcion<string> _idiomaSeleccionado;
        private Opcion<string> _dificultadSeleccionada;

        public VentanaPrincipalVistaModelo(IDialogService dialogService, SesionUsuarioActual sesionUsuarioActual = null)
        {
            _dialogService = dialogService ?? throw new ArgumentNullException(nameof(dialogService));
            _sesionUsuarioActual = sesionUsuarioActual ?? SesionUsuarioActual.Instancia;
            _sesionUsuarioActual.SesionActualizada += SesionUsuarioActualizada;

            _opcionesNumeroRondas = new List<Opcion<int>>
            {
                new Opcion<int>(1, "1"),
                new Opcion<int>(2, "2"),
                new Opcion<int>(3, "3")
            };

            _opcionesTiempoRonda = new List<Opcion<int>>
            {
                new Opcion<int>(60, "60"),
                new Opcion<int>(75, "75"),
                new Opcion<int>(90, "90"),
                new Opcion<int>(120, "120")
            };

            _opcionesIdioma = new List<Opcion<string>>
            {
                new Opcion<string>(Lang.TagEspañol, Lang.idiomaTextoEspañol),
                new Opcion<string>(Lang.TagIngles, Lang.idiomaTextoIngles)
            };

            _opcionesDificultad = new List<Opcion<string>>
            {
                new Opcion<string>("Facil", Lang.principalTextoFacil),
                new Opcion<string>("Media", Lang.principalTextoMedia),
                new Opcion<string>("Dificil", Lang.principalTextoDificil)
            };

            _amigos = new ObservableCollection<string>();

            _abrirPerfilCommand = new Comando(() => SolicitarAbrirPerfil?.Invoke(this, EventArgs.Empty));
            _abrirAjustesCommand = new Comando(() => SolicitarAbrirAjustes?.Invoke(this, EventArgs.Empty));
            _unirseSalaCommand = new Comando(UnirseSala);
            _abrirComoJugarCommand = new Comando(() => SolicitarAbrirComoJugar?.Invoke(this, EventArgs.Empty));
            _abrirClasificacionCommand = new Comando(() => SolicitarAbrirClasificacion?.Invoke(this, EventArgs.Empty));
            _iniciarJuegoCommand = new Comando(IniciarJuego);
            _abrirInvitacionesCommand = new Comando(() => SolicitarAbrirInvitaciones?.Invoke(this, EventArgs.Empty));
            _abrirSolicitudesCommand = new Comando(() => SolicitarAbrirSolicitudes?.Invoke(this, EventArgs.Empty));
            _abrirEliminarAmigoCommand = new Comando(() => SolicitarAbrirEliminarAmigo?.Invoke(this, EventArgs.Empty));
            _abrirBuscarAmigoCommand = new Comando(() => SolicitarAbrirBuscarAmigo?.Invoke(this, EventArgs.Empty));

            NumeroRondasSeleccionada = _opcionesNumeroRondas.FirstOrDefault();
            TiempoRondaSeleccionada = _opcionesTiempoRonda.Skip(1).FirstOrDefault() ?? _opcionesTiempoRonda.FirstOrDefault();
            IdiomaSeleccionado = _opcionesIdioma.FirstOrDefault();
            DificultadSeleccionada = _opcionesDificultad.FirstOrDefault();

            ActualizarNombreUsuario();
        }

        public event EventHandler SolicitarAbrirPerfil;

        public event EventHandler SolicitarAbrirAjustes;

        public event EventHandler SolicitarAbrirComoJugar;

        public event EventHandler SolicitarAbrirClasificacion;

        public event EventHandler SolicitarAbrirInvitaciones;

        public event EventHandler SolicitarAbrirSolicitudes;

        public event EventHandler SolicitarAbrirEliminarAmigo;

        public event EventHandler SolicitarAbrirBuscarAmigo;

        public event EventHandler<UnirseSalaEventArgs> SolicitarUnirseSala;

        public event EventHandler<IniciarJuegoEventArgs> SolicitarIniciarJuego;

        public IEnumerable<Opcion<int>> NumeroRondasOpciones => _opcionesNumeroRondas;

        public IEnumerable<Opcion<int>> TiempoRondaOpciones => _opcionesTiempoRonda;

        public IEnumerable<Opcion<string>> IdiomasDisponibles => _opcionesIdioma;

        public IEnumerable<Opcion<string>> DificultadesDisponibles => _opcionesDificultad;

        public ObservableCollection<string> Amigos => _amigos;

        public string NombreUsuario
        {
            get => _nombreUsuario;
            private set => EstablecerPropiedad(ref _nombreUsuario, value);
        }

        public string CodigoSala
        {
            get => _codigoSala;
            set => EstablecerPropiedad(ref _codigoSala, value);
        }

        public Opcion<int> NumeroRondasSeleccionada
        {
            get => _numeroRondasSeleccionada;
            set => EstablecerPropiedad(ref _numeroRondasSeleccionada, value);
        }

        public Opcion<int> TiempoRondaSeleccionada
        {
            get => _tiempoRondaSeleccionada;
            set => EstablecerPropiedad(ref _tiempoRondaSeleccionada, value);
        }

        public Opcion<string> IdiomaSeleccionado
        {
            get => _idiomaSeleccionado;
            set => EstablecerPropiedad(ref _idiomaSeleccionado, value);
        }

        public Opcion<string> DificultadSeleccionada
        {
            get => _dificultadSeleccionada;
            set => EstablecerPropiedad(ref _dificultadSeleccionada, value);
        }

        public ICommand AbrirPerfilCommand => _abrirPerfilCommand;

        public ICommand AbrirAjustesCommand => _abrirAjustesCommand;

        public ICommand UnirseSalaCommand => _unirseSalaCommand;

        public ICommand AbrirComoJugarCommand => _abrirComoJugarCommand;

        public ICommand AbrirClasificacionCommand => _abrirClasificacionCommand;

        public ICommand IniciarJuegoCommand => _iniciarJuegoCommand;

        public ICommand AbrirInvitacionesCommand => _abrirInvitacionesCommand;

        public ICommand AbrirSolicitudesCommand => _abrirSolicitudesCommand;

        public ICommand AbrirEliminarAmigoCommand => _abrirEliminarAmigoCommand;

        public ICommand AbrirBuscarAmigoCommand => _abrirBuscarAmigoCommand;

        public void Dispose()
        {
            _sesionUsuarioActual.SesionActualizada -= SesionUsuarioActualizada;
        }

        private void SesionUsuarioActualizada(object sender, EventArgs e)
        {
            ActualizarNombreUsuario();
        }

        private void ActualizarNombreUsuario()
        {
            string nombreUsuario = _sesionUsuarioActual.Usuario?.NombreUsuario;
            NombreUsuario = string.IsNullOrWhiteSpace(nombreUsuario)
                ? Lang.globalTextoUsuario
                : nombreUsuario;
        }

        private void UnirseSala()
        {
            string codigoNormalizado = CodigoSala?.Trim();

            if (string.IsNullOrWhiteSpace(codigoNormalizado))
            {
                _dialogService.Aviso(Lang.globalTextoIngreseCodigoPartida);
                return;
            }

            SolicitarUnirseSala?.Invoke(this, new UnirseSalaEventArgs(codigoNormalizado));
        }

        private void IniciarJuego()
        {
            if (NumeroRondasSeleccionada == null || TiempoRondaSeleccionada == null || IdiomaSeleccionado == null || DificultadSeleccionada == null)
            {
                _dialogService.Aviso(Lang.errorTextoErrorProcesarSolicitud);
                return;
            }

            SolicitarIniciarJuego?.Invoke(
                this,
                new IniciarJuegoEventArgs(
                    NumeroRondasSeleccionada.Valor,
                    TiempoRondaSeleccionada.Valor,
                    IdiomaSeleccionado.Valor,
                    DificultadSeleccionada.Valor));
        }
    }
}
