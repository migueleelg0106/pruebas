using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace PictionaryMusicalCliente.Modelo
{
    public class RedSocialPerfil : INotifyPropertyChanged
    {
        private string _identificador = "@";

        public string Nombre { get; set; }

        public string Clave { get; set; }

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
                Clave = Clave,
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

            string texto = valor.TrimStart();

            if (texto.Length == 0)
            {
                return "@";
            }

            int indice = 0;

            while (indice < texto.Length && texto[indice] == '@')
            {
                indice++;
            }

            string resto = texto.Substring(indice);
            string normalizado = "@" + resto;

            return normalizado.Length == 0 ? "@" : normalizado;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
