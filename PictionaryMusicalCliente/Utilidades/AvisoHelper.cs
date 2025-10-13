using System.Windows.Input;

namespace PictionaryMusicalCliente.Utilidades
{
    public static class AvisoHelper
    {
        public static void Mostrar(string mensaje)
        {
            Cursor cursorAnterior = Mouse.OverrideCursor;
            Mouse.OverrideCursor = null;

            try
            {
                new Vista.Avisos(mensaje).ShowDialog();
            }
            finally
            {
                Mouse.OverrideCursor = cursorAnterior;
            }
        }
    }
}
