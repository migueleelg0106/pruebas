using Datos.DAL.Implementaciones;
using Datos.DAL.Interfaces;
using Servicios.Contratos;
using Servicios.Contratos.DTOs;
using System;

namespace Servicios.Servicios
{
    public class InicioSesionManejador : IInicioSesionManejador
    {
        private readonly InicioSesionServicio _servicio;

        public InicioSesionManejador()
            : this(new UsuarioRepositorio())
        {
        }

        public InicioSesionManejador(IUsuarioRepositorio usuarioRepositorio)
        {
            if (usuarioRepositorio == null)
            {
                throw new ArgumentNullException(nameof(usuarioRepositorio));
            }

            _servicio = new InicioSesionServicio(usuarioRepositorio);
        }

        public ResultadoInicioSesionDTO IniciarSesion(CredencialesInicioSesionDTO credenciales)
        {
            return _servicio.IniciarSesion(credenciales);
        }
    }
}
