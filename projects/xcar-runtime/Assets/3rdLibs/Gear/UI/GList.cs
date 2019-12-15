using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System;
using static UnityEngine.UI.GridLayoutGroup;

namespace Gear.Runtime.UI
{

    public class GList : GUIBehaviour
    {
        public Action<GameObject> OnCreateItem;
        public Action<GameObject, int> OnInitItem;
        public Action<GameObject, int> OnClickItem;
        [SerializeField]
        protected int m_NumberOfRow;
        public int NumberOfRow { set { m_NumberOfRow = value; } }
        [SerializeField]
        protected int m_NumberOfColumn;
        public int NumberOfColumn { set { m_NumberOfColumn = value; } }
        [SerializeField]
        public RectTransform m_Cell;
        [SerializeField]
        protected Vector2 m_Spacing = Vector2.one;
        public Vector2 Spacing { set { m_Spacing = value; } }
        [SerializeField]
        private Axis m_Direction = Axis.Vertical;
        private int m_BufferNo = 4;
        private float m_PrevPos = 0;
        private int m_CurrentIndex;//页面的第一行（列）在整个conten中的位置
        private ScrollRect m_ScrollRect;
        private int m_Count;
        protected List<GItem> m_InstantiateItems = new List<GItem>();
        public int CurrentIndex { get { return m_CurrentIndex; } set { m_CurrentIndex = value; } }
        private Vector2 Page { get { return new Vector2(m_NumberOfRow, m_NumberOfColumn); } }
        public Vector2 CellRect { get { return m_Cell != null ? m_Cell.sizeDelta : new Vector2(100, 100); } }
        public Axis CurrentDirection { get { return m_Direction; } set { m_Direction = value; } }
        public float CellScale { get { return CurrentDirection == Axis.Horizontal ? (CellRect.x + m_Spacing.x) : (CellRect.y + m_Spacing.y); } }
        public float DirectionPos { get { return CurrentDirection == Axis.Horizontal ? rectTransform.anchoredPosition.x : rectTransform.anchoredPosition.y; } }
        public Vector2 InstantiateSize
        {
            get
            {
                var rows = CurrentDirection == Axis.Horizontal ? Page.x : Page.x + (float)m_BufferNo;
                var cols = CurrentDirection == Axis.Horizontal ? Page.y + (float)m_BufferNo : Page.y;
                var size = new Vector2(rows, cols);
                return size;
            }
        }
        public int PageScale { get { return CurrentDirection == Axis.Horizontal ? (int)Page.x : (int)Page.y; } }
        public int InstantiateCount { get { return (int)InstantiateSize.x * (int)InstantiateSize.y; } }
        public float Scale { get { return CurrentDirection == Axis.Horizontal ? 1f : -1f; } }
        public float MaxPrevPos
        {
            get
            {
                float result;
                Vector2 max = getRectByNum(this.m_Count);
                if (CurrentDirection == Axis.Horizontal)
                {
                    result = max.y - Page.y;
                }
                else
                {
                    result = max.x - Page.x;
                }
                return result * CellScale;
            }
        }

        public void Init()
        {
            m_ScrollRect = GetComponentInParent<ScrollRect>();
            m_ScrollRect.horizontal = CurrentDirection == Axis.Horizontal;
            m_ScrollRect.vertical = CurrentDirection == Axis.Vertical;
            rectTransform.anchorMax = rectTransform.anchorMin = rectTransform.pivot = Vector2.up;
            if (m_Cell != null)
                m_Cell.gameObject.SetActive(false);
        }

        public void SetCount(int count)
        {
            if (m_Cell == null)
            {
                m_Cell = rectTransform.GetChild(0) as RectTransform;
                if (m_Cell == null)
                {
                    Debug.Log("请为Item制定一个模板！");
                    return;
                }
            }

            m_Cell.gameObject.SetActive(false);

            this.m_Count = count;
            this.UpdateBound();
            this.FitRealItemCount();
            this.ReuseFormat();
        }

        public void UpdateBound()
        {
            var page = getRectByNum(this.m_Count);
            var realBound = new Vector2(page.y * (CellRect.x + m_Spacing.x), page.x * (CellRect.y + m_Spacing.y));
            var scrollTransform = m_ScrollRect.transform as RectTransform;
            var bw = realBound.x < scrollTransform.sizeDelta.x ? scrollTransform.sizeDelta.x : realBound.x;
            var bh = realBound.y < scrollTransform.sizeDelta.y ? scrollTransform.sizeDelta.y : realBound.y;
            this.rectTransform.sizeDelta = new Vector2(bw, bh);
        }

