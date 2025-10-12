using Datos.DAL.Interfaces;
using Servicios.Contratos.DTOs;
using System;

namespace Servicios.Servicios
{
    internal class CuentaRegistroServicio
    {
        private readonly ICuentaRepositorio _repositorioCuenta;

        public CuentaRegistroServicio(ICuentaRepositorio repositorioCuenta)
        {
            _repositorioCuenta = repositorioCuenta ?? throw new ArgumentNullException(nameof(repositorioCuenta));
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

            nuevaCuenta.Correo = correo;
            nuevaCuenta.Usuario = usuario;

            resultado.CorreoYaRegistrado = _repositorioCuenta.ExisteCorreo(correo);
            resultado.UsuarioYaRegistrado = _repositorioCuenta.ExisteUsuario(usuario);

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

            resultado.CorreoYaRegistrado = _repositorioCuenta.ExisteCorreo(correo);
            resultado.UsuarioYaRegistrado = _repositorioCuenta.ExisteUsuario(usuario);

            if (resultado.CorreoYaRegistrado || resultado.UsuarioYaRegistrado)
            {
                resultado.RegistroExitoso = false;
                resultado.Mensaje = "El correo o usuario ya está registrado.";
                return resultado;
            }

            bool cuentaCreada = _repositorioCuenta.CreateAccount(
                correo: correo,
                contrasenaHash: contrasenaHash,
                usuario: usuario,
                nombre: nombre,
                apellido: apellido,
                avatarId: avatarId);

            resultado.RegistroExitoso = cuentaCreada;
            resultado.Mensaje = cuentaCreada
                ? "Cuenta registrada correctamente."
                : "No se pudo registrar la cuenta. Intente más tarde.";

            return resultado;
        }
    }
}
