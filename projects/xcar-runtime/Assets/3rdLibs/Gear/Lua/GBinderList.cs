/*
 * instruction:	add instruction here...
 * author:		lizhixiong-pc
 * create time:	Thursday, August 1st 2019, 2:42:58 pm
 */

using XLua;
using UnityEngine;
using System;
using UnityEngine.UI;

namespace Gear.Runtime.UI
{
    [CSharpCallLua]
    public delegate void DelegateGameObjectInt(LuaTable t, GameObject s, int b);
    [RequireComponent(typeof(ScrollRect))]
    [RequireComponent(typeof(RectMask2D))]
    public class GBinderList : GBinder
    {
        private GList m_List;
        private Action<LuaTable, GameObject> m_OnItemCreate;
        private DelegateGameObjectInt m_OnItemInit, m_OnItemClick;
        public override void Bind(LuaTable table)
        {
            base.Bind(table);

            m_OnItemCreate = Table.Get<Action<LuaTable, GameObject>>("OnItemCreate");
            m_OnItemInit = Table.Get<DelegateGameObjectInt>("OnItemInit");
            m_OnItemClick = Table.Get<DelegateGameObjectInt>("OnItemClick");

            var grid = GetComponent<GridLayoutGroup>();
            var rectGrid = grid.transform as RectTransform;
            var width = rectGrid.sizeDelta.x;
            var padding = grid.padding.left + grid.padding.right;
            var spacing = grid.spacing.x;
            var cell = grid.cellSize.x;
            var noc = Mathf.RoundToInt((width + spacing - padding) / (cell + spacing));
            width = rectGrid.sizeDelta.y;
            padding = grid.padding.top + grid.padding.bottom;
            spacing = grid.spacing.y;
            cell = grid.cellSize.y;
            var nor = Mathf.RoundToInt((width + spacing - padding) / (cell + spacing));

            var container = transform.Find("container");
            var scroll = GetComponent<ScrollRect>();
            scroll.content = container as RectTransform;

            m_List = container.gameObject.AddComponent<GList>();
            m_List.NumberOfRow = nor;
            m_List.NumberOfColumn = noc;
            m_List.Spacing = grid.spacing;
            m_List.CurrentDirection = grid.startAxis;
            m_List.m_Cell = transform.GetChild(0) as RectTransform;
            m_List.m_Cell.sizeDelta = grid.cellSize;
            m_List.OnCreateItem = OnItemCreate;
            m_List.OnInitItem = OnItemInit;
            m_List.OnClickItem = OnItemClick;
            m_List.Init();

            Destroy(grid);
            Destroy(container.GetComponent<LayoutElement>());
        }

        public void SetCount(int count)
        {
            m_List.SetCount(count);
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
            m_OnItemCreate = null;
            m_OnItemInit = null;
            m_OnItemClick = null;
            base.Unbind();
        }
    }
}