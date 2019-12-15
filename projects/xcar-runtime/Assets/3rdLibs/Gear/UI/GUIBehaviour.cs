/*
 * instruction:	add instruction here...
 * author:		lizhixiong-pc
 * create time:	Thursday, August 1st 2019, 11:34:45 am
 */
using UnityEngine;
using UnityEngine.EventSystems;
namespace Gear.Runtime.UI
{
    [RequireComponent(typeof(RectTransform))]
    public class GUIBehaviour : UIBehaviour
    {
        private RectTransform m_RectTransform;
        public RectTransform rectTransform
        {
            get
            {
                if (m_RectTransform == null)
                    m_RectTransform = GetComponent<RectTransform>();
                return m_RectTransform;
            }
        }
        protected override void Awake() { }
        protected override void OnEnable() { }
        protected override void OnDestroy() { }
    }
}