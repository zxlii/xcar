using UnityEngine;
using UnityEngine.EventSystems;
using System;
namespace Gear.Runtime.UI
{
    public class GItem : GUIBehaviour, IPointerClickHandler
    {
        private int m_Index;
        private Action<GameObject, int> m_OnItemInit;
        private Action<GameObject, int> m_OnItemClick;
        public void Register(Action<GameObject, int> onInit, Action<GameObject, int> onClick)
        {
            this.m_OnItemInit = onInit;
            this.m_OnItemClick = onClick;
        }
        
        public void UpdateItem(int index)
        {
            this.m_Index = index;
            this.gameObject.name = "item-" + index;
            if (this.IsActive() && m_OnItemInit != null)
                m_OnItemInit(gameObject, index);
        }

        public virtual void OnPointerClick(PointerEventData eventData)
        {
            if (m_OnItemClick != null)
                m_OnItemClick(gameObject, m_Index);
        }
    }
}