using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using Salud.Security.SSO.Pages;

namespace Salud.Security.SSO
{
    public partial class ViewMessages : BasePage
    {
        protected override bool UseSSOMasterPage
        {
            get { return false; }
        }

        protected void Page_Load(object sender, EventArgs e)
        {

            if (!Page.IsPostBack && IsLoggedIn)
            {
                var query = from r in DataContext.SSO_Messages
                            join t in DataContext.SSO_Messages_Targets on r.id equals t.idMessage
                            where ((t.targetType == (int)SSOMessageTarget.User && t.target == SSOHelper.CurrentIdentity.Id)
                                  || (t.targetType == (int)SSOMessageTarget.Role && DataContext.SSO_UserInRole(SSOHelper.CurrentIdentity.Id, t.target) > 0))
                            orderby r.SSO_Messages_States.Count
                            orderby r.date descending
                            select new
                            {
                                r.message,
                                messageType = ((SSOMessageType)r.type),
                                r.date,
                                intranet = r.SSO_Messages_Notifications.Where(i => i.notificationType == (int)SSOMessageNotification.Intranet).Count() > 0,
                                sms = r.SSO_Messages_Notifications.Where(i => i.notificationType == (int)SSOMessageNotification.SMS).Count() > 0,
                                email = r.SSO_Messages_Notifications.Where(i => i.notificationType == (int)SSOMessageNotification.Email).Count() > 0
                                //state = (from s in DataContext.SSO_Messages_States where s.idMessage == r.id && s.idUser == SSOHelper.CurrentIdentity.Id select new { s.SSO_Messages_StateTypes.id, s.SSO_Messages_StateTypes.name, s.updatedOn }).SingleOrDefault()
                                //isNew = r.SSO_Messages_States.Count == 0
                            };
                messages.DataSource = query.Take(50);
                messages.DataBind();
            }
        }

        protected void ImageDataBind(object sender, EventArgs e)
        {
            switch ((SSOMessageType)Eval("messageType"))
            {
                case SSOMessageType.Alarm:
                    (sender as Image).ImageUrl = "/Resources.Net/Iconos/alarma.png";
                    break;
                case SSOMessageType.Message:
                    (sender as Image).ImageUrl = "/Resources.Net/Iconos/informacion.png";
                    break;
                case SSOMessageType.Error:
                    (sender as Image).ImageUrl = "/Resources.Net/Iconos/error.png";
                    break;
                case SSOMessageType.Warning:
                    (sender as Image).ImageUrl = "/Resources.Net/Iconos/advertencia.png";
                    break;
            }
        }
    }
}