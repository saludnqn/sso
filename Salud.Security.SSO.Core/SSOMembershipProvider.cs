using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Configuration;
using System.Configuration.Provider;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Web.Security;

namespace Salud.Security.SSO
{

    internal sealed class SSOMembershipProvider : MembershipProvider
    {
        #region Constants

        /// <summary>
        /// Restricción impuesta por el campo "variable" en la tabla "SSO_Variables"
        /// </summary>
        private static int MaxVariableName = 350;
        public static string SSOMembershipProviderName = "SSOMembershipProvider";

        #endregion

        #region MembershipProvider Properties and Getters

        public override string ApplicationName
        {
            get { return null; }
            set { /* No application name is used */ }
        }

        public override bool EnablePasswordReset
        {
            get { return false; }
        }


        public override bool EnablePasswordRetrieval
        {
            get { return false; }
        }


        public override bool RequiresQuestionAndAnswer
        {
            get { return false; }
        }


        public override bool RequiresUniqueEmail
        {
            get { return true; }
        }


        public override int MaxInvalidPasswordAttempts
        {
            get { return 0; }
        }


        public override int PasswordAttemptWindow
        {
            get { return 0; }
        }


        public override MembershipPasswordFormat PasswordFormat
        {
            get { return MembershipPasswordFormat.Hashed; }
        }

        public override int MinRequiredNonAlphanumericCharacters
        {
            get { return 0; }
        }

        public override int MinRequiredPasswordLength
        {
            get { return 0; }
        }

        public override string PasswordStrengthRegularExpression
        {
            get { return null; }
        }

        #endregion

        #region Properties and Getters

        private string connectionString;
        private bool alwaysAuthenticate;
        private bool allowAnonymousAccessToImages;
        private bool allowAnonymousAccessToScripts;
        private bool updateTimeoutOnWebMethod;
        private bool useCache;
        private string cookieName;

        /// <summary>
        /// Indica si el <see cref="SSOHttpModule"/> debe autenticar siempre cada request, o se permitirá el acceso anónimo a algunos requests
        /// Valor por defecto: true
        /// </summary>
        public bool AlwaysAuthenticate
        {
            get
            {
                return alwaysAuthenticate;
            }
        }

        /// <summary>
        /// Indica si el <see cref="SSOHttpModule"/> permite un el aceso anónimo a imágenes
        /// Valor por defecto: false
        /// </summary>
        public bool AllowAnonymousAccessToImages
        {
            get
            {
                return allowAnonymousAccessToImages;
            }
        }

        /// <summary>
        /// Indica si el <see cref="SSOHttpModule"/> permite un el aceso anónimo a scripts (.js) y webresources (.axd)
        /// Valor por defecto: false
        /// </summary>
        public bool AllowAnonymousAccessToScripts
        {
            get
            {
                return allowAnonymousAccessToScripts;
            }
        }

        /// <summary>
        /// Indica si el <see cref="SSOHttpModule"/> actualiza cuando se ejecuta un web method
        /// Valor por defecto: true
        /// </summary>
        public bool UpdateTimeoutOnWebMethod
        {
            get
            {
                return updateTimeoutOnWebMethod;
            }
        }

        /// <summary>
        /// Indica si se cachean los permisos de usuarios
        /// Valor por defecto: true
        /// </summary>
        public bool UseCache
        {
            get
            {
                return useCache;
            }
        }

        /// <summary>
        /// Indica el nombre de la cookie de autenticación guardada en el browser del cliente
        /// Valor por defecto: SSO_AUTH_COOKIE_SSS
        /// </summary>
        public string CookieName
        {
            get
            {
                return cookieName;
            }
        }

        public SSOConfiguration Configuration = new SSOConfiguration();

        #endregion

        #region Initialization

        public override void Initialize(string name, NameValueCollection config)
        {
            name = SSOMembershipProvider.SSOMembershipProviderName;
            base.Initialize(name, config);

            // Initialize connection string
            ConnectionStringSettings ConnectionStringSettings = ConfigurationManager.ConnectionStrings[config["connectionStringName"]];
            if (ConnectionStringSettings == null || ConnectionStringSettings.ConnectionString.Trim() == "")
            {
                throw new ProviderException("Connection string cannot be blank.");
            }
            connectionString = ConnectionStringSettings.ConnectionString;

            // Initialize SSO options
            if (!Boolean.TryParse(config["alwaysAuthenticate"], out alwaysAuthenticate))
                alwaysAuthenticate = true;
            if (!Boolean.TryParse(config["allowAnonymousAccessToImages"], out allowAnonymousAccessToImages))
                allowAnonymousAccessToImages = false;
            if (!Boolean.TryParse(config["allowAnonymousAccessToScripts"], out allowAnonymousAccessToScripts))
                allowAnonymousAccessToScripts = false;
            if (!Boolean.TryParse(config["updateTimeoutOnWebMethod"], out updateTimeoutOnWebMethod))
                updateTimeoutOnWebMethod = true;
            if (!Boolean.TryParse(config["useCache"], out useCache))
                useCache = true;
            cookieName = config["cookieName"];
            if (String.IsNullOrEmpty(cookieName))
                cookieName = "SSO_AUTH_COOKIE_SSS";
        }


        //
        // A helper function to retrieve config values from the configuration file.
        //

        private string GetConfigValue(string configValue, string defaultValue)
        {
            if (String.IsNullOrEmpty(configValue))
                return defaultValue;

            return configValue;
        }
        #endregion

        #region Private methods and helpers

        private string HashPassword(string password)
        {
            // Calcula el hash
            System.Security.Cryptography.MD5CryptoServiceProvider md5 = new System.Security.Cryptography.MD5CryptoServiceProvider();
            byte[] hash = md5.ComputeHash(System.Text.Encoding.UTF8.GetBytes(password));

            StringBuilder sBuilder = new StringBuilder();
            for (int i = 0; i < hash.Length; i++)
                sBuilder.Append(hash[i].ToString("x2"));
            return sBuilder.ToString();
        }

        #endregion

        #region Session Methods

        internal void UpdateTimeout(SSOIdentity identity)
        {
            SqlConnection conn = null;
            try
            {
                conn = new SqlConnection(this.connectionString);
                conn.Open();
                SqlCommand cmd = null;
                try
                {
                    cmd = new SqlCommand("UPDATE SSO_Sessions SET lasttime = GETDATE() WHERE ID = @sessionId", conn);
                    cmd.CommandType = System.Data.CommandType.Text;
                    cmd.Parameters.Add(new global::System.Data.SqlClient.SqlParameter("@sessionId", global::System.Data.SqlDbType.VarChar, 512, global::System.Data.ParameterDirection.Input, 10, 0, null, global::System.Data.DataRowVersion.Current, false, identity.SessionId, "", "", ""));
                    cmd.ExecuteNonQuery();
                }
                catch (Exception e)
                {
                    throw new SSOException(SSOException.ExceptionCodeType.DatabaseWriteError, e.ToString());
                }
                finally
                {
                    cmd.Dispose();
                }
            }
            catch (Exception e)
            {
                throw new SSOException(SSOException.ExceptionCodeType.DatabaseConnectionError, e.ToString());
            }
            finally
            {
                conn.Dispose();
            }
        }

        internal void ChangeLockStatus(SSOIdentity identity, bool isLocked)
        {
            SqlConnection conn = null;
            try
            {
                conn = new SqlConnection(this.connectionString);
                conn.Open();
                SqlCommand cmd = null;
                try
                {
                    cmd = new SqlCommand("UPDATE SSO_Sessions SET locked = @isLocked, lastTime = GETDATE() WHERE ID = @sessionId", conn);
                    cmd.CommandType = System.Data.CommandType.Text;
                    cmd.Parameters.Add(new global::System.Data.SqlClient.SqlParameter("@sessionId", global::System.Data.SqlDbType.VarChar, 512, global::System.Data.ParameterDirection.Input, 10, 0, null, global::System.Data.DataRowVersion.Current, false, identity.SessionId, "", "", ""));
                    cmd.Parameters.Add(new global::System.Data.SqlClient.SqlParameter("@isLocked", global::System.Data.SqlDbType.Bit, 1, global::System.Data.ParameterDirection.Input, 10, 0, null, global::System.Data.DataRowVersion.Current, false, isLocked, "", "", ""));
                    cmd.ExecuteNonQuery();
                }
                catch (Exception e)
                {
                    throw new SSOException(SSOException.ExceptionCodeType.DatabaseWriteError, e.ToString());
                }
                finally
                {
                    cmd.Dispose();
                }
            }
            catch (Exception e)
            {
                throw new SSOException(SSOException.ExceptionCodeType.DatabaseConnectionError, e.ToString());
            }
            finally
            {
                conn.Dispose();
            }
        }

