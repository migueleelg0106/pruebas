namespace PictionaryMusicalCliente.Modelo
{
    public class ResultadoInicioSesion
    {
        public bool InicioSesionExitoso { get; set; }
        public string Mensaje { get; set; }
        public bool CuentaNoEncontrada { get; set; }
        public bool ContrasenaIncorrecta { get; set; }
        public UsuarioAutenticado Usuario { get; set; }
    }
}
