﻿using System;
using System.Collections.Generic;
using System.Data.Entity.Core.EntityClient;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Datos.Utilidades
{
    public static class Conexion
    {
        public static String ObtenerConexion()
        {
            var constructorSql = new SqlConnectionStringBuilder
            {
                DataSource = Environment.GetEnvironmentVariable("BD_SERVIDOR") ?? "localhost",
                InitialCatalog = "BaseDatosPrueba",
                UserID = Environment.GetEnvironmentVariable("BD_USUARIO"),
                Password = Environment.GetEnvironmentVariable("BD_CONTRASENA"),
                MultipleActiveResultSets = true
            };

            var constructorEntidad = new EntityConnectionStringBuilder
            {
                Provider = "System.Data.SqlClient",
                ProviderConnectionString = constructorSql.ToString(),
                Metadata = "res://*/Modelo.BasePictionaryMusical.csdl|res://*/Modelo.BasePictionaryMusical.ssdl|res://*/Modelo.BasePictionaryMusical.msl"

            };
            return constructorEntidad.ToString();
        }
    }
}
