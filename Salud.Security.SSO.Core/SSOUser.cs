using System;
using System.Web.Security;

namespace Salud.Security.SSO
{
    [Serializable]
    public sealed class SSOUser : MembershipUser
    {
        #region Properties and Getters

        private string fullname;
        private string firstname;
        private string surname;
        private int? documento;
        private string legajo;
        private string observacion;
        private bool must_change_password;
        private string externalId;
        private DateTime lockoutUntilDate;
        private DateTime permissions_updatedOn;
        private string mobile;
        private int? mobileCarrier;
        private bool isContentAdministrator;
        private bool isGlobalAdministrator;
        private bool isApplicationAdministrator;

        public int Id { get { return (int)base.ProviderUserKey; } }
        public string Fullname { get { return fullname; } }
        public string FirstName { get { return firstname; } }
        public string Surname { get { return surname; } }
        public int? Documento { get { return documento; } }
        public string Legajo { get { return legajo; } }
        public string Observacion { get { return observacion; } }

        [Obsolete]
        public bool IsAdministrator { get { return isGlobalAdministrator; } }
        public bool IsContentAdministrator { get { return isContentAdministrator; } }
        public bool IsGlobalAdministrator { get { return isGlobalAdministrator; } }
        public bool IsApplicationAdministrator { get { return isApplicationAdministrator; } }

        public bool Must_change_password { get { return must_change_password; } }
        public string ExternalId { get { return externalId; } }
        public DateTime LockoutUntilDate { get { return lockoutUntilDate; } }
        public DateTime Permissions_updatedOn { get { return permissions_updatedOn; } }
        public string Mobile { get { return mobile; } }
        public int? MobileCarrier { get { return mobileCarrier; } }        

        #endregion

        #region Constructor

        public SSOUser(object id, string username, string firstname, string surname, string email, string mobile, int? mobileCarrier, string externalId, 
            Int32 documento, string legajo, string observacion,
            string comment, bool isApproved, bool isLockedOut, bool isGlobalAdministrator, bool isApplicationAdministrator, bool isContentAdministrator)
            : base(SSOMembershipProvider.SSOMembershipProviderName, username, id, email, null, comment, isApproved, isLockedOut, System.DateTime.MinValue, 
            System.DateTime.MinValue, System.DateTime.MinValue, System.DateTime.MinValue, System.DateTime.MinValue)
        {
            this.firstname = firstname;
            this.surname = surname;
            this.fullname = firstname + " " + surname;            
            this.externalId = externalId;
            this.documento = documento;
            this.legajo = legajo;
            this.observacion = observacion;
            this.mobile = mobile;
            this.mobileCarrier = mobileCarrier;
            this.isApplicationAdministrator = isApplicationAdministrator;
            this.isGlobalAdministrator = isGlobalAdministrator;
            this.isContentAdministrator = isContentAdministrator;
        }
        #endregion
    }
}