        private void FitRealItemCount()
        {
            int left = this.m_Count > InstantiateCount ? InstantiateCount - m_InstantiateItems.Count : this.m_Count - m_InstantiateItems.Count;

            if (left > 0)
            {
                for (int i = 0; i < left; i++)
                {
                    m_InstantiateItems.Add(createEmptyItem());
                }
            }
            else if (left < 0)
            {
                left = -left;
                List<GItem> garbages = m_InstantiateItems.GetRange(m_InstantiateItems.Count - left, left);
                m_InstantiateItems.RemoveRange(m_InstantiateItems.Count - left, left);
                for (int num = 0; num < garbages.Count; num++)
                {
                    GameObject.Destroy(garbages[num].gameObject);
                }
            }
        }

        private void ReuseFormat()
        {
            for (int num = 0; num < m_InstantiateItems.Count; num++)
            {
                int index = m_CurrentIndex * PageScale + num;
                GItem item = m_InstantiateItems[num];
                moveItemToIndex(index, item);
            }
        }

        public void ResetListPos()
        {
            m_ScrollRect.StopMovement();
            rectTransform.anchoredPosition = Vector2.zero;
        }

        public void MoveToIndex(int index)
        {
            m_CurrentIndex = index;
            this.ReuseFormat();
        }

        private GItem createEmptyItem()
        {
            RectTransform item = UnityEngine.Object.Instantiate(m_Cell) as RectTransform;
            item.SetParent(transform, false);
            item.anchorMax = Vector2.up;
            item.anchorMin = Vector2.up;
            item.pivot = Vector2.up;
            item.anchoredPosition = Vector2.zero;
            GItem itemCon = item.GetComponent<GItem>();
            if (itemCon == null)
                itemCon = item.gameObject.AddComponent<GItem>();
            OnCreateItem(itemCon.gameObject);
            itemCon.Register(OnInitItem, OnClickItem);
            item.gameObject.SetActive(true);
            return itemCon;
        }

        private Vector2 getRectByNum(int num)
        {
            return CurrentDirection == Axis.Horizontal ?
                new Vector2(Page.x, Mathf.CeilToInt(num / Page.x)) :
                new Vector2(Mathf.CeilToInt(num / Page.y), Page.y);

        }

        void Update()
        {
            if (m_InstantiateItems.Count == 0) return;
            while (Scale * DirectionPos - m_PrevPos < -CellScale * 2)
            {
                if (m_PrevPos <= -MaxPrevPos) return;
                m_PrevPos -= CellScale;
                List<GItem> range = m_InstantiateItems.GetRange(0, PageScale);
                m_InstantiateItems.RemoveRange(0, PageScale);
                m_InstantiateItems.AddRange(range);
                for (int i = 0; i < range.Count; i++)
                    moveItemToIndex(m_CurrentIndex * PageScale + m_InstantiateItems.Count + i, range[i]);
                m_CurrentIndex++;
            }
            while (Scale * DirectionPos - m_PrevPos > -CellScale)
            {
                if (Mathf.RoundToInt(m_PrevPos) >= 0) return;
                m_PrevPos += CellScale;
                m_CurrentIndex--;
                if (m_CurrentIndex < 0) return;
                List<GItem> range = m_InstantiateItems.GetRange(m_InstantiateItems.Count - PageScale, PageScale);
                m_InstantiateItems.RemoveRange(m_InstantiateItems.Count - PageScale, PageScale);
                m_InstantiateItems.InsertRange(0, range);
                for (int i = 0; i < range.Count; i++)
                    moveItemToIndex(m_CurrentIndex * PageScale + i, range[i]);
            }
        }

        private void moveItemToIndex(int index, GItem item)
        {
            if (index >= this.m_Count) return;
            item.rectTransform.anchoredPosition = getPosByIndex(index);
            item.gameObject.SetActive(index < this.m_Count);
            item.UpdateItem(index);
        }

        private Vector2 getPosByIndex(int index)
        {
            int x, y;
            if (CurrentDirection == Axis.Horizontal)
            {
                x = index % (int)Page.x;
                y = Mathf.FloorToInt(index / Page.x);
            }
            else
            {
                x = Mathf.FloorToInt(index / Page.y);
                y = index % (int)Page.y;
            }
            return new Vector2(y * Mathf.RoundToInt(CellRect.x + m_Spacing.x), -x * Mathf.RoundToInt(CellRect.y + m_Spacing.y));
        }
    }
}