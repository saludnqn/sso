using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI.HtmlControls;

namespace Salud.Security.SSO.Pages
{
    public abstract class BasePage : Salud.Applications.Shared.Pages.BasePage
    {
        private Data.DataContext _dataContext;

        internal Data.DataContext DataContext
        {
            get
            {
                if (_dataContext == null)
                    _dataContext = SSOHelper.GetDataContext();
                return _dataContext;
            }
        }


        /// <summary>
        /// Indica si utiliza el MasterPage especial para SSO o bien el MasterPage del tema actual
        /// </summary>
        protected abstract bool UseSSOMasterPage
        {
            get;
        }

        public override string CurrentTheme
        {
            get
            {
                if (UseSSOMasterPage)
                    return null;
                else
                    return base.CurrentTheme;
            }
        }

        public bool IsLoggedIn
        {
            get
            {
                return User.Identity.IsAuthenticated && (User.Identity is SSOIdentity);
            }
        }
    }
}