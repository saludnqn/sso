using System;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using Salud.Applications.Shared.Pages;

namespace Salud.Security.SSO
{
    public partial class MasterPage : Salud.Applications.Shared.Themes.Tierra.MasterPage
    {
        protected override void ApplyTheme()
        {


            RadScriptManager.Scripts.Add(new ScriptReference(String.Format("{0}/Resources/jQuery.Rotate/jQueryRotate.js", Request.ApplicationPath)));
            //lblEfector.Text = SSOHelper.GetNombreEfectorRol(SSOHelper.CurrentIdentity.IdEfectorRol); 
            HtmlLink headFile = new HtmlLink();
            headFile.Href = (Page as BasePage).ThemeURL + "Tierra/Style.css";
            headFile.Attributes["rel"] = "stylesheet";
            headFile.Attributes["type"] = "text/css";
            Page.Header.Controls.Add(headFile);            

            styleFullPage.Visible = String.IsNullOrEmpty(Request.QueryString["inside"]);
            styleInnerPage.Visible = !String.IsNullOrEmpty(Request.QueryString["inside"]);

            //base.ApplyTheme();
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                string strIdHospital = SSOHelper.Configuration["idHospital"] as string;
                if (strIdHospital != "0")
                    lnkStyleSheet.Href = "styleHospital.css";
                else
                    lnkStyleSheet.Href = "style.css";
            }

        }
    }
}