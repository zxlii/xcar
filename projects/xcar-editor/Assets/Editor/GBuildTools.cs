using UnityEditor;
using UnityEngine;
using System.IO;
using System.Text;
// using zplayx;

public class GBuildTools
{
    static BuildAssetBundleOptions DEFAULT_COMPRESSION = BuildAssetBundleOptions.ChunkBasedCompression;
    static GConfigEditor conf { get { return GConfigEditor.Instance; } }
    public static void BuildAll(BuildTarget target)
    {
        Caching.ClearCache();
        var path = PrepairOutputPath(target);
        BuildPipeline.BuildAssetBundles(path, DEFAULT_COMPRESSION, target);
        BuildAssetBundleConfig(target);
    }

    public static void RebuildObject(BuildTarget target, string path)
    {
        var bundleOutputPaht = PrepairOutputPath(target);
        var bundlePath = path.Replace(".prefab", ".unity3d").Replace("Assets/", string.Empty);
        Debug.Log(string.Format("build asset : {0}", bundlePath));
        AssetBundleBuild[] builds = new AssetBundleBuild[] {
            new AssetBundleBuild {
                assetNames = new string[] { path } ,
                assetBundleName = bundlePath
                }
            };
        BuildPipeline.BuildAssetBundles(bundleOutputPaht, builds, DEFAULT_COMPRESSION, target);
        BuildAssetBundleConfig(target);
    }

    private static void BuildAssetBundleConfig(BuildTarget target)
    {
        /*
        var assetPathRoot = GetAssetBundlesPath(target);
        var manifest = GetAssetBundleManifest(target);
        var assetBundles = manifest.GetAllAssetBundles();
        var newEntryDic = new Dictionary<string, AssetBundleEntryConfig>();

        foreach (var bundleName in assetBundles)
        {
            var bundlePath = PathCombine(assetPathRoot, bundleName);
            var bundle = AssetBundle.LoadFromFile(bundlePath);
            var assetPaths = bundle.GetAllAssetNames();
            bundle.Unload(false);
            if (assetPaths != null && assetPaths.Length > 0)
            {
                var entry = GetEntryItem(target, manifest, bundleName, assetPaths);
                newEntryDic.Add(entry.bundleName, entry);
            }
        }

        var configPath = PathCombine(assetPathRoot, "asset.json");
        var config = AssetBundleConfig.CreateFromFile(configPath);
        var stack = new Stack<AssetBundleEntryConfig>();
        foreach (var entry in config.bundle)
        {
            var p = entry.bundleName.Replace("assetBundles/", string.Empty);
            var path = string.Join(Path.AltDirectorySeparatorChar.ToString(), assetPathRoot, p);
            if (!File.Exists(path))
                stack.Push(entry);
        }
        while (stack.Count > 0)
            config.bundle.Remove(stack.Pop());

        foreach (var oldEntry in config.bundle)
        {
            AssetBundleEntryConfig newEntry;
            if (newEntryDic.TryGetValue(oldEntry.bundleName, out newEntry))
            {
                if (!oldEntry.hash.Equals(newEntry.hash))
                    oldEntry.Convert(newEntry);
                newEntryDic.Remove(newEntry.bundleName);
            }
        }

        if (newEntryDic.Count > 0)
        {
            foreach (var kvp in newEntryDic)
                config.bundle.Add(kvp.Value);
        }

        WriteTextToFile(JsonUtility.ToJson(config, true), configPath);
        // CopyFiles(target);
        */
    }

    public static void WriteTextToFile(string content, string filename, bool flush = true)
    {
        var fn = Path.GetFileName(filename);
        var fileDir = filename.Replace(fn, string.Empty);
        EditorUtil.CheckPath(fileDir);
        File.WriteAllText(filename, content);
    }

