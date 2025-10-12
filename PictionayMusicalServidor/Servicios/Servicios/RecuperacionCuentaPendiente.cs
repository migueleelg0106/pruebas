using Servicios.Servicios.Utilidades;
using System;

namespace Servicios.Servicios
{
    internal class RecuperacionCuentaPendiente
    {
        private RecuperacionCuentaPendiente(string token, int usuarioId, string correo, string codigo, TimeSpan duracionCodigo, DateTime creadoEn)
        {
            Token = token ?? throw new ArgumentNullException(nameof(token));
            UsuarioId = usuarioId;
            Correo = correo ?? string.Empty;
            Codigo = codigo ?? throw new ArgumentNullException(nameof(codigo));
            ExpiraCodigo = creadoEn.Add(duracionCodigo);
            UltimoEnvio = creadoEn;
        }

        public string Token { get; }
        public int UsuarioId { get; }
        public string Correo { get; }
        public string Codigo { get; private set; }
        public DateTime ExpiraCodigo { get; private set; }
        public DateTime UltimoEnvio { get; private set; }
        public bool CodigoVerificado { get; private set; }
        public DateTime? ExpiraActualizacion { get; private set; }

        public static RecuperacionCuentaPendiente Crear(int usuarioId, string correo, string codigo, TimeSpan duracionCodigo, DateTime creadoEn)
        {
            string token = TokenGenerator.GenerarToken();
            return new RecuperacionCuentaPendiente(token, usuarioId, correo, codigo, duracionCodigo, creadoEn);
        }

        public bool EstaExpiradoParaVerificacion(DateTime fechaActual)
        {
            return ExpiraCodigo <= fechaActual;
        }

        public bool EstaCompletamenteExpirado(DateTime fechaActual)
        {
            if (CodigoVerificado)
            {
                if (!ExpiraActualizacion.HasValue)
                {
                    return true;
                }

                return ExpiraActualizacion.Value <= fechaActual;
            }

            return ExpiraCodigo <= fechaActual;
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
            ExpiraCodigo = fechaActual.Add(duracionCodigo);
            UltimoEnvio = fechaActual;
            CodigoVerificado = false;
            ExpiraActualizacion = null;
        }

        public bool CodigoCoincide(string codigoIngresado)
        {
            return string.Equals(Codigo, codigoIngresado?.Trim(), StringComparison.Ordinal);
        }

        public void MarcarCodigoVerificado(DateTime fechaActual, TimeSpan duracionActualizacion)
        {
            CodigoVerificado = true;
            ExpiraActualizacion = fechaActual.Add(duracionActualizacion);
        }

        public bool PuedeActualizar(DateTime fechaActual)
        {
            return CodigoVerificado && ExpiraActualizacion.HasValue && ExpiraActualizacion.Value > fechaActual;
        }
    }
}
