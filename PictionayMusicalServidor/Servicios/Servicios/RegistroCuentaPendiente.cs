using Servicios.Contratos.DTOs;
using Servicios.Servicios.Utilidades;
using System;

namespace Servicios.Servicios
{
    internal class RegistroCuentaPendiente
    {
        public RegistroCuentaPendiente(string token, string codigo, NuevaCuentaDTO nuevaCuenta, TimeSpan duracionCodigo, DateTime creadoEn)
        {
            if (nuevaCuenta == null)
            {
                throw new ArgumentNullException(nameof(nuevaCuenta));
            }

            Token = token ?? throw new ArgumentNullException(nameof(token));
            Codigo = codigo ?? throw new ArgumentNullException(nameof(codigo));
            Correo = nuevaCuenta.Correo?.Trim();
            Usuario = nuevaCuenta.Usuario?.Trim();
            Nombre = nuevaCuenta.Nombre?.Trim();
            Apellido = nuevaCuenta.Apellido?.Trim();
            AvatarId = nuevaCuenta.AvatarId;
            ContrasenaHash = BCrypt.Net.BCrypt.HashPassword(nuevaCuenta.Contrasena);
            Expira = creadoEn.Add(duracionCodigo);
            UltimoEnvio = creadoEn;
        }

        public string Token { get; }
        public string Codigo { get; private set; }
        public string Correo { get; }
        public string Usuario { get; }
        public string Nombre { get; }
        public string Apellido { get; }
        public int AvatarId { get; }
        public string ContrasenaHash { get; }
        public DateTime Expira { get; private set; }
        public DateTime UltimoEnvio { get; private set; }

        public static RegistroCuentaPendiente Crear(NuevaCuentaDTO nuevaCuenta, string codigo, TimeSpan duracionCodigo, DateTime creadoEn)
        {
            string token = TokenGenerator.GenerarToken();
            return new RegistroCuentaPendiente(token, codigo, nuevaCuenta, duracionCodigo, creadoEn);
        }

        public bool EstaExpirado(DateTime fechaActual)
        {
            return Expira <= fechaActual;
        }

        public bool PuedeReenviar(DateTime fechaActual, TimeSpan tiempoEspera, out int segundosRestantes)
        {
            DateTime siguienteEnvioPermitido = UltimoEnvio.Add(tiempoEspera);
            if (fechaActual < siguienteEnvioPermitido)
            {
                segundosRestantes = (int)Math.Ceiling((siguienteEnvioPermitido - fechaActual).TotalSeconds);
                return false;
            }

            segundosRestantes = 0;
            return true;
        }

        public void ActualizarCodigo(string nuevoCodigo, TimeSpan duracionCodigo, DateTime fechaActual)
        {
            Codigo = nuevoCodigo ?? throw new ArgumentNullException(nameof(nuevoCodigo));
            Expira = fechaActual.Add(duracionCodigo);
            UltimoEnvio = fechaActual;
        }

        public bool CodigoCoincide(string codigoIngresado)
        {
            return string.Equals(Codigo, codigoIngresado?.Trim(), StringComparison.Ordinal);
        }
    }
}
