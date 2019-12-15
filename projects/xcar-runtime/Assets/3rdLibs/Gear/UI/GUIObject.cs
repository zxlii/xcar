/*
 * instruction:	add instruction here...
 * author:		lizhixiong-pc
 * create time:	Wednesday, July 31st 2019, 7:54:02 pm
 */
using UnityEngine;
using UnityEngine.U2D;

namespace Gear.Runtime.UI
{
    public abstract class GUIObject
    {
        private GameObject m_GameObject;
        public GameObject gameObject { get { return m_GameObject; } }
        private RectTransform m_RectTransform;
        public RectTransform rectTransform { get { return m_RectTransform; } }

        public GUIObject(GameObject gameObject)
        {
            m_GameObject = gameObject;
            m_RectTransform = gameObject.transform as RectTransform;
        }

        protected abstract void OnCreate();
        public virtual void SetData<T>(T data) { }
        protected virtual void OnDestroy()
        {
            m_GameObject = null;
            m_RectTransform = null;
        }

    }
}