        /// <summary>
        /// Devuelve una variable de sesión. Este método es llamado por <see cref="SSOSessionStateStoreItems"/>
        /// </summary>
        /// <param name="identity">The identity.</param>
        /// <param name="variable">The variable.</param>
        /// <returns></returns>

        internal int GetEfector(SSOIdentity identity)
        {
            int result = 0;
            SqlConnection conn = null;
            try
            {
                conn = new SqlConnection(this.connectionString);
                conn.Open();
                SqlCommand cmd = null;
                try
                {
                    cmd = new SqlCommand("SELECT idEfector FROM SSO_Sessions WHERE id = @sessionId", conn);
                    cmd.CommandType = System.Data.CommandType.Text;
                    cmd.Parameters.Add(new global::System.Data.SqlClient.SqlParameter("@sessionId", global::System.Data.SqlDbType.VarChar, 512, global::System.Data.ParameterDirection.Input, 10, 0, null, global::System.Data.DataRowVersion.Current, false, identity.SessionId, "", "", ""));                    
                    try
                    {

                        result = (int)cmd.ExecuteScalar();   
                        
                    }
                    catch (Exception)
                    {
                        result = 0;
                    }
                }
                catch (Exception e)
                {
                    throw new SSOException(SSOException.ExceptionCodeType.DatabaseReadError, "Efector: " + " " + e.ToString());
                }
                finally
                {
                    cmd.Dispose();
                }
            }
            catch (Exception e)
            {
                throw new SSOException(SSOException.ExceptionCodeType.DatabaseConnectionError, e.ToString());
            }
            finally
            {
                conn.Dispose();
            }
            return result;
        }

        internal int GetEfectorRol(SSOIdentity identity)
        {
            int result = 0;
            SqlConnection conn = null;
            try
            {
                conn = new SqlConnection(this.connectionString);
                conn.Open();
                SqlCommand cmd = null;
                try
                {
                    cmd = new SqlCommand("SELECT idEfectorRol FROM SSO_Sessions WHERE id = @sessionId", conn);
                    cmd.CommandType = System.Data.CommandType.Text;
                    cmd.Parameters.Add(new global::System.Data.SqlClient.SqlParameter("@sessionId", global::System.Data.SqlDbType.VarChar, 512, global::System.Data.ParameterDirection.Input, 10, 0, null, global::System.Data.DataRowVersion.Current, false, identity.SessionId, "", "", ""));
                    try
                    {

                        result = (int)cmd.ExecuteScalar();

                    }
                    catch (Exception)
                    {
                        result = 0;
                    }
                }
                catch (Exception e)
                {
                    throw new SSOException(SSOException.ExceptionCodeType.DatabaseReadError, "Efector: " + " " + e.ToString());
                }
                finally
                {
                    cmd.Dispose();
                }
            }
            catch (Exception e)
            {
                throw new SSOException(SSOException.ExceptionCodeType.DatabaseConnectionError, e.ToString());
            }
            finally
            {
                conn.Dispose();
            }
            return result;
        }
                
        internal object GetVariable(SSOIdentity identity, string variable)
        {
            object result;
            variable = variable.Substring(0, Math.Min(MaxVariableName, variable.Length));

            SqlConnection conn = null;
            try
            {
                conn = new SqlConnection(this.connectionString);
                conn.Open();
                SqlCommand cmd = null;
                try
                {
                    cmd = new SqlCommand("SELECT currentValue FROM SSO_Variables WHERE sessionId = @sessionId AND variable = @variable", conn);
                    cmd.CommandType = System.Data.CommandType.Text;
                    cmd.Parameters.Add(new global::System.Data.SqlClient.SqlParameter("@sessionId", global::System.Data.SqlDbType.VarChar, 512, global::System.Data.ParameterDirection.Input, 10, 0, null, global::System.Data.DataRowVersion.Current, false, identity.SessionId, "", "", ""));
                    cmd.Parameters.Add(new global::System.Data.SqlClient.SqlParameter("@variable", global::System.Data.SqlDbType.VarChar, MaxVariableName, global::System.Data.ParameterDirection.Input, 10, 0, null, global::System.Data.DataRowVersion.Current, false, variable.Trim(), "", "", ""));
                    try
                    {
                        byte[] bytes = (byte[])cmd.ExecuteScalar();
                        BinaryFormatter bf = new BinaryFormatter();
                        using (MemoryStream ms = new MemoryStream(bytes))
                        {
                            result = bf.Deserialize(ms);
                        }
                    }
                    catch (Exception)
                    {
                        result = null;
                    }
                }
                catch (Exception e)
                {
                    throw new SSOException(SSOException.ExceptionCodeType.DatabaseReadError, "Variable: " + variable + " " + e.ToString());
                }
                finally
                {
                    cmd.Dispose();
                }
            }
            catch (Exception e)
            {
                throw new SSOException(SSOException.ExceptionCodeType.DatabaseConnectionError, e.ToString());
            }
            finally
            {
                conn.Dispose();
            }
            return result;
        }

        /// <summary>
        /// Guarda las variables modificadas de una sesión. Este método es llamado por <see cref="SSOSessionStateStore.SetAndReleaseItemExclusive"/>
        /// </summary>
        
        internal void SetVariables(SSOIdentity identity, SSOSessionStateStoreItems items)
        {
            SqlConnection conn = null;
            try
            {
                conn = new SqlConnection(this.connectionString);
                conn.Open();
                SqlCommand cmd = new SqlCommand("sp_SSO_SetSessionVariable", conn);
                cmd.CommandType = System.Data.CommandType.StoredProcedure;
                cmd.Parameters.Add(new System.Data.SqlClient.SqlParameter("@sessionId", global::System.Data.SqlDbType.VarChar, 512, global::System.Data.ParameterDirection.Input, 0, 0, null, global::System.Data.DataRowVersion.Current, false, identity.SessionId, "", "", ""));
                cmd.Parameters.Add(new global::System.Data.SqlClient.SqlParameter("@variable", global::System.Data.SqlDbType.VarChar, MaxVariableName, global::System.Data.ParameterDirection.Input, 10, 0, null, global::System.Data.DataRowVersion.Current, false, null, "", "", ""));
                cmd.Parameters.Add(new global::System.Data.SqlClient.SqlParameter("@value", global::System.Data.SqlDbType.Image, 0, global::System.Data.ParameterDirection.Input, 10, 0, null, global::System.Data.DataRowVersion.Current, false, null, "", "", ""));
                string debugString = "";
                try
                {
                    BinaryFormatter bf = new BinaryFormatter();
                    foreach (string key in items.DirtyKeys)
                    {
                        string keyTrimmed = key.Trim().Substring(0, Math.Min(MaxVariableName, key.Length));
                        debugString = keyTrimmed;
                        using (MemoryStream ms = new MemoryStream())
                        {
                            cmd.Parameters["@variable"].Value = keyTrimmed;
                            object value = items[key];
                            if (value != null)
                            {
                                bf.Serialize(ms, items[key]);
                                cmd.Parameters["@value"].Value = ms.ToArray();
                            }
                            else
                                cmd.Parameters["@value"].Value = DBNull.Value;
                            cmd.ExecuteNonQuery();
                        }
                    }
                }
                catch (Exception e)
                {
                    // Si es un error 547 (Constraint conflict) no mostrar un error, ya que es porque el usuario hizo un logout y la sesión expiró
                    if ((!(e is System.Data.SqlClient.SqlException)) || (e is System.Data.SqlClient.SqlException && ((System.Data.SqlClient.SqlException)e).Number != 547))
                        throw new SSOException(SSOException.ExceptionCodeType.DatabaseWriteError, String.Format("{0} | SessionId: {1} | Variable: {2}", e.ToString(), identity.SessionId, debugString));
                }
                finally
                {
                    cmd.Dispose();
                }
            }
            catch (Exception e)
            {
                throw new SSOException(SSOException.ExceptionCodeType.DatabaseConnectionError, e.ToString());
            }
            finally
            {
                conn.Dispose();
            }
        }

