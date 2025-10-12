namespace PictionaryMusicalCliente.Modelo
{
    public class ResultadoSolicitudRecuperacion
    {
        public bool CodigoEnviado { get; set; }
        public bool CuentaEncontrada { get; set; }
        public string Mensaje { get; set; }
        public string TokenRecuperacion { get; set; }
        public string CorreoDestino { get; set; }
    }
}
