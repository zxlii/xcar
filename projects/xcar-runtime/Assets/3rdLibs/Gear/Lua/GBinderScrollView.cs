/*
 * instruction:	add instruction here...
 * author:		lizhixiong-pc
 * create time:	Tuesday, August 6th 2019, 6:23:05 pm
 */


using UnityEngine;
using XLua;

namespace Gear.Runtime.UI
{
    [RequireComponent(typeof(GScrollView))]
    public class GBinderScrollView : GBinder
    {
        private GScrollView m_ScrollView;
        private DelegateGameObjectInt m_OnScrollStart, m_OnScrollEnd;
        public override void Bind(LuaTable table)
        {
            base.Bind(table);

            m_OnScrollStart = Table.Get<DelegateGameObjectInt>("OnScrollStart");
            m_OnScrollEnd = Table.Get<DelegateGameObjectInt>("OnScrollEnd");

            m_ScrollView = GetComponent<GScrollView>();
            m_ScrollView.Init(OnScrollStart, OnScrollEnd);
        }

        public void LinkBar(GameObject go)
        {
            m_ScrollView.LinkBar(go);
        }
        
        public void UnlinkBar(GameObject go)
        {
            m_ScrollView.UnlinkBar(go);
        }

        public void ChangeViewImmediately(int index)
        {
            m_ScrollView.ChangeViewImmediately(index);
        }

        public void ChangeView(int index)
        {
            m_ScrollView.ChangeView(index);
        }

        private void OnScrollStart(GameObject gameObject, int newIndex)
        {
            if (m_OnScrollStart != null)
                m_OnScrollStart(Table, gameObject, newIndex);
        }

        private void OnScrollEnd(GameObject gameObject, int newIndex)
        {
            if (m_OnScrollEnd != null)
                m_OnScrollEnd(Table, gameObject, newIndex);
        }

        protected override void Unbind()
        {
            m_OnScrollStart = null;
            m_OnScrollEnd = null;
            base.Unbind();
        }
    }
}