        #endregion

        #region MembershipProvider Methods

        internal SSOUser GetUser(int? userId, string username, object externalId)
        {
            using (Data.DataContext DataContext = SSOHelper.GetDataContext())
                return (from r in DataContext.SSO_Users
                        where (userId.HasValue && r.id == userId) || (externalId != null && r.external_id == externalId) || (r.username == username)
                        select new SSOUser(r.id, r.username, r.name, r.surname, r.email, r.mobile, r.idCarrier, r.external_id == null ? null : r.external_id.ToString(), 
                            r.documento.GetValueOrDefault(0), r.legajo, r.observacion,  //Gustavo Saraceni: agregados el 12/11/2013
                            r.description, r.enabled, r.locked, DataContext.SSO_UserInRole(r.id, (int)WellKnownRoles.GlobalAdministrator) > 0, DataContext.SSO_UserInRole(r.id, (int)WellKnownRoles.ApplicationAdministrator) > 0, DataContext.SSO_UserInRole(r.id, (int)WellKnownRoles.ContentAdministrator) > 0)
                        ).FirstOrDefault();
        }

        public override MembershipUser GetUser(object providerUserKey, bool userIsOnline)
        {
            return GetUser((int)providerUserKey, null, null);
        }

        public override MembershipUser GetUser(string username, bool userIsOnline)
        {
            if (String.IsNullOrEmpty(username))
                throw new NullReferenceException("username");

            return GetUser(null, username, null);
        }

        /*  Autor:Gustavo  
            Fecha: 29/07/2013
            Comentario: agrego este método para traer el nombre del efectorRol */
        public string GetNombreEfectorRol(int idEfectorRol)
        {
            string result = "";
            SqlConnection conn = null;
            try
            {
                conn = new SqlConnection(this.connectionString);
                conn.Open();
                SqlCommand cmd = null;
                try
                {
                    cmd = new SqlCommand("SELECT name FROM SSO_Roles WHERE id = @idEfectorRol", conn);
                    cmd.CommandType = System.Data.CommandType.Text;
                    cmd.Parameters.Add(new global::System.Data.SqlClient.SqlParameter("@idEfectorRol", global::System.Data.SqlDbType.VarChar, 512, global::System.Data.ParameterDirection.Input, 10, 0, null, global::System.Data.DataRowVersion.Current, false, idEfectorRol, "", "", ""));
                    try
                    {

                        result = (string)cmd.ExecuteScalar();

                    }
                    catch (Exception)
                    {
                        result = "";
                    }
                }
                catch (Exception e)
                {
                    throw new SSOException(SSOException.ExceptionCodeType.DatabaseReadError, "Efector: " + " " + e.ToString());
                }
                finally
                {
                    cmd.Dispose();
                }
            }
            catch (Exception e)
            {
                throw new SSOException(SSOException.ExceptionCodeType.DatabaseConnectionError, e.ToString());
            }
            finally
            {
                conn.Dispose();
            }
            return result;
        }

        public bool TestPassword(string username, string password, out int userId)
        {
            // Inicializa
            SqlConnection conn = null;
            bool result = false;
            userId = 0;

            try
            {
                conn = new SqlConnection(this.connectionString);
                conn.Open();
                SqlCommand cmd = null;
                try
                {
                    cmd = new SqlCommand("sp_SSO_Login", conn);
                    cmd.CommandType = System.Data.CommandType.StoredProcedure;
                    cmd.Parameters.Add(new System.Data.SqlClient.SqlParameter("@RETURN_VALUE", global::System.Data.SqlDbType.Int, 4, global::System.Data.ParameterDirection.ReturnValue, 10, 0, null, global::System.Data.DataRowVersion.Current, false, null, "", "", ""));
                    cmd.Parameters.Add(new System.Data.SqlClient.SqlParameter("@username", global::System.Data.SqlDbType.VarChar, 512, global::System.Data.ParameterDirection.Input, 0, 0, null, global::System.Data.DataRowVersion.Current, false, username, "", "", ""));
                    cmd.Parameters.Add(new System.Data.SqlClient.SqlParameter("@password", global::System.Data.SqlDbType.VarChar, 512, global::System.Data.ParameterDirection.Input, 0, 0, null, global::System.Data.DataRowVersion.Current, false, HashPassword(password), "", "", ""));
                    cmd.Parameters.Add(new System.Data.SqlClient.SqlParameter("@onlyTestPassword", global::System.Data.SqlDbType.Bit, 1, global::System.Data.ParameterDirection.Input, 0, 0, null, global::System.Data.DataRowVersion.Current, false, true, "", "", ""));
                    cmd.Parameters.Add(new System.Data.SqlClient.SqlParameter("@loginKey", global::System.Data.SqlDbType.Int, 4, global::System.Data.ParameterDirection.InputOutput, 0, 0, null, global::System.Data.DataRowVersion.Current, false, 0, "", "", ""));
                    cmd.Parameters.Add(new System.Data.SqlClient.SqlParameter("@sessionId", global::System.Data.SqlDbType.Int, 4, global::System.Data.ParameterDirection.InputOutput, 0, 0, null, global::System.Data.DataRowVersion.Current, false, 0, "", "", ""));
                    cmd.Parameters.Add(new System.Data.SqlClient.SqlParameter("@userId", global::System.Data.SqlDbType.Int, 4, global::System.Data.ParameterDirection.InputOutput, 10, 0, null, global::System.Data.DataRowVersion.Current, false, 0, "", "", ""));
                    //cmd.Parameters.Add(new System.Data.SqlClient.SqlParameter("@idEfector", global::System.Data.SqlDbType.Int, 4, global::System.Data.ParameterDirection.InputOutput, 10, 0, null, global::System.Data.DataRowVersion.Current, false, 0, "", "", ""));  //gustavo 19/06/2013
                    //cmd.Parameters.Add(new System.Data.SqlClient.SqlParameter("@idEfectorRol", global::System.Data.SqlDbType.Int, 4, global::System.Data.ParameterDirection.InputOutput, 10, 0, null, global::System.Data.DataRowVersion.Current, false, 0, "", "", ""));  //gustavo 19/06/2013
                    cmd.ExecuteNonQuery();
                    if ((int)cmd.Parameters["@RETURN_VALUE"].Value == SSOConstants.Ok)
                    {
                        result = true;
                        userId = (int)cmd.Parameters["@userId"].Value;
                    }
                }
                catch (Exception e)
                {
                    throw new SSOException(SSOException.ExceptionCodeType.DatabaseReadError, e.ToString());
                }
                finally
                {
                    cmd.Dispose();
                }
            }
            catch (Exception e)
            {
                throw new SSOException(SSOException.ExceptionCodeType.DatabaseConnectionError, e.ToString());
            }
            finally
            {
                conn.Dispose();
            }
            return result;
        }

        // Summary:
        //     Processes a request to update the password for a membership user.
        //
        // Parameters:
        //   username:
        //     The user to update the password for.
        //
        //   oldPassword:
        //     The current password for the specified user.
        //
        //   newPassword:
        //     The new password for the specified user.
        //
        // Returns:
        //     true if the password was updated successfully; otherwise, false.
        
