/*
 * instruction:	add instruction here...
 * author:		lizhixiong-pc
 * create time:	Tuesday, August 6th 2019, 5:49:51 pm
 */

using UnityEngine;
using UnityEngine.UI;
using static UnityEngine.RectTransform;
using System;
using System.Collections.Generic;
using DG.Tweening;

namespace Gear.Runtime.UI
{
    [RequireComponent(typeof(RectMask2D))]
    public class GScrollView : GUIBehaviour
    {
        static Vector2 s_Pos = Vector2.zero;
        readonly static Dictionary<string, Edge> s_Anchors = new Dictionary<string, Edge> { { "Top", Edge.Top }, { "Bottom", Edge.Bottom }, { "Left", Edge.Left }, { "Right", Edge.Right } };
        const string CONTENT = "_content";
        private int m_ViewIndex = -1;
        private int m_ViewIndexNext = 0;
        private int m_ViewCount = 0;
        private Edge m_StartSide = Edge.Left;
        private Vector2 m_ViewPortSize;
        private IList<GameObject> m_Views;
        private RectTransform m_ContentRect;
        private Action<GameObject, int> m_OnScrollStart, m_OnScrollEnd;
        private Action<int, int> m_OnViewChange;
        [SerializeField]
        private ToggleGroup m_Group;
        private GToggleBar m_Bar;
        protected override void Awake()
        {
            var suffix = name.Split('_')[1];
            if (string.IsNullOrEmpty(suffix) || !s_Anchors.TryGetValue(suffix, out m_StartSide))
            {
                m_StartSide = Edge.Left;
                Debug.LogError("View组件命名有误:后缀TBLR分别表示Top、Bottom、Left、Right");
            }

            m_ViewCount = rectTransform.childCount;
            m_ViewPortSize = rectTransform.sizeDelta;
            m_Views = new List<GameObject>(m_ViewCount);
            for (int i = 0; i < m_ViewCount; i++)
                m_Views.Add(rectTransform.GetChild(i).gameObject);

            // 初始化container
            var trans = rectTransform.Find(CONTENT);
            if (trans == null)
            {
                trans = new GameObject(CONTENT).transform;
                trans.SetParent(rectTransform, false);
                trans.localScale = Vector3.one;
                trans.localRotation = Quaternion.identity;
            }
            m_ContentRect = trans.gameObject.AddComponent<RectTransform>();
            m_ContentRect.pivot = m_ContentRect.anchorMin = m_ContentRect.anchorMax = Vector2.up;

            var contentSize = m_ViewPortSize;
            switch (m_StartSide)
            {
                case Edge.Left:
                case Edge.Right:
                    contentSize.x = m_ViewCount * m_ViewPortSize.x;
                    break;
                case Edge.Top:
                case Edge.Bottom:
                    contentSize.y = m_ViewCount * m_ViewPortSize.y;
                    break;

            }
            m_ContentRect.sizeDelta = contentSize;
            m_ContentRect.anchoredPosition = CalculateContentPosition(m_ViewIndex);

            // 初始化view
            for (int i = 0; i < m_ViewCount; i++)
            {
                var child = m_Views[i].transform;
                child.SetParent(m_ContentRect, false);
                child.localScale = Vector3.one;
                child.localRotation = Quaternion.identity;

                var childRect = child as RectTransform;
                childRect.pivot = childRect.anchorMin = childRect.anchorMax = Vector2.up;
                childRect.anchoredPosition = -CalculateContentPosition(i);
                childRect.sizeDelta = m_ViewPortSize;
            }
        }

        public void Init(Action<GameObject, int> scrollStart, Action<GameObject, int> scrollEnd)
        {
            m_OnScrollStart = scrollStart;
            m_OnScrollEnd = scrollEnd;
        }


        public void LinkBar(GameObject go)
        {
            m_Bar = go.GetComponent<GToggleBar>();
            if (m_Bar == null)
                m_Bar = go.AddComponent<GToggleBar>();
            m_Bar.Register(ChangeView);
        }

        public void UnlinkBar(GameObject go)
        {
            if (m_Bar == null) return;
            m_Bar.Unregister();
        }

        public void UpdateBarIndex()
        {
            if (m_Bar == null) return;
            m_Bar.Select(m_ViewIndexNext, true);
        }

        public void ChangeView(int index)
        {
            if (index < 0 || index > m_ViewCount - 1)
            {
                Debug.LogError("GScrollView::ChangeView(index) => index out of range.");
                return;
            }
            if (m_ViewIndex == index)
                return;
            // if (DOTween.IsTweening(m_ContentRect) && m_ViewIndexNext == index)
            //     return;

            m_ViewIndexNext = index;
            var targetPos = CalculateContentPosition(index);
            m_ContentRect.DOAnchorPos(targetPos, .5f)
            .SetEase(Ease.OutQuart)
            .OnStart(TweenStart)
            .OnComplete(TweenComplete);
        }

        public void ChangeViewImmediately(int index)
        {
            if (index < 0 || index > m_ViewCount - 1)
            {
                Debug.LogError("GScrollView::ChangeViewImmediately(index) => index out of range.");
                return;
            }
            if (m_ViewIndex == index)
                return;

            m_ViewIndex = m_ViewIndexNext = index;
            var targetPos = CalculateContentPosition(index);
            m_ContentRect.anchoredPosition = targetPos;

            TweenStart();
        }

        private void TweenStart()
        {
            if (m_OnScrollStart != null)
            {
                var nextObject = m_Views[m_ViewIndexNext];
                m_OnScrollStart(nextObject, m_ViewIndexNext);
            }
        }

        private void TweenComplete()
        {
            var old = m_ViewIndex;
            m_ViewIndex = m_ViewIndexNext;

            if (m_OnScrollEnd != null)
            {
                var nextObject = m_Views[m_ViewIndexNext];
                m_OnScrollEnd(nextObject, m_ViewIndexNext);
            }

            if (m_OnViewChange != null)
                m_OnViewChange(old, m_ViewIndexNext);

            UpdateBarIndex();
        }

        private Vector2 CalculateContentPosition(int index)
        {
            index = Mathf.Max(index, 0);
            s_Pos.Set(0, 0);
            var contentSize = m_ContentRect.sizeDelta;
            switch (m_StartSide)
            {
                case Edge.Top:
                    s_Pos.y = m_ViewPortSize.y * index;
                    break;
                case Edge.Bottom:
                    s_Pos.y = contentSize.y - m_ViewPortSize.y * (index + 1);
                    break;
                case Edge.Left:
                    s_Pos.x = -m_ViewPortSize.x * index;
                    break;
                case Edge.Right:
                    s_Pos.x = -contentSize.x + m_ViewPortSize.x * (index + 1);
                    break;
            }
            return s_Pos;
        }
    }
}