    // private static AssetBundleEntryConfig GetEntryItem(BuildTarget target, AssetBundleManifest manifest, string bundleName, string[] assetPaths)
    // {
    //     var layout = FileLayoutConfig.GetConfig();
    //     var entry = new AssetBundleEntryConfig();
    //     entry.filename = GetAssetBundleFilenameWithHash(target, bundleName, manifest);
    //     foreach (var assetPath in assetPaths)
    //     {
    //         if (assetPath.StartsWith("assets/spine") && assetPath.EndsWith(".json"))
    //             continue;
    //         string assetName = assetPath.Replace("assets/", string.Empty);
    //         string assetRootName = GetAssetRootName(assetName);
    //         string relativePath = assetName.Replace(assetRootName + "/", string.Empty);
    //         string path, category, resolution, language, virtualName;
    //         layout.ParseAttributes(relativePath, out path, out category, out resolution, out language, out virtualName);
    //         virtualName = assetRootName + "/" + virtualName;
    //         virtualName = virtualName.Substring(0, virtualName.LastIndexOf('.'));
    //         entry.asset.Add(virtualName.ToLower());
    //     }
    //     foreach (var depend in AssetDatabase.GetAssetBundleDependencies(bundleName, false))
    //         entry.depend.Add(GetAssetBundleFilenameWithHash(target, depend, manifest));

    //     return entry;
    // }

    private static string PrepairOutputPath(BuildTarget target)
    {
        var bundleOutputPaht = conf.GetAssetBundleBuildPath(target);
        EditorUtil.CheckPath(bundleOutputPaht);
        return bundleOutputPaht;
    }

    private static string GetAssetBundlesPath(BuildTarget target)
    {
        var bundleOutputPaht = conf.GetAssetBundleBuildPath(target);
        EditorUtil.CheckPath(bundleOutputPaht);
        return bundleOutputPaht;
    }
    private static AssetBundleManifest GetAssetBundleManifest(BuildTarget target)
    {
        var assetBundlesPath = GetAssetBundlesPath(target);
        AssetBundleManifest manifest = null;
        string manifestPath = Path.Combine(assetBundlesPath, target.ToString());
        AssetBundle manifestBundle = AssetBundle.LoadFromFile(manifestPath);
        if (manifestBundle != null)
        {
            manifest = manifestBundle.LoadAsset<AssetBundleManifest>("AssetBundleManifest");
            manifestBundle.Unload(false);
        }
        return manifest;
    }

    private static string GetAssetBundleFilenameWithHash(BuildTarget target, string bundleName, AssetBundleManifest manifest)
    {
        var filename = string.Join(Path.AltDirectorySeparatorChar.ToString(), "assetBundles", bundleName);
        if (target == BuildTarget.StandaloneOSX)// || target == BuildTarget.StandaloneWindows64)
        {
            return filename;
        }
        else
        {
            var hash = manifest.GetAssetBundleHash(bundleName).ToString();
            if (!string.IsNullOrEmpty(hash))
            {
                var pos = filename.LastIndexOf('.');
                if (pos == -1)
                {
                    Debug.LogError("filename is not ends with .prefab:" + filename);
                }
                filename = string.Format("{0}.{1}.{2}", filename.Substring(0, pos), hash, filename.Substring(pos + 1));
            }
        }
        return filename;
    }

    private static string GetAssetRootName(string asset)
    {
        var pos = asset.IndexOf('/');
        return asset.Substring(0, pos);
    }

    public static string PathCombine(string root, params string[] paths)
    {
        StringBuilder path = new StringBuilder(root);

        var iter = paths.GetEnumerator();
        while (iter.MoveNext())
        {
            string subPath = (string)iter.Current;
            if (path.Length < 1)
            {
                path.Append(subPath);
            }
            else
            {
                if (path[path.Length - 1] != '/') path.Append('/');
                if (!string.IsNullOrEmpty(subPath)) path.Append(subPath);
            }
        }

        return path.ToString();
    }


    public static void CopyFiles(BuildTarget target)
    {
        var rootSourceDir = PrepairOutputPath(target);
        var rootDestDir = rootSourceDir.Replace(target.ToString(), "Files/assetBundles");

        if (Directory.Exists(rootDestDir))
        {
            FileUtil.DeleteFileOrDirectory(rootDestDir);
        }

        FileUtil.CopyFileOrDirectory(rootSourceDir, rootDestDir);
    }
}