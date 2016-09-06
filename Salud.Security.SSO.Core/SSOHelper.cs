using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data.SqlClient;
using Salud.Security;
using System.Web.Security;
using System.Text;
using System.IO;
using System.Diagnostics;
using System.Globalization;
using System.Data.Common;
using System.Data;
using Microsoft.Win32;
using System.Security.Principal;
using Salud.Security.SSO.Data;
using System.Net;

namespace Salud.Security.SSO
{
    public static class SSOHelper
    {
        private static object cacheLock = new object();

        private static void InnerInitDBAuditData(SqlConnection connection, SqlTransaction transaction)
        {
            if (SSOHelper.CurrentIdentity != null)
            {
                System.Data.SqlClient.SqlCommand command = new System.Data.SqlClient.SqlCommand(String.Format("EXEC sp_SSO_AuditDB_SetUser @username"), connection, transaction);
                command.CommandType = System.Data.CommandType.Text;
                command.Parameters.Add(new System.Data.SqlClient.SqlParameter("@username", System.Data.SqlDbType.VarChar, 0, System.Data.ParameterDirection.Input, 0, 0, "username", System.Data.DataRowVersion.Current, false, null, "", "", ""));
                command.Parameters[0].Value = SSOHelper.CurrentIdentity.Username;
                command.ExecuteNonQuery();
            }
        }

        private static void InitDBAuditConnectionStateChange(object sender, StateChangeEventArgs e)
        {
            if (e.CurrentState == ConnectionState.Open && e.OriginalState == ConnectionState.Closed)
            {
                InnerInitDBAuditData(sender as SqlConnection, null);
            }
        }

        /// <summary>
        /// Crea un DataContext utilizando SSOMembershipProviderConnectionString definido en Web.config
        /// </summary>
        /// <returns></returns>
        internal static DataContext GetDataContext()
        {
            return new DataContext(System.Configuration.ConfigurationManager.ConnectionStrings["SSOMembershipProviderConnectionString"].ConnectionString);
        }

        /// <summary>
        /// Devuelve la instancia actual de <see cref="SSOMembershipProvider"/>
        /// </summary>
        internal static SSOMembershipProvider MembershipProvider
        {
            get
            {
                SSOMembershipProvider ssoMembershipProvider;
                try
                {
                    ssoMembershipProvider = (SSOMembershipProvider)Membership.Providers["SSOMembershipProvider"];
                }
                catch (Exception e)
                {
                    throw new SSOException(SSOException.ExceptionCodeType.MembershipProviderIncorrect, e.ToString());
                }
                return ssoMembershipProvider;
            }
        }

        /// <summary>
        /// Devuelve la instancia actual de <see cref="SSOIdentity"/> asociada con el usuario que está loggeado
        /// </summary>
        /// <value>The current identity.</value>
        public static SSOIdentity CurrentIdentity
        {
            get
            {
                if (HttpContext.Current.User == null)
                    return null;
                else
                    return HttpContext.Current.User.Identity as SSOIdentity;
            }
        }

        /// <summary>
        /// Devuelve un parámetro de configuración
        /// </summary>
        public static SSOConfiguration Configuration
        {
            get
            {
                return SSOHelper.MembershipProvider.Configuration;
            }
        }

        /// <summary>
        /// Devuelve el módulo actual en una instancia de <see cref="SSOModule"/> asociado a la URL de la página que el usuario está visitando
        /// </summary>
        /// <value>The current module.</value>
        public static SSOModule CurrentModule
        {
            get
            {
                return (SSOHelper.CurrentIdentity == null) ? null : SSOHelper.CurrentIdentity.CurrentModule;
            }
        }