        public bool ChangePassword(string newPassword)
        {
            // Inicializa
            SqlConnection conn = null;

            try
            {
                conn = new SqlConnection(this.connectionString);
                conn.Open();
                SqlCommand cmd = null;
                try
                {
                    cmd = new SqlCommand("sp_SSO_Op_Users", conn);
                    cmd.CommandType = System.Data.CommandType.StoredProcedure;
                    cmd.Parameters.Add(new System.Data.SqlClient.SqlParameter("@RETURN_VALUE", global::System.Data.SqlDbType.Int, 4, global::System.Data.ParameterDirection.ReturnValue, 10, 0, null, global::System.Data.DataRowVersion.Current, false, null, "", "", ""));
                    cmd.Parameters.Add(new System.Data.SqlClient.SqlParameter("@operation", global::System.Data.SqlDbType.Int, 4, global::System.Data.ParameterDirection.Input, 0, 0, null, global::System.Data.DataRowVersion.Current, false, 7, "", "", ""));
                    cmd.Parameters.Add(new System.Data.SqlClient.SqlParameter("@id", global::System.Data.SqlDbType.Int, 4, global::System.Data.ParameterDirection.Input, 0, 0, null, global::System.Data.DataRowVersion.Current, false, SSOHelper.CurrentIdentity.Id, "", "", ""));
                    cmd.Parameters.Add(new System.Data.SqlClient.SqlParameter("@password", global::System.Data.SqlDbType.VarChar, 512, global::System.Data.ParameterDirection.Input, 0, 0, null, global::System.Data.DataRowVersion.Current, false, HashPassword(newPassword), "", "", ""));
                    cmd.Parameters.Add(new System.Data.SqlClient.SqlParameter("@must_change_password", global::System.Data.SqlDbType.Bit, 1, global::System.Data.ParameterDirection.Input, 0, 0, null, global::System.Data.DataRowVersion.Current, false, false, "", "", ""));
                    cmd.Parameters.Add(new System.Data.SqlClient.SqlParameter("@enabled", global::System.Data.SqlDbType.Bit, 1, global::System.Data.ParameterDirection.Input, 0, 0, null, global::System.Data.DataRowVersion.Current, false, true, "", "", ""));
                    cmd.Parameters.Add(new System.Data.SqlClient.SqlParameter("@resultKey", global::System.Data.SqlDbType.Int, 4, global::System.Data.ParameterDirection.Output, 0, 0, null, global::System.Data.DataRowVersion.Current, false, null, "", "", ""));
                    cmd.ExecuteNonQuery();
                }
                catch (Exception e)
                {
                    throw new SSOException(SSOException.ExceptionCodeType.DatabaseReadError, e.ToString());
                }
                finally
                {
                    cmd.Dispose();
                }
            }
            catch (Exception e)
            {
                throw new SSOException(SSOException.ExceptionCodeType.DatabaseConnectionError, e.ToString());
            }
            finally
            {
                conn.Dispose();
            }
            return true;
        }


        internal void RegistarEfector(int idEfector, int idEfectorRol)
        {
            // Inicializa            
            SqlConnection conn = null;

            try
            {
                conn = new SqlConnection(this.connectionString);
                conn.Open();
                SqlCommand cmd = null;
                try
                {
                    cmd = new SqlCommand("sp_SSO_LoginEfector", conn);
                    cmd.CommandType = System.Data.CommandType.StoredProcedure;
                    cmd.Parameters.Add(new System.Data.SqlClient.SqlParameter("@loginKey", global::System.Data.SqlDbType.Int, 4, global::System.Data.ParameterDirection.Input, 0, 0, null, global::System.Data.DataRowVersion.Current, false, 0, "", "", ""));
                    cmd.Parameters.Add(new System.Data.SqlClient.SqlParameter("@sessionId", global::System.Data.SqlDbType.VarChar, 512, global::System.Data.ParameterDirection.Input, 0, 0, null, global::System.Data.DataRowVersion.Current, false, SSOHelper.CurrentIdentity.SessionId, "", "", ""));
                    cmd.Parameters.Add(new System.Data.SqlClient.SqlParameter("@idEfector", global::System.Data.SqlDbType.Int, 4, global::System.Data.ParameterDirection.Input, 0, 0, null, global::System.Data.DataRowVersion.Current, false, idEfector, "", "", ""));
                    cmd.Parameters.Add(new System.Data.SqlClient.SqlParameter("@idEfectorRol", global::System.Data.SqlDbType.Int, 4, global::System.Data.ParameterDirection.Input, 0, 0, null, global::System.Data.DataRowVersion.Current, false, idEfectorRol, "", "", ""));                    

                    cmd.ExecuteNonQuery();
                }
                catch (Exception e)
                {
                    throw new SSOException(SSOException.ExceptionCodeType.DatabaseReadError, e.ToString());
                }
                finally
                {
                    cmd.Dispose();
                }
            }
            catch (Exception e)
            {
                throw new SSOException(SSOException.ExceptionCodeType.DatabaseConnectionError, e.ToString());
            }
            finally
            {
                conn.Dispose();
            }            
        }




        /// <summary>
        /// Realiza el login para el usuario indicado
        /// </summary>
        /// <param name="username">Usuario.</param>
        /// <param name="password">Contraseña</param>
        /// <param name="ipAddress">IP del usuario</param>
        /// <param name="sessionID">ID de sesión</param>
        /// <param name="idEfector">Id del Efector </param>
        /// <returns></returns>
        /// 


        internal bool Login(string username, string password, string ipAddress, out string sessionID, out bool userLocked)
        {
            // Inicializa
            SqlConnection conn = null;
            bool result = false;
            sessionID = null;
            userLocked = false;

            try
            {
                conn = new SqlConnection(this.connectionString);
                conn.Open();
                SqlCommand cmd = null;
                try
                {
                    cmd = new SqlCommand("sp_SSO_Login", conn);
                    cmd.CommandType = System.Data.CommandType.StoredProcedure;
                    cmd.Parameters.Add(new System.Data.SqlClient.SqlParameter("@RETURN_VALUE", global::System.Data.SqlDbType.Int, 4, global::System.Data.ParameterDirection.ReturnValue, 10, 0, null, global::System.Data.DataRowVersion.Current, false, null, "", "", ""));
                    cmd.Parameters.Add(new System.Data.SqlClient.SqlParameter("@username", global::System.Data.SqlDbType.VarChar, 512, global::System.Data.ParameterDirection.Input, 0, 0, null, global::System.Data.DataRowVersion.Current, false, username, "", "", ""));
                    cmd.Parameters.Add(new System.Data.SqlClient.SqlParameter("@password", global::System.Data.SqlDbType.VarChar, 512, global::System.Data.ParameterDirection.Input, 0, 0, null, global::System.Data.DataRowVersion.Current, false, HashPassword(password), "", "", ""));
                    cmd.Parameters.Add(new System.Data.SqlClient.SqlParameter("@IP", global::System.Data.SqlDbType.VarChar, 512, global::System.Data.ParameterDirection.Input, 0, 0, null, global::System.Data.DataRowVersion.Current, false, ipAddress, "", "", ""));
                    cmd.Parameters.Add(new System.Data.SqlClient.SqlParameter("@onlyTestPassword", global::System.Data.SqlDbType.Bit, 0, global::System.Data.ParameterDirection.Input, 0, 0, null, global::System.Data.DataRowVersion.Current, false, false, "", "", ""));
                    cmd.Parameters.Add(new System.Data.SqlClient.SqlParameter("@loginKey", global::System.Data.SqlDbType.Int, 4, global::System.Data.ParameterDirection.Output, 0, 0, null, global::System.Data.DataRowVersion.Current, false, 0, "", "", ""));
                    cmd.Parameters.Add(new System.Data.SqlClient.SqlParameter("@sessionId", global::System.Data.SqlDbType.VarChar, 512, global::System.Data.ParameterDirection.Output, 0, 0, null, global::System.Data.DataRowVersion.Current, false, "", "", "", ""));
                    cmd.Parameters.Add(new System.Data.SqlClient.SqlParameter("@userId", global::System.Data.SqlDbType.Int, 4, global::System.Data.ParameterDirection.Output, 10, 0, null, global::System.Data.DataRowVersion.Current, false, 0, "", "", ""));                    
                    cmd.ExecuteNonQuery();
                    if ((int)cmd.Parameters["@RETURN_VALUE"].Value == SSOConstants.Ok)
                    {
                        result = true;
                        sessionID = (string)cmd.Parameters["@sessionId"].Value;
                    }
                    else
                        userLocked = ((int)cmd.Parameters["@RETURN_VALUE"].Value) == 1002;
                }
                catch (Exception e)
                {
                    throw new SSOException(SSOException.ExceptionCodeType.DatabaseReadError, e.ToString());
                }
                finally
                {
                    cmd.Dispose();
                }
            }
            catch (Exception e)
            {
                throw new SSOException(SSOException.ExceptionCodeType.DatabaseConnectionError, e.ToString());
            }
            finally
            {
                conn.Dispose();
            }
            return result;
        }

