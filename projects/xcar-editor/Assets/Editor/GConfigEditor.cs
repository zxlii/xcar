using UnityEngine;
using System.IO;
using UnityEditor;

public class GConfigEditor
{
    private static GConfigEditor m_Instance;
    public static GConfigEditor Instance
    {
        get
        {
            if (m_Instance == null)
                m_Instance = new GConfigEditor();
            return m_Instance;
        }
    }
    private string m_RuntimeProjectPath = string.Empty;
    private string m_EditorProjectPath = string.Empty;
    public string RuntimeProjectPath
    {
        get
        {
            if (string.IsNullOrEmpty(m_RuntimeProjectPath))
                m_RuntimeProjectPath = EditorProjectPath.Replace("pop3-edit", "trunk");
            return m_RuntimeProjectPath;
        }
    }

    public string EditorProjectPath
    {
        get
        {
            if (string.IsNullOrEmpty(m_EditorProjectPath))
                m_EditorProjectPath = EditorUtil.DirectoryBack(Application.dataPath);
            return m_EditorProjectPath;
        }
    }
    private string m_ProjectRoot;
    public string ProjectRoot
    {
        get
        {
            if (string.IsNullOrEmpty(m_ProjectRoot))
                m_ProjectRoot = Application.dataPath.Replace("Assets", string.Empty);
            return m_ProjectRoot;
        }
    }

    // UI prefab 保存相对路径，Assets/Prefab/UI
    public string GetPrefabLocalPath()
    {
        return "Assets/Res/Prefab/UI";
    }

    // 存放p2u导出的Common图片的路径，G:/Work/popstar3/proj/pop3-edit/Assets\StandardAssets/Sprites/Common
    public string GetCommonSpritePath()
    {
        return ProjectRoot + "/Assets/StandardAssets/Sprites/Common";
    }

    public string GetP2UPrefabSavePath()
    {
        return "Assets/Res/Prefab/UI";
    }

    public string GetAssetBundleBuildPath(BuildTarget target)
    {
        return string.Join(Path.AltDirectorySeparatorChar.ToString(), ProjectRoot, "AssetBundles", target.ToString());
        // return string.Join(Path.AltDirectorySeparatorChar.ToString(), RuntimeProjectPath, "Files", "assetBundles", suffix);
    }

    public string GetCodeGenPath()
    {
        return string.Join(Path.AltDirectorySeparatorChar.ToString(), ProjectRoot, "Files", "scripts");
    }

    public string GetP2UExportPath()
    {
        return ProjectRoot + "/Assets/StandardAssets/Sprites";
    }

    public string GetFontPath()
    {
        return ProjectRoot + "/Assets/StandardAssets/Fonts";
    }

    public string GetUIEditorRoot()
    {
        return "Canvas";
    }
}