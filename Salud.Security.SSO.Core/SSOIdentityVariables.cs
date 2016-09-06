
namespace Salud.Security.SSO
{
    /// <summary>
    /// Clase auxiliar que sirve para generar un indexer para la propiedad <see cref="SSOIdentity.StoredVariables"/>
    /// </summary>
    public class SSOIdentityVariables
    {
        SSOIdentity identity;

        internal SSOIdentityVariables(SSOIdentity identity)
        {
            this.identity = identity;
        }

        public SSOVariables this[string index]
        {
            get
            {
                return SSOHelper.MembershipProvider.GetStoredVariables(identity.Id, index);
            }
        }
    }
}