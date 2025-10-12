using System.Collections.Generic;

namespace PictionaryMusicalCliente.Modelo.Catalogos
{
    public static class CatalogoAvataresLocales
    {
        private static readonly IReadOnlyList<ObjetoAvatar> Avatares = new List<ObjetoAvatar>
        {
            Crear(1, "AC/DC", "Recursos/ACDC.jpg"),
            Crear(2, "Aleks Syntek", "Recursos/Aleks_Syntek.jpg"),
            Crear(3, "Anuel AA", "Recursos/Anuel_AA.jpg"),
            Crear(4, "Bad Bunny", "Recursos/Bad_Bunny.jpg"),
            Crear(5, "Britney Spears", "Recursos/Britney_Spears.jpg"),
            Crear(6, "Bruno Mars", "Recursos/Bruno_Mars.jpg"),
            Crear(7, "Drake", "Recursos/Drake.jpg"),
            Crear(8, "Guns N' Roses", "Recursos/Guns_N_Roses.jpg"),
            Crear(9, "J Balvin", "Recursos/J_Balvin.jpg"),
            Crear(10, "José José", "Recursos/Jose_Jose.jpg"),
            Crear(11, "Kanye West", "Recursos/Kanye_West.jpg"),
            Crear(12, "Luis Miguel", "Recursos/Luis_Miguel.jpg"),
            Crear(13, "Mariah Carey", "Recursos/Mariah_Carey.jpg"),
            Crear(14, "Taylor Swift", "Recursos/Taylor_Swift.jpg"),
            Crear(15, "The Weeknd", "Recursos/The_Weeknd.jpg"),
            Crear(16, "Travis Scott", "Recursos/Travis_Scott.jpg"),
        };

        public static IReadOnlyList<ObjetoAvatar> ObtenerAvatares() => Avatares;

        private static ObjetoAvatar Crear(int id, string nombre, string ruta) => new ObjetoAvatar
        {
            Id = id,
            Nombre = nombre,
            RutaRelativa = ruta
        };
    }
}
