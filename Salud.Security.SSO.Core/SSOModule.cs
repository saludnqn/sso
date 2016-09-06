using System;
using System.Web.Security;
using System.Data.SqlClient;
using System.Web;
using System.Collections.Generic;
using System.Linq;
using System.Collections.Specialized;

namespace Salud.Security.SSO
{
    [Serializable]
    public sealed class SSOModule : SSOBaseClass
    {
        private static object cacheLock = new object();

        private string urlPath;
        private string urlQuery;
        private int applicationId;
        private int module;
        private bool isProtected;
        private int? groupId;
        SSOApplication application;

        public int ApplicationId
        {
            get
            {
                return applicationId;
            }
        }

        public int Module
        {
            get
            {
                return module;
            }
        }

        public bool IsProtected
        {
            get
            {
                return isProtected;
            }
        }

        public int? GroupId
        {
            get
            {
                return groupId;
            }
        }

        public SSOApplication Application
        {
            get
            {
                return application;
            }
        }

        public SSOModule(SqlDataReader reader)
        {
            id = SharedDBCode.GetReaderIntField(reader, "id", SharedDBCode.DBReaderAction.DBReaderThrowException);
            applicationId = SharedDBCode.GetReaderIntField(reader, "applicationId", SharedDBCode.DBReaderAction.DBReaderThrowException);
            module = SharedDBCode.GetReaderIntField(reader, "module", SharedDBCode.DBReaderAction.DBReaderThrowException);
            name = SharedDBCode.GetReaderStringField(reader, "name", SharedDBCode.DBReaderAction.DBReaderThrowException);
            description = SharedDBCode.GetReaderStringField(reader, "description", SharedDBCode.DBReaderAction.DBReaderSetDefaultValue);
            url = SharedDBCode.GetReaderStringField(reader, "url", SharedDBCode.DBReaderAction.DBReaderSetDefaultValue);
            isProtected = SharedDBCode.GetReaderBoolField(reader, "protected", SharedDBCode.DBReaderAction.DBReaderThrowException);
            image = SharedDBCode.GetReaderStringField(reader, "interfase_image", SharedDBCode.DBReaderAction.DBReaderSetDefaultValue);
            priority = SharedDBCode.GetReaderIntField(reader, "interfase_priority", SharedDBCode.DBReaderAction.DBReaderSetDefaultValue, 0);
            isVisible = SharedDBCode.GetReaderBoolField(reader, "interfase_visible", SharedDBCode.DBReaderAction.DBReaderSetDefaultValue, false);
            try
            {
                groupId = SharedDBCode.GetReaderIntField(reader, "groupId", SharedDBCode.DBReaderAction.DBReaderThrowException);
            }
            catch (Exception)
            {
                groupId = null;
            }
        }

        public SSOModule(Data.SSO_Applications application, int id, int module, string url, string name, string description, bool isProtected, string image, int? priority, bool isVisible, int? groupId)
        {
            // Inicializa aplicación
            this.application = new SSOApplication(application.id, application.name, application.description, application.url, application.intefase_visible);
            this.applicationId = application.id;

            // Inicializa módulo
            this.id = id;
            this.module = module;
            this.name = name;
            this.description = description;
            this.isProtected = isProtected;
            this.image = image;
            this.priority = priority.HasValue ? priority.Value : 0;
            this.isVisible = isVisible;
            this.groupId = groupId;

            // Prepara URLs para futuras búsquedas
            this.url = url.Replace("//", "/").Trim();
            if (this.url != "/" && this.url.EndsWith("/"))
                this.url = this.url.Substring(0, this.url.Length - 1);

            if (this.url.Contains('?'))
            {
                string[] s = this.url.Split('?');
                this.urlPath = s[0].Trim();
                this.urlQuery = s[1].Trim();
            }
            else
            {
                this.urlPath = this.url;
                this.urlQuery = String.Empty;
            }
        }

