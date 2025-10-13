using Servicios.Contratos.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;

namespace Servicios.Contratos
{
    [ServiceContract]
    public interface ICatalogoAvatares
    {
        [OperationContract]
        [FaultContract(typeof(ErrorDetalleServicioDTO))]
        List<AvatarDTO> ObtenerAvataresDisponibles();
    }
}
