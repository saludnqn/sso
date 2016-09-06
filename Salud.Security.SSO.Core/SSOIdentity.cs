using System;
using System.Linq;
using System.Security.Principal;
using System.Web;

namespace Salud.Security.SSO
{
    [Serializable]
    public class SSOIdentity : MarshalByRefObject, IIdentity
    {
        # region Properties and Getters

        private SSOUser user;
        private string ip;
        private SSOIdentitySessionState state;
        private string sessionId;
        private int idEfector; //gustavo 19/05/2015
        private int idEfectorRol;
        private SSOIdentityVariables variables;
        private int?currentAccessKey;
        private SSOModule currentModule;

        public int Id { get { return user.Id; } }
        public string Username { get { return user.UserName; } }
        public string Fullname { get { return user.Fullname; } }
        public string FirstName { get { return user.FirstName; } }
        public string Surname { get { return user.Surname; } }
        public string Email { get { return user.Email; } }
        public string Mobile { get { return user.Mobile; } }
        public int? MobileCarrier { get { return user.MobileCarrier; } }
        public string Comment { get { return user.Comment; } }
        public bool IsGlobalAdministrator { get { return user.IsGlobalAdministrator; } }
        public bool IsContentAdministrator { get { return user.IsContentAdministrator; } }
        public bool IsApplicationAdministrator { get { return user.IsApplicationAdministrator; } }
        public bool IsApproved { get { return user.IsApproved; } }
        public bool IsLockedOut { get { return user.IsLockedOut; } }
        public bool Must_change_password { get { return user.Must_change_password; } }
        public string ExternalId { get { return user.ExternalId; } }
        public DateTime LastLockoutDate { get { return user.LastLockoutDate; } }
        public DateTime LockoutUntilDate { get { return user.LockoutUntilDate; } }
        public DateTime Permissions_updatedOn { get { return user.Permissions_updatedOn; } }
        public SSOIdentitySessionState State { get { return state; } }
        public string IP { get { return ip; } }
        public int? Documento { get { return user.Documento; } }   //gustavo 13/11/2013
        public string Legajo { get { return user.Legajo; } }      //gustavo 13/11/2013
        public string Observacion { get { return user.Observacion; } }  //gustavo 13/11/2013
        public string SessionId { get { return sessionId; } }
        public int IdEfector { get { return idEfector; } }     //gustavo 19/06/2013
        public int IdEfectorRol { get { return idEfectorRol; } }     //gustavo 19/06/2013
               
        /// <summary>
        /// Indica el módulo actual
        /// </summary>
        /// <value>The current module.</value>
        internal SSOModule CurrentModule
        {
            get
            {
                return currentModule;
            }
        }
        
        #endregion

        # region Interface Properties implementation
        // Properties
        /// <summary>
        /// Gets the Authentication Type
        /// </summary>
        public string AuthenticationType
        {
            get { return "Custom"; }
        }

        /// <summary>
        /// Indicates whether the User is authenticated
        /// </summary>
        public bool IsAuthenticated
        {
            get { return State != SSOIdentitySessionState.Inexistent; }
        }

        /// <summary>
        /// Gets or sets the UserID of the User
        /// </summary>
        public string Name
        {
            get
            {
                return user.UserName;
            }
        }

        #endregion

        # region Constructors