        /// <summary>
        /// Realiza el logout para la sesión indicada
        /// </summary>
        /// <param name="sessionID">ID de la sesión</param>
        /// <returns></returns>
        internal bool Logout(string sessionID)
        {
            // Inicializa
            SqlConnection conn = null;
            bool result = false;

            try
            {
                conn = new SqlConnection(this.connectionString);
                conn.Open();
                SqlCommand cmd = null;
                try
                {
                    cmd = new SqlCommand("sp_SSO_Logout", conn);
                    cmd.CommandType = System.Data.CommandType.StoredProcedure;
                    cmd.Parameters.Add(new System.Data.SqlClient.SqlParameter("@RETURN_VALUE", global::System.Data.SqlDbType.Int, 4, global::System.Data.ParameterDirection.ReturnValue, 10, 0, null, global::System.Data.DataRowVersion.Current, false, null, "", "", ""));
                    cmd.Parameters.Add(new System.Data.SqlClient.SqlParameter("@loginKey", global::System.Data.SqlDbType.Int, 4, global::System.Data.ParameterDirection.Input, 0, 0, null, global::System.Data.DataRowVersion.Current, false, 0, "", "", ""));
                    cmd.Parameters.Add(new System.Data.SqlClient.SqlParameter("@sessionId", global::System.Data.SqlDbType.VarChar, 512, global::System.Data.ParameterDirection.Input, 0, 0, null, global::System.Data.DataRowVersion.Current, false, sessionID, "", "", ""));
                    cmd.ExecuteNonQuery();
                    result = true;
                }
                catch (Exception e)
                {
                    throw new SSOException(SSOException.ExceptionCodeType.DatabaseReadError, e.ToString());
                }
                finally
                {
                    cmd.Dispose();
                }
            }
            catch (Exception e)
            {
                throw new SSOException(SSOException.ExceptionCodeType.DatabaseConnectionError, e.ToString());
            }
            finally
            {
                conn.Dispose();
            }
            return result;
        }

        #endregion

        #region Other SSO Methods

        public List<SSOApplication> GetAllowedApplications(SSOIdentity identity)
        {
            SqlConnection conn = null;
            List<SSOApplication> result = null;
            int efectorRol = GetEfectorRol(identity);
            try
            {
                conn = new SqlConnection(this.connectionString);
                conn.Open();
                SqlCommand cmd = null;
                try
                {
                    cmd = new SqlCommand("sp_SSO_AllowedAppsByEfector", conn);  //gustavo 05/06/2013 update
                    cmd.CommandType = System.Data.CommandType.StoredProcedure;
                    cmd.Parameters.Add(new global::System.Data.SqlClient.SqlParameter("@userId", global::System.Data.SqlDbType.Int, 4, global::System.Data.ParameterDirection.Input, 10, 0, null, global::System.Data.DataRowVersion.Current, false, identity.Id, "", "", ""));
                    cmd.Parameters.Add(new global::System.Data.SqlClient.SqlParameter("@roleId", global::System.Data.SqlDbType.Int, 4, global::System.Data.ParameterDirection.Input, 10, 0, null, global::System.Data.DataRowVersion.Current, false, efectorRol, "", "", ""));
                    SqlDataReader reader = cmd.ExecuteReader();
                    try
                    {
                        result = new List<SSOApplication>();
                        while (reader.Read())
                        {
                            result.Add(new SSOApplication(reader));
                        }
                    }
                    finally
                    {
                        reader.Dispose();
                    }
                }
                catch (Exception e)
                {
                    throw new SSOException(SSOException.ExceptionCodeType.DatabaseReadError, e.ToString());
                }
                finally
                {
                    cmd.Dispose();
                }
            }
            catch (Exception e)
            {
                throw new SSOException(SSOException.ExceptionCodeType.DatabaseConnectionError, e.ToString());
            }
            finally
            {
                conn.Dispose();
            }
            return result;
        }

        public List<SSOApplication> GetAllowedApplicationsHospital(SSOIdentity identity)
        {
            SqlConnection conn = null;
            List<SSOApplication> result = null;
            int efectorRol = GetEfectorRol(identity);
            try
            {
                conn = new SqlConnection(this.connectionString);
                conn.Open();
                SqlCommand cmd = null;
                try
                {
                    cmd = new SqlCommand("sp_SSO_AllowedAppsByEfectorHospital", conn);  //carolina 26/05/2014 filtro por modulo hospitalario
                    cmd.CommandType = System.Data.CommandType.StoredProcedure;
                    cmd.Parameters.Add(new global::System.Data.SqlClient.SqlParameter("@userId", global::System.Data.SqlDbType.Int, 4, global::System.Data.ParameterDirection.Input, 10, 0, null, global::System.Data.DataRowVersion.Current, false, identity.Id, "", "", ""));
                    cmd.Parameters.Add(new global::System.Data.SqlClient.SqlParameter("@roleId", global::System.Data.SqlDbType.Int, 4, global::System.Data.ParameterDirection.Input, 10, 0, null, global::System.Data.DataRowVersion.Current, false, efectorRol, "", "", ""));
                    SqlDataReader reader = cmd.ExecuteReader();
                    try
                    {
                        result = new List<SSOApplication>();
                        while (reader.Read())
                        {
                            
                            result.Add(new SSOApplication(reader));
                        }
                    }
                    finally
                    {
                        reader.Dispose();
                    }
                }
                catch (Exception e)
                {
                    throw new SSOException(SSOException.ExceptionCodeType.DatabaseReadError, e.ToString());
                }
                finally
                {
                    cmd.Dispose();
                }
            }
            catch (Exception e)
            {
                throw new SSOException(SSOException.ExceptionCodeType.DatabaseConnectionError, e.ToString());
            }
            finally
            {
                conn.Dispose();
            }
            return result;
        }

        public List<SSOApplicationGroup> GetApplicationGroups(SSOApplication application)
        {
            SqlConnection conn = null;
            List<SSOApplicationGroup> result = null;
            try
            {
                conn = new SqlConnection(this.connectionString);
                conn.Open();
                SqlCommand cmd = null;
                try
                {
                    cmd = new SqlCommand("SELECT * FROM dbo.SSO_GetApplicationGroupsEx(@applicationId, NULL)", conn);
                    cmd.CommandType = System.Data.CommandType.Text;
                    cmd.Parameters.Add(new global::System.Data.SqlClient.SqlParameter("@applicationId", global::System.Data.SqlDbType.Int, 4, global::System.Data.ParameterDirection.Input, 10, 0, null, global::System.Data.DataRowVersion.Current, false, application.Id, "", "", ""));
                    SqlDataReader reader = cmd.ExecuteReader();
                    try
                    {
                        result = new List<SSOApplicationGroup>();
                        while (reader.Read())
                        {
                            result.Add(new SSOApplicationGroup(application, reader));
                        }
                    }
                    finally
                    {
                        reader.Dispose();
                    }
                }
                catch (Exception e)
                {
                    throw new SSOException(SSOException.ExceptionCodeType.DatabaseReadError, e.ToString());
                }
                finally
                {
                    cmd.Dispose();
                }
            }
            catch (Exception e)
            {
                throw new SSOException(SSOException.ExceptionCodeType.DatabaseConnectionError, e.ToString());
            }
            finally
            {
                conn.Dispose();
            }
            return result;
        }


        public List<SSOModule> GetAllowedModulesByEfector(SSOIdentity identity, SSOApplication application)
        {
            SqlConnection conn = null;
            List<SSOModule> result = null;
            int efectorRol = GetEfectorRol(identity);
            var aplicacion = SSOHelper.CurrentIdentity.CurrentModule.ApplicationId;

            try
            {
                conn = new SqlConnection(this.connectionString);
                conn.Open();
                SqlCommand cmd = null;
                try
                {
                    cmd = new SqlCommand("SET arithabort ON; EXEC sp_SSO_AllowedModulesByEfector @userId, @applicationId, @idEfector; SET arithabort OFF;", conn);
                    cmd.CommandType = System.Data.CommandType.Text;
                    cmd.Parameters.Add(new global::System.Data.SqlClient.SqlParameter("@userId", global::System.Data.SqlDbType.Int, 4, global::System.Data.ParameterDirection.Input, 10, 0, null, global::System.Data.DataRowVersion.Current, false, identity.Id, "", "", ""));
                    cmd.Parameters.Add(new global::System.Data.SqlClient.SqlParameter("@applicationId", global::System.Data.SqlDbType.Int, 4, global::System.Data.ParameterDirection.Input, 10, 0, null, global::System.Data.DataRowVersion.Current, false, aplicacion, "", "", ""));
                    cmd.Parameters.Add(new global::System.Data.SqlClient.SqlParameter("@idEfector", global::System.Data.SqlDbType.Int, 4, global::System.Data.ParameterDirection.Input, 10, 0, null, global::System.Data.DataRowVersion.Current, false, efectorRol, "", "", ""));
                    SqlDataReader reader = cmd.ExecuteReader();
                    try
                    {
                        result = new List<SSOModule>();
                        while (reader.Read())
                        {
                            result.Add(new SSOModule(reader));
                        }
                    }
                    finally
                    {
                        reader.Dispose();
                    }
                }
                catch (Exception e)
                {
                    throw new SSOException(SSOException.ExceptionCodeType.DatabaseReadError, e.ToString());
                }
                finally
                {
                    cmd.Dispose();
                }
            }
            catch (Exception e)
            {
                throw new SSOException(SSOException.ExceptionCodeType.DatabaseConnectionError, e.ToString());
            }
            finally
            {
                conn.Dispose();
            }
            return result;
        }
        
