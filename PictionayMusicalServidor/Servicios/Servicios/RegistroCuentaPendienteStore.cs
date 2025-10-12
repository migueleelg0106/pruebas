using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace Servicios.Servicios
{
    internal class RegistroCuentaPendienteStore
    {
        private readonly ConcurrentDictionary<string, RegistroCuentaPendiente> _registros = new ConcurrentDictionary<string, RegistroCuentaPendiente>();

        private RegistroCuentaPendienteStore()
        {
        }

        public static RegistroCuentaPendienteStore Instancia { get; } = new RegistroCuentaPendienteStore();

        public IEnumerable<KeyValuePair<string, RegistroCuentaPendiente>> ObtenerTodos()
        {
            return _registros.ToArray();
        }

        public bool TryAdd(RegistroCuentaPendiente registro)
        {
            if (registro == null)
            {
                throw new ArgumentNullException(nameof(registro));
            }

            return _registros.TryAdd(registro.Token, registro);
        }

        public bool TryGet(string token, out RegistroCuentaPendiente registro)
        {
            return _registros.TryGetValue(token, out registro);
        }

        public bool TryRemove(string token)
        {
            return _registros.TryRemove(token, out _);
        }

        public void LimpiarExpirados(DateTime fechaActual)
        {
            foreach (var entrada in ObtenerTodos())
            {
                if (entrada.Value.EstaExpirado(fechaActual))
                {
                    _registros.TryRemove(entrada.Key, out _);
                }
            }
        }

        public void RemoverPorCorreoOUsuario(string correo, string usuario)
        {
            foreach (var entrada in ObtenerTodos())
            {
                bool mismoCorreo = !string.IsNullOrWhiteSpace(correo)
                    && string.Equals(entrada.Value.Correo, correo, StringComparison.OrdinalIgnoreCase);
                bool mismoUsuario = !string.IsNullOrWhiteSpace(usuario)
                    && string.Equals(entrada.Value.Usuario, usuario, StringComparison.OrdinalIgnoreCase);

                if (mismoCorreo || mismoUsuario)
                {
                    _registros.TryRemove(entrada.Key, out _);
                }
            }
        }
    }
}