        public SSOIdentity(HttpCookie ssoCookie)
        {
            using (Data.DataContext DataContext = SSOHelper.GetDataContext())
            {
                var session = (from r in DataContext.SSO_Sessions
                               where r.id == ssoCookie.Value
                               select new
                               {
                                   user = r.SSO_Users,
                                   isGlobalAdministrator = DataContext.SSO_UserInRole(r.userId, (int)WellKnownRoles.GlobalAdministrator) > 0,
                                   isContentAdministrator = DataContext.SSO_UserInRole(r.userId, (int)WellKnownRoles.ContentAdministrator) > 0,
                                   isApplicationAdministrator = DataContext.SSO_UserInRole(r.userId, (int)WellKnownRoles.ApplicationAdministrator) > 0,
                                   locked = r.locked,
                                   ip = r.userIP,
                                   idEfector = r.idEfector,   //gustavo 19/06/2013 
                                   idEfectorRol = r.idEfectorRol //gustavo 19/06/2013 
                               }).SingleOrDefault();

                if (session == null)
                {
                    state = SSOIdentitySessionState.Inexistent;
                    sessionId = null;
                    user = new SSOUser(0, null, null, null, null, null, null, null,0 , null, null, null, false, false, false, false, false);
                }
                else
                {
                    sessionId = ssoCookie.Value;
                    user = new SSOUser(session.user.id, session.user.username, session.user.name, session.user.surname, session.user.email, session.user.mobile, 
                           session.user.idCarrier, session.user.external_id == null ? null : session.user.external_id.ToString(), 
                           session.user.documento.GetValueOrDefault(0), session.user.legajo, session.user.observacion,
                           session.user.description, session.user.enabled, session.user.locked, session.isGlobalAdministrator, session.isApplicationAdministrator, session.isContentAdministrator);
                    if (session.locked)
                        state = SSOIdentitySessionState.Locked;
                    else
                    {
                        // Controla la seguridad de la sesión (sólo si está activada y no se está utilizado SSL). Esto evita que un usuario malintencionado robe la cookie de SSO y permita loggearse con la identidad de otro.
                        object o = SSOHelper.Configuration["SessionID_Protection"];
                        if (o != null && o.ToString() == "1" && !HttpContext.Current.Request.IsSecureConnection && (session.ip != HttpContext.Current.Request.UserHostAddress))
                            state = SSOIdentitySessionState.SecurityError;
                        else
                            state = SSOIdentitySessionState.Ok;
                    }

                    idEfector = session.idEfector;
                    idEfectorRol = session.idEfectorRol;
                }
            }
        }

        #endregion

        #region Public Methods
        public SSOIdentityVariables StoredVariable
        {
            get
            {
                if (variables == null)
                    variables = new SSOIdentityVariables(this);
                return variables;
            }
        }

        public object GetSetting(object settingId)
        {
            return SSOHelper.MembershipProvider.GetUserSetting(this, settingId);
        }

        public void SetSetting(object settingId, object value)
        {
            SSOHelper.MembershipProvider.SetUserSetting(this, settingId, value);
        }

        public void LogAccessKey(string key, string value)
        {
            if (this.currentAccessKey.HasValue)
                SSOHelper.MembershipProvider.LogAccessKey(this.currentAccessKey.Value, key, value);
        }

        /// <summary>
        /// Verifica si el usuario tiene permisos para una url con el efector que esta logueado
        /// Julio 20130712
        /// </summary>
        /// <param name="key">url a buscar</param>
        /// <returns>true en caso que tenga permisos, false si no tiene permisos</returns>
        public bool TestPermissionByEfector(string key)
        {
            var mm = SSOHelper.MembershipProvider.GetAllowedModulesByEfector(SSOHelper.CurrentIdentity, SSOHelper.CurrentModule.Application);
            return mm.Count(x => x.URL == key) > 0;
        }

        internal void BeginAccess(SSOModule module)
        {
            this.currentModule = module;
            // Si el módulo no está protegido no genera un registro en el access log
            this.currentAccessKey = module.IsProtected ? (int?)SSOHelper.MembershipProvider.BeginAccess(this, module) : null;            
        }

        internal void FinalizeAccess()
        {
            this.currentModule = null;
            if (this.currentAccessKey.HasValue)
            {
                try { SSOHelper.MembershipProvider.FinalizeAccess(this.currentAccessKey.Value, true); }
                catch (Exception) { };
                this.currentAccessKey = null;
            }
        }
        
        #endregion
    }
}
