using System.Collections.Generic;
using System.Web;

namespace Salud.Security.SSO
{
    public class SSOConfiguration
    {
        public object this[string index]
        {
            get
            {
                SortedDictionary<string, object> configurations = SSOHelper.MembershipProvider.UseCache ? HttpContext.Current.Cache["Salud.Security.SSO.Configurations"] as SortedDictionary<string, object> : null;
                if (configurations == null)
                {
                    configurations = SSOHelper.MembershipProvider.GetConfigurations();
                    HttpContext.Current.Cache["Salud.Security.SSO.Configurations"] = configurations;
                }
                if (configurations.ContainsKey(index))
                    return configurations[index];
                else
                    return null;
            }
        }
    }
}