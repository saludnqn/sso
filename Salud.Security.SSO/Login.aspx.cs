using System;
using System.Web;
using System.Web.UI;
using System.Xml;

namespace Salud.Security.SSO
{
    public partial class Login : System.Web.UI.Page
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
            if (!Page.Request.IsSecureConnection)
                SSOHelper.RedirectToSecure();
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!Page.IsPostBack)
            {

                // ¿Ya está logeado?
                if (IsLoggedIn)
                    //Redireccionar a la seleccion de efector en caso que tenga mas de uno
                    SSOHelper.RedirectToURL();
                else
                {
                    // Utiliza protocolo seguro
                    if (!Page.Request.IsSecureConnection)
                    {
                        string url = Page.Request.Url.ToString();
                        if (url.StartsWith("http:"))
                            url = "https:" + url.Substring(5);
                        btnIniciarSesion.PostBackUrl = url;
                    }
                }
            }
            RefreshUI();
        }

        protected void RefreshUI()
        {
            if (!Page.IsPostBack)
            {
                lblLoginCaption.Text = SSOHelper.Configuration["Login_Caption"] != null ? SSOHelper.Configuration["Login_Caption"].ToString() : "Iniciar sesión";

                // Muestra advertencias
                if (Request.QueryString["timeout"] == "1")
                {
                    pnlWarningBox.Visible = true;
                    lblWarning.Text = "La sesión anterior fue cerrada por inactividad. Debe iniciar una sesión nuevamente";
                }

                // Tip de seguridad
                XmlDocument xml = new XmlDocument();
                xml.LoadXml(Resources.Resources.tips);
                var tips = xml.GetElementsByTagName("tip");
                lblTipSeguridad.Text = tips[new Random().Next(0, tips.Count - 1)].InnerText;
            }
            else
            {
                // Si estaba visible, la oculta
                pnlWarningBox.Visible = false;
            }
        }

        protected void btnIniciarSesion_Click(object sender, EventArgs e)
        {
            if (String.IsNullOrEmpty(tbUsername.Text.Trim()) || String.IsNullOrEmpty(tbPassword.Text.Trim()))
            {
                pnlErrorBox.Visible = true;
                lblError.Text = "Debes ingresar tu nombre de usuario y contraseña";
            }
            else
            {
                string sessionID;
                bool userLocked;
                if (SSOHelper.MembershipProvider.Login(tbUsername.Text.Trim(), tbPassword.Text.Trim(), Request.UserHostAddress, out sessionID, out userLocked))
                {
                    // Guarda la cookie de sesión
                    Response.Cookies.Add(new HttpCookie(SSOHelper.MembershipProvider.CookieName, sessionID) { Path = "/", Domain = SSOHelper.Configuration["Domain"].ToString() });
                    // Redirige a la URL original
                    //SSOHelper.RedirectToURL();

                    //Redirige a la URL de la selección de efector

                    string url = Page.Request.Url.ToString();
                    if (url.StartsWith("http:"))
                        url = "https:" + url.Substring(5);
                    HttpContext.Current.Response.Redirect("EfectorSession.aspx?url=" + url);                   
                }
                else
                {
                    pnlErrorBox.Visible = true;
                    lblError.Text = (userLocked) ? "Su usuario está bloqueado porque ingresó una contraseña incorrecta repetidas veces. Espere algunos minutos e intente ingresar nuevamente" : "Usuario o contraseña incorrectos";
                }
            }
        }
    }
}