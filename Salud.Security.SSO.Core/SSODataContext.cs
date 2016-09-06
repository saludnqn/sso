using System;
using System.Data;
using System.Data.Linq;
using System.Data.Linq.Mapping;

namespace Salud.Security.SSO
{
    /// <summary>
    /// Base class for handling transaction management around
    /// a DataContext.
    /// </summary>
   public partial class SSODataContext : DataContext
    {
        #region Constructors

            public SSODataContext(string connectionString, MappingSource mapping) : this(connectionString, IsolationLevel.ReadCommitted, mapping) { }
            public SSODataContext(System.Data.IDbConnection connection) : this(connection.ConnectionString) { }
            public SSODataContext(System.Data.IDbConnection connection, MappingSource mapping) : this(connection.ConnectionString, IsolationLevel.ReadCommitted, mapping) { }


            /// <summary>
            /// Constructor
            /// </summary>
            /// <param name="connectionString">The connection string to connect to the database.</param>
            public SSODataContext(string connectionString)
                : this(connectionString, IsolationLevel.ReadCommitted)
            {
            }
            /// <summary>
            /// Constructor
            /// </summary>
            /// <param name="connectionString">The connection string to connect to the database.</param>
            /// <param name="noLock">If performing READ-ONLY operations, set to true. If 
            /// a transaction is required to ensure data-integrity, set to false.</param>
            /// <exception cref="T:System.ArgumentNullException">
            /// Thrown when the specified <paramref name="connectionString"/> is <see langword="null"/>.
            /// </exception>
            public SSODataContext(string connectionString, bool noLock)
                : this(connectionString, noLock == true ? System.Data.IsolationLevel.ReadUncommitted : System.Data.IsolationLevel.ReadCommitted)
            {

            }

            public SSODataContext(string connectionString, IsolationLevel level, MappingSource mapping)
                : base(connectionString, mapping)
            {
                if (string.IsNullOrEmpty(connectionString))
                {
                    throw new ArgumentNullException("connectionString");
                }
                this.Connection.StateChange += new StateChangeEventHandler(OnConnectionStateChange);
            }

            /// <summary>
            /// Constructor
            /// </summary>
            /// <param name="connectionString">The connection string to connect to the database.</param>
            /// <param name="level">The isolation level for the current transaction. <see cref="IsolationLevel"/></param>
            public SSODataContext(string connectionString, IsolationLevel level)
                : this(connectionString, level, new AttributeMappingSource())
            {
            }
            #endregion

        public event EventHandler OnBeforeSubmitChanges;

        private void OnConnectionStateChange(object sender, StateChangeEventArgs e)
        {
            if (e.CurrentState == ConnectionState.Open && e.OriginalState == ConnectionState.Closed)
                SSOHelper.InitDBAuditData(this.Connection, this.Transaction, false);
        }

        public override void SubmitChanges(ConflictMode failureMode)
        {
            if (OnBeforeSubmitChanges != null)
                OnBeforeSubmitChanges(this, EventArgs.Empty);

            base.SubmitChanges(failureMode);
        }

    }

}
