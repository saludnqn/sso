using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using Salud.Security.SSO.Pages;

namespace Salud.Security.SSO
{
    public partial class Logout : BasePage
    {
        protected override bool UseSSOMasterPage
        {
            get { return true; }
        }

        protected void Page_Init(object sender, EventArgs e)
        {
            if (IsLoggedIn)
            {
                // Logout
                SSOHelper.MembershipProvider.Logout(SSOHelper.CurrentIdentity.SessionId);
                // Clear cookie
                Response.Cookies.Add(new HttpCookie(SSOHelper.MembershipProvider.CookieName, null) { Path = "/", Domain = SSOHelper.Configuration["Domain"].ToString(), Expires = DateTime.Now });
            }

            // Redirect
            if (Request.QueryString["relogin"] == "1")
                SSOHelper.RedirectToSSOPage("Login.aspx", Request.QueryString["url"]);
            else
                SSOHelper.RedirectToURL();
        }
    }
}