        /// <summary>
        /// Inicializa la auditoría de SSO en la conexión asociada con "instance"
        /// </summary>
        /// <param name="ObjectOrSqlConnection">Puede ser una conexion <see cref="SqlConnection"/> o bien un objecto con una propiedad llamada "Connection"</param>
        /// <param name="ObjectOrSqlTransaction">Puede ser una transacción <see cref="SqlTransaction"/>  o bien un objecto con una propiedad llamada "Transaction"</param>
        /// <param name="delayedInit">Indica si inicializa inmediatamente la auditoría (abriendo la conexión <see cref="ObjectOrSqlConnection"/> o sólo cuando es necesario</param>
        public static void InitDBAuditData(object ObjectOrSqlConnection, object ObjectOrSqlTransaction, bool delayedInit)
        {
            // Get SqlConnection
            SqlConnection connection;
            if (ObjectOrSqlConnection.GetType() == typeof(System.Data.SqlClient.SqlConnection))
                connection = (System.Data.SqlClient.SqlConnection)ObjectOrSqlConnection;
            else
            {
                System.Reflection.PropertyInfo pinfo = ObjectOrSqlConnection.GetType().GetProperty("Connection", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.IgnoreCase);
                connection = (System.Data.SqlClient.SqlConnection)pinfo.GetValue(ObjectOrSqlConnection, null);
            }

            if (connection != null)
            {
                // Get SqlTransaction
                SqlTransaction transaction;
                if (ObjectOrSqlTransaction == null)
                    transaction = null;
                else
                {
                    if (ObjectOrSqlConnection.GetType() == typeof(System.Data.SqlClient.SqlTransaction))
                        transaction = (System.Data.SqlClient.SqlTransaction)ObjectOrSqlTransaction;
                    else
                    {
                        System.Reflection.PropertyInfo pinfo = ObjectOrSqlTransaction.GetType().GetProperty("Transaction", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.IgnoreCase);
                        transaction = (System.Data.SqlClient.SqlTransaction)pinfo.GetValue(ObjectOrSqlTransaction, null);
                    }
                }

                if ((transaction != null) | (!delayedInit))
                {
                    // Init now
                    if (connection.State != System.Data.ConnectionState.Open)
                        connection.Open();
                    InnerInitDBAuditData(connection, transaction);
                }
                else
                {
                    // Delay init
                    connection.StateChange += new StateChangeEventHandler(InitDBAuditConnectionStateChange);
                }
            }
        }

        /// <summary>
        /// Devuelve el URL a la página que muestra el log de acceso de una aplicación.
        /// </summary>
        /// <param name="application">Aplicación</param>
        /// <param name="accessKeyFilters">[Opcional] Indica que claves buscar en los registros.</param>
        /// <returns></returns>
        public static Uri GetLogAccessURL(int applicationId, SSOLogAccessKeys accessKeyFilters)
        {
            if (accessKeyFilters != null && accessKeyFilters.Count > 1)
                throw new NotImplementedException("Por el momento sólo se puede consultar un accessKey por vez");
            else
            {
                if (accessKeyFilters == null || accessKeyFilters.Count == 0)
                    return SSOHelper.GetLogAccessURL(applicationId, null, null);
                else
                    return SSOHelper.GetLogAccessURL(applicationId, accessKeyFilters[0].name, accessKeyFilters[0].value);
            }
        }

        /// <summary>
        /// Devuelve el URL a la página que muestra el log de acceso de una aplicación.
        /// </summary>
        /// <param name="application">Aplicación</param>
        /// <param name="accessKeyFilters">[Opcional] Indica que claves buscar en los registros.</param>
        /// <returns></returns>
        public static Uri GetLogAccessURL(int applicationId, string keyName, string keyValue)
        {
            string filterQuery;
            if (String.IsNullOrEmpty(keyName) || String.IsNullOrEmpty(keyValue))
                filterQuery = "";
            else
            {
                filterQuery = String.Format("&key={0}&value={1}", keyName, keyValue);
            }

            string s = String.Format("{0}AccessLog.aspx?applicationId={1}{2}", (string)SSOHelper.Configuration["SSO_URL"], applicationId, filterQuery);
            try
            {
                return new Uri(s);
            }
            catch (Exception)
            {
                // Si es un URL relativo, agrega la página base
                return new Uri(String.Format("{0}{1}", (string)SSOHelper.Configuration["StartPage"], s));
            }
        }

