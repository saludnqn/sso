
namespace Salud.Security.SSO
{
    public class SSOBaseClass
    {
        protected int id;
        protected string name;
        protected string description;
        protected string url;
        protected int menuId = -1;
        protected bool isVisible;
        protected int priority;
        protected string image;   //agregó GDS 31/05/2013
        protected string urlPage; //agregó GDS 31/05/2013

        #region Public properties
        public int Id
        {
            get
            {
                return id;
            }
        }
        public string Name
        {
            get
            {
                return name;
            }
        }
        public string Description
        {
            get
            {
                return description;
            }
        }
        public string URL
        {
            get
            {
                return url;
            }
        }
        public bool IsVisible
        {
            get
            {
                return isVisible;
            }
        }     
        public int Priority
        {
            get
            {
                return priority;
            }
        }
        public string Image
        {
            get
            {
                return image;
            }
        }    
        public string UrlPage
        {
            get
            {
                return urlPage;
            }
        }

        #endregion
    }

    public static class SSOBaseClassComparer
    {
        public static int CompareByPriority(SSOBaseClass x, SSOBaseClass y)
        {
            int result = x.Priority.CompareTo(y.Priority);
            if (result == 0)
            {
                if ((x.Description == null) || (y.Description == null))
                {
                    result = x.Name.CompareTo(y.Name);
                }
                else
                {
                    result = x.Description.CompareTo(y.Description);
                }
            }
            return result;
        }
    }

}