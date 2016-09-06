using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using Salud.Security;
using Salud.Security.SSO;
using System.Web.SessionState;
using System.IO;
using Salud.Security.SSO.Data;

namespace Salud
{
    public partial class _Default : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (Context.User.Identity.IsAuthenticated)
                TextBox2.Text = "Postback!";

            //if (Context.User.Identity.IsAuthenticated)
            //{
            //    //SSOUser s = SSOHelper.FindUserByExternalID("26108063");
            //    SSOUser s = SSOHelper.FindUser(1);
            //    TextBox2.Text = s.Fullname;


            //    object o;
            //    o = SSOHelper.CurrentIdentity.StoredVariable["Prueba"].SingleValue;
            //    o = Session["juan"];
            //    o = Session["temp"];
            //    Session["temp"] = new Object();
            //    Session["temp"] = new Object();
            //    Session["temp"] = null;
            //    Session["temp"] = "Hola";
            //    Session["temp"] = "hola";
            //    Session["temp"] = null;
            //    Session["temp"] = null;
            //    Session["temp"] = 3;
            //    Session["juan"] = "segunda";
            //    //Session["temp"] = null;
            //    bool b = Context.User.Identity.IsAuthenticated;
            //}

            //SSOHelper.CurrentIdentity.StoredVariables("Common_Medicos").List;
            //TextBox1.Text = SSOHelper.MembershipProvider.GetStoredVariables((SSOIdentity)Context.User.Identity, "Compras_SuperUsuario").SingleValue.ToString();          
        }

        protected void Button1_Click(object sender, EventArgs e)
        {
            if (Context.User.Identity.IsAuthenticated)
                TextBox2.Text = "Postback!";
        }

        protected void ButtonEx1_Click(object sender, EventArgs e)
        {
            TextBox2.Text = "ButtonEx1 Click!" ;
        }
    }
}
