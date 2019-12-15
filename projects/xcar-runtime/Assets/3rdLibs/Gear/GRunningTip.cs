using UnityEngine;
using UnityEngine.UI;
using System;
using DG.Tweening;
using UnityEditor;

namespace Gear.Runtime.UI
{
    public class GRunningTip : GUIBehaviour
    {
        private static Font _DefaultFont;
        private readonly static ObjectPool<GameObject> _TipPool = new ObjectPool<GameObject>(null, o => o.SetActive(false), m => Destroy(m), 3);
        private GRunningTipModel _Model;

        public void SetData(object data)
        {
            _Model = data as GRunningTipModel;
            if (!IsActive())
                gameObject.SetActive(true);
            rectTransform.SetParent(GRunningTipModel.TipRoot, false);

            var startPos = _Model.GetStartPos() + Vector3.down * 500;
            startPos.z = 0;
            rectTransform.anchoredPosition3D = startPos;
            rectTransform.anchorMax = rectTransform.anchorMin = rectTransform.pivot = Vector2.one * 0.5f;
            Text txt = gameObject.GetComponent<Text>();
            if (txt == null)
                txt = gameObject.AddComponent<Text>();
            txt.raycastTarget = false;
            txt.font = _Model._Font;
            txt.fontSize = _Model._FontSize;
            txt.text = _Model.GetHtmlText();
            // txt.fontStyle = FontStyle.Bold;

            // var outline = gameObject.AddComponent<Outline>();
            // Color c;
            // if (ColorUtility.TryParseHtmlString("#573F0D", out c))
            // {
            //     c.a = 0.6f;
            //     outline.effectColor = c;
            //     var dis = Vector2.one * 3;
            //     outline.effectDistance = dis;
            // }

            ContentSizeFitter fitter = gameObject.GetComponent<ContentSizeFitter>();
            if (fitter == null)
                fitter = gameObject.AddComponent<ContentSizeFitter>();
            fitter.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;
            fitter.verticalFit = _Model._MultyLine ? ContentSizeFitter.FitMode.PreferredSize : ContentSizeFitter.FitMode.Unconstrained;
            Invoke("DoRun", _Model._Delay);
        }
        private void DoRun()
        {
            Graphic graphic = GetComponent<Graphic>();
            Color newColor = graphic.color;
            newColor.a = 0f;
            graphic.CrossFadeColor(newColor, _Model._Duration * .7f, true, true);

            rectTransform.DOAnchorPosY(rectTransform.anchoredPosition.y + _Model._Offset, _Model._Duration).OnComplete(TweenComplete).SetEase(_Model._EaseType);
        }
        private void TweenComplete()
        {
            _TipPool.Release(gameObject);
        }
        public static void Show(string text, bool useDefaultPos = true, Vector2 pos = default(Vector2), string color = "#FAB627", float offset = 500f, float delay = 1.5f, float duration = 1f, Ease easeType = Ease.InQuint, Font mFont = null, int fontSize = 48, bool multyLine = true)
        {
            GRunningTipModel model = new GRunningTipModel();
            model._UseDefaultPos = useDefaultPos;
            model._Pos = pos;
            model._Text = text;
            model._Color = color;
            model._MultyLine = multyLine;
            model._Offset = offset;
            model._Delay = delay;
            model._Duration = duration;
            model._FontSize = fontSize;
            model._EaseType = easeType;

            Action<Font> showAction = font =>
            {
                model._Font = font;
                Show(model);
            };

            if (mFont == null)
            {
                if (_DefaultFont == null)
                {

#if UNITY_EDITOR
                    _DefaultFont = UnityEditor.AssetDatabase.LoadAssetAtPath<Font>("Assets/StandardAssets/Fonts/FZCYJ.ttf");
#else
                    _DefaultFont = Resources.GetBuiltinResource<Font>("Arial.ttf");
#endif


                    showAction(_DefaultFont);
                    //TODO lizhixiong
                    // CAssetBundleManager.AsyncLoadResource(GConfig.Instance.DefaultFontPath, font_obj =>
                    // {
                    //     _DefaultFont = font_obj as Font;
                    //     if (_DefaultFont == null)
                    //     {
                    //         Debug.LogWarning("the default font is null.");
                    //         return;
                    //     }
                    //     showAction(_DefaultFont);
                    // });
                }
                else
                {
                    showAction(_DefaultFont);
                }
            }
            else
            {
                showAction(mFont);
            }
        }

        public static void Show(GRunningTipModel model)
        {
            if (model != null && model.IsValid())
            {
                GameObject obj = _TipPool.Get();
                obj.name = "_running_text";
                obj.layer = LayerMask.NameToLayer("UI");
                GRunningTip tip = obj.GetComponent<GRunningTip>();
                if (tip == null)
                    tip = obj.AddComponent<GRunningTip>();
                tip.SetData(model);

                if (obj.GetComponent<Canvas>() == null)
                {
                    var cvs = obj.AddComponent<Canvas>();
                    cvs.overrideSorting = true;
                    cvs.sortingOrder = 999;
                    cvs.sortingLayerName = "UI";
                }
            }
        }
    }





    public class GRunningTipModel
    {
        static Vector2 DEFAULT_POS = Vector2.up * 150;
        public static Font _DefaultFont;
        public bool _UseDefaultPos = true;
        public Vector3 _Pos;
        public string _Text;
        public string _Color;
        public bool _MultyLine;
        public float _Offset;
        public float _Delay;
        public float _Duration;
        public Font _Font;
        public int _FontSize;
        public Ease _EaseType = Ease.InQuint;
        private static Transform _TipRoot;
        public static Transform TipRoot
        {
            get
            {
                if (_TipRoot == null)
                    _TipRoot = GameObject.Find("UIRoot/CanvasRoot/CanvasPanel").transform;
                return _TipRoot;
            }
        }

        public Transform GetParent()
        {
            return _TipRoot;
        }
        public float GetEndValue(float startValue)
        {
            return startValue + _Offset;
        }
        //位置暂时不支持世界中的物体到UI中位置的转换
        public Vector3 GetStartPos()
        {
            if (_UseDefaultPos)
            {
                return DEFAULT_POS;
            }
            else
            {
                var startPos = TipRoot.worldToLocalMatrix.MultiplyPoint(_Pos);

                return startPos;
                // CanvasScaler canvasScaler = GameObject.Find("UIRootCanvas").gameObject.GetComponent<CanvasScaler>();
                // float offect = (Screen.width / canvasScaler.referenceResolution.x) * (1 - canvasScaler.matchWidthOrHeight) + (Screen.height / canvasScaler.referenceResolution.y) * canvasScaler.matchWidthOrHeight;
                // Vector2 a = rectTransformformUtility.WorldToScreenPoint(Camera.main, _Pos);
                // return new Vector3(a.x / offect, a.y / offect,0f);
            }
        }
        public Color GetColor()
        {
            Color rst;
            ColorUtility.TryParseHtmlString(this._Color, out rst);
            return rst;
        }
        public string GetHtmlText()
        {
            return "<color=" + _Color + ">" + this._Text + "</color>";
        }
        public bool IsValid()
        {
            return !string.IsNullOrEmpty(_Text);// && delay>=0f && delay < 60f && duration >= 0f && duration < 60f;
        }
    }
}