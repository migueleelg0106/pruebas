using Servicios.Contratos;
using Servicios.Contratos.DTOs;
using Datos.DAL.Interfaces;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Datos.DAL.Implementaciones;

namespace Servicios.Servicios
{
    public class CatalogoAvatares : ICatalogoAvatares
    {
        private readonly IAvatarRepositorio _repo;

        public CatalogoAvatares() : this(new AvatarRepositorio()) { }
        public CatalogoAvatares(IAvatarRepositorio repo) => _repo = repo;

        public List<AvatarDTO> ObtenerAvataresDisponibles()
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
    }
}