        private bool MatchURL(Uri url)
        {
            string absolutePath = url.AbsolutePath;
            absolutePath = absolutePath.EndsWith("/") ? absolutePath.Substring(0, absolutePath.Length - 1) : absolutePath;
            bool match = this.urlPath.Equals(absolutePath, StringComparison.InvariantCultureIgnoreCase);

            if (match)
            {
                if (String.IsNullOrEmpty(this.urlQuery))
                    return true;
                else
                {
                    NameValueCollection query1 = HttpUtility.ParseQueryString(this.urlQuery);
                    NameValueCollection query2 = HttpUtility.ParseQueryString(url.Query);
                    foreach (string key in query1.Keys)
                        if (!query1[key].Equals(query2[key], StringComparison.InvariantCultureIgnoreCase))
                            return false;

                    return true;
                }
            }
            else
                return false;
        }

        internal static SSOModule FindByURL(Uri url)
        {
            /* Here's the basic pattern:
                - Check the cache for the value, return if its available
                - If the value is not in the cache, then implement a lock
                - Inside the lock, check the cache again, you might have been blocked
                - Perform the value look up and cache it
                - Release the lock
            */
            string urlString = url.ToString();
            SortedDictionary<string, SSOModule> urls = SSOHelper.MembershipProvider.UseCache ? HttpContext.Current.Cache["Salud.Security.SSO.URLs"] as SortedDictionary<string, SSOModule> : null;

            if (urls != null && urls.ContainsKey(urlString))
                return urls[urlString];
            else
                lock (cacheLock)
                {
                    // Busca de nuevo (ver explicación más arriba)
                    urls = SSOHelper.MembershipProvider.UseCache ? HttpContext.Current.Cache["Salud.Security.SSO.URLs"] as SortedDictionary<string, SSOModule> : null;
                    if (urls != null && urls.ContainsKey(urlString))
                        return urls[urlString];
                    else
                    {
                        // Busca en el caché de módulos
                        List<SSOModule> modules = SSOHelper.MembershipProvider.UseCache ? HttpContext.Current.Cache["Salud.Security.SSO.Modules"] as List<SSOModule> : null;
                        if (modules == null)
                        {
                            using (Data.DataContext DataContext = SSOHelper.GetDataContext())
                            {
                                var query = from module in DataContext.SSO_Modules
                                            join pages in DataContext.SSO_ModulePages on module.id equals pages.moduleId into joined
                                            from page in joined.DefaultIfEmpty()
                                            where module.SSO_Applications.url != null && module.SSO_Applications.url.Length > 0
                                            orderby module.SSO_Applications.url + "/" + ((page == null) ? "" : page.page) descending /* Este orden permite que primero haga el matching en las URLS XX/YY/ZZ, luego en XX/YY, luego en XX, ... */
                                            select new SSOModule(module.SSO_Applications, module.id, module.module, module.SSO_Applications.url + "/" + ((page == null) ? "" : page.page), module.name, module.description, module.@protected, module.interfase_image, module.interfase_priority, module.interfase_visible, module.groupId);
                                modules = query.ToList();
                                HttpContext.Current.Cache["Salud.Security.SSO.Modules"] = modules;
                            }
                        }

                        var result = modules.FirstOrDefault(r => r.MatchURL(url));
                        if (result == null)
                        {
                            // Busca un nivel más arriba (XX/YY/ZZ --> XX/YY)
                            string s = String.Format("{0}{1}{2}{3}", url.Scheme, Uri.SchemeDelimiter, url.Authority, url.AbsolutePath);
                            if (s.EndsWith("/"))
                                s = s.Substring(0, s.Length - 1);
                            s = s.Substring(0, s.LastIndexOf('/'));
                            if (Uri.IsWellFormedUriString(s, UriKind.Absolute))
                                result = SSOModule.FindByURL(new Uri(s));
                        }

                        if (urls == null)
                            urls = new SortedDictionary<string, SSOModule>();
                        urls.Add(urlString, result);
                        HttpContext.Current.Cache["Salud.Security.SSO.URLs"] = urls;
                        return result;
                    }
                }
        }
    }
}