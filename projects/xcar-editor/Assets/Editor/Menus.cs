
using UnityEditor;
using UnityEngine;
using System.IO;

public class Menus
{
    // [MenuItem("Gear/UI/导出当前UI-Android", false, 1)]
    // static void Export_Android()
    // {
    //     GPanelPreprocess.ExportCurrent(BuildTarget.Android);
    // }
    // [MenuItem("Gear/UI/导出选中UI-Android", false, 2)]
    // static void ExportSelect_Android()
    // {
    //     var path = AssetDatabase.GetAssetPath(Selection.activeObject);
    //     GBuildTools.RebuildObject(BuildTarget.Android, path);
    // }
    // [MenuItem("Gear/UI/更新全部UI-Android", false, 3)]
    // static void Build_Android()
    // {
    //     GBuildTools.BuildAll(BuildTarget.Android);
    // }



    // [MenuItem("Gear/UI/导出当前UI-iOS", false, 101)]
    // static void Export_iOS()
    // {
    //     GPanelPreprocess.ExportCurrent(BuildTarget.iOS);
    // }
    // [MenuItem("Gear/UI/导出选中UI-iOS", false, 102)]
    // static void ExportSelect_iOS()
    // {
    //     var path = AssetDatabase.GetAssetPath(Selection.activeObject);
    //     GBuildTools.RebuildObject(BuildTarget.iOS, path);
    // }
    // [MenuItem("Gear/UI/更新全部UI-iOS", false, 103)]
    // static void Build_iOS()
    // {
    //     GBuildTools.BuildAll(BuildTarget.iOS);
    // }



    // [MenuItem("Gear/UI/导出当前UI-Windows64", false, 201)]
    // static void Export_Windows64()
    // {
    //     GPanelPreprocess.ExportCurrent(BuildTarget.StandaloneWindows64);
    // }
    // [MenuItem("Gear/UI/导出选中UI-Windows64", false, 202)]
    // static void ExportSelect_Windows64()
    // {
    //     var path = AssetDatabase.GetAssetPath(Selection.activeObject);
    //     GBuildTools.RebuildObject(BuildTarget.StandaloneWindows64, path);
    // }
    // [MenuItem("Gear/UI/更新全部UI-Windows64", false, 203)]
    // static void Build_Windows64()
    // {
    //     GBuildTools.BuildAll(BuildTarget.StandaloneWindows64);
    // }

    [MenuItem("Gear/UI/保存prefab+更新lua代码", false, 201)]
    static void Export()
    {
        GPanelPreprocess.ExportCurrent();
    }


    // [MenuItem("Gear/P2U/手动更新全部ui prefab")]
    // static void P2UProcess()
    // {
    //     EditorUtil.IterateDirectoriesRecursive(GConfigEditor.Instance.GetP2UExportPath(), path =>
    //     {
    //         var directoryName = Path.GetFileName(path);
    //         var fileName = directoryName + "Panel.xml";
    //         var asset = path + Path.AltDirectorySeparatorChar + fileName;
    //         if (File.Exists(asset))
    //         {
    //             asset = FileUtil.GetProjectRelativePath(asset);
    //             var name = Path.GetFileNameWithoutExtension(asset);
    //             var fontPath = GConfigEditor.Instance.GetFontPath();
    //             var commonSpritePath = GConfigEditor.Instance.GetCommonSpritePath();
    //             var savePath = GConfigEditor.Instance.GetP2UPrefabSavePath() + Path.AltDirectorySeparatorChar + directoryName + Path.AltDirectorySeparatorChar + name + ".prefab";
    //             var processor = new GP2UProcessor(asset, fontPath, commonSpritePath);
    //             var go = processor.Create();
    //             PrefabUtility.SaveAsPrefabAsset(go, savePath);
    //             GameObject.DestroyImmediate(go);

    //             Debug.LogFormat("[Tool] save prefab:{0}", savePath);
    //         }
    //     });
    // }
}