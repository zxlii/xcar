using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
namespace Gear.Runtime.UI
{

    [ExecuteInEditMode]
    [RequireComponent(typeof(RectTransform))]
    public class GLayout : GUIBehaviour
    {
        [SerializeField]
        protected int _Priority = 0;
        public int Priority { get { return _Priority; } set { _Priority = value; } }
        [SerializeField]
        protected Vector2 _Size = Vector2.one;//x一行多少个，y一列多少个
        public Vector2 Size { get { return _Size; } set { _Size = value; } }
        [SerializeField]
        protected Vector2 _Spacing = Vector2.one * 5;
        public Vector2 Spacing { get { return _Spacing; } set { _Spacing = value; } }
        [SerializeField]
        protected RectOffset _Padding = new RectOffset();
        public RectOffset Padding { get { return _Padding; } set { _Padding = value; } }
        [SerializeField]
        protected SortType _SortType = SortType.Priority;
        public SortType SortedType { get { return _SortType; } set { _SortType = value; } }
        [SerializeField]
        protected GridLayoutGroup.Corner _StartCorner = GridLayoutGroup.Corner.UpperLeft;
        public GridLayoutGroup.Corner StartCorner { get { return _StartCorner; } set { _StartCorner = value; } }
        [SerializeField]
        protected GridLayoutGroup.Axis _StartAxis = GridLayoutGroup.Axis.Horizontal;
        public GridLayoutGroup.Axis StartAxis { get { return _StartAxis; } set { _StartAxis = value; } }

        protected List<GLayout> _ChildNodes = new List<GLayout>();

