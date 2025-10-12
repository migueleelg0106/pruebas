using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using Datos.DAL.Interfaces;
using Datos.Modelo;
using Datos.Utilidades;

namespace Datos.DAL.Implementaciones
{
    public class AvatarRepositorio : IAvatarRepositorio
    {
        public IEnumerable<Avatar> ObtenerAvatares()
        {
            using (var contexto = new BaseDatosPruebaEntities1(Conexion.ObtenerConexion()))
            {
                return contexto.Avatar.AsNoTracking().ToList();
            }
        }

        public bool ExisteAvatar(int avatarId)
        {
            using (var contexto = new BaseDatosPruebaEntities1(Conexion.ObtenerConexion()))
            {
                return contexto.Avatar.Any(a => a.idAvatar == avatarId);
            }
        }
    }
}