        //public List<SSOModule> GetAllowedModules(SSOIdentity identity, SSOApplication application)
        //{
        //    SqlConnection conn = null;
        //    List<SSOModule> result = null;
        //    try
        //    {
        //        conn = new SqlConnection(this.connectionString);
        //        conn.Open();
        //        SqlCommand cmd = null;
        //        try
        //        {
        //            cmd = new SqlCommand("SET arithabort ON; EXEC sp_SSO_AllowedModules @userId, @applicationId; SET arithabort OFF;", conn);
        //            cmd.CommandType = System.Data.CommandType.Text;
        //            cmd.Parameters.Add(new global::System.Data.SqlClient.SqlParameter("@userId", global::System.Data.SqlDbType.Int, 4, global::System.Data.ParameterDirection.Input, 10, 0, null, global::System.Data.DataRowVersion.Current, false, identity.Id, "", "", ""));
        //            cmd.Parameters.Add(new global::System.Data.SqlClient.SqlParameter("@applicationId", global::System.Data.SqlDbType.Int, 4, global::System.Data.ParameterDirection.Input, 10, 0, null, global::System.Data.DataRowVersion.Current, false, application.Id, "", "", ""));
        //            SqlDataReader reader = cmd.ExecuteReader();
        //            try
        //            {
        //                result = new List<SSOModule>();
        //                while (reader.Read())
        //                {
        //                    result.Add(new SSOModule(reader));
        //                }
        //            }
        //            finally
        //            {
        //                reader.Dispose();
        //            }
        //        }
        //        catch (Exception e)
        //        {
        //            throw new SSOException(SSOException.ExceptionCodeType.DatabaseReadError, e.ToString());
        //        }
        //        finally
        //        {
        //            cmd.Dispose();
        //        }
        //    }
        //    catch (Exception e)
        //    {
        //        throw new SSOException(SSOException.ExceptionCodeType.DatabaseConnectionError, e.ToString());
        //    }
        //    finally
        //    {
        //        conn.Dispose();
        //    }
        //    return result;
        //}
                                          

        public SSOApplication FindApplication(int applicationId)
        {
            SqlConnection conn = null;
            SSOApplication result = null;
            try
            {
                conn = new SqlConnection(this.connectionString);
                conn.Open();
                SqlCommand cmd = null;
                try
                {
                    cmd = new SqlCommand("sp_SSO_GetApplicationData", conn);
                    cmd.CommandType = System.Data.CommandType.StoredProcedure;
                    cmd.Parameters.Add(new global::System.Data.SqlClient.SqlParameter("@applicationId", global::System.Data.SqlDbType.VarChar, 200, global::System.Data.ParameterDirection.Input, 0, 0, null, global::System.Data.DataRowVersion.Current, false, applicationId, "", "", ""));
                    SqlDataReader reader = cmd.ExecuteReader();
                    try
                    {
                        if (reader.Read())
                            result = new SSOApplication(reader);
                    }
                    finally
                    {
                        reader.Dispose();
                    }
                }
                catch (Exception e)
                {
                    throw new SSOException(SSOException.ExceptionCodeType.DatabaseReadError, e.ToString());
                }
                finally
                {
                    cmd.Dispose();
                }
            }
            catch (Exception e)
            {
                throw new SSOException(SSOException.ExceptionCodeType.DatabaseConnectionError, e.ToString());
            }
            finally
            {
                conn.Dispose();
            }
            return result;
        }

        //gustavo 05/06/2013
        internal HashSet<int> GetAllowedModulesByEfector(SSOIdentity identity)
        {
            using (Data.DataContext DataContext = SSOHelper.GetDataContext())
            {
                return new HashSet<int>((from r in
                                             (from module in DataContext.SSO_Modules
                                              select new
                                              {
                                                  id = module.id,
                                                  allowed = (from r in DataContext.SSO_Permissions_Cache
                                                             where r.userId == identity.Id
                                                             && r.targetType == (int)SSOPermissionTarget.Module
                                                             && r.target == module.id
                                                             orderby r.inherited, r.roleDepthFromUser, r.groupId descending /* priorize 1º) users / 2º) role groups / 3º) roles */
                                                             select r.allow).FirstOrDefault()
                                              })
                                         where r.allowed
                                         select r.id).ToArray());
            }
        }    

        //internal HashSet<int> GetAllowedModules(SSOIdentity identity)
        //{
        //    using (Data.DataContext DataContext = SSOHelper.GetDataContext())
        //    {
        //        return new HashSet<int>((from r in
        //                                     (from module in DataContext.SSO_Modules
        //                                      select new
        //                                      {
        //                                          id = module.id,
        //                                          allowed = (from r in DataContext.SSO_Permissions_Cache
        //                                                     where r.userId == identity.Id
        //                                                     && r.targetType == (int)SSOPermissionTarget.Module
        //                                                     && r.target == module.id
        //                                                     orderby r.inherited, r.roleDepthFromUser, r.groupId descending /* priorize 1º) users / 2º) role groups / 3º) roles */
        //                                                     select r.allow).FirstOrDefault()
        //                                      })
        //                                 where r.allowed
        //                                 select r.id).ToArray());
        //    }
        //}

        internal int BeginAccess(SSOIdentity identity, SSOModule module)
        {
            using (Data.DataContext DataContext = SSOHelper.GetDataContext())
            {
                Data.SSO_AccessLog accessLog = new Data.SSO_AccessLog();
                accessLog.moduleId = module.Id;
                accessLog.userId = identity.Id;
                accessLog.timeIn = DateTime.Now;
                DataContext.SSO_AccessLog.InsertOnSubmit(accessLog);
                DataContext.SubmitChanges();
                return accessLog.id;
            }
        }

        internal void FinalizeAccess(int accessKey, bool result)
        {
            SqlConnection conn = null;
            try
            {
                conn = new SqlConnection(this.connectionString);
                conn.Open();
                SqlCommand cmd = null;
                try
                {
                    cmd = new SqlCommand("sp_SSO_FinalizeModuleAccess", conn);
                    cmd.CommandType = System.Data.CommandType.StoredProcedure;
                    cmd.Parameters.Add(new global::System.Data.SqlClient.SqlParameter("@result", global::System.Data.SqlDbType.Bit, 4, global::System.Data.ParameterDirection.Input, 10, 0, null, global::System.Data.DataRowVersion.Current, false, result, "", "", ""));
                    cmd.Parameters.Add(new global::System.Data.SqlClient.SqlParameter("@accessKey", global::System.Data.SqlDbType.Int, 4, global::System.Data.ParameterDirection.Input, 10, 0, null, global::System.Data.DataRowVersion.Current, false, accessKey, "", "", ""));
                    cmd.ExecuteNonQuery();
                }
                catch (Exception e)
                {
                    throw new SSOException(SSOException.ExceptionCodeType.DatabaseWriteError, e.ToString());
                }
                finally
                {
                    cmd.Dispose();
                }
            }
            catch (Exception e)
            {
                throw new SSOException(SSOException.ExceptionCodeType.DatabaseConnectionError, e.ToString());
            }
            finally
            {
                conn.Dispose();
            }
        }

