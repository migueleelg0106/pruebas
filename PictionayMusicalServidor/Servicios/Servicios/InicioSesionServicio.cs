using Datos.DAL.Interfaces;
using Datos.Modelo;
using Servicios.Contratos.DTOs;
using System;

namespace Servicios.Servicios
{
    internal class InicioSesionServicio
    {
        private readonly IUsuarioRepositorio _usuarioRepositorio;
        private readonly IRedSocialRepositorio _redSocialRepositorio;

        public InicioSesionServicio(
            IUsuarioRepositorio usuarioRepositorio,
            IRedSocialRepositorio redSocialRepositorio)
        {
            _usuarioRepositorio = usuarioRepositorio ?? throw new ArgumentNullException(nameof(usuarioRepositorio));
            _redSocialRepositorio = redSocialRepositorio ?? throw new ArgumentNullException(nameof(redSocialRepositorio));
        }

        public ResultadoInicioSesionDTO IniciarSesion(CredencialesInicioSesionDTO credenciales)
        {
            if (credenciales == null)
            {
                throw new ArgumentNullException(nameof(credenciales));
            }

            var resultado = CrearResultadoInicial();

            NormalizarCredenciales(credenciales);

            Usuario usuario = _usuarioRepositorio.ObtenerUsuarioPorIdentificador(credenciales.Identificador);

            if (usuario == null)
            {
                return CrearResultadoCredencialesInvalidas(resultado, cuentaNoEncontrada: true, contrasenaIncorrecta: false);
            }

            if (string.IsNullOrEmpty(credenciales.Contrasena))
            {
                return CrearResultadoCredencialesInvalidas(resultado, cuentaNoEncontrada: false, contrasenaIncorrecta: true);
            }

            if (!BCrypt.Net.BCrypt.Verify(credenciales.Contrasena, usuario.Contrasena))
            {
                return CrearResultadoCredencialesInvalidas(resultado, cuentaNoEncontrada: false, contrasenaIncorrecta: true);
            }

            resultado.InicioSesionExitoso = true;
            resultado.Mensaje = "Inicio de sesión exitoso.";
            resultado.Usuario = CrearUsuarioDto(usuario);

            return resultado;
        }

        private static void NormalizarCredenciales(CredencialesInicioSesionDTO credenciales)
        {
            credenciales.Identificador = credenciales.Identificador?.Trim();
        }

        private static ResultadoInicioSesionDTO CrearResultadoCredencialesInvalidas(
            ResultadoInicioSesionDTO resultado,
            bool cuentaNoEncontrada,
            bool contrasenaIncorrecta)
        {
            resultado.Mensaje = "Credenciales incorrectas.";
            resultado.CuentaNoEncontrada = cuentaNoEncontrada;
            resultado.ContrasenaIncorrecta = contrasenaIncorrecta;

            return resultado;
        }

        private static ResultadoInicioSesionDTO CrearResultadoInicial()
        {
            return new ResultadoInicioSesionDTO
            {
                InicioSesionExitoso = false,
                Mensaje = string.Empty,
                CuentaNoEncontrada = false,
                ContrasenaIncorrecta = false,
                Usuario = null
            };
        }

        private UsuarioDTO CrearUsuarioDto(Usuario usuario)
        {
            if (usuario == null)
            {
                return null;
            }

            Jugador jugador = usuario.Jugador;
            RedSocial redSocial = null;

            if (jugador != null)
            {
                redSocial = _redSocialRepositorio.ObtenerPorJugadorId(jugador.idJugador);
            }

            return new UsuarioDTO
            {
                IdUsuario = usuario.idUsuario,
                JugadorId = usuario.Jugador_idJugador,
                NombreUsuario = usuario.Nombre_Usuario,
                Nombre = jugador?.Nombre,
                Apellido = jugador?.Apellido,
                Correo = jugador?.Correo,
                AvatarId = jugador?.Avatar_idAvatar ?? 0,
                Instagram = redSocial?.Instagram,
                Facebook = redSocial?.facebook,
                X = redSocial?.x,
                Discord = redSocial?.discord
            };
        }
    }
}
