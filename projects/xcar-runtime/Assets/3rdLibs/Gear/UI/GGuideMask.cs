/*
 * instruction:	add instruction here...
 * author:		lizhixiong-pc
 * create time:	Wednesday, September 11th 2019, 5:38:59 pm
 */
using UnityEngine;
using UnityEngine.UI;
namespace Gear.Runtime.UI
{
    [RequireComponent(typeof(Image))]
    public class GGuideMask : GUIBehaviour, ICanvasRaycastFilter
    {
        private Transform m_Target;
        private Image m_Image;
        private Material m_Material;
        private Canvas m_RelatedCanvas;
        private Bounds m_Bounds;
        protected override void Awake()
        {
            m_Image = GetComponent<Image>();
            m_Material = m_Image.material;
            m_RelatedCanvas = GetLatestCanvas();
            SetGuideTarget(Vector4.zero, true, 0);
        }

        public void SetGuideTarget(RectTransform rect, bool isRect, int gradient)
        {
            var corners = new Vector3[4];
            rect.GetWorldCorners(corners);
            var crect = m_RelatedCanvas.transform as RectTransform;
            var p1 = GameUtil.WorldToLocalRectPosition(corners[0], crect);
            var p2 = GameUtil.WorldToLocalRectPosition(corners[2], crect);

            m_Bounds.SetMinMax(p1, p2);

            var info = new Vector4();
            if (isRect)
            {
                info.Set(p1.x, p1.y, p2.x, p2.y);
            }
            else
            {
                var offset = (p2 - p1) * 0.5f;
                var center = p1 + offset;
                info.Set(center.x, center.y, offset.magnitude, 0);
            }

            SetGuideTarget(info, isRect, gradient);
        }

        public void SetGuideTarget(Vector4 info, bool isRect, int gradient)
        {
            m_Material.SetFloat("_IsRect", isRect ? 1 : 0);
            m_Material.SetVector("_Params", info);
            m_Material.SetInt("_Gradient", gradient);
        }

        private Canvas GetLatestCanvas()
        {
            var node = transform;
            while (node != null)
            {
                var canvas = node.GetComponent<Canvas>();
                if (canvas != null)
                    return canvas;
                node = node.parent;
            }
            return null;
        }

        public bool IsRaycastLocationValid(Vector2 sp, Camera eventCamera)
        {
            var rect = m_RelatedCanvas.transform as RectTransform;
            Vector2 local;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(rect, sp, eventCamera, out local);
            return !m_Bounds.Contains(local);
        }
    }
}