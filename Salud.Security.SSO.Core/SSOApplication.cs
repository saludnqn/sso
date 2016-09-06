using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Web;

namespace Salud.Security.SSO
{
    public sealed class SSOApplication : SSOBaseClass
    {
        private string executable;
        private bool hasViews;
        private bool hasProtectedModules;
        private string version;        
        private List<SSOApplicationGroup> groups;
        private List<SSOModule> allowedModules;
        private List<SSOBaseClass> menuItems;

        #region Public properties
        public string Executable
        {
            get
            {
                return executable;
            }
        }
        public bool HasProtectedModules
        {
            get
            {
                return hasProtectedModules;
            }
        }
        #endregion

        #region List properties

        public List<SSOApplicationGroup> Groups
        {
            get
            {
                if (groups == null)
                {
                    groups = SSOHelper.MembershipProvider.GetApplicationGroups(this);                    
                }
                return groups;
            }
        }
        
        //public List<SSOModule> AllowedModules
        //{
        //    get
        //    {
        //        if (allowedModules == null)
        //        {
        //            allowedModules = SSOHelper.MembershipProvider.GetAllowedModules(SSOHelper.CurrentIdentity, this);
        //            allowedModules.Sort(SSOBaseClassComparer.CompareByPriority);
        //        }
        //        return allowedModules;
        //    }
        //}

        public List<SSOModule> AllowedModulesByEfector
        {
            get
            {
                if (allowedModules == null)
                {
                    allowedModules = SSOHelper.MembershipProvider.GetAllowedModulesByEfector(SSOHelper.CurrentIdentity, this);
                    allowedModules.Sort(SSOBaseClassComparer.CompareByPriority);
                }
                return allowedModules;
            }
        }

        //public List<SSOBaseClass> MenuItems
        //{
        //    get
        //    {
        //        if (menuItems == null)
        //        {
        //            menuItems = new List<SSOBaseClass>();
        //            // Add groups
        //            foreach (SSOApplicationGroup group in Groups)
        //            {
        //                if (group.ParentId == null)
        //                {
        //                    menuItems.Add(group);
        //                }
        //            }
        //            SSOApplicationGroup.DeleteEmptyMenuGroups(menuItems);

        //            // Add modules
        //            foreach (SSOModule module in AllowedModules)
        //            {
        //                if ((module.GroupId == null) && (module.IsVisible))
        //                {
        //                    menuItems.Add(module);
        //                }
        //            }
        //            menuItems.Sort(SSOBaseClassComparer.CompareByPriority);
        //        }
        //        return menuItems;
        //    }
        //}

        public List<SSOBaseClass> MenuItemsByEfector
        {
            get
            {
                if (menuItems == null)
                {
                    menuItems = new List<SSOBaseClass>();
                    // Add groups
                    foreach (SSOApplicationGroup group in Groups)
                    {
                        if (group.ParentId == null)
                        {
                            menuItems.Add(group);
                        }
                    }
                    SSOApplicationGroup.DeleteEmptyMenuGroupsByEfector(menuItems);

                    // Add modules
                    foreach (SSOModule module in AllowedModulesByEfector)
                    {
                        if ((module.GroupId == null) && (module.IsVisible))
                        {
                            menuItems.Add(module);
                        }
                    }
                    menuItems.Sort(SSOBaseClassComparer.CompareByPriority);
                }
                return menuItems;
            }
        }
        
        #endregion

        public SSOApplication(SqlDataReader reader)
        {
            id = SharedDBCode.GetReaderIntField(reader, "id", SharedDBCode.DBReaderAction.DBReaderThrowException);
            name = SharedDBCode.GetReaderStringField(reader, "name", SharedDBCode.DBReaderAction.DBReaderThrowException);
            description = SharedDBCode.GetReaderStringField(reader, "description", SharedDBCode.DBReaderAction.DBReaderSetDefaultValue);
            executable = SharedDBCode.GetReaderStringField(reader, "executable", SharedDBCode.DBReaderAction.DBReaderSetDefaultValue);
            hasViews = SharedDBCode.GetReaderBoolField(reader, "hasViews", SharedDBCode.DBReaderAction.DBReaderThrowException);
            hasProtectedModules = SharedDBCode.GetReaderBoolField(reader, "hasProtectedModules", SharedDBCode.DBReaderAction.DBReaderThrowException);
            url = SharedDBCode.GetReaderStringField(reader, "url", SharedDBCode.DBReaderAction.DBReaderSetDefaultValue);
            isVisible = SharedDBCode.GetReaderBoolField(reader, "intefase_visible", SharedDBCode.DBReaderAction.DBReaderThrowException);
            version = SharedDBCode.GetReaderStringField(reader, "version", SharedDBCode.DBReaderAction.DBReaderSetDefaultValue);
            image = SharedDBCode.GetReaderStringField(reader, "image", SharedDBCode.DBReaderAction.DBReaderSetDefaultValue);  //agregó gustavo
            urlPage = SharedDBCode.GetReaderStringField(reader, "urlPage", SharedDBCode.DBReaderAction.DBReaderSetDefaultValue);  //agregó gustavo

            // Unused properties
            priority = 0;
        }       

        public SSOApplication(int id, string name, string description, string url, bool isVisible)  
        {
            this.id = id;
            this.name = name;
            this.description = description;
            this.url = url;          
            this.isVisible = isVisible;           
        }

        /// <summary>
        /// Busca un módulo en la aplicación
        /// </summary>
        /// <param name="url">URL del módulo</param>
        /// <returns></returns>
        public SSOModule FindModule(string url)
        {
            Uri uri = new Uri(String.Format("{0}://{1}{2}/{3}", HttpContext.Current.Request.Url.Scheme, HttpContext.Current.Request.Url.Host,HttpContext.Current.Request.ApplicationPath, url));
            return SSOHelper.FindModule(uri);
        }
    }
}