        /// <summary>
        /// Busca un usuario
        /// </summary>
        /// <param name="id">Id del usuario</param>
        /// <returns></returns>
        public static SSOUser FindUser(int id)
        {
            if (HttpContext.Current == null)
                return SSOHelper.MembershipProvider.GetUser(id, null, null);
            else
            {
                /* Here's the basic pattern:
                    - Check the cache for the value, return if its available
                    - If the value is not in the cache, then implement a lock
                    - Inside the lock, check the cache again, you might have been blocked
                    - Perform the value look up and cache it
                    - Release the lock
                */
                string cacheKey = String.Format("SSOCache_UserById_{0}", id);
                SSOUser result = SSOHelper.MembershipProvider.UseCache ? HttpContext.Current.Cache[cacheKey] as SSOUser : null;
                if (result != null)
                    return result;
                else
                    lock (cacheLock)
                    {
                        result = SSOHelper.MembershipProvider.UseCache ? HttpContext.Current.Cache[cacheKey] as SSOUser : null;
                        if (result == null)
                        {
                            result = SSOHelper.MembershipProvider.GetUser(id, null, null);
                            if (result != null)
                                HttpContext.Current.Cache[cacheKey] = result;
                        }
                        return result;
                    }
            }
        }

        /// <summary>
        /// Busca un usuario según la clave externa
        /// </summary>
        /// <param name="externalID">Clave externa del usuario</param>
        /// <returns></returns>
        public static SSOUser FindUserByExternalID(object externalID)
        {
            if (externalID == null)
                return null;
            else
                if (HttpContext.Current == null)
                    return SSOHelper.MembershipProvider.GetUser(null, null, externalID);
                else
                {
                    /* Here's the basic pattern:
                        - Check the cache for the value, return if its available
                        - If the value is not in the cache, then implement a lock
                        - Inside the lock, check the cache again, you might have been blocked
                        - Perform the value look up and cache it
                        - Release the lock
                    */
                    string cacheKey = String.Format("SSOCache_UserByExternalId_{0}", externalID);
                    SSOUser result = SSOHelper.MembershipProvider.UseCache ? HttpContext.Current.Cache[cacheKey] as SSOUser : null;
                    if (result != null)
                        return result;
                    else lock (cacheLock)
                        {
                            result = SSOHelper.MembershipProvider.UseCache ? HttpContext.Current.Cache[cacheKey] as SSOUser : null;
                            if (result == null || !SSOHelper.MembershipProvider.UseCache)
                            {
                                result = SSOHelper.MembershipProvider.GetUser(null, null, externalID);
                                if (result != null)
                                    HttpContext.Current.Cache[cacheKey] = result;
                            }
                            return result;
                        }
                }
        }

        /// <summary>
        /// Busca un módulo
        /// </summary>
        /// <param name="url">URL del módulo</param>
        /// <returns></returns>
        public static SSOModule FindModule(Uri url)
        {
            return SSOModule.FindByURL(url);
        }

        /// <summary>
        /// Permite verificar la contraseña de un usuario
        /// </summary>
        /// <param name="username">Nombre de usuario</param>
        /// <param name="password">Contraseña</param>
        /// <param name="userId">Devuelve el ID del usuario si la contraseña es correceta</param>
        /// <returns></returns>
        public static bool TestPassword(string username, string password, out int userId)
        {
            return SSOHelper.MembershipProvider.TestPassword(username, password, out userId);
        }

        public static string GetNombreEfectorRol(int idEfectorRol)
        {
            return SSOHelper.MembershipProvider.GetNombreEfectorRol(idEfectorRol);
        }

        /// <summary>
        /// Verifica si el usuario indicado tiene una variable almacenada con ese valor.
        /// Este método es útil para comprar datos sin romper la "privacidad" de un tercero preguntando por cualquier variable almacenada.
        /// </summary>
        /// <param name="userId">ID del usuario</param>
        /// <param name="variableName">Nombre de la variable</param>
        /// <param name="variableValue">Valor de la variable</param>
        /// <returns></returns>
        public static bool TestStoredVariable(int userId, string variableName, object variableValue)
        {
            SSOVariables variables = SSOHelper.MembershipProvider.GetStoredVariables(userId, variableName);
            for (var i = 0; i < variables.List.Count; i++)
            {
                if (variables.List[i].value.ToString() == variableValue.ToString())
                    return true;
            }
            return false;
        }


