
using UnityEngine;
namespace Gear.Runtime.UI
{
    [ExecuteInEditMode]
    [RequireComponent(typeof(GLayout))]
    public class GLayoutSizeRestriction : GUIBehaviour
    {
        [SerializeField]
        private bool _UseDefaultSize = true;
        [SerializeField]
        private Vector2 _Value = Vector2.zero;
        private Vector2 _FirstGetter = Vector2.zero;
        public Vector2 GetRestrictionSize()
        {
            if (_UseDefaultSize)
            {
                if (_FirstGetter == Vector2.zero)
                    _FirstGetter = rectTransform.sizeDelta;
                return _FirstGetter;
            }
            else
            {
                return _Value;
            }
        }
    }
}