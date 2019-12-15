using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using UnityEditor;
using UnityEngine;

public static class EditorUtil
{
    public static void CheckPath(string directory)
    {
        if (!Directory.Exists(directory))
            Directory.CreateDirectory(directory);
    }

    public static string DirectoryBack(string dir, int times = 1)
    {
        string result = string.Empty;
        for (int i = 0; i < times; i++)
            dir = dir.Substring(0, dir.LastIndexOf('/'));
        return dir;
    }

    public static void IterateDirectoriesRecursive(string root, Action<string> processor)
    {
        var node = root;
        var stack = new Stack<string>();
        stack.Push(node);
        while (stack.Count > 0)
        {
            node = stack.Pop();
            processor(node);
            var dirs = Directory.GetDirectories(node);
            if (dirs.Length > 0)
                for (int i = dirs.Length - 1; i >= 0; i--)
                    stack.Push(dirs[i]);
        }
    }

    public static void DestroyChildren(GameObject root)
    {
        var count = root.transform.childCount;
        if (count <= 0) return;
        for (int i = count - 1; i >= 0; i--)
            GameObject.DestroyImmediate(root.transform.GetChild(i).gameObject);
    }

    private static string[] s_StringParam = new string[2];
    public static string[] TranslateUIElemete(string nodeName)
    {
        int i;
        for (i = 0; i < nodeName.Length; i++)
        {
            var a = (int)nodeName[i];
            if (a >= 65 && a <= 90)
                break;
        }
        s_StringParam[0] = nodeName.Substring(0, i);
        s_StringParam[1] = nodeName.Substring(i, nodeName.Length - i);
        return s_StringParam;
    }

    private static void UpdateDirectoryMD5(string path, BuildTarget target)
    {
        var builder = new StringBuilder();
        var list = new List<string>();
        EditorUtil.IterateDirectoriesRecursive(path, dir =>
        {
            var files = Directory.GetFiles(dir, "*.unity3d");
            if (files.Length > 0)
            {
                foreach (var file in files)
                {
                    builder.Append(CreateMD5(file));
                    builder.Append("\t");
                    builder.Append(file.Replace(path + "\\", string.Empty));
                    builder.Append("\n");
                }
            }
        });
        File.WriteAllText(path + Path.AltDirectorySeparatorChar + "BundleList.txt", builder.ToString(), UnicodeEncoding.UTF8);
        AssetDatabase.Refresh();
    }

    private static string CreateMD5(string filePath)
    {
        try
        {
            var fileStream = new FileStream(filePath, FileMode.Open);
            var len = (int)fileStream.Length;
            var data = new byte[len];

            fileStream.Read(data, 0, len);
            fileStream.Close();

            var md5 = new MD5CryptoServiceProvider();
            var hash = md5.ComputeHash(data);
            var result = "";

            foreach (var bt in hash)
                result += Convert.ToString(bt, 16);
            return result;
        }
        catch (FileNotFoundException e)
        {
            Debug.LogError(e.StackTrace);
            return "";
        }
    }
}