        public void LayoutChange()
        {
            Transform parent = transform.parent;
            if (parent == null)
            {
                Layout();
            }
            else
            {
                GLayout parentLayout = parent.GetComponent<GLayout>();
                if (parentLayout == null || !parentLayout.enabled)
                {
                    Layout();
                }
                else
                {
                    parentLayout.LayoutChange();
                }
            }
        }
        public void Layout()
        {
            UpdateChildren();
            SortChildren();
            LayoutChildren();
            // CheckGText();
        }
        // private void CheckGText()
        // {
        //     GText gt = GetComponent<GText>();
        //     if (gt)
        //     {
        //         gt.FormatBoard();
        //     }
        // }
        public virtual void UpdateChildren()
        {
            Vector2 anchor = Vector2.zero;
            if (_StartCorner == GridLayoutGroup.Corner.UpperLeft)
            {
                anchor = Vector2.up;
            }
            else if (_StartCorner == GridLayoutGroup.Corner.UpperRight)
            {
                anchor = Vector2.one;
            }
            else if (_StartCorner == GridLayoutGroup.Corner.LowerRight)
            {
                anchor = Vector2.right;
            }
            else if (_StartCorner == GridLayoutGroup.Corner.LowerLeft)
            {
                anchor = Vector2.zero;
            }

            _ChildNodes.Clear();
            int childCount = transform.childCount;
            int i;
            for (i = 0; i < childCount; i++)
            {
                Transform child = transform.GetChild(i);
                GLayout layout = child.GetComponent<GLayout>();
                if (!child.gameObject.activeSelf || layout == null || layout.rectTransform.sizeDelta.x <= 0 || layout.rectTransform.sizeDelta.y <= 0) continue;
                layout.rectTransform.anchorMax = layout.rectTransform.anchorMin = layout.rectTransform.pivot = anchor;
                _ChildNodes.Add(layout);
            }
        }
        public virtual void SortChildren()
        {
            if (_SortType == SortType.Position)
            {
                if (_StartAxis == GridLayoutGroup.Axis.Horizontal)
                {
                    switch (_StartCorner)
                    {
                        case GridLayoutGroup.Corner.UpperLeft:
                        case GridLayoutGroup.Corner.LowerLeft:
                            _ChildNodes.Sort((v1, v2) => v1.rectTransform.anchoredPosition.x.CompareTo(v2.rectTransform.anchoredPosition.x));
                            break;
                        case GridLayoutGroup.Corner.UpperRight:
                        case GridLayoutGroup.Corner.LowerRight:
                            _ChildNodes.Sort((v1, v2) => v2.rectTransform.anchoredPosition.x.CompareTo(v1.rectTransform.anchoredPosition.x));
                            break;
                    }
                }
                else if (_StartAxis == GridLayoutGroup.Axis.Vertical)
                {
                    switch (_StartCorner)
                    {
                        case GridLayoutGroup.Corner.UpperLeft:
                        case GridLayoutGroup.Corner.UpperRight:
                            _ChildNodes.Sort((v1, v2) => v2.rectTransform.anchoredPosition.y.CompareTo(v1.rectTransform.anchoredPosition.y));
                            break;
                        case GridLayoutGroup.Corner.LowerLeft:
                        case GridLayoutGroup.Corner.LowerRight:
                            _ChildNodes.Sort((v1, v2) => v1.rectTransform.anchoredPosition.y.CompareTo(v2.rectTransform.anchoredPosition.y));
                            break;
                    }
                }
            }
            else if (_SortType == SortType.Priority)
            {
                _ChildNodes.Sort((v1, v2) => v1.Priority.CompareTo(v2.Priority));
            }
        }
        public virtual void LayoutChildren()
        {
            if (_ChildNodes.Count <= 0) return;
            float fx = 0;
            float fy = 0;
            int paddingx1 = 0;
            int paddingx2 = 0;
            int paddingy1 = 0;
            int paddingy2 = 0;
            if (_StartCorner == GridLayoutGroup.Corner.UpperLeft)
            {
                fx = 1;
                fy = -1;
                paddingx1 = _Padding.left;
                paddingx2 = _Padding.right;
                paddingy1 = _Padding.top;
                paddingy2 = _Padding.bottom;
            }
            else if (_StartCorner == GridLayoutGroup.Corner.UpperRight)
            {
                fx = -1;
                fy = -1;
                paddingx1 = _Padding.right;
                paddingx2 = _Padding.left;
                paddingy1 = _Padding.top;
                paddingy2 = _Padding.bottom;
            }
            else if (_StartCorner == GridLayoutGroup.Corner.LowerRight)
            {
                fx = -1;
                fy = 1;
                paddingx1 = _Padding.right;
                paddingx2 = _Padding.left;
                paddingy1 = _Padding.bottom;
                paddingy2 = _Padding.top;

            }
            else if (_StartCorner == GridLayoutGroup.Corner.LowerLeft)
            {
                fx = 1;
                fy = 1;
                paddingx1 = _Padding.left;
                paddingx2 = _Padding.right;
                paddingy1 = _Padding.bottom;
                paddingy2 = _Padding.top;
            }

            int i;
            int count;
            int index;
            float width = 0;
            float height = 0;
            float offset = 0;
            float x;
            float y;
            float max;
            if (_StartAxis == GridLayoutGroup.Axis.Horizontal)
            {
                count = Mathf.RoundToInt(_Size.x);
                x = paddingx1 * fx;
                y = paddingy1 * fy;
                for (i = 0; i < _ChildNodes.Count; i++)
                {
                    index = i % count;
                    GLayout layout = _ChildNodes[i];
                    layout.Layout();
                    layout.rectTransform.anchoredPosition = new Vector2(x, y);

                    max = Mathf.Abs(x) + layout.rectTransform.sizeDelta.x;
                    if (max > width)  //最宽的那一行的宽度
                    {
                        width = max;
                    }
                    if (layout.rectTransform.sizeDelta.y > offset)  //一行中最高的那一个
                    {
                        offset = layout.rectTransform.sizeDelta.y;
                    }
                    if (index < count - 1)
                    {
                        x += layout.rectTransform.sizeDelta.x * fx;
                        x += _Spacing.x * fx;
                    }
                    else if (index == count - 1)
                    {
                        x = paddingx1 * fx;
                        y += offset * fy;
                        if (i < _ChildNodes.Count - 1)
                            y += _Spacing.y * fy;
                        offset = 0;
                    }
                }
                width += paddingx2;
                y += offset * fy;
                y += paddingy2 * fy;
                height = Mathf.Abs(y);
                SetSize(width, height);
            }
            else if (_StartAxis == GridLayoutGroup.Axis.Vertical)
            {
                count = Mathf.RoundToInt(_Size.y);
                x = paddingx1 * fx;
                y = paddingy1 * fy;
                for (i = 0; i < _ChildNodes.Count; i++)
                {
                    index = i % count;
                    GLayout layout = _ChildNodes[i];
                    layout.Layout();
                    layout.rectTransform.anchoredPosition = new Vector2(x, y);

                    max = Mathf.Abs(y) + layout.rectTransform.sizeDelta.y;
                    if (max > height)  //最高的那一行的高度
                    {
                        height = max;
                    }
                    if (layout.rectTransform.sizeDelta.x > offset)  //一列中最宽的那一个
                    {
                        offset = layout.rectTransform.sizeDelta.x;
                    }
                    if (index < count - 1)
                    {
                        y += layout.rectTransform.sizeDelta.y * fy;
                        y += _Spacing.y * fy;
                    }
                    else if (index == count - 1)
                    {
                        y = paddingy1 * fy;
                        x += offset * fx;
                        if (i < _ChildNodes.Count - 1)
                            x += _Spacing.x * fx;
                        offset = 0;
                    }
                }
                height += paddingy2;
                x += offset * fx;
                x += paddingx2 * fx;
                width = Mathf.Abs(x);
                SetSize(width, height);
            }
        }
        private void SetSize(float width, float height)
        {
            GLayoutSizeRestriction restriction = GetComponent<GLayoutSizeRestriction>();
            if (restriction != null)
            {
                Vector2 size = restriction.GetRestrictionSize();
                if (width > size.x)
                {
                    width = size.x;
                }
                if (height > size.y)
                {
                    height = size.y;
                }
                rectTransform.sizeDelta = new Vector2(width, height);
            }
            else
            {
                rectTransform.sizeDelta = new Vector2(width, height);
            }
        }
    }
}