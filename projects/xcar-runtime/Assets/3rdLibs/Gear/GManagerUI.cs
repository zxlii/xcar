using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.UI;
public class GManagerUI
{
    /*
    UI层级-界面顶替关系
    https://blog.uwa4d.com/archives/TechSharing_146.html
     */
    private static Dictionary<string, GameObject> s_Cache;

    private static Transform m_DebugRoot;
    public static Transform DebugRoot
    {
        get
        {
            if (m_DebugRoot == null)
                m_DebugRoot = GameObject.Find("UIRoot/CanvasRoot/CanvasDebug").transform;
            return m_DebugRoot;
        }
    }
    public static void AppendLog(string log)
    {
        var txtLog = DebugRoot.GetComponentInChildren<Text>();
        var builder = new StringBuilder(txtLog.text);
        builder.AppendLine(log);
        txtLog.text = builder.ToString();
    }
}