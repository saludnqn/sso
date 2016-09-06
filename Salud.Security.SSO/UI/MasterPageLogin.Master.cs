using System;
using System.Web.UI;
using System.Web.UI.HtmlControls;

namespace Salud.Security.SSO
{
    public partial class MasterPageLogin : Salud.Applications.Shared.Themes.Login.MasterPage
        
    {
        protected override void ApplyTheme()
        {
            ScriptManager.Scripts.Add(new ScriptReference(String.Format("{0}/Resources/jQuery.Rotate/jQueryRotate.js", Request.ApplicationPath)));
            HtmlLink headFile = new HtmlLink();
            headFile.Href = ThemeURL + "../Tierra/Style.css";
            headFile.Attributes["rel"] = "stylesheet";
            headFile.Attributes["type"] = "text/css";
            Page.Header.Controls.Add(headFile);
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