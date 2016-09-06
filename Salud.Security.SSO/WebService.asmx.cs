using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Services;
using System.Net;
using System.IO;

namespace Salud.Security.SSO
{
    [System.Web.Script.Services.ScriptService]
    public class WebService : System.Web.Services.WebService
    {
        /// <summary>
        /// Evita que se bloquee una sesión
        /// </summary>
        [WebMethod]
        public void PreventLock()
        {
            try
            {
                if (SSOHelper.CurrentIdentity.State == SSOIdentitySessionState.Ok)
                    SSOHelper.MembershipProvider.UpdateTimeout(SSOHelper.CurrentIdentity);
            }
            catch (Exception)
            {
                // Por si el usuario no está loggeado
            }
        }

        /// <summary>
        /// Devuelve true/false indicando si la sesión está bloqueada o no
        /// </summary>
        /// <returns></returns>
        [WebMethod]
        public string CheckState()
        {
            try
            {
                return ((int)SSOHelper.CurrentIdentity.State).ToString();
            }
            catch (Exception)
            {
                return ((int)SSOIdentitySessionState.Inexistent).ToString();
            }
        }

        /// <summary>
        /// Devuelve los mensajes sin leer que sean para la Intranet
        /// </summary>
        /// <param name="fromIndex">Permite devolver mensajes por rangos. Si es igual a cero devuelve todos</param>
        /// <param name="maxRecords">Cantidad máxima de mensajes a devolver. Si es igual a cero devuelve todos</param>
        /// <returns></returns>
        [WebMethod]
        public List<Classes.SSOMessage> GetMessages(int fromIndex, int maxRecords)
        {
            if (SSOHelper.CurrentIdentity != null)
                using (Data.DataContext dataContext = SSOHelper.GetDataContext())
                {
                    List<Classes.SSOMessage> messages;
                    DateTime? timeStamp = dataContext.hsp_Common_LastUpdated("SSO_Messages_States"); /* Consulta esta tabla porque cambia más frecuentemente que SSO_Messages */
                    if (HttpContext.Current.Cache[String.Format("SSO_Messages_Timestamp_{0}", SSOHelper.CurrentIdentity.Id)] as DateTime? != timeStamp)
                    {
                        messages = (from r in dataContext.SSO_Messages
                                    join t in dataContext.SSO_Messages_Targets on r.id equals t.idMessage
                                    where (!r.expiresOn.HasValue || (r.expiresOn.HasValue && r.expiresOn >= DateTime.Now.Date))
                                       && r.SSO_Messages_Notifications.Count(n => n.notificationType == (int)SSOMessageNotification.Intranet) > 0
                                       && ((t.targetType == (int)SSOMessageTarget.User && t.target == SSOHelper.CurrentIdentity.Id)
                                          || (t.targetType == (int)SSOMessageTarget.Role && dataContext.SSO_UserInRole(SSOHelper.CurrentIdentity.Id, t.target) > 0))
                                       && ((from s in dataContext.SSO_Messages_States where s.idMessage == r.id && s.idUser == SSOHelper.CurrentIdentity.Id && s.idState == (int)SSOMessageState.Sent || (s.idState == (int)SSOMessageState.RememberOn && s.rememberOn <= DateTime.Now) select s).Count() > 0)
                                    orderby r.date descending
                                    select new Classes.SSOMessage { id = r.id, message = r.message, type = r.type, date = r.date }).Distinct().Take(2).ToList(); /* Guarda en caché sólo 10. Este número debe ser siempre >= al número esperado de mensajes 'maxRecords'. */
                        HttpContext.Current.Cache[String.Format("SSO_Messages_List_{0}", SSOHelper.CurrentIdentity.Id)] = messages;
                        HttpContext.Current.Cache[String.Format("SSO_Messages_Timestamp_{0}", SSOHelper.CurrentIdentity.Id)] = timeStamp;
                    }
                    else
                        messages = HttpContext.Current.Cache[String.Format("SSO_Messages_List_{0}", SSOHelper.CurrentIdentity.Id)] as List<Classes.SSOMessage>;

                    if (maxRecords == 0)
                        return messages.Where(m => m.id > fromIndex).ToList();
                    else
                        return messages.Where(m => m.id > fromIndex).Take(maxRecords).ToList();
                    //return (from m in messages where m.id > fromIndex select m).Take(maxRecords);
                }
            else
                return null;
        }

        /// <summary>
        /// Devuelve la cantidad de mensajes sin leer para el usuario actual
        /// </summary>
        /// <returns></returns>
        [WebMethod]
        public int GetMessageCount()
        {
            if (SSOHelper.CurrentIdentity != null)
                return GetMessages(0, 0).Count;
            else
                return 0;
        }