        public void LogAccessKey(int accessKey, string key, string value)
        {
            SqlConnection conn = null;
            try
            {
                conn = new SqlConnection(this.connectionString);
                conn.Open();
                SqlCommand cmd = null;
                try
                {
                    cmd = new SqlCommand("sp_SSO_ModuleAccessSetKey", conn);
                    cmd.CommandType = System.Data.CommandType.StoredProcedure;
                    cmd.Parameters.Add(new global::System.Data.SqlClient.SqlParameter("@accessKey", global::System.Data.SqlDbType.Int, 4, global::System.Data.ParameterDirection.Input, 10, 0, null, global::System.Data.DataRowVersion.Current, false, accessKey, "", "", ""));
                    cmd.Parameters.Add(new global::System.Data.SqlClient.SqlParameter("@key", global::System.Data.SqlDbType.VarChar, 8000, global::System.Data.ParameterDirection.Input, 10, 0, null, global::System.Data.DataRowVersion.Current, false, key, "", "", ""));
                    cmd.Parameters.Add(new global::System.Data.SqlClient.SqlParameter("@value", global::System.Data.SqlDbType.VarChar, 8000, global::System.Data.ParameterDirection.Input, 10, 0, null, global::System.Data.DataRowVersion.Current, false, value, "", "", ""));
                    cmd.ExecuteNonQuery();
                }
                catch (Exception e)
                {
                    throw new SSOException(SSOException.ExceptionCodeType.DatabaseWriteError, e.ToString());
                }
                finally
                {
                    cmd.Dispose();
                }
            }
            catch (Exception e)
            {
                throw new SSOException(SSOException.ExceptionCodeType.DatabaseConnectionError, e.ToString());
            }
            finally
            {
                conn.Dispose();
            }
        }

        /// <summary>
        /// Obtiene toda la tabla SSO_Config. Se realiza de esta manera para optimizar el acceso a los sistemas
        /// </summary>
        /// <returns></returns>
        internal SortedDictionary<string, object> GetConfigurations()
        {
            SortedDictionary<string, object> result = new SortedDictionary<string, object>();
            using (SqlConnection conn = new SqlConnection(this.connectionString))
            {
                conn.Open();
                SqlCommand cmd = new SqlCommand("SELECT name, valueInt, valueStr as value FROM SSO_Config", conn);
                cmd.CommandType = System.Data.CommandType.Text;
                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                        result.Add(reader.GetString(0), reader.GetValue(1) != DBNull.Value ? reader.GetValue(1) : reader.GetValue(2));
                }
            }
            return result;
        }

        internal SSOVariables GetStoredVariables(int userId, string variable)
        {
            SSOVariables result = null;
            SqlConnection conn = null;
            try
            {
                conn = new SqlConnection(this.connectionString);
                conn.Open();
                SqlCommand cmd = null;
                try
                {
                    cmd = new SqlCommand("SELECT * FROM dbo.SSO_GetStoredVariable(@variable, @userId, null)", conn);
                    cmd.CommandType = System.Data.CommandType.Text;
                    cmd.Parameters.Add(new global::System.Data.SqlClient.SqlParameter("@variable", global::System.Data.SqlDbType.VarChar, 200, global::System.Data.ParameterDirection.Input, 10, 0, null, global::System.Data.DataRowVersion.Current, false, variable, "", "", ""));
                    cmd.Parameters.Add(new global::System.Data.SqlClient.SqlParameter("@userId", global::System.Data.SqlDbType.Int, 4, global::System.Data.ParameterDirection.Input, 10, 0, null, global::System.Data.DataRowVersion.Current, false, userId, "", "", ""));
                    SqlDataReader reader = cmd.ExecuteReader();
                    result = new SSOVariables(reader);
                }
                catch (Exception e)
                {
                    throw new SSOException(SSOException.ExceptionCodeType.DatabaseReadError, e.ToString());
                }
                finally
                {
                    cmd.Dispose();
                }
            }
            catch (Exception e)
            {
                throw new SSOException(SSOException.ExceptionCodeType.DatabaseConnectionError, e.ToString());
            }
            finally
            {
                conn.Dispose();
            }

            return result;
        }

        internal object GetUserSetting(SSOIdentity identity, object settingId)
        {
            SqlConnection conn = null;
            object result = null;
            try
            {
                conn = new SqlConnection(this.connectionString);
                conn.Open();
                SqlCommand cmd = null;
                try
                {
                    cmd = new SqlCommand("sp_SSO_GetUserSetting", conn);
                    cmd.CommandType = System.Data.CommandType.StoredProcedure;
                    cmd.Parameters.Add(new System.Data.SqlClient.SqlParameter("@userId", global::System.Data.SqlDbType.VarChar, 512, global::System.Data.ParameterDirection.Input, 0, 0, null, global::System.Data.DataRowVersion.Current, false, identity.Id, "", "", ""));
                    cmd.Parameters.Add(new System.Data.SqlClient.SqlParameter("@settingId", global::System.Data.SqlDbType.VarChar, 512, global::System.Data.ParameterDirection.Input, 0, 0, null, global::System.Data.DataRowVersion.Current, false, settingId, "", "", ""));
                    cmd.Parameters.Add(new System.Data.SqlClient.SqlParameter("@value", global::System.Data.SqlDbType.Variant, 4, global::System.Data.ParameterDirection.Output, 10, 0, null, global::System.Data.DataRowVersion.Current, false, 0, "", "", ""));
                    cmd.ExecuteNonQuery();
                    result = cmd.Parameters["@value"].Value;
                    if (result == DBNull.Value)
                        result = null;
                }
                catch (Exception e)
                {
                    throw new SSOException(SSOException.ExceptionCodeType.DatabaseReadError, e.ToString());
                }
                finally
                {
                    cmd.Dispose();
                }
            }
            catch (Exception e)
            {
                throw new SSOException(SSOException.ExceptionCodeType.DatabaseConnectionError, e.ToString());
            }
            finally
            {
                conn.Dispose();
            }
            return result;
        }

        internal void SetUserSetting(SSOIdentity identity, object settingId, object value)
        {
            SqlConnection conn = null;
            object result = null;
            try
            {
                conn = new SqlConnection(this.connectionString);
                conn.Open();
                SqlCommand cmd = null;
                try
                {
                    cmd = new SqlCommand("sp_SSO_SetUserSetting", conn);
                    cmd.CommandType = System.Data.CommandType.StoredProcedure;
                    cmd.Parameters.Add(new System.Data.SqlClient.SqlParameter("@userId", global::System.Data.SqlDbType.VarChar, 512, global::System.Data.ParameterDirection.Input, 0, 0, null, global::System.Data.DataRowVersion.Current, false, identity.Id, "", "", ""));
                    cmd.Parameters.Add(new System.Data.SqlClient.SqlParameter("@settingId", global::System.Data.SqlDbType.VarChar, 512, global::System.Data.ParameterDirection.Input, 0, 0, null, global::System.Data.DataRowVersion.Current, false, settingId, "", "", ""));
                    cmd.Parameters.Add(new System.Data.SqlClient.SqlParameter("@value", global::System.Data.SqlDbType.Variant, 0, global::System.Data.ParameterDirection.Input, 10, 0, null, global::System.Data.DataRowVersion.Current, false, value, "", "", ""));
                    cmd.ExecuteNonQuery();
                }
                catch (Exception e)
                {
                    throw new SSOException(SSOException.ExceptionCodeType.DatabaseReadError, e.ToString());
                }
                finally
                {
                    cmd.Dispose();
                }
            }
            catch (Exception e)
            {
                throw new SSOException(SSOException.ExceptionCodeType.DatabaseConnectionError, e.ToString());
            }
            finally
            {
                conn.Dispose();
            }
        }

