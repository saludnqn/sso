using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Web.SessionState;

namespace Salud.Security.SSO
{
    [Serializable]
    public class SSOSessionStateStoreItems : NameObjectCollectionBase, ISessionStateItemCollection, ICollection, IEnumerable
    {
        private bool _dirty;
        private List<string> dirtyKeys = new List<string>();
        private List<string> loadedKeys = new List<string>();

        public bool Dirty
        {
            get
            {
                return this._dirty;
            }
            set
            {
                this._dirty = value;
            }
        }


        public List<string> DirtyKeys
        {
            get
            {
                return this.dirtyKeys;
            }
        }

        public object this[string name] {
            get
            {
                name = name.Trim();
                // Lazy load
                if (loadedKeys.IndexOf(name) == -1)
                {
                    this.BaseSet(name, LoadKey(name));
                    loadedKeys.Add(name);                    
                }

                return base.BaseGet(name);                
            }
            set
            {
                name = name.Trim();
                object currentValue = this[name];
                bool updateValue;
                // ¿uno de los dos es null (xor)?
                if (currentValue == null && value == null)
                    updateValue = false;
                else
                {
                    if (currentValue == null)
                        updateValue = true;
                    else
                    {
                        if (value == null)
                            updateValue = true;
                        else
                        {
                            // Tanto currentValue como value son diferentes a null, entonces compara los valores
                            // ... si es un tipo simple, compara
                            if (value.GetType().IsValueType || value.GetType() == typeof(String))
                                updateValue = !currentValue.Equals(value);
                            else
                                // ... si es un objecto, actualiza siempre, porque la comparación es una operación muy costosa
                                updateValue = true;
                        }
                    }
                }

                if (updateValue)
                {
                    base.BaseSet(name, value);
                    Dirty = true;
                    if (loadedKeys.IndexOf(name) == -1)
                        loadedKeys.Add(name);                    
                    if (DirtyKeys.IndexOf(name) == -1)
                        DirtyKeys.Add(name);
                }
            }
        }

        public object this[int index]
        {
            get
            {
                // Lazy load
                string name = base.BaseGetKey(index);
                if (loadedKeys.IndexOf(name) == -1)
                {
                    this.BaseSet(name, LoadKey(name));
                    loadedKeys.Add(name);
                }
                return base.BaseGet(index);
            }
            set
            {
                object currentValue = this[index];
                if ((currentValue != null && value != null && (!currentValue.Equals(value))) 
                    || (currentValue == null ^ value == null))
                {
                    base.BaseSet(index, value);
                    Dirty = true;
                    string name = base.BaseGetKey(index);
                    if (loadedKeys.IndexOf(name) == -1)
                        loadedKeys.Add(name);
                    if (DirtyKeys.IndexOf(name) == -1)
                        DirtyKeys.Add(name);
                }
            }
        }

        public SSOSessionStateStoreItems()
        {
        }

        private object LoadKey(string name)
        {
            return SSOHelper.MembershipProvider.GetVariable(SSOHelper.CurrentIdentity, name);
        }

        public void Clear()
        {
            base.BaseClear();
        }

        public void Remove(string name)
        {
            name = name.Trim();
            base.BaseRemove(name);
        }

        public void RemoveAt(int index)
        {
            base.BaseRemoveAt(index);
        }
    }
}