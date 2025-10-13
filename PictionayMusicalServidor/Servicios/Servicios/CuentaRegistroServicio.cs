using Datos.DAL.Interfaces;
using Datos.Modelo;
using Servicios.Contratos.DTOs;
using System;

namespace Servicios.Servicios
{
    internal class CuentaRegistroServicio
    {
        private readonly IJugadorRepositorio _jugadorRepositorio;
        private readonly IUsuarioRepositorio _usuarioRepositorio;
        private readonly IClasificacionRepositorio _clasificacionRepositorio;

        public CuentaRegistroServicio(
            IJugadorRepositorio jugadorRepositorio,
            IUsuarioRepositorio usuarioRepositorio,
            IClasificacionRepositorio clasificacionRepositorio)
        {
            _jugadorRepositorio = jugadorRepositorio ?? throw new ArgumentNullException(nameof(jugadorRepositorio));
            _usuarioRepositorio = usuarioRepositorio ?? throw new ArgumentNullException(nameof(usuarioRepositorio));
            _clasificacionRepositorio = clasificacionRepositorio ?? throw new ArgumentNullException(nameof(clasificacionRepositorio));
        }

        public bool ValidarNuevaCuenta(NuevaCuentaDTO nuevaCuenta, ResultadoRegistroCuentaDTO resultado)
        {
            if (nuevaCuenta == null)
            {
                throw new ArgumentNullException(nameof(nuevaCuenta));
            }

            if (resultado == null)
            {
                throw new ArgumentNullException(nameof(resultado));
            }

            resultado.RegistroExitoso = false;
            resultado.Mensaje = string.Empty;
            resultado.CorreoYaRegistrado = false;
            resultado.UsuarioYaRegistrado = false;

            bool correoInvalido = string.IsNullOrWhiteSpace(nuevaCuenta.Correo);
            bool usuarioInvalido = string.IsNullOrWhiteSpace(nuevaCuenta.Usuario);
            bool contrasenaInvalida = string.IsNullOrWhiteSpace(nuevaCuenta.Contrasena);
            bool nombreInvalido = string.IsNullOrWhiteSpace(nuevaCuenta.Nombre);
            bool apellidoInvalido = string.IsNullOrWhiteSpace(nuevaCuenta.Apellido);

            if (correoInvalido || usuarioInvalido || contrasenaInvalida || nombreInvalido || apellidoInvalido)
            {
                resultado.Mensaje = "Los datos de la cuenta están incompletos.";
                return false;
            }

            string correo = nuevaCuenta.Correo.Trim();
            string usuario = nuevaCuenta.Usuario.Trim();
            string nombre = nuevaCuenta.Nombre.Trim();
            string apellido = nuevaCuenta.Apellido.Trim();

            nuevaCuenta.Correo = correo;
            nuevaCuenta.Usuario = usuario;
            nuevaCuenta.Nombre = nombre;
            nuevaCuenta.Apellido = apellido;

            resultado.CorreoYaRegistrado = _jugadorRepositorio.ExisteCorreo(correo);
            resultado.UsuarioYaRegistrado = _usuarioRepositorio.ExisteUsuario(usuario);

            if (resultado.CorreoYaRegistrado && resultado.UsuarioYaRegistrado)
            {
                resultado.Mensaje = "El correo y el usuario ya están registrados.";
                return false;
            }

            if (resultado.CorreoYaRegistrado)
            {
                resultado.Mensaje = "El correo electrónico ya está asociado a otra cuenta.";
                return false;
            }

            if (resultado.UsuarioYaRegistrado)
            {
                resultado.Mensaje = "El nombre de usuario ya se encuentra en uso.";
                return false;
            }

            return true;
        }

        public ResultadoRegistroCuentaDTO RegistrarCuenta(NuevaCuentaDTO nuevaCuenta)
        {
            if (nuevaCuenta == null)
            {
                throw new ArgumentNullException(nameof(nuevaCuenta));
            }

            var resultado = new ResultadoRegistroCuentaDTO();

            if (!ValidarNuevaCuenta(nuevaCuenta, resultado))
            {
                return resultado;
            }

            string contrasenaHash = BCrypt.Net.BCrypt.HashPassword(nuevaCuenta.Contrasena);

            return CrearCuenta(nuevaCuenta.Correo, contrasenaHash, nuevaCuenta.Usuario, nuevaCuenta.Nombre, nuevaCuenta.Apellido, nuevaCuenta.AvatarId);
        }

        public ResultadoRegistroCuentaDTO RegistrarCuentaPendiente(RegistroCuentaPendiente registro)
        {
            if (registro == null)
            {
                return new ResultadoRegistroCuentaDTO
                {
                    RegistroExitoso = false,
                    Mensaje = "Los datos de la cuenta no están disponibles."
                };
            }

            return CrearCuenta(registro.Correo, registro.ContrasenaHash, registro.Usuario, registro.Nombre, registro.Apellido, registro.AvatarId);
        }

        private ResultadoRegistroCuentaDTO CrearCuenta(string correo, string contrasenaHash, string usuario, string nombre, string apellido, int avatarId)
        {
            var resultado = new ResultadoRegistroCuentaDTO();

            resultado.CorreoYaRegistrado = _jugadorRepositorio.ExisteCorreo(correo);
            resultado.UsuarioYaRegistrado = _usuarioRepositorio.ExisteUsuario(usuario);

            if (resultado.CorreoYaRegistrado || resultado.UsuarioYaRegistrado)
            {
                resultado.RegistroExitoso = false;
                resultado.Mensaje = "El correo o usuario ya está registrado.";
                return resultado;
            }

            Clasificacion clasificacion = _clasificacionRepositorio.CrearClasificacionInicial();
            Jugador jugador = _jugadorRepositorio.CrearJugador(nombre, apellido, correo, avatarId, clasificacion.idClasificacion);
            _usuarioRepositorio.CrearUsuario(jugador.idJugador, usuario, contrasenaHash);

            resultado.RegistroExitoso = true;
            resultado.Mensaje = "Cuenta registrada correctamente.";

            return resultado;
        }
    }
}
