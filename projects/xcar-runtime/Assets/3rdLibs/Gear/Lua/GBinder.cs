using XLua;
using System;
using UnityEngine;

namespace Gear.Runtime.UI
{
    public class GBinder : MonoBehaviour
    {
        private LuaTable m_LuaTable;
        protected LuaTable Table { get { return m_LuaTable; } }
        private Action<LuaTable> m_OnDestroy;
        private Action<LuaTable> m_OnApplicationQuit;
        public virtual void Bind(LuaTable table)
        {
            m_LuaTable = table;
            table.Set("self", this);
            m_OnDestroy = table.Get<Action<LuaTable>>("OnDestroy");
            m_OnApplicationQuit = table.Get<Action<LuaTable>>("OnApplicationQuit");
        }

        private void OnDestroy()
        {
            if (m_OnDestroy != null)
            {
                m_OnDestroy(m_LuaTable);
            }
            Unbind();
        }

        private void OnApplicationQuit()
        {
            if (m_OnApplicationQuit != null)
            {
                m_OnApplicationQuit(m_LuaTable);
            }
            Unbind();
        }

        protected virtual void Unbind()
        {
            m_OnDestroy = null;
            m_OnApplicationQuit = null;
            m_LuaTable.Dispose();
        }
    }
}