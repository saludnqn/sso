using System;
using System.Text;
using System.Web;
using System.Web.UI;

namespace Salud.Security.SSO
{
    public class SSOHttpModule : IHttpModule
    {
        public void Init(HttpApplication application)
        {
            application.AuthenticateRequest += Application_AuthenticateRequest;
            application.EndRequest += Application_EndRequest;
            application.Error += Application_Error;
        }

        public void Dispose() { }

        private bool IsImage()
        {
            string path = HttpContext.Current.Request.Url.AbsolutePath.ToLower();
            return path.EndsWith(".jpg") || path.EndsWith(".gif") || path.EndsWith(".png") || path.EndsWith(".css") || path.EndsWith(".swf");
        }

        private bool IsScript()
        {
            string path = HttpContext.Current.Request.Url.AbsolutePath.ToLower();
            return path.EndsWith(".js") || path.EndsWith(".axd") /* Web resources */;
        }

        private bool IsWebMethod()
        {
            string path = HttpContext.Current.Request.PhysicalPath.ToLower();
            return path.EndsWith(".asmx");
        }

        private bool RequireAccess(SSOModule module)
        {
            if (!module.IsProtected)
                return true;
            else
            {
                if (SSOHelper.TestPermissionByEfector(module))
                {
                    SSOHelper.CurrentIdentity.BeginAccess(module);
                    return true;
                }
                else
                    return false;
            }
        }

        private void Application_AuthenticateRequest(object sender, EventArgs e)
        {
            SSOHelper.Authenticate();

            if (!((SSOHelper.MembershipProvider.AllowAnonymousAccessToImages && IsImage()) || (SSOHelper.MembershipProvider.AllowAnonymousAccessToScripts && IsScript())))
            {
                SSOModule module = SSOModule.FindByURL(HttpContext.Current.Request.Url);
                if (module == null)
                {
                    if (SSOHelper.MembershipProvider.AlwaysAuthenticate && (SSOHelper.CurrentIdentity == null || SSOHelper.CurrentIdentity.State == SSOIdentitySessionState.Inexistent))
                        SSOHelper.RedirectToSSOPage("Login.aspx", HttpContext.Current.Request.Url.ToString());
                    else
                    {
                        if (SSOHelper.CurrentIdentity == null || SSOHelper.CurrentIdentity.State == SSOIdentitySessionState.Inexistent)
                            HttpContext.Current.User = null;
                        else
                            if (!IsWebMethod() || SSOHelper.MembershipProvider.UpdateTimeoutOnWebMethod)
                                SSOHelper.MembershipProvider.UpdateTimeout(SSOHelper.CurrentIdentity);
                    }
                }
                else
                {
                    // Check if it needs to authenticate
                    if (SSOHelper.MembershipProvider.AlwaysAuthenticate || module.IsProtected)
                    {
                        if (SSOHelper.CurrentIdentity == null)
                            SSOHelper.RedirectToSSOPage("Login.aspx", HttpContext.Current.Request.Url.ToString());
                        else
                            switch (SSOHelper.CurrentIdentity.State)
                            {
                                case SSOIdentitySessionState.Ok:
                                    if (RequireAccess(module))
                                    {
                                        // Access allowed --> Update timeout
                                        if (!IsWebMethod() || SSOHelper.MembershipProvider.UpdateTimeoutOnWebMethod)
                                            SSOHelper.MembershipProvider.UpdateTimeout(SSOHelper.CurrentIdentity);
                                    }
                                    else
                                        SSOHelper.RedirectToErrorPage(403, 0, null);
                                    break;
                                case SSOIdentitySessionState.Locked:
                                    SSOHelper.RedirectToSSOPage("LockSession.aspx", HttpContext.Current.Request.Url.ToString());
                                    break;
                                case SSOIdentitySessionState.Inexistent:
                                    SSOHelper.RedirectToSSOPage("Login.aspx?timeout=1", HttpContext.Current.Request.Url.ToString());
                                    break;
                                case SSOIdentitySessionState.SecurityError:
                                    SSOHelper.RedirectToErrorPage(403, 4, null);
                                    break;
                            }
                    }
                    else
                    {
                        // Access allowed --> Update timeout
                        if (SSOHelper.CurrentIdentity != null && SSOHelper.CurrentIdentity.State == SSOIdentitySessionState.Ok && (!IsWebMethod() || SSOHelper.MembershipProvider.UpdateTimeoutOnWebMethod))
                            SSOHelper.MembershipProvider.UpdateTimeout(SSOHelper.CurrentIdentity);
                    }
                }
            }
        }

        private void Application_EndRequest(object sender, EventArgs e)
        {
            if (SSOHelper.CurrentIdentity != null)
                SSOHelper.CurrentIdentity.FinalizeAccess();
        }

