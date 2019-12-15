/*
 * instruction:	add instruction here...
 * author:		lizhixiong-pc
 * create time:	Thursday, August 1st 2019, 2:42:58 pm
 */

using XLua;
using UnityEngine;
using System;
using System.Collections.Generic;

namespace Gear.Runtime.UI
{
    public class GBinderSimpleList : GBinder
    {
        private int m_ItemCount;
        private IList<GItem> m_Items;
        private Action<LuaTable, GameObject> m_OnItemCreate;
        private DelegateGameObjectInt m_OnItemInit, m_OnItemClick;
        public override void Bind(LuaTable table)
        {
            base.Bind(table);

            m_OnItemCreate = Table.Get<Action<LuaTable, GameObject>>("OnItemCreate");
            m_OnItemInit = Table.Get<DelegateGameObjectInt>("OnItemInit");
            m_OnItemClick = Table.Get<DelegateGameObjectInt>("OnItemClick");

            var tag = gameObject.name;
            var idx = tag.IndexOf("_Simple") + 7;
            var n = tag.Substring(idx, tag.Length - idx);
            m_ItemCount = int.Parse(n);

            m_Items = new List<GItem>(m_ItemCount);

            var template = transform.GetChild(0).gameObject;
            var templateItem = template.GetComponent<GItem>();
            if (templateItem == null)
            {
                templateItem = template.AddComponent<GItem>();
                templateItem.Register(OnItemInit, OnItemClick);
            }

            m_Items.Add(templateItem);

            for (int i = 1; i < m_ItemCount; i++)
            {
                var dup = GameObject.Instantiate(template);
                dup.transform.SetParent(transform);
                dup.transform.localScale = Vector3.one;
                dup.transform.localRotation = Quaternion.identity;
                var itc = dup.GetComponent<GItem>();
                if (itc == null)
                    itc = dup.AddComponent<GItem>();
                itc.Register(OnItemInit, OnItemClick);
                m_Items.Add(itc);
            }

            foreach (var itm in m_Items)
                OnItemCreate(itm.gameObject);
        }

        public void UpdateItem()
        {
            for (int i = 0; i < m_Items.Count; i++)
                m_Items[i].UpdateItem(i);
        }

        private void OnItemCreate(GameObject go)
        {
            m_OnItemCreate(Table, go);
        }
        private void OnItemInit(GameObject go, int index)
        {
            m_OnItemInit(Table, go, index);
        }
        private void OnItemClick(GameObject go, int index)
        {
            m_OnItemClick(Table, go, index);
        }

        protected override void Unbind()
        {
            m_ItemCount = 0;
            m_Items.Clear();
            m_OnItemCreate = null;
            m_OnItemInit = null;
            m_OnItemClick = null;
            base.Unbind();
        }
    }
}