        /// <summary>
        /// Redirige a la página especificada en el parámetro URL del query. Si el parámetro no existe, redirige a la página de inicio de la intranet.
        /// </summary>
        internal static void RedirectToURL()
        {
            string url = HttpContext.Current.Request.QueryString["url"];
            if (string.IsNullOrEmpty(url))
                url = SSOHelper.Configuration["StartPage"] as string;

            HttpContext.Current.Response.Redirect(url);
        }

        /// <summary>
        /// Redirige a la página SSO especificada
        /// </summary>
        /// <param name="url">Página SSO</param>
        /// <param name="passURL">URL opcional para redirigir desde la página SSO</param>        
        public static void RedirectToSSOPage(string url, string passURL)
        {
            url = String.Format("{0}{1}{2}url={3}", SSOHelper.Configuration["SSO_URL"], url, (url.Contains("?")) ? "&" : "?", HttpContext.Current.Server.UrlEncode(passURL));
            HttpContext.Current.Response.Redirect(url);
        }

        public static void RedirectToErrorPage(int code, int subCode, string reason)
        {
            HttpContext.Current.Response.Redirect(String.Format("{0}?code={1}&subcode={2}&url={3}&reason={4}", SSOHelper.Configuration["ErrorPage_URL"], code, subCode, HttpContext.Current.Server.UrlEncode(HttpContext.Current.Request.Url.ToString()), (reason == null) ? "" : HttpUtility.UrlEncode(reason)));
        }

        internal static void RedirectToSecure()
        {
            if (HttpContext.Current.Request.Url.ToString().StartsWith("http:"))
                HttpContext.Current.Response.Redirect(String.Format("https://{0}{1}{2}{3}RedirectToSecure=1", HttpContext.Current.Request.Url.Host, HttpContext.Current.Request.Url.AbsolutePath, HttpContext.Current.Request.Url.Query, String.IsNullOrEmpty(HttpContext.Current.Request.Url.Query) ? "?" : "&"));
        }

        /// <summary>
        /// Autentica desde la cookie, aún cuando el módulo de <see cref="SSOHttpModule"/> no fue ejecutado (Por ejemplo en el caso de archivos .axd)
        /// </summary>
        public static void Authenticate()
        {
            if (SSOHelper.CurrentIdentity == null)
            {
                // Check cookie                
                HttpCookie ssoCookie = HttpContext.Current.Request.Cookies[SSOHelper.MembershipProvider.CookieName];
                if ((ssoCookie != null) && (!String.IsNullOrEmpty(ssoCookie.Value)))
                    HttpContext.Current.User = new GenericPrincipal(new SSOIdentity(ssoCookie), null);
            }
        }

        /// <summary>
        /// Devuelve el listado de IDs de módulos que el usuario tiene permiso. Es utilizado principalmente por <see cref="SSOHttpModule"/>.
        /// </summary>
        /// <returns></returns>
        //internal static HashSet<int> GetUserPermissions()
        //{
        //    // 27/07/2012 | jgabriel | TO DO: Invalidar el caché con un timestamp de actualización de los permisos

        //    /* Here's the basic pattern:
        //        - Check the cache for the value, return if its available
        //        - If the value is not in the cache, then implement a lock
        //        - Inside the lock, check the cache again, you might have been blocked
        //        - Perform the value look up and cache it
        //        - Release the lock
        //    */
        //    string cacheKey = String.Format("Salud.Security.SSO.Permissions_{0}", SSOHelper.CurrentIdentity.Id);
        //    HashSet<int> result = SSOHelper.MembershipProvider.UseCache ? HttpContext.Current.Cache[cacheKey] as HashSet<int> : null;
        //    if (result != null)
        //        return result;
        //    else
        //        lock (cacheLock)
        //        {
        //            result = SSOHelper.MembershipProvider.UseCache ? HttpContext.Current.Cache[cacheKey] as HashSet<int> : null;
        //            if (result == null || !SSOHelper.MembershipProvider.UseCache)
        //            {
        //                result = SSOHelper.MembershipProvider.GetAllowedModules(SSOHelper.CurrentIdentity);
        //                HttpContext.Current.Cache[cacheKey] = result;
        //            }
        //            return result;
        //        }
        //}

