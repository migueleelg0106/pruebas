using Servicios.Contratos;
using Servicios.Contratos.DTOs;
using Datos.DAL.Interfaces;
using Datos.DAL.Implementaciones;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using log4net;

namespace Servicios.Servicios
{
    public class CatalogoAvatares : ICatalogoAvatares
    {
        private readonly IAvatarRepositorio _repo;
        private static readonly ILog Bitacora = LogManager.GetLogger(typeof(CatalogoAvatares));

        public CatalogoAvatares() : this(new AvatarRepositorio())
        {
        }

        public CatalogoAvatares(IAvatarRepositorio repo)
        {
            _repo = repo;
        }

        public List<AvatarDTO> ObtenerAvataresDisponibles()
        {
            Bitacora.Info("Solicitud para obtener avatares disponibles recibida.");

            try
            {
                return _repo.ObtenerAvatares()
                            .Select(a => new AvatarDTO
                            {
                                Id = a.idAvatar,
                                Nombre = a.Nombre_Avatar,
                                RutaRelativa = a.Avatar_Ruta
                            })
                            .ToList();
            }
            catch (ArgumentNullException ex)
            {
                Bitacora.Warn("El repositorio de avatares devolvió información inválida.", ex);
                throw FabricaFallaServicio.Crear("SOLICITUD_INVALIDA", "No fue posible obtener los avatares solicitados.", "Solicitud inválida.");
            }
            catch (InvalidOperationException ex)
            {
                Bitacora.Error("Operación inválida al obtener el catálogo de avatares.", ex);
                throw FabricaFallaServicio.Crear("OPERACION_INVALIDA", "No fue posible recuperar el catálogo de avatares.", "Operación inválida en la capa de datos.");
            }
            catch (DataException ex)
            {
                Bitacora.Error("Error en la base de datos al obtener avatares disponibles.", ex);
                throw FabricaFallaServicio.Crear("ERROR_BASE_DATOS", "Ocurrió un problema al obtener el catálogo de avatares.", "Fallo en la base de datos.");
            }
            catch (Exception ex)
            {
                Bitacora.Fatal("Error inesperado al obtener avatares disponibles.", ex);
                throw FabricaFallaServicio.Crear("ERROR_NO_CONTROLADO", "Ocurrió un error inesperado al obtener los avatares.", "Error interno del servidor.");
            }
        }
    }
}
