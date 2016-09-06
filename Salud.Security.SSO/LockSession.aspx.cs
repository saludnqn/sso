using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Xml;
using Salud.Security.SSO.Pages;

namespace Salud.Security.SSO
{
    /// <summary>
    /// Permite bloquear y desbloquear la sesión activa
    /// Parámetros:
    ///     closePopUp (opcional): Cierra la ventana (ver más abajo nota sobre problema de HTTPS)
    /// </summary>
    public partial class LockSession : BasePage
    {
        protected override bool UseSSOMasterPage
        {
            get { return true; }
        }

        protected void Page_Init(object sender, EventArgs e)
        {
            if (!IsLoggedIn)
               SSOHelper.RedirectToSSOPage("Login.aspx", null);
            else
            {
                if (!Page.Request.IsSecureConnection && String.IsNullOrEmpty(Request.QueryString["closePopUp"]))
                    SSOHelper.RedirectToSecure();
            }
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!Page.IsPostBack)
            {
                if (!String.IsNullOrEmpty(Request.QueryString["closePopUp"]))
                    this.ClosePopup(true);
                else
                {
                    // Si la sesión no estaba bloqueada, la bloquea
                    if (SSOHelper.CurrentIdentity.State != SSOIdentitySessionState.Locked)
                        SSOHelper.MembershipProvider.ChangeLockStatus(SSOHelper.CurrentIdentity, true);
                }
            }
            RefreshUI();
        }

        protected void RefreshUI()
        {
            if (!Page.IsPostBack)
            {                
                tbUsername.Text = SSOHelper.CurrentIdentity.Username;

                // Tip de seguridad
                XmlDocument xml = new XmlDocument();
                xml.LoadXml(Resources.Resources.tips);
                var tips = xml.GetElementsByTagName("tip");
                lblTipSeguridad.Text = tips[new Random().Next(0, tips.Count - 1)].InnerText;
            }
        }
        protected void btnContinuar_Click(object sender, EventArgs e)
        {
            bool doReturn = false;
            if (SSOHelper.CurrentIdentity != null && SSOHelper.CurrentIdentity.State == SSOIdentitySessionState.Locked)
            {
                if (!String.IsNullOrEmpty(tbPassword.Text))
                {
                    int userId;
                    if (SSOHelper.MembershipProvider.TestPassword(SSOHelper.CurrentIdentity.Username, tbPassword.Text, out userId))
                    {
                        SSOHelper.MembershipProvider.ChangeLockStatus(SSOHelper.CurrentIdentity, false);
                        doReturn = true;
                    }
                    else
                    {
                        pnlErrorBox.Visible = true;
                        pnlWarningBox.Visible = false;
                    }
                }
                else
                {
                    pnlErrorBox.Visible = true;
                    pnlWarningBox.Visible = false;
                }
            }
            else
                doReturn = true;

            if (doReturn)
            {
                // Si está en un popup ...
                if (!String.IsNullOrEmpty(Request.QueryString["inside"]))
                {
                    // 10/08/2011 | jgabriel | Como se ejecuta bajo HTTPS las función javascript cross-iframe no funciona, entonces hago una redirección
                    // ¿Hubo cambio de protocolo HTTP -> HTTPS? Si se cambió, hace una redirección.
                    if (String.IsNullOrEmpty(Request.QueryString["RedirectToSecure"]))
                        ClosePopup(true);
                    else
                    {
                        string url = Page.Request.Url.ToString();
                        if (url.StartsWith("https:"))
                            url = "http:" + url.Substring(6);
                        Response.Redirect(String.Format("{0}&closePopUp=1", url));
                    }
                }
                else
                    SSOHelper.RedirectToURL();
            }
        }

    }
}