using System.Collections.Generic;
using System.Data.SqlClient;

namespace Salud.Security.SSO
{
    public enum SSOVariableOwner
    {
        voView,
        voUser,
        voRole, 
        voSuperRole
    }

    public class SSOVariable
    {
        public object value;
        public int priority;
        public SSOVariableOwner owner;
    }

    public class SSOVariables
    {
        private List<SSOVariable> list;

        public SSOVariables(SqlDataReader reader)
        {
            list = new List<SSOVariable>();
            while (reader.Read())
            {
                SSOVariable variable = new SSOVariable();
                variable.value = reader.GetValue(reader.GetOrdinal("value"));
                variable.priority = reader.GetInt32(reader.GetOrdinal("priority"));
                switch (variable.priority)
                {
                    case 0:
                        variable.owner = SSOVariableOwner.voView;
                        break;
                    case 1:
                        variable.owner = SSOVariableOwner.voUser;
                        break;
                    case 2:
                        variable.owner = SSOVariableOwner.voRole;
                        break;
                    default:
                        variable.owner = SSOVariableOwner.voSuperRole;
                        break;
                }
                list.Add(variable);
            }
        }

        public object this[SSOVariableOwner[] owners]
        {
            get
            {
                object result = null;
                if (list.Count > 0)
                {
                    if (owners.Length == 0)
                    {
                        result = list[0].value;
                    }
                    else
                    {
                        for (int i = 0; i < list.Count; i++)
                        {
                            for (int j = 0; j < owners.Length; j++)
                            {
                                if (list[i].owner == owners[j])
                                {
                                    result = list[i].value;
                                    break;
                                }
                                if (result != null)
                                    break;
                            }
                        }
                    }
                }

                return result;
            }
        }

        public object SingleValue
        {
            get
            {
                SSOVariableOwner[] array = {};
                return this[array];
            }
        }

        public List<SSOVariable> List
        {
            get
            {
                return list;
            }
        }
    }
}