        internal static HashSet<int> GetUserPermissionsByEfector()
        {

            /* Here's the basic pattern:
                - Check the cache for the value, return if its available
                - If the value is not in the cache, then implement a lock
                - Inside the lock, check the cache again, you might have been blocked
                - Perform the value look up and cache it
                - Release the lock
            */
            string cacheKey = String.Format("Salud.Security.SSO.Permissions_{0}", SSOHelper.CurrentIdentity.Id);
            HashSet<int> result = SSOHelper.MembershipProvider.UseCache ? HttpContext.Current.Cache[cacheKey] as HashSet<int> : null;
            if (result != null)
                return result;
            else
                lock (cacheLock)
                {
                    result = SSOHelper.MembershipProvider.UseCache ? HttpContext.Current.Cache[cacheKey] as HashSet<int> : null;
                    if (result == null || !SSOHelper.MembershipProvider.UseCache)
                    {
                        result = SSOHelper.MembershipProvider.GetAllowedModulesByEfector(SSOHelper.CurrentIdentity);
                        HttpContext.Current.Cache[cacheKey] = result;
                    }
                    return result;
                }
        }

        /// <summary>
        /// Verifica que el usuario tenga permisos al módulo indicado
        /// </summary>
        /// <returns></returns>
        //public static bool TestPermission(SSOModule module)
        //{
        //    if (!module.IsProtected)
        //        return true;
        //    else
        //    {
        //        if (SSOHelper.CurrentIdentity.IsGlobalAdministrator || SSOHelper.CurrentIdentity.IsApplicationAdministrator)
        //            return true;
        //        else
        //            return SSOHelper.GetUserPermissions().Contains(module.Id);
        //    }
        //}

        public static bool TestPermissionByEfector(SSOModule module)
        {
            if (!module.IsProtected)
                return true;
            else
            {
                if (SSOHelper.CurrentIdentity.IsGlobalAdministrator || SSOHelper.CurrentIdentity.IsApplicationAdministrator)
                    return true;
                else
                    return SSOHelper.GetUserPermissionsByEfector().Contains(module.Id);
            }
        }

        /// <summary>
        /// Permite enviar un SMS
        /// </summary>
        /// <param name="recipient">Número del teléfono móvil</param>
        /// <param name="carrier">ID del carrier. Corresponde a uno de los valores de la tabla SSO_SMS_Carriers</param>
        /// <param name="text">Texto del mensaje</param>
        public static void SendSMS(string recipient, int carrier, string text)
        {
            text = text.Trim();
            if (text.Length < 2)
                throw new Exception("Mensaje muy corto");
            else
                if (String.IsNullOrEmpty(recipient))
                    throw new ArgumentNullException("recipient");
                else
                {
                    using (Data.DataContext DataContext = new Data.DataContext(System.Configuration.ConfigurationManager.ConnectionStrings["SMSServiceConnectionString"].ConnectionString))
                    {
                        string result = "";
                        // Crea el registro porque el Webservice de la OPTIC va a consultar este registro para obtener el número de celular
                        var log = new Data.SSO_SMS_Log();
                        log.sourceUserId = SSOHelper.CurrentIdentity.Id;
                        log.datetime = DateTime.Now;
                        log.result = "Waiting";
                        log.text = text;
                        log.mobile = recipient;
                        log.idCarrier = carrier;
                        DataContext.SSO_SMS_Log.InsertOnSubmit(log);
                        DataContext.SubmitChanges();
                        try
                        {
                            string url = String.Format((string)SSOHelper.Configuration["SMS_WebService"], log.id, text);
                            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
                            request.Timeout = 10000;
                            request.Method = "GET";
                            request.ContentType = "text/html; charset=utf-8";
                            ServicePointManager.ServerCertificateValidationCallback = SendSMS_ValidateServerCertificate;
                            HttpWebResponse response = (HttpWebResponse)request.GetResponse();
                            result = response.StatusCode.ToString();
                            if (response.StatusCode != HttpStatusCode.OK)
                                throw new Exception("Webservice response: " + response.StatusCode.ToString());
                        }
                        catch (Exception exception)
                        {
                            if (String.IsNullOrEmpty(result))
                                result = exception.ToString();
                            throw exception;
                        }
                        finally
                        {
                            ServicePointManager.ServerCertificateValidationCallback = null;
                            log.result = result;
                            DataContext.SubmitChanges();
                        }
                    }
                }
        }

