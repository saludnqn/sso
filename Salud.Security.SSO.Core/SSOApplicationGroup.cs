using System;
using System.Collections.Generic;
using System.Data.SqlClient;

namespace Salud.Security.SSO
{
    public sealed class SSOApplicationGroup : SSOBaseClass
    {
        private SSOApplication application;
        private int? parentId;
        private List<SSOApplicationGroup>groups;
        private List<SSOBaseClass> menuItems;

        public SSOApplication Application
        { get { return application; } }
        public int? ParentId
        {
            get
            {
                return parentId;
            }
        }
        public bool IsSeparator
        {
            get { return description == "-"; }
        }

        public List<SSOApplicationGroup> Groups
        {
            get
            {
                if (groups == null)
                {
                    groups = new List<SSOApplicationGroup>();
                    foreach (SSOApplicationGroup group in application.Groups)
                    {
                        if (group.ParentId == this.id)
                        {
                            groups.Add(group);
                        }
                    }
                }
                return groups;
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
        //                menuItems.Add(group);
        //            }

        //            // Add modules
        //            foreach (SSOModule module in application.AllowedModules)
        //            {
        //                if ((module.GroupId == this.id) && module.IsVisible)
        //                {
        //                    menuItems.Add(module);
        //                }
        //            }

        //            // Sort
        //            SSOApplicationGroup.DeleteEmptyMenuGroups(menuItems);
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
                        menuItems.Add(group);
                    }

                    // Add modules
                    foreach (SSOModule module in application.AllowedModulesByEfector)
                    {
                        if ((module.GroupId == this.id) && module.IsVisible)
                        {
                            menuItems.Add(module);
                        }
                    }

                    // Sort
                    SSOApplicationGroup.DeleteEmptyMenuGroupsByEfector(menuItems);
                    menuItems.Sort(SSOBaseClassComparer.CompareByPriority);
                }
                return menuItems;
            }
        }


        public SSOApplicationGroup(SSOApplication application, SqlDataReader reader)
        {
            this.application = application;
            id = SharedDBCode.GetReaderIntField(reader, "id", SharedDBCode.DBReaderAction.DBReaderThrowException);
            description = SharedDBCode.GetReaderStringField(reader, "description", SharedDBCode.DBReaderAction.DBReaderThrowException);
            image = SharedDBCode.GetReaderStringField(reader, "interfase_image", SharedDBCode.DBReaderAction.DBReaderSetDefaultValue);
            priority = SharedDBCode.GetReaderIntField(reader, "interfase_priority", SharedDBCode.DBReaderAction.DBReaderSetDefaultValue, 0);
            try
            {
                parentId = SharedDBCode.GetReaderIntField(reader, "parent", SharedDBCode.DBReaderAction.DBReaderThrowException);
            }
            catch(Exception)
            {
                parentId = null;
            }

            // Unused properties
            name = description;
            isVisible = true;
        }

        //internal static void DeleteEmptyMenuGroups(List<SSOBaseClass> groups)
        //{
        //    int i = 0;
        //    while (i < groups.Count)
        //    {
        //        if (groups[i] is SSOApplicationGroup)
        //        {
        //            SSOApplicationGroup group = (SSOApplicationGroup)groups[i];
        //            DeleteEmptyMenuGroups(group.MenuItems);
        //            if (!group.IsSeparator && (group.MenuItems.Count == 0))
        //            {
        //                groups.RemoveAt(i);
        //            }
        //            else
        //                i++;
        //        }
        //        else
        //            i++;
        //    }
        //}

        internal static void DeleteEmptyMenuGroupsByEfector(List<SSOBaseClass> groups)
        {
            int i = 0;
            while (i < groups.Count)
            {
                if (groups[i] is SSOApplicationGroup)
                {
                    SSOApplicationGroup group = (SSOApplicationGroup)groups[i];
                    DeleteEmptyMenuGroupsByEfector(group.MenuItemsByEfector);
                    if (!group.IsSeparator && (group.MenuItemsByEfector.Count == 0))
                    {
                        groups.RemoveAt(i);
                    }
                    else
                        i++;
                }
                else
                    i++;
            }
        }
    }
}