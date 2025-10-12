namespace PictionaryMusicalCliente.Modelo
{
    public class ResultadoSolicitudCodigo
    {
        public bool CodigoEnviado { get; set; }
        public string Mensaje { get; set; }
        public string TokenVerificacion { get; set; }
        public bool CorreoYaRegistrado { get; set; }
        public bool UsuarioYaRegistrado { get; set; }
        public string TokenRecuperacion { get; set; }
    }
}
