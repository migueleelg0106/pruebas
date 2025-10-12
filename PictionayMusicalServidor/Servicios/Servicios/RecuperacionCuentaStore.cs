using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace Servicios.Servicios
{
    internal class RecuperacionCuentaStore
    {
        private readonly ConcurrentDictionary<string, RecuperacionCuentaPendiente> _solicitudes = new ConcurrentDictionary<string, RecuperacionCuentaPendiente>();

        private RecuperacionCuentaStore()
        {
        }

        public static RecuperacionCuentaStore Instancia { get; } = new RecuperacionCuentaStore();

        public IEnumerable<KeyValuePair<string, RecuperacionCuentaPendiente>> ObtenerTodos()
        {
            return _solicitudes.ToArray();
        }

        public bool TryAdd(RecuperacionCuentaPendiente solicitud)
        {
            if (solicitud == null)
            {
                throw new ArgumentNullException(nameof(solicitud));
            }

            return _solicitudes.TryAdd(solicitud.Token, solicitud);
        }

        public bool TryGet(string token, out RecuperacionCuentaPendiente solicitud)
        {
            return _solicitudes.TryGetValue(token, out solicitud);
        }

        public bool TryRemove(string token)
        {
            return _solicitudes.TryRemove(token, out _);
        }

        public void LimpiarExpirados(DateTime fechaActual)
        {
            foreach (var entrada in ObtenerTodos())
            {
                if (entrada.Value.EstaCompletamenteExpirado(fechaActual))
                {
                    _solicitudes.TryRemove(entrada.Key, out _);
                }
            }
        }

        public void RemoverPorUsuario(int usuarioId)
        {
            foreach (var entrada in ObtenerTodos())
            {
                if (entrada.Value.UsuarioId == usuarioId)
                {
                    _solicitudes.TryRemove(entrada.Key, out _);
                }
            }
        }
    }
}
