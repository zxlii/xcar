using UnityEngine;
using UnityEditor;
using System.IO;
using System.Collections.Generic;
using Gear.Runtime;
using UnityEngine.UI;

public class GNodeOpera
{
    // 组件
    private static Dictionary<string, string> c_TypeMap = new Dictionary<string, string> {

        { "obj", "GameObject" },
        { "tra", "Transform" },
        { "rec", "RectTransform" },
        //u3d 内置组件
        { "img", "Image" },
        { "raw", "RawImage" },
        { "txt", "Text" },
        { "btn", "Button" },
        { "ipt", "InputField" },
        { "tog", "Toggle" },
        { "scb", "Scrollbar" },
        { "sld", "Slider" },
        { "dop", "Dropdown" },
        { "skr", "ScrollRect" },
        //自定义组件g开头 
        { "gbtn", "GButton"},
        { "glst", "GList" },
        { "gtgb", "GToggleBar" },
        { "gviw", "GView" },
        { "gbld", "GBloodBar" }
    };

    //容器
    private static Dictionary<string, string> c_ContianerMap = new Dictionary<string, string> {
        { "panel", "Pane" },
        { "gitem", "Item" },
        { "gview", "View" },
        { "gctnr", "Container"}
    };

    //需要忽略注入的组件
    private static HashSet<string> s_InjectIgnores = new HashSet<string> { 
        // "btn", 
        // "tog",
        "scb",
        "sld",
        "dop",
        "skr"
        };

    const string DefaultBackName = "btnPanelBackground_Default";

    private Transform m_OriginRoot;
    private Transform m_Root;
    private string m_CodePath;
    private string m_PrefabPath;
    private BuildTarget m_Target;
    private string m_rootName = string.Empty;
    private string rootName
    {
        get
        {
            if (string.IsNullOrEmpty(m_rootName))
                m_rootName = m_Root.name;
            return m_rootName;
        }
    }
    private string[] param { get { return EditorUtil.TranslateUIElemete(rootName); } }
    private string nodeType { get { return param[0]; } }
    private string nodeName { get { return param[1]; } }
    private string usedName { get { return c_ContianerMap[nodeType] + nodeName; } }
    private string pathPrefabFile
    {
        get
        {
            return
                m_PrefabPath +
                Path.AltDirectorySeparatorChar +
                nodeName +
                Path.AltDirectorySeparatorChar +
                rootName + ".prefab";
        }
    }
    private GDescriptor m_Descriptor = null;
    private List<string> m_ButtonNames = new List<string>();

    public GNodeOpera(Transform root, Transform origin, string codePath, string prefabPath, BuildTarget target = BuildTarget.NoTarget)
    {
        m_OriginRoot = root;
        m_Root = origin;
        m_CodePath = codePath;
        m_PrefabPath = prefabPath;
        m_Target = target;
    }

    public void Execute()
    {
        if (!IsValidName(rootName, false)) return;
        InitRoot();
        CreateDefaultPanelBackground();
        TreeWalk(m_Root);
        CodeGen();
        SavePrefab();
        AssetDatabase.Refresh();
    }

    private void InitRoot()
    {
        m_Descriptor = m_Root.GetComponent<GDescriptor>();
        if (m_Descriptor == null)
            m_Descriptor = m_Root.gameObject.AddComponent<GDescriptor>();
        m_Descriptor.Clear();
    }

    private void CreateDefaultPanelBackground()
    {
        if (this.nodeType.Equals("panel"))
        {
            var trans = m_Root.Find(DefaultBackName);
            if (trans != null) return;

            var obj = new GameObject(DefaultBackName);
            obj.transform.SetParent(m_Root, false);
            var img = obj.AddComponent<Image>();
            img.color = new Color(0, 0, 0, .5f);

            var rootRect = m_Root as RectTransform;
            img.rectTransform.sizeDelta = rootRect.sizeDelta;
            img.rectTransform.anchorMin = Vector2.zero;
            img.rectTransform.anchorMax = Vector2.one;
            img.rectTransform.offsetMin = Vector2.zero;
            img.rectTransform.offsetMax = Vector2.zero;
            img.rectTransform.SetAsFirstSibling();

            var btn = obj.AddComponent<Button>();
            btn.transition = Selectable.Transition.None;

            // PrefabUtility.ApplyPrefabInstance(m_Root.gameObject, InteractionMode.UserAction);
        }
    }

    private void TreeWalk(Transform root)
    {
        var node = root;
        var queue = new Queue<Transform>();
        queue.Enqueue(node);
        while (queue.Count > 0)
        {
            node = queue.Dequeue();

            var nodeName = node.name;
            var param = EditorUtil.TranslateUIElemete(node.name);
            var prefix = param[0];

            var isRoot = node == root;
            var isValidName = IsValidName(nodeName);
            var isContainer = c_ContianerMap.ContainsKey(prefix);

            if (!isRoot && isValidName)
            {
                if (isContainer)
                    ProcessContainer(node, prefix); // 如果是容器则，就停止迭代。把以当前节点为跟的子树 当成单数的一颗树来处理。
                else
                    ProcessComponent(node, prefix); // 如果是组件则，把组件信息收集，等生成代码时使用。
            }

            if (isRoot || !isContainer)
            {
                foreach (Transform child in node)
                    queue.Enqueue(child);
            }
        }
    }

