using UnityEditor;
using UnityEngine;

public class GPanelPreprocess
{
    static GConfigEditor conf { get { return GConfigEditor.Instance; } }
    public static void ExportCurrent(BuildTarget target = BuildTarget.NoTarget)
    {
        var codePath = conf.GetCodeGenPath();
        var prefabPath = conf.GetPrefabLocalPath();
        var root = GameObject.Find(conf.GetUIEditorRoot());
        foreach (Transform child in root.transform)
        {
            if (child.GetComponent<Canvas>() != null)
                new GNodeOpera(child, child, codePath, prefabPath, target).Execute();
        }
    }
}
