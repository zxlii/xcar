using UnityEditor;
using UnityEngine;
using System.IO;
using System;
using System.Collections.Generic;

namespace Gear.Editor
{
    public class GAssetPostprocessor : AssetPostprocessor
    {
        // public override int GetPostprocessOrder() { return 1000; }
        const string BUNDLE_EXTENSION = ".unity3d";
        const string POST_ROOT = "Assets/Res/";
        static Dictionary<string, Action<string>> s_PostProcessed = new Dictionary<string, Action<string>>()
        {
            {"Atlas",PostAtlas},
            {"Textures/UI",PostUITexture},
            {"Prefab/UI",PostUIPrefab},
            {"Font",PostFont},
            {"Shader",PostShader},
            {"Effects/Prefab",PostUIPrefab},
            {"Audio",PostAudio},
        };

        private static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths)
        {
            foreach (var asset in importedAssets)
            {
                if (!asset.StartsWith(POST_ROOT))
                    continue;
                foreach (var pair in s_PostProcessed)
                {
                    var directory = POST_ROOT + pair.Key;
                    if (asset.StartsWith(directory))
                        pair.Value.Invoke(asset);
                }
            }
            AssetDatabase.Refresh();

            // 对接psd工具
            // EditorApplication.delayCall += () => P2UCreate(ref importedAssets);
        }

        private static void PostAtlas(string assetPath)
        {
            var extension = Path.GetExtension(assetPath);
            if (!extension.EndsWith("png"))
                return;

            SetBundleName(assetPath);

            var fileName = Path.GetFileName(assetPath);
            var param = fileName.Split('_');
            if (param[0].Equals("Font"))
            {
                TextureImporter seting = AssetImporter.GetAtPath(assetPath) as TextureImporter;
                seting.maxTextureSize = 1024;
                GFontMaker.Create(assetPath);
            }
        }

        private static void PostUITexture(string assetPath)
        {
            var extension = Path.GetExtension(assetPath);
            if (!extension.EndsWith("jpg"))
                return;

            SetBundleName(assetPath);
        }

        private static void PostUIPrefab(string assetPath)
        {
            var extension = Path.GetExtension(assetPath);
            if (!extension.EndsWith("prefab"))
                return;

            SetBundleName(assetPath);
        }

        private static void PostAudio(string assetPath)
        {
            // var extension = Path.GetExtension(assetPath);
            // if (!extension.EndsWith("wav"))
            //     return;

            SetBundleName(assetPath);
        }

        private static void PostFont(string assetPath)
        {
            var extension = Path.GetExtension(assetPath);
            if (!extension.EndsWith("fontsettings") && !extension.EndsWith("ttf"))
                return;

            SetBundleName(assetPath);
        }

        private static void PostShader(string assetPath)
        {
            var extension = Path.GetExtension(assetPath);
            if (!extension.EndsWith("jpg"))
                return;

            SetBundleName(assetPath);
        }

        private static void SetBundleName(string assetPath)
        {
            var extension = Path.GetExtension(assetPath);
            var bundlePath = assetPath.Replace(POST_ROOT, string.Empty).Replace(extension, BUNDLE_EXTENSION);
            AssetImporter ai = AssetImporter.GetAtPath(assetPath);
            ai.assetBundleName = bundlePath;
        }

        // private static void P2UCreate(ref string[] assets)
        // {
        //     var config = GConfigEditor.Instance;
        //     foreach (string asset in assets)
        //     {
        //         var relativePath = FileUtil.GetProjectRelativePath(config.GetP2UExportPath());
        //         if (!asset.Contains(relativePath)) continue;
        //         if (!asset.EndsWith("Panel.xml", System.StringComparison.Ordinal)) continue;

        //         var name = Path.GetFileNameWithoutExtension(asset);
        //         var fontPath = config.GetFontPath();
        //         var commonSpritePath = config.GetCommonSpritePath();
        //         var str = name.Replace("Panel", "");
        //         var savePath = config.GetP2UPrefabSavePath() + Path.AltDirectorySeparatorChar + str + Path.AltDirectorySeparatorChar + name + ".prefab";
        //         var processor = new GP2UProcessor(asset, fontPath, commonSpritePath);
        //         var go = processor.Create();
        //         PrefabUtility.SaveAsPrefabAsset(go, savePath);
        //         GameObject.DestroyImmediate(go);

        //         Debug.LogFormat("[Tool] save prefab:{0}", savePath);
        //     }
        //     AssetDatabase.Refresh();
        // }
    }
}