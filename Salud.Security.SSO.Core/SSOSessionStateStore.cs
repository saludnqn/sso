using System;
using System.Collections.Specialized;
using System.Web;
using System.Web.SessionState;

namespace Salud.Security.SSO
{
    public sealed class SSOSessionStateStore : SessionStateStoreProviderBase
    {
        private SessionStateStoreData InnerGetItem(bool lockRecord,
          HttpContext context,
          string id,
          out bool locked,
          out TimeSpan lockAge,
          out object lockId,
          out SessionStateActions actionFlags)
        {
            lockAge = TimeSpan.MaxValue;
            lockId = 1;
            locked = true;
            actionFlags = SessionStateActions.None;
            return CreateNewStoreData(context, 0);
        }
     
        public override void Initialize(string name, NameValueCollection config)
        {
        }

        public override bool SetItemExpireCallback(SessionStateItemExpireCallback expireCallback)
        {
            return false;
        }

        public override void SetAndReleaseItemExclusive(HttpContext context,
          string id,
          SessionStateStoreData item,
          object lockId,
          bool newItem)
        {
            SSOHelper.MembershipProvider.SetVariables(SSOHelper.CurrentIdentity, (SSOSessionStateStoreItems)item.Items);
        }

        public override SessionStateStoreData GetItem(HttpContext context,
          string id,
          out bool locked,
          out TimeSpan lockAge,
          out object lockId,
          out SessionStateActions actionFlags)
        {
            return InnerGetItem(false, context, id, out locked,
              out lockAge, out lockId, out actionFlags);
        }


        public override SessionStateStoreData GetItemExclusive(HttpContext context,
          string id,
          out bool locked,
          out TimeSpan lockAge,
          out object lockId,
          out SessionStateActions actionFlags)
        {
            return InnerGetItem(true, context, id, out locked,
              out lockAge, out lockId, out actionFlags);
        }

        public override void ReleaseItemExclusive(HttpContext context,
          string id,
          object lockId)
        {
        }

        public override void RemoveItem(HttpContext context,
          string id,
          object lockId,
          SessionStateStoreData item)
        {
            throw new NotImplementedException("Este método no debe utilizarse. Hace el logout vía SSO");
        }

        public override void CreateUninitializedItem(HttpContext context,
          string id,
          int timeout)
        {
            throw new NotImplementedException("El usuario no tiene una sesión SSO inicializada");
        }

        public override SessionStateStoreData CreateNewStoreData(
          HttpContext context,
          int timeout)
        {
            return new SessionStateStoreData(new SSOSessionStateStoreItems(),
              SessionStateUtility.GetSessionStaticObjects(context),
              timeout);
        }


        public override void ResetItemTimeout(HttpContext context,string id)
        {           
        }

        public override void Dispose()
        {            
        }

        public override void InitializeRequest(System.Web.HttpContext context)
        {
            SSOHelper.Authenticate();
        }

        public override void EndRequest(HttpContext context)
        {
        }
    }
}