        [WebMethod]
        public void ChangeMessageState(int idMessage, int idState, string rememberOn)
        {
            using (Data.DataContext dataContext = SSOHelper.GetDataContext())
            {
                // Controla permisos
                var query = from r in dataContext.SSO_Messages
                            join t in dataContext.SSO_Messages_Targets on r.id equals t.idMessage
                            where r.id == idMessage
                                && ((t.targetType == (int)SSOMessageTarget.User && t.target == SSOHelper.CurrentIdentity.Id)
                                || (t.targetType == (int)SSOMessageTarget.Role && dataContext.SSO_UserInRole(SSOHelper.CurrentIdentity.Id, t.target) > 0))
                            select r;
                if (query.Count() > 0)
                {
                    Data.SSO_Messages_States state = (from r in dataContext.SSO_Messages_States where r.idMessage == idMessage && r.idUser == SSOHelper.CurrentIdentity.Id select r).SingleOrDefault();
                    if (state == null)
                    {
                        state = new Data.SSO_Messages_States();
                        state.idMessage = idMessage;
                        state.idUser = SSOHelper.CurrentIdentity.Id;
                        dataContext.SSO_Messages_States.InsertOnSubmit(state);
                    }
                    state.idState = idState;
                    state.updatedOn = DateTime.Now;
                    state.rememberOn = String.IsNullOrEmpty(rememberOn) ? null : (DateTime?)DateTime.ParseExact(rememberOn, "yyyy/MM/dd", null).Date;
                    dataContext.SubmitChanges();
                }
            }
        }

        [WebMethod]
        public void SendSMS(int recipientType, string recipient, int? carrier, string text)
        {
            text = text.Trim();
            if (text.Length < 2)
                throw new Exception("Mensaje muy corto");
            else
            {
                using (Data.DataContext DataContext = new Data.DataContext(System.Configuration.ConfigurationManager.ConnectionStrings["SMSServiceConnectionString"].ConnectionString))
                {
                    switch (recipientType)
                    {
                        case 2: /* Usuario */
                            var user = (from r in DataContext.SSO_Users where r.id == int.Parse(recipient) && r.mobile != null && r.idCarrier != null select new { r.name, r.surname, r.mobile, r.idCarrier }).SingleOrDefault();
                            if (user != null)
                            {
                                recipient = user.mobile;
                                carrier = user.idCarrier;
                            }
                            else
                                recipient = null;
                            break;
                        case 3: /* Paciente */
                            throw new NotImplementedException();
                    }

                    if (!String.IsNullOrEmpty(recipient) && carrier.HasValue)
                        SSOHelper.SendSMS(recipient, carrier.Value, text);
                }
            }
        }

        /// <summary>
        /// Método privado que utiliza GetApplicationMenu()
        /// </summary>
        //private static List<_SSOMenuItem> GetApplicationMenu_GetItems(SSOBaseClass parent)
        //{
        //    List<_SSOMenuItem> result = new List<_SSOMenuItem>();
        //    var items = (parent is SSOApplicationGroup) ? ((SSOApplicationGroup)parent).MenuItems : ((SSOApplication)parent).MenuItems;
        //    foreach (SSOBaseClass item in items)
        //    {
        //        if (item.IsVisible)
        //        {
        //            if (item is SSOApplicationGroup)
        //            {
        //                _SSOMenuItem group = new _SSOMenuItem()
        //                            {
        //                                id = item.Id,
        //                                text = item.Description,
        //                                imageURL = item.Image,
        //                                isSeparator = item.Description == "-",
        //                                url = item.URL,
        //                                priority = item.Priority,
        //                                items = GetApplicationMenu_GetItems(item)
        //                            };
        //                if (group.items.Count > 0)
        //                    result.Add(group);
        //            }
        //            else
        //                result.Add(new _SSOMenuItem() { id = item.Id, text = item.Description ?? item.Name, imageURL = item.Image, isSeparator = item.Description == "-", url = item.URL, priority = item.Priority });
        //        }
        //    }

        //    // Ordena por prioridad
        //    result.Sort(delegate(_SSOMenuItem t1, _SSOMenuItem t2)
        //    {
        //        return (t1.priority.CompareTo(t2.priority));
        //    });

        //    // Controla separadores
        //    if (result.Count > 0 && result[0].isSeparator)
        //        result.RemoveAt(0);

        //    return result;
        //}

        /// <summary>
        /// Devuelve el menú de aplicaciones y módulos permitidos
        /// </summary>
        //[WebMethod]
        //public object GetApplicationMenu()
        //{
        //    string scriptsURL = (string)SSOHelper.Configuration["Scripts_BaseURL"];
        //    List<_SSOMenuItem> items = (from r in SSOHelper.MembershipProvider.GetAllowedApplications(SSOHelper.CurrentIdentity)
        //                                where r.IsVisible && r.URL != null
        //                                orderby r.Description
        //                                select new _SSOMenuItem()
        //                                {
        //                                    id = r.Id,
        //                                    priority = r.Priority,
        //                                    text = r.Description,
        //                                    url = r.URL.StartsWith("/") ? r.URL : scriptsURL + r.URL,
        //                                    items = GetApplicationMenu_GetItems(r)
        //                                }
        //                        ).ToList();
        //    return items.Where(i => i.items.Count > 0);
        //}

        [WebMethod]
        public object GetApplicationMenu()
        {
            return SSOHelper.GetApplicationMenuByEfector();  //se puso el ByEfector
        }
    }
}
