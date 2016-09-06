using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using Salud.Security.SSO.Pages;

namespace Salud.Security.SSO
{
    public partial class AccessLog : BasePage
    {
        protected override bool UseSSOMasterPage
        {
            get { return false; }
        }

        protected override bool UseInnerMasterPage
        {
            get
            {
                return true;
            }
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!base.IsLoggedIn)
                SSOHelper.RedirectToErrorPage(403, 0, null);
        }

        /*
        protected void gridAccessLog_SelectedIndexChanged(object sender, EventArgs e)
        {
            gridData.Rebind();
        }

        protected void dsData_Selecting(object sender, SqlDataSourceSelectingEventArgs e)
        {
            if (gridAccessLog.SelectedValues == null)
            {
                e.Cancel = true;
            }
            else
            {
                e.Command.Parameters["@userId"].Value = gridAccessLog.SelectedValues["userId"];
                e.Command.Parameters["@fromDate"].Value = gridAccessLog.SelectedValues["timeIn"];
                e.Command.Parameters["@toDate"].Value = gridAccessLog.SelectedValues["timeOut"];
            }
        }

        protected void dsAccessLog_Selected(object sender, SqlDataSourceStatusEventArgs e)
        {
            if (gridAccessLog.SelectedIndexes.Count == 0 && e.AffectedRows > 0)
            {
                gridAccessLog.SelectedIndexes.Add(0);
            }
        }

        protected void gridAccessLog_DataBound(object sender, EventArgs e)
        {
            gridData.Rebind();
        }
         * */
    }
}