        private void Application_Error(object sender, EventArgs e)
        {
            if (SSOHelper.CurrentIdentity != null)
                SSOHelper.CurrentIdentity.FinalizeAccess();

            // Envía un mail con la excepción
            //string mailServer;
            string debugHosts;
            //try
            //{
            //    mailServer = (string)SSOHelper.Configuration["Email_Server"];
            //}
            //catch (Exception)
            //{
            //    mailServer = "exchange2010.hospitalneuquen.org.ar";
            //}

            try
            {
                debugHosts = ((string)SSOHelper.Configuration["Debug_Hosts"]).ToUpper();
            }
            catch (Exception)
            {
                debugHosts = "HOST1,HOST2,HOST352";
            }


            Exception exception = HttpContext.Current.Server.GetLastError();
            HttpException httpException = exception as HttpException;

            // Define que tipo de excepciones enviará por mail
            if ((httpException == null || (httpException != null && httpException.GetHttpCode() != 404)) && !exception.Message.ToLower().StartsWith("the client disconnected") && (String.IsNullOrEmpty(debugHosts) || !debugHosts.Contains(HttpContext.Current.Server.MachineName.ToUpper())))
            {
                // Construye el mensaje con la mayor cantidad de datos
                StringBuilder sb = new StringBuilder();
                try { sb.Append(String.Format("Servidor: <b>{0}</b><br/>", HttpContext.Current.Server.MachineName)); }
                catch (Exception) { };
                try { sb.Append(String.Format("URL: <b>{0}</b><br/>", HttpContext.Current.Request.Url.ToString())); }
                catch (Exception) { };
                try { sb.Append(String.Format("Host: <b>{0}</b><br/>", HttpContext.Current.Request.UserHostName)); }
                catch (Exception) { };
                try { sb.Append(String.Format("IP: <b>{0}</b><br/>", HttpContext.Current.Request.UserHostAddress)); }
                catch (Exception) { };
                try { sb.Append(String.Format("Usuario: <b>{0}</b> ({1})<br/>", SSOHelper.CurrentIdentity.Fullname, SSOHelper.CurrentIdentity.Username)); }
                catch (Exception) { };
                try { sb.Append(String.Format("¿Es Postback?: <b>{0}</b><br/>", ((Page)HttpContext.Current.Handler).IsPostBack)); }
                catch (Exception) { };
                try { sb.Append(String.Format("¿Es Ajax?: <b>{0}</b><br/>", ScriptManager.GetCurrent((Page)HttpContext.Current.Handler).IsInAsyncPostBack)); }
                catch (Exception) { };

                sb.Append("<br/><b>Source:</b> ");
                sb.Append(exception.Source.Replace("\n", "<br/>"));
                sb.Append("<br/><br/><b>Message:</b> ");
                sb.Append(exception.Message.Replace("\n", "<br/>"));
                sb.Append("<br/><br/><b>Stack Trace:</b> ");
                sb.Append(exception.StackTrace.Replace("\n", "<br/>"));
                sb.Append("<br/><br/><b>Otra información:</b> ");
                sb.Append(exception.ToString().Replace("\n", "<br/>"));

                //System.Net.Mail.SmtpClient SmtpClient = new System.Net.Mail.SmtpClient(mailServer);
                //System.Net.Mail.MailMessage message = new System.Net.Mail.MailMessage("intranet@hospitalneuquen.org.ar", "jgabriel@hospitalneuquen.org.ar,sgiacosa@hospitalneuquen.org.ar,hfernandez@hospitalneuquen.org.ar");
                //message.Subject = String.Format("[Intranet] Servidor {0} | Error {1}", HttpContext.Current.Server.MachineName, (httpException == null ? "desconocido" : httpException.GetHttpCode().ToString()));
                //message.IsBodyHtml = true;
                //message.Body = sb.ToString();
                //SmtpClient.Send(message);

                // Redirige sólo si no es ajax o un WebService
                bool redirect;
                try
                {
                    //redirect = !(((Page)HttpContext.Current.Handler).IsPostBack && ScriptManager.GetCurrent((Page)HttpContext.Current.Handler).IsInAsyncPostBack) && !((Page)HttpContext.Current.Handler).Request.Path.ToLower().EndsWith(".asmx");
                    redirect = !(((Page)HttpContext.Current.Handler).IsPostBack && ScriptManager.GetCurrent((Page)HttpContext.Current.Handler).IsInAsyncPostBack);
                }
                catch (Exception)
                {
                    redirect = true;
                }

                if (redirect)
                    try
                    {
                        SSOHelper.RedirectToErrorPage(500, 0, null);
                    }
                    catch (Exception)
                    {
                        throw HttpContext.Current.Server.GetLastError();
                    }
                else
                    throw HttpContext.Current.Server.GetLastError();
            }
        }
    }


    public class Global : System.Web.HttpApplication
    {
        public static SSOHttpModule Module = new SSOHttpModule();

        public override void Init()
        {
            base.Init();
            Module.Init(this);
        }
    }

}