        public override MembershipUserCollection GetAllUsers(int pageIndex, int pageSize, out int totalRecords)
        {
            throw new NotImplementedException();
            /*
            MembershipUserCollection users = new MembershipUserCollection();
            SqlConnection conn = null;
            try
            {
                conn = new SqlConnection(this.connectionString);
                conn.Open();
                SqlCommand cmd = null;
                try
                {
                    cmd = new SqlCommand("select COUNT(*) from SSO_Users", conn);
                    cmd.CommandType = System.Data.CommandType.Text;
                    totalRecords = (int)cmd.ExecuteScalar();

                    totalRecords = 1;
                    pageSize = 1;

                    cmd = new SqlCommand("SELECT * FROM dbo.SSO_GetUserData(NULL, NULL)", conn);
                    cmd.CommandType = System.Data.CommandType.Text;
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        // Seek
                        int position = 0;
                        int positionRequired = pageIndex * pageSize;
                        while (reader.Read() && position < positionRequired)
                            position++;

                        // Count
                        users.Add(GetUserFromReader(reader));
                        int count = 1;
                        while (reader.Read() && (pageSize == 0 || count < pageSize))
                        {
                            count++;
                            users.Add(GetUserFromReader(reader));
                        }
                    }
                }
                catch (Exception e)
                {
                    throw new SSOException(SSOException.ExceptionCodeType.DatabaseReadError, e.ToString());
                }
                finally
                {
                    cmd.Dispose();
                }
            }
            catch (Exception e)
            {
                throw new SSOException(SSOException.ExceptionCodeType.DatabaseConnectionError, e.ToString());
            }
            finally
            {
                conn.Dispose();
            }
            return users;
             * */
        }

        public override int GetNumberOfUsersOnline()
        {
            SqlConnection conn = null;
            int result = 0;
            try
            {
                conn = new SqlConnection(this.connectionString);
                conn.Open();
                SqlCommand cmd = null;
                try
                {
                    cmd = new SqlCommand("SELECT COUNT(*) FROM (SELECT DISTINCT userId FROM SSO_Sessions) SSO_Sessions", conn);
                    cmd.CommandType = System.Data.CommandType.Text;
                    result = (int)cmd.ExecuteScalar();
                }
                catch (Exception e)
                {
                    throw new SSOException(SSOException.ExceptionCodeType.DatabaseReadError, e.ToString());
                }
                finally
                {
                    cmd.Dispose();
                }
            }
            catch (Exception e)
            {
                throw new SSOException(SSOException.ExceptionCodeType.DatabaseConnectionError, e.ToString());
            }
            finally
            {
                conn.Dispose();
            }

            return result;
        }

        #endregion

        #region Not implemented methods (To Do)
        // Summary:
        //     Processes a request to update the password for a membership user.
        //
        // Parameters:
        //   username:
        //     The user to update the password for.
        //
        //   oldPassword:
        //     The current password for the specified user.
        //
        //   newPassword:
        //     The new password for the specified user.
        //
        // Returns:
        //     true if the password was updated successfully; otherwise, false.
        public override bool ChangePassword(string username, string oldPassword, string newPassword) { throw new NotImplementedException(); }
        //
        // Summary:
        //     Processes a request to update the password question and answer for a membership
        //     user.
        //
        // Parameters:
        //   username:
        //     The user to change the password question and answer for.
        //
        //   password:
        //     The password for the specified user.
        //
        //   newPasswordQuestion:
        //     The new password question for the specified user.
        //
        //   newPasswordAnswer:
        //     The new password answer for the specified user.
        //
        // Returns:
        //     true if the password question and answer are updated successfully; otherwise,
        //     false.
        public override bool ChangePasswordQuestionAndAnswer(string username, string password, string newPasswordQuestion, string newPasswordAnswer) { throw new NotImplementedException(); }
        //
        // Summary:
        //     Adds a new membership user to the data source.
        //
        // Parameters:
        //   username:
        //     The user name for the new user.
        //
        //   password:
        //     The password for the new user.
        //
        //   email:
        //     The e-mail address for the new user.
        //
        //   passwordQuestion:
        //     The password question for the new user.
        //
        //   passwordAnswer:
        //     The password answer for the new user
        //
        //   isApproved:
        //     Whether or not the new user is approved to be validated.
        //
        //   providerUserKey:
        //     The unique identifier from the membership data source for the user.
        //
        //   status:
        //     A System.Web.Security.MembershipCreateStatus enumeration value indicating
        //     whether the user was created successfully.
        //
        // Returns:
        //     A System.Web.Security.MembershipUser object populated with the information
        //     for the newly created user.
        public override MembershipUser CreateUser(string username, string password, string email, string passwordQuestion, string passwordAnswer, bool isApproved, object providerUserKey, out MembershipCreateStatus status) { throw new NotImplementedException(); }
        //
        // Summary:
        //     Decrypts an encrypted password.
        //
        // Parameters:
        //   encodedPassword:
        //     A byte array that contains the encrypted password to decrypt.
        //
        // Returns:
        //     A byte array that contains the decrypted password.
        //
        // Exceptions:
        //   System.Configuration.Provider.ProviderException:
        //     The System.Web.Configuration.MachineKeySection.ValidationKey property or
        //     System.Web.Configuration.MachineKeySection.DecryptionKey property is set
        //     to AutoGenerate.
        public override bool DeleteUser(string username, bool deleteAllRelatedData) { throw new NotImplementedException(); }
        //
        // Summary:
        //     Encrypts a password.
        //
        // Parameters:
        //   password:
        //     A byte array that contains the password to encrypt.
        //
        // Returns:
        //     A byte array that contains the encrypted password.
        //
        // Exceptions:
        //   System.Configuration.Provider.ProviderException:
        //     The System.Web.Configuration.MachineKeySection.ValidationKey property or
        //     System.Web.Configuration.MachineKeySection.DecryptionKey property is set
        //     to AutoGenerate.
        public override MembershipUserCollection FindUsersByEmail(string emailToMatch, int pageIndex, int pageSize, out int totalRecords) { throw new NotImplementedException(); }
        //
        // Summary:
        //     Gets a collection of membership users where the user name contains the specified
        //     user name to match.
        //
        // Parameters:
        //   usernameToMatch:
        //     The user name to search for.
        //
        //   pageIndex:
        //     The index of the page of results to return. pageIndex is zero-based.
        //
        //   pageSize:
        //     The size of the page of results to return.
        //
        //   totalRecords:
        //     The total number of matched users.
        //
        // Returns:
        //     A System.Web.Security.MembershipUserCollection collection that contains a
        //     page of pageSizeSystem.Web.Security.MembershipUser objects beginning at the
        //     page specified by pageIndex.
        public override MembershipUserCollection FindUsersByName(string usernameToMatch, int pageIndex, int pageSize, out int totalRecords) { throw new NotImplementedException(); }
        public override string GetPassword(string username, string answer) { throw new NotImplementedException(); }
        //
        // Summary:
        //     Gets user information from the data source based on the unique identifier
        //     for the membership user. Provides an option to update the last-activity date/time
        //     stamp for the user.
        //
        // Parameters:
        //   providerUserKey:
        //     The unique identifier for the membership user to get information for.
        //
        //   userIsOnline:
        //     true to update the last-activity date/time stamp for the user; false to return
        //     user information without updating the last-activity date/time stamp for the
        //     user.
        //
        // Returns:
        //     A System.Web.Security.MembershipUser object populated with the specified
        //     user's information from the data source.       
        public override string GetUserNameByEmail(string email) { throw new NotImplementedException(); }
        //
        // Summary:
        //     Raises the System.Web.Security.MembershipProvider.ValidatingPassword event
        //     if an event handler has been defined.
        //
        // Parameters:
        //   e:
        //     The System.Web.Security.ValidatePasswordEventArgs to pass to the System.Web.Security.MembershipProvider.ValidatingPassword
        //     event handler.
        public override string ResetPassword(string username, string answer) { throw new NotImplementedException(); }
        //
        // Summary:
        //     Clears a lock so that the membership user can be validated.
        //
        // Parameters:
        //   userName:
        //     The membership user whose lock status you want to clear.
        //
        // Returns:
        //     true if the membership user was successfully unlocked; otherwise, false.
        public override bool UnlockUser(string userName) { throw new NotImplementedException(); }
        //
        // Summary:
        //     Updates information about a user in the data source.
        //
        // Parameters:
        //   user:
        //     A System.Web.Security.MembershipUser object that represents the user to update
        //     and the updated information for the user.
        public override void UpdateUser(MembershipUser user) { throw new NotImplementedException(); }
        //
        // Summary:
        //     Verifies that the specified user name and password exist in the data source.
        //
        // Parameters:
        //   username:
        //     The name of the user to validate.
        //
        //   password:
        //     The password for the specified user.
        //
        // Returns:
        //     true if the specified username and password are valid; otherwise, false.
        public override bool ValidateUser(string username, string password) { throw new NotImplementedException(); }

        #endregion

    }
}