using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using Telerik.Web.UI;
using Salud.Security.SSO;

namespace Salud
{
    public partial class Menutest : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            SSOHelper.CurrentIdentity.SetSetting("cssStyle", "Naranja");
            object v = SSOHelper.CurrentIdentity.GetSetting("cssStyle");
        }

        protected void RadMenu1_DataBound(object sender, EventArgs e)
        {
            //RadMenuItem item = RadMenu1.Items.FindItemByValue("114");
            //if (item != null) item.Selected = true;
        }

        protected void RadMenu4_ItemDataBound(object sender, RadMenuEventArgs e)
        {
            e.Item.IsSeparator = (e.Item.DataItem as SSOMenuItem).IsSeparator;
        }
    }
}
