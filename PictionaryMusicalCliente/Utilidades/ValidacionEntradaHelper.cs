using System.Text.RegularExpressions;

namespace PictionaryMusicalCliente.Utilidades
{
    public static class ValidacionEntradaHelper
    {
        public const int LongitudMaximaNombreUsuario = 50;
        public const int LongitudMaximaNombre = 50;
        public const int LongitudMaximaApellido = 50;
        public const int LongitudMaximaCorreo = 50;
        public const int LongitudMaximaContrasena = 60;
        public const int LongitudMaximaRedSocial = 50;

        private static readonly Regex PatronCorreoValido = new Regex(@"^[^@\s]+@[^@\s]+\.[^@\s]+$", RegexOptions.Compiled);
        private static readonly Regex PatronContrasenaValida = new Regex(@"^(?=.*[A-Z])(?=.*\d)(?=.*[^A-Za-z0-9]).{8,15}$", RegexOptions.Compiled);

        public static bool EsCampoObligatorioValido(string valor)
        {
            return !string.IsNullOrWhiteSpace(valor);
        }

        public static bool EsCorreoValido(string correo)
        {
            return !string.IsNullOrWhiteSpace(correo) && PatronCorreoValido.IsMatch(correo.Trim());
        }

        public static bool EsContrasenaValida(string contrasena)
        {
            return !string.IsNullOrWhiteSpace(contrasena) && PatronContrasenaValida.IsMatch(contrasena);
        }

        public static bool TieneLongitudValidaUsuario(string usuario)
        {
            return TieneLongitudValida(usuario, LongitudMaximaNombreUsuario);
        }

        public static bool TieneLongitudValidaNombre(string nombre)
        {
            return TieneLongitudValida(nombre, LongitudMaximaNombre);
        }

        public static bool TieneLongitudValidaApellido(string apellido)
        {
            return TieneLongitudValida(apellido, LongitudMaximaApellido);
        }

        public static bool TieneLongitudValidaCorreo(string correo)
        {
            return TieneLongitudValida(correo, LongitudMaximaCorreo);
        }

        public static bool TieneLongitudValidaContrasena(string contrasena)
        {
            return TieneLongitudValida(contrasena, LongitudMaximaContrasena, aplicarTrim: false);
        }

        public static bool TieneLongitudValidaRedSocial(string identificador)
        {
            return TieneLongitudValida(identificador, LongitudMaximaRedSocial);
        }

        private static bool TieneLongitudValida(string valor, int longitudMaxima, bool aplicarTrim = true)
        {
            if (string.IsNullOrEmpty(valor))
            {
                return true;
            }

            string texto = aplicarTrim ? valor.Trim() : valor;
            return texto.Length <= longitudMaxima;
        }
    }
}
