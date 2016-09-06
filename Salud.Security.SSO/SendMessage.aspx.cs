using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using Telerik.Web.UI;
using Salud.Applications.Shared.UI;
using Salud.Applications.Shared;
using Salud.Security.SSO.Pages;

namespace Salud.Security.SSO
{
    public partial class SendMessage : BasePage
    {
        [Serializable]
        private class ListItem
        {
            public SSOMessageTarget TargetType
            {
                get;
                set;
            }
            public int Target
            {
                get;
                set;
            }
            public string Name
            {
                get;
                set;
            }

            public override string ToString()
            {
                return String.Format("[{0},{1}]", (int)TargetType, Target);
            }
        }

        private List<ListItem> Targets
        {
            get
            {
                var o = ViewState["TargetsItems"];
                if (o == null)
                    return new List<ListItem>();
                else
                    return o as List<ListItem>;
            }
        }

        protected override bool UseSSOMasterPage
        {
            get { return false; }
        }

        protected override object SaveViewState()
        {
            ViewState["TargetsItems"] = Targets;
            return base.SaveViewState();
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            if (IsLoggedIn && (SSOHelper.CurrentIdentity.IsGlobalAdministrator || SSOHelper.CurrentIdentity.IsContentAdministrator))
            {
                RefreshUI();
                Ajaxify(cbUser, cbUser);
                Ajaxify(cbUser, lvTargets);
                Ajaxify(cbRole, cbRole);
                Ajaxify(cbRole, lvTargets);
                Ajaxify(lvTargets, lvTargets);
                Ajaxify(cbSMS, pnlSMS);
            }
            else
                SendAccessDenied();
        }

        protected void RefreshUI()
        {
            lvTargets.DataSource = Targets;
            lvTargets.DataBind();
        }

        protected void editor_Load(object sender, EventArgs e)
        {
            editor.CssFiles.Add(String.Format("{0}/Editor.css", ResourcesURL));
        }

        protected void cbUser_ItemsRequested(object sender, Telerik.Web.UI.RadComboBoxItemsRequestedEventArgs e)
        {
            string text = e.Text.Trim();
            if (!String.IsNullOrEmpty(text))
            {
                RadComboBox combo = (sender as RadComboBox);
                combo.Items.Clear();
                var query = from r in DataContext.SSO_Users
                            where r.username.StartsWith(text)
                                || r.name.StartsWith(text) || r.surname.StartsWith(text)
                            orderby r.surname
                            orderby r.name
                            select new { value = r.id.ToString(), name = r.surname + ", " + r.name + " (" + r.username + ")" };
                foreach (var r in query)
                    combo.Items.Add(new RadComboBoxItem(r.name, r.value));
            }
        }

        protected void cbRole_ItemsRequested(object sender, Telerik.Web.UI.RadComboBoxItemsRequestedEventArgs e)
        {
            string text = e.Text.Trim();
            if (!String.IsNullOrEmpty(text))
            {
                RadComboBox combo = (sender as RadComboBox);
                combo.Items.Clear();
                var query = from r in DataContext.SSO_Roles
                            where r.name.Contains(text)
                            orderby r.name
                            select new { value = r.id.ToString(), r.name };
                foreach (var r in query)
                    combo.Items.Add(new RadComboBoxItem(r.name, r.value));
            }
        }

        protected void cbUser_SelectedIndexChanged(object sender, RadComboBoxSelectedIndexChangedEventArgs e)
        {
            int id;
            if (int.TryParse(e.Value, out id))
            {
                RadComboBox combo = (sender as RadComboBox);
                if ((from r in Targets where r.TargetType == SSOMessageTarget.User && r.Target == id select r).Count() == 0)
                    Targets.Add(new ListItem() { Name = e.Text, TargetType = SSOMessageTarget.User, Target = id });

                combo.Text = "";
                combo.ClearSelection();
                combo.Focus();
                RefreshUI();
            }
        }

        protected void cbRole_SelectedIndexChanged(object sender, RadComboBoxSelectedIndexChangedEventArgs e)
        {
            int id;
            if (int.TryParse(e.Value, out id))
            {
                RadComboBox combo = (sender as RadComboBox);
                if ((from r in Targets where r.TargetType == SSOMessageTarget.Role && r.Target == id select r).Count() == 0)
                    Targets.Add(new ListItem() { Name = e.Text, TargetType = SSOMessageTarget.Role, Target = id });

                combo.Text = "";
                combo.ClearSelection();
                combo.Focus();
                RefreshUI();
            }
        }

        protected void btnDelete_Click(object sender, EventArgs e)
        {
            string commandArgument = (sender as ButtonEx).CommandArgument;
            int targetType = int.Parse(commandArgument.Substring(0, commandArgument.IndexOf('_')));
            int target = int.Parse(commandArgument.Substring(commandArgument.IndexOf('_') + 1));
            Targets.Remove((from r in Targets where (int)r.TargetType == targetType && r.Target == target select r).Single());

            RefreshUI();
        }

        protected void btnSendMessage_Click(object sender, EventArgs e)
        {
            editor.FixContent(RadEditorExtensions.Options.All);
            if (String.IsNullOrEmpty(editor.Text) || (Targets.Count == 0))
                ShowPopup("Debe seleccionar uno o más destinatarios y escribir un mensaje");
            else if (!cbIntranet.Checked && !cbEmail.Checked && !cbSMS.Checked)
                ShowPopup("Debe seleccionar uno o más medios de notificación");
            else
            {
                List<int> notifications = new List<int>();
                if (cbIntranet.Checked)
                    notifications.Add((int)SSOMessageNotification.Intranet);
                if (cbEmail.Checked)
                    notifications.Add((int)SSOMessageNotification.Email);
                if (cbSMS.Checked)
                    notifications.Add((int)SSOMessageNotification.SMS);

                var result = DataContext.sp_SSO_QueueMessage(true, tbSubject.Text, editor.Content, dpExpiresOn.SelectedDate, int.Parse(cbType.SelectedValue), String.Join(",", notifications), String.Join(",", Targets), true, true);
                var errors = from r in result.ToList() where r.errorType != 0 select r;
                if (errors.Count() == 0)
                    ClosePopup(true);
                else
                {
                    grid.DataSource = errors;
                    grid.DataBind();
                    
                    // Prepara la UI para reintentar el envío
                    pnlResultados.Visible = true;
                    pnlEnviar.Visible = false;

                    var users = (from r in errors select new { r.userId, r.userFullname }).Distinct();
                    Targets.Clear();
                    foreach (var user in users)
                        Targets.Add(new ListItem() { Name = user.userFullname, TargetType = SSOMessageTarget.User, Target = user.userId.Value });
                }
            }
        }

        protected void cbSMS_CheckedChanged(object sender, EventArgs e)
        {
            pnlSMS.Visible = cbSMS.Checked;
        }

        protected void btnReintentar_Click(object sender, EventArgs e)
        {
            pnlResultados.Visible = false;
            pnlEnviar.Visible = true;
            cbIntranet.Checked = false;
            cbIntranet.Visible = false;
        }
    }
}