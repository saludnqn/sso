using Salud.Security.SSO.Pages;
using System;
using System.Web;
using System.DirectoryServices.AccountManagement;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web.UI;

namespace Salud.Security.SSO
{
    public partial class Options : BasePage
    {
        protected override bool UseSSOMasterPage
        {
            get { return true; }
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            if (IsLoggedIn)
            {
                SSOHelper.RedirectToSecure();

                if (!Page.IsPostBack)
                {
                    //if (!String.IsNullOrEmpty(Request.QueryString["url"])) asi solo devuelve null ... Gustavo Saraceni
                    //string url = HttpContext.Current.Request.QueryString["url"];
                    //if (string.IsNullOrEmpty(url))
                    //    url = SSOHelper.Configuration["StartPage"] as string;

                    //btnVolver.OnClientClick = String.Format("window.location='{0}'", Request.QueryString["url" + "/sips"]);
                    RefreshUI();
                }
            }
            else
                SSOHelper.RedirectToSSOPage("Login.aspx", Request.Url.ToString());
        }

        private void RefreshUI()
        {
            if (!Page.IsPostBack)
            {
                tbPassword.Focus();

                // Bloqueo
                try
                {
                    object time = SSOHelper.CurrentIdentity.GetSetting("SessionBlock");
                    if (time == null)
                        cbTiempoBloqueo.SelectedValue = SSOHelper.Configuration["Default_SessionLock"].ToString();
                    else
                        cbTiempoBloqueo.SelectedValue = time.ToString();
                }
                catch (Exception)
                {
                    cbTiempoBloqueo.SelectedIndex = 0;
                }
            }
        }

        protected void btnGuardar_Click(object sender, EventArgs e)
        {

            if (tbNewPassword.Text.Length > 0)
            {
                bool ok = tbNewPassword.Text.Length >= 5; /* && Regex.IsMatch(tbNewPassword.Text, "[a-z]") && Regex.IsMatch(tbNewPassword.Text, "[A-Z]") && Regex.IsMatch(tbNewPassword.Text, "[0-9]") */

                try
                {
                    if (!ok)
                        ShowPopup("Debe ingresar una contraseña más segura");
                    else
                    {
                        if (tbNewPassword.Text != tbNewPasswordConfirmation.Text)
                            ShowPopup("La confirmación de la nueva contraseña no coincide");
                        else
                        {
                            object o = SSOHelper.Configuration["ActiveDirectory_Enabled"];
                            if (o != null && o.ToString() == "1")
                                try
                                {
                                    using (var context = new PrincipalContext(ContextType.Domain))
                                    using (var user = UserPrincipal.FindByIdentity(context, IdentityType.SamAccountName, SSOHelper.CurrentIdentity.Username))
                                        if (user != null)
                                            user.SetPassword(tbNewPassword.Text);
                                }
                                catch (Exception)
                                {
                                    ok = false;
                                }

                            if (ok)
                            {
                                SSOHelper.MembershipProvider.ChangePassword(tbNewPassword.Text);

                                try
                                {
                                    SSOHelper.CurrentIdentity.SetSetting("SessionBlock", cbTiempoBloqueo.SelectedValue);
                                }
                                catch (Exception) { }

                                pnlPasswordOk.Visible = ok;
                                pnlPasswordError.Visible = !ok;
                                btnGuardar.Visible = false;
                                btnCancelar.Text = "Volver";
                                btnCancelar.CssClass = "botonVolver";
                            }
                            else
                                ShowPopup("La contraseña no pudo cambiarse. Por favor comuníquese con " + (SSOHelper.Configuration["HelpDeskInformation"] as string));
                        }
                    }
                }
                catch (Exception) { }
            }
            else
            {
                try
                {
                    SSOHelper.CurrentIdentity.SetSetting("SessionBlock", cbTiempoBloqueo.SelectedValue);
                }
                catch (Exception) { }

                pnlPasswordOk.Visible = true;
                pnlPasswordError.Visible = false;
                btnCancelar.Text = "Volver";
                btnCancelar.CssClass = "botonVolver";
            }
        }

        protected void btnCancelar_Click(object sender, EventArgs e)
        {
            //carolina: cambio para que tome la ruta del sips desde la tabla configuracion
            string publicacion_sips = SSOHelper.Configuration["Publicacion_Sips"] as string;
            Response.Redirect("/"+publicacion_sips+"/default.aspx");
            //Response.Redirect("/sips/default.aspx");
        }
        
        protected void btnContinuar_Click(object sender, EventArgs e)
        {
            int temp;
            if (!SSOHelper.MembershipProvider.TestPassword(SSOHelper.CurrentIdentity.Username, tbPassword.Text, out temp))
            {
                pnlWarningBox.Visible = true;
                lblWarning.Text = "Contraseña incorrecta";
            }
            else
            {
                
                pnlLogin.Visible = false;
                pnlOptions.Visible = true;                
                pnlWarningBox.Visible = false;
            }
        }                
    }
}