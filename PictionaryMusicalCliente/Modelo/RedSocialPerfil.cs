using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace PictionaryMusicalCliente.Modelo
{
    public class RedSocialPerfil : INotifyPropertyChanged
    {
        private string _identificador = "@";

        public string Nombre { get; set; }

        public string RutaIcono { get; set; }

        public string Identificador
        {
            get => _identificador;
            set
            {
                string normalizado = NormalizarIdentificador(value);

                if (_identificador != normalizado)
                {
                    _identificador = normalizado;
                    OnPropertyChanged();
                }
            }
        }

        public RedSocialPerfil Clonar()
        {
            return new RedSocialPerfil
            {
                Nombre = Nombre,
                RutaIcono = RutaIcono,
                Identificador = Identificador
            };
        }

        private static string NormalizarIdentificador(string valor)
        {
            if (string.IsNullOrWhiteSpace(valor))
            {
                return "@";
            }

            string texto = valor.Trim();

            if (!texto.StartsWith("@", StringComparison.Ordinal))
            {
                texto = "@" + texto.TrimStart('@');
            }

            return texto.Length == 0 ? "@" : texto;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
