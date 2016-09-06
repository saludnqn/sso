using System;

namespace Salud.Security.SSO
{
    public class SSOException : ApplicationException
    {
        public enum ExceptionCodeType
        {
            MembershipProviderIncorrect,
            //MembershipUserInexistent,
            DatabaseConnectionError,
            DatabaseReadError,
            DatabaseWriteError,
            AccessDenied_Application,
            AccessDenied_Module,
            AccessDenied_Record
        }

        private ExceptionCodeType exceptionCode;
        private string message;

        public ExceptionCodeType ExceptionCode
        {
            get
            {
                return exceptionCode;
            }
        }

        public SSOException(ExceptionCodeType exceptionCode)
        {
            this.exceptionCode = exceptionCode;
            this.message = exceptionCode.ToString();
        }

        public SSOException(ExceptionCodeType exceptionCode, string detail)
        {
            this.exceptionCode = exceptionCode;
            this.message = exceptionCode.ToString();
            if (!String.IsNullOrEmpty(detail))
            {
                this.message += " | " + detail;
            }
        }

        public override string Message
        {
            get
            {
                return this.message;
            }
        }
    }

}