    private void ProcessComponent(Transform node, string prefix)
    {
        var isIgnore = s_InjectIgnores.Contains(prefix);
        var isButton = prefix.Equals("btn");
        var isDefaultBackButton = node.name.Equals(DefaultBackName);

        if (!isIgnore)
            m_Descriptor.AddObject(node.gameObject);

        if (isButton && !isDefaultBackButton)
            m_ButtonNames.Add(node.name);
    }
    private void ProcessContainer(Transform node, string prefix)
    {
        if (prefix.Equals("gitem"))
        {
            var parent = node.parent;
            if (!parent.name.Contains("_Simple"))
                FormatList(parent as RectTransform, node);

            if (!node.name.Contains("_Easy"))
                new GNodeOpera(m_OriginRoot, node, m_CodePath, m_PrefabPath, m_Target).Execute();
        }
        else if (prefix.Equals("gview"))
        {
            new GNodeOpera(m_OriginRoot, node, m_CodePath, m_PrefabPath, m_Target).Execute();
        }
        else if (prefix.Equals("gctnr"))
        {
            ProcessComponent(node, prefix);
            new GNodeOpera(m_OriginRoot, node, m_CodePath, m_PrefabPath, m_Target).Execute();
        }
    }

    private void FormatList(RectTransform list, Transform firstChild)
    {
        var grid = list.GetComponent<GridLayoutGroup>();
        if (grid == null)
        {
            Debug.LogErrorFormat("请为{0}添加GridLayoutGroup，并调整参数！");
            return;
        }
        grid.enabled = false;
        const string containerName = "container";
        var content = list.Find(containerName) as RectTransform;
        if (content == null)
        {
            var obj = new GameObject(containerName);
            content = obj.AddComponent<RectTransform>();
            content.SetParent(list, false);
            content.localPosition = Vector3.zero;
            content.localScale = Vector3.one;
            content.localRotation = Quaternion.identity;
            content.sizeDelta = list.sizeDelta;
            content.pivot = content.anchorMin = content.anchorMax = Vector2.one * .5f;

            var ele = obj.AddComponent<LayoutElement>();
            ele.ignoreLayout = true;
            ele.enabled = false;
        }

        var mask = list.GetComponent<RectMask2D>();
        if (mask == null)
            list.gameObject.AddComponent<RectMask2D>();

        var ele1 = content.GetComponent<LayoutElement>();
        ele1.enabled = true;
        grid.enabled = true;
        // PrefabUtility.ApplyPrefabInstance(m_Root.gameObject, InteractionMode.UserAction);
    }

    private void CodeGen()
    {
        new GCodeGenOpera(m_OriginRoot, m_Descriptor, m_CodePath, nodeType, nodeName, m_ButtonNames).Execute();
    }

    private void SavePrefab()
    {
        if (File.Exists(pathPrefabFile))
            if (!EditorUtility.DisplayDialog("提示", usedName + ".prefab文件已经存在，是否要重新生成？", "是", "否"))
                return;

        // Debug.Log(string.Format("node type = {0}", nodeType));
        if (!this.nodeType.Equals("panel")) return;

        var path =
            EditorUtil.DirectoryBack(Application.dataPath) +
            Path.AltDirectorySeparatorChar +
            m_PrefabPath +
            Path.AltDirectorySeparatorChar +
            nodeName;

        EditorUtil.CheckPath(path);
        // Debug.LogFormat("prefab save path:{0}", pathPrefabFile);
        // PrefabUtility.ApplyPrefabInstance(m_Root.gameObject, InteractionMode.UserAction);
        PrefabUtility.SaveAsPrefabAsset(m_Root.gameObject, pathPrefabFile);
        if (m_Target != BuildTarget.NoTarget)
            GBuildTools.RebuildObject(m_Target, pathPrefabFile);

        DestroyPrefab();
    }

    private void DestroyPrefab()
    {
        if (this.nodeType.Equals("panel"))
        {
            // if (!EditorUtility.DisplayDialog("提示", "更新完成！", "关闭窗口", "关闭窗口并删除实例"))
            UnityEngine.Object.DestroyImmediate(m_Root.gameObject);
        }
    }

    private bool IsValidName(string str, bool silence = true)
    {
        var first = (int)str[0];
        if (first > 122 || first < 97)
        {
            if (!silence)
                Debug.LogError("命名错误：：[" + str + "],请以小写字母开头（驼峰表示法）");
            return false;
        }

        var pms = EditorUtil.TranslateUIElemete(str);
        if (string.IsNullOrEmpty(pms[0]) || string.IsNullOrEmpty(pms[1]))
        {
            if (!silence)
                Debug.LogError("命名错误：：[" + str + "],存在参数为空情况");
            return false;
        }

        return true;
    }
}