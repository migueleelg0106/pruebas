using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using PictionaryMusicalCliente.Comandos;
using PictionaryMusicalCliente.Properties.Langs;
using PictionaryMusicalCliente.Servicios;
using PictionaryMusicalCliente.Servicios.Abstracciones;
using ClasificacionSrv = PictionaryMusicalCliente.PictionaryServidorServicioClasificacion;

namespace PictionaryMusicalCliente.VistaModelo.Cuentas
{
    public class ClasificacionVistaModelo : BaseVistaModelo
    {
        private readonly IDialogService _dialogService;
        private readonly IClasificacionService _clasificacionService;
        private readonly Comando _ordenarPorRondasCommand;
        private readonly Comando _ordenarPorPuntosCommand;
        private readonly IComandoAsincrono _cargarClasificacionCommand;

        private List<ClasificacionSrv.ClasificacionUsuarioDTO> _clasificacionOriginal = new List<ClasificacionSrv.ClasificacionUsuarioDTO>();
        private IEnumerable<ClasificacionSrv.ClasificacionUsuarioDTO> _clasificacion = Enumerable.Empty<ClasificacionSrv.ClasificacionUsuarioDTO>();
        private bool _estaCargando;

        public ClasificacionVistaModelo(IDialogService dialogService, IClasificacionService clasificacionService)
        {
            _dialogService = dialogService ?? throw new ArgumentNullException(nameof(dialogService));
            _clasificacionService = clasificacionService ?? throw new ArgumentNullException(nameof(clasificacionService));

            _ordenarPorRondasCommand = new Comando(OrdenarPorRondas, PuedeOrdenarClasificacion);
            _ordenarPorPuntosCommand = new Comando(OrdenarPorPuntos, PuedeOrdenarClasificacion);
            _cargarClasificacionCommand = new ComandoAsincrono(() => CargarClasificacionAsync(), () => !EstaCargando);
            CerrarCommand = new Comando(() => SolicitarCerrar?.Invoke(this, EventArgs.Empty));
        }

        public event EventHandler SolicitarCerrar;

        public IEnumerable<ClasificacionSrv.ClasificacionUsuarioDTO> Clasificacion
        {
            get => _clasificacion;
            private set => EstablecerPropiedad(ref _clasificacion, value ?? Enumerable.Empty<ClasificacionSrv.ClasificacionUsuarioDTO>());
        }

        public bool EstaCargando
        {
            get => _estaCargando;
            private set
            {
                if (EstablecerPropiedad(ref _estaCargando, value))
                {
                    ActualizarEstadoComandos();
                }
            }
        }

        public ICommand OrdenarPorRondasCommand => _ordenarPorRondasCommand;

        public ICommand OrdenarPorPuntosCommand => _ordenarPorPuntosCommand;

        public ICommand CerrarCommand { get; }

        public IComandoAsincrono CargarClasificacionCommand => _cargarClasificacionCommand;

        private async Task CargarClasificacionAsync()
        {
            if (EstaCargando)
            {
                return;
            }

            EstaCargando = true;

            try
            {
                IReadOnlyList<ClasificacionSrv.ClasificacionUsuarioDTO> clasificacion = await _clasificacionService
                    .ObtenerTopJugadoresAsync().ConfigureAwait(true);

                _clasificacionOriginal = clasificacion?.ToList() ?? new List<ClasificacionSrv.ClasificacionUsuarioDTO>();
                Clasificacion = _clasificacionOriginal;
                ActualizarEstadoComandos();
            }
            catch (ServicioException ex)
            {
                string mensaje = string.IsNullOrWhiteSpace(ex.Message)
                    ? Lang.errorTextoErrorProcesarSolicitud
                    : ex.Message;
                _dialogService.Aviso(mensaje);
            }
            catch (Exception ex)
            {
                _dialogService.Aviso(ex.Message);
            }
            finally
            {
                EstaCargando = false;
            }
        }

        private void OrdenarPorRondas()
        {
            if (!PuedeOrdenarClasificacion())
            {
                return;
            }

            Clasificacion = _clasificacionOriginal
                .OrderByDescending(entrada => entrada?.RondasGanadas ?? 0)
                .ThenByDescending(entrada => entrada?.Puntos ?? 0)
                .ToList();
        }

        private void OrdenarPorPuntos()
        {
            if (!PuedeOrdenarClasificacion())
            {
                return;
            }

            Clasificacion = _clasificacionOriginal
                .OrderByDescending(entrada => entrada?.Puntos ?? 0)
                .ThenByDescending(entrada => entrada?.RondasGanadas ?? 0)
                .ToList();
        }

        private bool PuedeOrdenarClasificacion()
        {
            return !EstaCargando && _clasificacionOriginal != null && _clasificacionOriginal.Count > 0;
        }

        private void ActualizarEstadoComandos()
        {
            _ordenarPorRondasCommand.NotificarPuedeEjecutarCambio();
            _ordenarPorPuntosCommand.NotificarPuedeEjecutarCambio();
            _cargarClasificacionCommand.NotificarPuedeEjecutar();
        }
    }
}