        private static bool SendSMS_ValidateServerCertificate(Object sender, System.Security.Cryptography.X509Certificates.X509Certificate certificate, System.Security.Cryptography.X509Certificates.X509Chain chain, System.Net.Security.SslPolicyErrors sslPolicyErrors)
        {
            return true;
        }

        /// <summary>
        /// Método privado que utiliza GetApplicationMenu()
        /// </summary>
        //private static List<SSOMenuItem> GetApplicationMenu_GetItems(SSOBaseClass parent)
        //{
        //    List<SSOMenuItem> result = new List<SSOMenuItem>();
        //    var items = (parent is SSOApplicationGroup) ? ((SSOApplicationGroup)parent).MenuItems : ((SSOApplication)parent).MenuItems;
        //    foreach (SSOBaseClass item in items)
        //    {
        //        if (item.IsVisible)
        //        {
        //            if (item is SSOApplicationGroup)
        //            {
        //                SSOMenuItem group = new SSOMenuItem()
        //                {
        //                    id = item.Id,
        //                    text = item.Description,
        //                    imageURL = item.Image,
        //                    isSeparator = item.Description == "-",
        //                    url = item.URL,
        //                    priority = item.Priority,
        //                    items = GetApplicationMenu_GetItems(item)
        //                };
        //                if (group.items.Count > 0)
        //                    result.Add(group);
        //            }
        //            else
        //                result.Add(new SSOMenuItem() { id = item.Id, text = item.Description ?? item.Name, imageURL = item.Image, isSeparator = item.Description == "-", url = item.URL, priority = item.Priority });
        //        }
        //    }

        //    // Ordena por prioridad
        //    result.Sort(delegate(SSOMenuItem t1, SSOMenuItem t2)
        //    {
        //        return (t1.priority.CompareTo(t2.priority));
        //    });

        //    // Controla separadores
        //    if (result.Count > 0 && result[0].isSeparator)
        //        result.RemoveAt(0);

        //    return result;
        //}

        private static List<SSOMenuItem> GetApplicationMenu_GetItemsByEfector(SSOBaseClass parent)
        {
            List<SSOMenuItem> result = new List<SSOMenuItem>();
            var items = (parent is SSOApplicationGroup) ? ((SSOApplicationGroup)parent).MenuItemsByEfector : ((SSOApplication)parent).MenuItemsByEfector;
            foreach (SSOBaseClass item in items)
            {
                if (item.IsVisible)
                {
                    if (item is SSOApplicationGroup)
                    {
                        SSOMenuItem group = new SSOMenuItem()
                        {
                            id = item.Id,
                            text = item.Description,
                            imageURL = item.Image,
                            isSeparator = item.Description == "-",
                            url = item.URL,                            
                            priority = item.Priority,
                            items = GetApplicationMenu_GetItemsByEfector(item)
                        };
                        if (group.items.Count > 0)
                            result.Add(group);
                    }
                    else
                        result.Add(new SSOMenuItem() { id = item.Id, text = item.Description ?? item.Name, imageURL = item.Image, isSeparator = item.Description == "-", url = item.URL, priority = item.Priority });
                }
            }

            // Ordena por prioridad
            result.Sort(delegate(SSOMenuItem t1, SSOMenuItem t2)
            {
                return (t1.priority.CompareTo(t2.priority));
            });

            // Controla separadores
            if (result.Count > 0 && result[0].isSeparator)
                result.RemoveAt(0);

            return result;
        }

