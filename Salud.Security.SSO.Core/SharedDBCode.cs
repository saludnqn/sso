using System;
using System.Data.SqlClient;

namespace Salud.Security.SSO
{
    /// <summary>
    /// CLASE TEMPORAL. BORRAR EN CUANTO SEA POSIBLE
    /// </summary>
    internal static class SharedDBCode
    {
        public enum DBReaderAction { DBReaderThrowException, DBReaderSetDefaultValue };

        public static int GetReaderIntField(SqlDataReader reader, string fieldname, DBReaderAction actionOnError, int defaultValue)
        {
            try
            {
                return reader.GetInt32(reader.GetOrdinal(fieldname));
            }
            catch (Exception)
            {
                if (actionOnError == DBReaderAction.DBReaderSetDefaultValue)
                    return defaultValue;
                else throw;
            }
        }

        public static int GetReaderIntField(SqlDataReader reader, string fieldname, DBReaderAction actionOnError)
        {
            return GetReaderIntField(reader, fieldname, actionOnError, 0);
        }

        public static string GetReaderStringField(SqlDataReader reader, string fieldname, DBReaderAction actionOnError, string defaultValue)
        {
            try
            {
                return reader.GetString(reader.GetOrdinal(fieldname));
            }
            catch (Exception)
            {
                if (actionOnError == DBReaderAction.DBReaderSetDefaultValue)
                    return defaultValue;
                else throw;
            }
        }

        public static string GetReaderStringField(SqlDataReader reader, string fieldname, DBReaderAction actionOnError)
        {
            return GetReaderStringField(reader, fieldname, actionOnError, null);
        }

        public static bool GetReaderBoolField(SqlDataReader reader, string fieldname, DBReaderAction actionOnError, bool defaultValue)
        {
            try
            {
                return reader.GetBoolean(reader.GetOrdinal(fieldname));
            }
            catch (Exception)
            {
                if (actionOnError == DBReaderAction.DBReaderSetDefaultValue)
                    return defaultValue;
                else throw;
            }
        }

        public static bool GetReaderBoolField(SqlDataReader reader, string fieldname, DBReaderAction actionOnError)
        {
            return GetReaderBoolField(reader, fieldname, actionOnError, false);
        }

        public static DateTime GetReaderDateTimeField(SqlDataReader reader, string fieldname, DBReaderAction actionOnError, DateTime defaultValue)
        {
            try
            {
                return reader.GetDateTime(reader.GetOrdinal(fieldname));
            }
            catch (Exception)
            {
                if (actionOnError == DBReaderAction.DBReaderSetDefaultValue)
                    return defaultValue;
                else throw;
            }
        }

        public static DateTime GetReaderDateTimeField(SqlDataReader reader, string fieldname, DBReaderAction actionOnError)
        {
            return GetReaderDateTimeField(reader, fieldname, actionOnError, DateTime.MinValue);
        }

    }
}
