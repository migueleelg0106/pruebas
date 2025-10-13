using System;
using System.Security.Cryptography;

namespace Servicios.Servicios.Utilidades
{
    internal static class CodigoVerificacionGenerator
    {
        public static string GenerarCodigo()
        {
            var bytes = new byte[4];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(bytes);
            }

            int valor = BitConverter.ToInt32(bytes, 0) & int.MaxValue;
            int codigo = valor % 1000000;
            return codigo.ToString("D6");
        }
    }
}
