using System;
using System.Configuration;
using System.Linq;
using System.Web.UI;

namespace Salud.Security.SSO
{

    /// <summary>
    /// Permite bloquear y desbloquear la sesión activa
    /// Parámetros:
    ///     closePopUp (opcional): Cierra la ventana (ver más abajo nota sobre problema de HTTPS)
    /// </summary>
    public partial class EfectorSession : System.Web.UI.Page
    {        
        public bool IsLoggedIn
        {
            get
            {
                return User.Identity.IsAuthenticated && (User.Identity is SSOIdentity);
            }
        }

        protected void Page_Init(object sender, EventArgs e)
        {
            if (!IsLoggedIn)
               SSOHelper.RedirectToSSOPage("Login.aspx", null);
            else
            {
                if (!Page.Request.IsSecureConnection)
                    SSOHelper.RedirectToSecure();
            }
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!Page.IsPostBack)
            {
                var query = from r in DataContext.SSO_ObtenerEfectores(SSOHelper.CurrentIdentity.Id)
                            select new
                            {
                                r.id,
                                r.name
                            };

                
                ddlEfector.DataSource = query;
                ddlEfector.DataTextField = "name";
                ddlEfector.DataValueField = "id";
                ddlEfector.DataBind();

                if (ddlEfector.Items.Count == 0)
                { 
                    lblMensaje.Text = "No tiene asignado permisos a efectores. Contactese con el administrador del sistema."; lblMensaje.Visible = true;
                    Panel1.Visible = false;
                    ddlEfector.Visible = false;
                    btnContinuar.Visible = true;
                }
                
                if (ddlEfector.Items.Count == 1)
                   accederAlSistema();
                
            
            }            
        }

        protected void RefreshUI()
        {
            if (!Page.IsPostBack)
            {                
                
            }
        }

        protected void btnRegresar_Click(object sender, EventArgs e)
        {
            SSOHelper.RedirectToSSOPage("Login.aspx", null);
        }
        protected void btnContinuar_Click(object sender, EventArgs e)
        {
            bool doReturn = false;
            if (SSOHelper.CurrentIdentity != null)
            {
                if (!String.IsNullOrEmpty(ddlEfector.Text))
                {                    
                    string[] efector = new string[3];
                    string[] valores = ddlEfector.SelectedValue.Split(';');
                    efector[0] = valores[0]; //idEfectorRol -- lo usamos para armar los menues
                    efector[1] = ddlEfector.SelectedItem.Text; //descripcion
                    efector[2] = valores[1]; //idEfector del SysEfector
                    Session["idefector"] = efector;
                    SSOHelper.MembershipProvider.RegistarEfector((int.Parse(((string[])Session["idefector"])[2])),
                                                                 (int.Parse(((string[])Session["idefector"])[0])));
                        
                        
                                                                  
                    // Redirige a la URL original
                    SSOHelper.RedirectToURL();                    
                }
                else
                {
                    //pnlWarningBox.Visible = false;
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

        private void accederAlSistema()
        {
            string[] efector = new string[3];
            string[] valores = ddlEfector.SelectedValue.Split(';');
            efector[0] = valores[0];
            efector[1] = ddlEfector.Text;
            efector[2] = valores[1];
            Session["idefector"] = efector;
            SSOHelper.MembershipProvider.RegistarEfector((int.Parse(((string[])Session["idefector"])[2])),
                                                         (int.Parse(((string[])Session["idefector"])[0])));

            SSOHelper.RedirectToURL();
        }

        public void ClosePopup(object parameter)
        {
            string closeParam;
            if (parameter == null)
                closeParam = "null";
            else
                if (parameter is Enum)
                    closeParam = ((int)Convert.ChangeType(parameter, typeof(int))).ToString();
                else
                    closeParam = (parameter is string) ? String.Format("'{0}'", parameter) : parameter.ToString().ToLower();
                                
            // Previene que se renderee el contenido de la página
            try
            {
                this.Master.FindControl("ContentBodyPlaceHolder").Visible = false;
            }
            catch (Exception) { }
            try
            {
                this.Master.FindControl("HeaderPlaceHolder").Visible = false;
            }
            catch (Exception) { }
            try
            {
                this.Master.FindControl("FooterPlaceHolder").Visible = false;
            }
            catch (Exception) { }
        }

    }
}