        /// <summary>
        /// Devuelve el menú de aplicaciones y módulos permitidos
        /// </summary>
        //public static List<SSOMenuItem> GetApplicationMenu()
        //{
        //    string cacheKey = String.Format("SSOCache_Menu_{0}", SSOHelper.CurrentIdentity.SessionId);
        //    List<SSOMenuItem> result = SSOHelper.MembershipProvider.UseCache ? HttpContext.Current.Cache[cacheKey] as List<SSOMenuItem> : null;
        //    if (result != null)
        //        return result;
        //    else
        //        lock (cacheLock)
        //        {
        //            result = SSOHelper.MembershipProvider.UseCache ? HttpContext.Current.Cache[cacheKey] as List<SSOMenuItem> : null;
        //            if (result == null)
        //            {
        //                string scriptsURL = SSOHelper.Configuration["Scripts_BaseURL"] as string;
        //                List<SSOMenuItem> items = (from r in SSOHelper.MembershipProvider.GetAllowedApplications(SSOHelper.CurrentIdentity)
        //                                           where r.IsVisible && r.URL != null 
        //                                           && r.Id == SSOHelper.CurrentModule.ApplicationId  //gds: 31/05/2013
        //                                           orderby r.Description
        //                                           select new SSOMenuItem()
        //                                           {
        //                                               id = r.Id,
        //                                               priority = r.Priority,
        //                                               text = r.Description,
        //                                               url = r.URL.StartsWith("/") ? r.URL : scriptsURL + r.URL,
        //                                               items = GetApplicationMenu_GetItems(r)
        //                                           }
        //                                    ).ToList();
        //                result = items.Where(i => i.items.Count > 0).ToList();
        //                if (result != null)
        //                    HttpContext.Current.Cache[cacheKey] = result;
        //            }
        //            return result;
        //        }
        //}

        public static List<SSOMenuItem> GetApplicationMenuByEfector()
        {
            string cacheKey = String.Format("SSOCache_Menu_{0}", SSOHelper.CurrentIdentity.SessionId);
            
            List<SSOMenuItem> result = SSOHelper.MembershipProvider.UseCache ? HttpContext.Current.Cache[cacheKey] as List<SSOMenuItem> : null;
            
            if (result != null)
                return result;
            else
                lock (cacheLock)
                {
                    result = SSOHelper.MembershipProvider.UseCache ? HttpContext.Current.Cache[cacheKey] as List<SSOMenuItem> : null;
                    if (result == null)
                    {
                        string scriptsURL = SSOHelper.Configuration["Scripts_BaseURL"] as string;
            
                        List<SSOMenuItem> items = (from r in SSOHelper.MembershipProvider.GetAllowedApplications(SSOHelper.CurrentIdentity)
                                                   where r.IsVisible && r.URL != null
                                                   && r.Id ==  SSOHelper.CurrentModule.ApplicationId  //gds: 31/05/2013
                                                   orderby r.Description
                                                   select new SSOMenuItem()
                                                   {
                                                       id = r.Id,
                                                       priority = r.Priority,
                                                       text = r.Description,
                                                       url = r.URL.StartsWith("/") ? r.URL : scriptsURL + r.URL,
                                                       items = GetApplicationMenu_GetItemsByEfector(r)
                                                   }
                                            ).ToList();
                        result = items.Where(i => i.items.Count > 0).ToList();
                        if (result != null)
                            HttpContext.Current.Cache[cacheKey] = result;
                    }
                    return result; 
                }
        }

        public static List<SSOApplication> GetAllowedApplications()
        {
            List<SSOApplication> result = SSOHelper.MembershipProvider.GetAllowedApplications(SSOHelper.CurrentIdentity);
            return result;
        }

        public static List<SSOApplication> GetAllowedApplicationsHospital()
        {
            List<SSOApplication> result = SSOHelper.MembershipProvider.GetAllowedApplicationsHospital(SSOHelper.CurrentIdentity);
            return result;
        }

        
    }
}