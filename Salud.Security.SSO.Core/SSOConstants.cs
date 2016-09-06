
namespace Salud.Security.SSO
{
    public enum SSOIdentitySessionState
    {
        Ok = 0,
        Inexistent = 7003,
        Locked = 7004,
        SecurityError = 7010
    }

    public enum SSOModuleAccess
    {
        Denied = 5000,
        Allowed = 5001
        //,Readonly = 5002
    }

    public enum SSOPermissionTarget
    {
        View = 1,
        Module = 2,
        Alarm = 3
    }

    public enum SSOPermissionSource
    {
        Role = 1,
        User = 2,
        RoleGroup = 3
    }

    public enum SSOMessageType
    {
        Message = 1,
        Warning = 2,
        Error = 3,
        Alarm = 4
    }

    public enum SSOMessageTarget
    {
        Role = 1,
        User = 2
    }

    public enum SSOMessageState
    {
        Read = 1,
        RememberOn = 2,
        Sent = 3
    }

    public enum SSOMessageNotification
    {
        Intranet = 1,
        Email = 2,
        SMS = 3
    }

    public class SSOConstants
    {
        public static int Ok = 0;
        public static int Error = -1;
    }

    public enum WellKnownRoles    
    {
        GlobalAdministrator = 6,
        ApplicationAdministrator = 7,
        PermissionAdministrator = 8,
        UserAdministrator = 9,
        RoleAdministrator = 10,
        ContentAdministrator = 11        
    }
}