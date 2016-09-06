using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Salud.Applications.SSO.App_Code
{
    /// <summary>
    ///   Código estático que se ejecuta cuando se inicializa la aplicación
    /// </summary>
    public class Initialization
    {
        public static void AppInitialize()
        {
            var connectionString = System.Configuration.ConfigurationManager.ConnectionStrings["SSOMembershipProviderConnectionString"].ConnectionString;
            Salud.Applications.Shared.Initialization.Initialize(connectionString);
        }
    }
}