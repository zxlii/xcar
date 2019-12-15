using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
public static class GameUtil
{
    static Vector3[] s_Corners = new Vector3[4];
    static Vector3 s_Min = Vector3.zero;
    static Vector3 s_Max = Vector3.zero;
    static Bounds s_BufferBounds = new Bounds();
    static Bounds s_TargetBounds = new Bounds();
    static Material s_ImageGrayMaterial = null;
    public static Material GetImageGrayMaterial()
    {
        if (s_ImageGrayMaterial == null)
            s_ImageGrayMaterial = Resources.Load<Material>("ImageGray");
        return s_ImageGrayMaterial;
    }
    public static void SetGraphicGray(Graphic graph, bool isGray = true)
    {
        if (graph is Text)
        {
            var shadow = graph.GetComponent<Shadow>();
            if (shadow != null)
                shadow.enabled = !isGray;
            var outline = graph.GetComponent<Outline>();
            if (outline != null)
                outline.enabled = !isGray;
        }
        else
        {
            var target = isGray ? GetImageGrayMaterial() : null;
            if (graph.material != target)
                graph.material = target;
        }
    }

    public static void SetNodeGray(Transform root, bool isGray = true)
    {
        var node = root;
        var stack = new Stack<Transform>();
        stack.Push(root);
        while (stack.Count > 0)
        {
            node = stack.Pop();
            Graphic graph = node.GetComponent<Graphic>();
            if (graph != null)
                SetGraphicGray(graph, isGray);
            for (int i = 0; i < node.childCount; i++)
                stack.Push(node.GetChild(i));
        }
    }

    private static Camera GetDefaultCamer()
    {
        return GameObject.Find("Main Camera").GetComponent<Camera>();
    }

    public static Vector2 WorldToLocalRectPosition(Vector3 worldPoint, RectTransform rect)
    {
        var camera = GetDefaultCamer();
        var screenPoint = RectTransformUtility.WorldToScreenPoint(camera, worldPoint);
        Vector2 local;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(rect, screenPoint, camera, out local);
        return local;
    }

    public static void SetScrollRectListener(GameObject obj, UnityAction<Vector2> action)
    {
        var scrollRect = obj.GetComponent<ScrollRect>();
        if (scrollRect == null)
            return;
        if (action == null)
            scrollRect.onValueChanged.RemoveAllListeners();
        else
            scrollRect.onValueChanged.AddListener(action);
    }

    public static float GetTextHeightBeforeShow(Text txt, string content)
    {
        var settings = txt.GetGenerationSettings(new Vector2(txt.GetPixelAdjustedRect().size.x, 0.0f));
        var height = txt.cachedTextGeneratorForLayout.GetPreferredHeight(content, settings) / txt.pixelsPerUnit;
        return height;
    }

    private static void ClearMinMax()
    {
        for (int i = 0; i < 3; i++)
        {
            s_Min[i] = float.MaxValue;
            s_Max[i] = float.MinValue;
        }
    }

    private static Bounds GetBounds(RectTransform rootRect, RectTransform childRect)
    {
        ClearMinMax();
        childRect.GetWorldCorners(s_Corners);
        for (int j = 0; j < 4; j++)
        {
            Vector3 v = rootRect.worldToLocalMatrix.MultiplyPoint3x4(s_Corners[j]);
            s_Min = Vector3.Min(v, s_Min);
            s_Max = Vector3.Max(v, s_Max);
        }
        var bounds = new Bounds(s_Min, Vector3.zero);
        bounds.Encapsulate(s_Max);
        return bounds;
    }

    public static bool IsRectIntersects(RectTransform viewPortRect, RectTransform containerRect, float startPosition, float height, Vector2 offset)
    {
        var viewPortBounds = GetBounds(viewPortRect, viewPortRect);
        var min = viewPortBounds.min;
        var max = viewPortBounds.max;
        min.y = min.y - offset.y;
        max.y = max.y + offset.x;
        s_BufferBounds.SetMinMax(min, max);

        var containerBounds = GetBounds(viewPortRect, containerRect);
        min = containerBounds.min;
        max = containerBounds.max;
        max.y -= startPosition;
        min.y = max.y - height;
        s_TargetBounds.SetMinMax(min, max);

        return s_BufferBounds.Intersects(s_TargetBounds);
    }

    public static void OpenComment()
    {
#if UNITY_IOS || UNITY_EDITOR
        const string APP_ID = "1483005292";
        var url = string.Format("itms-apps://itunes.apple.com/app/id{0}?action=write-review", APP_ID);
        Application.OpenURL(url);
#endif
    }

    public static void OpenEmail(string email, string title, string content)
    {
        var subject = MyEscapeURL(title);
        var body = MyEscapeURL(content);
        var url = string.Format("mailto:{0}?subject={1}&body={2}", email, subject, body);
        Application.OpenURL(url);
    }

    static string MyEscapeURL(string url)
    {
        //%20是空格在url中的编码，这个方法将url中非法的字符转换成%20格式
        return WWW.EscapeURL(url).Replace("+", "%20");
    }

    static TouchScreenKeyboard keyboard;
    public static bool OpenVK(string text)
    {
        if (!TouchScreenKeyboard.isSupported)
            return false;
        keyboard = TouchScreenKeyboard.Open(text, TouchScreenKeyboardType.Default, false, true, false, false, "please input...", 50);
        return true;
    }

    public static string GetVKText()
    {
        var rst = "";
        if (keyboard != null)
            rst = keyboard.text;
        return rst;
    }

    public static int GetVKStatus()
    {
        var state = 0;
        if (keyboard != null)
            state = (int)keyboard.status;
        return state;
    }

}