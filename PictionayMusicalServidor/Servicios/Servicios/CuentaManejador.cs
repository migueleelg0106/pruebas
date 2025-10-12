using Datos.DAL.Implementaciones;
using Datos.DAL.Interfaces;
using Servicios.Contratos;
using Servicios.Contratos.DTOs;
using System;

namespace Servicios.Servicios
{
    public class CuentaManejador : ICuentaManejador
    {
        private readonly CuentaRegistroServicio _registroServicio;

        public CuentaManejador()
            : this(new CuentaRepositorio())
        {
        }

        public CuentaManejador(ICuentaRepositorio repositorioCuenta)
        {
            if (repositorioCuenta == null)
            {
                throw new ArgumentNullException(nameof(repositorioCuenta));
            }

            _registroServicio = new CuentaRegistroServicio(repositorioCuenta);
        }

        public ResultadoRegistroCuentaDTO RegistrarCuenta(NuevaCuentaDTO nuevaCuenta)
        {
            return _registroServicio.RegistrarCuenta(nuevaCuenta);
        }
    }
}
