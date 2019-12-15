/*
 * instruction:	add instruction here...
 * author:		lizhixiong-pc
 * create time:	Monday, August 5th 2019, 10:46:50 am
 */

using System.IO;
using System.Text;
using Gear.Runtime;
using UnityEngine;
using System.Collections.Generic;

public class GCodeGenOpera
{
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

    private static HashSet<string> s_GenCodeType = new HashSet<string>{
        "img",
        "raw",
        "txt",
        "ipt"
        };

    //type,name,content
    const string BASE_CODE =
@"
----------------------生成代码 请勿手动修改-------------------------

local UI{0} = require 'ui.base.UI{0}'
local {0}{1}Base = class('{0}{1}Base', UI{0})
function {0}{1}Base:OnCreate()
    UI{0}.OnCreate(self)
{2}
end
return {0}{1}Base";

    // type,name,namelower,buttoncode
    const string LOGIC_CODE =
@"local {0}{1}Base = require('ui.{2}.{0}{1}Base')
local {0}{1} = class('{0}{1}', {0}{1}Base)
function {0}{1}:OnShow()
    {0}{1}Base.OnShow(self)
end
function {0}{1}:OnButtonClick(btn_name)
{3}
end
{4}
return {0}{1}";
    static Dictionary<string, string> s_Types = new Dictionary<string, string>() { { "panel", "Panel" }, { "gitem", "Item" }, { "gview", "View" }, { "gctnr", "Container" } };
    private Transform m_Root;
    private GDescriptor m_Desc;
    private string m_NodeType;
    private string m_NodeName;
    private string m_CodePathRoot;
    private IList<string> m_ButtonNames;
    private bool m_IsLua;
    private string currentSystemRoot
    {
        get
        {
            return string.Join(Path.AltDirectorySeparatorChar.ToString(), m_CodePathRoot, "ui", systemName);
        }
    }

    private string m_SystemName;
    private string systemName
    {
        get
        {
            var result = m_SystemName.ToLower();
            if (IsCommon(m_Desc.transform))
                result = "common";
            return result;
        }
    }
    public GCodeGenOpera(Transform root, GDescriptor desc, string codePathRoot, string nodeType, string nodeName, List<string> buttons, bool isLua = true)
    {
        m_Root = root;
        m_Desc = desc;
        m_NodeType = nodeType;
        m_NodeName = nodeName;
        m_CodePathRoot = codePathRoot;
        m_ButtonNames = buttons;
        m_IsLua = isLua;

        m_SystemName = EditorUtil.TranslateUIElemete(m_Root.name)[1];
    }

    public void Execute()
    {
        if (m_IsLua)
            GenLua();
        else
            GenCsharp();
    }

    private void GenLua()
    {
        var props = m_Desc.GetAllGameObjects();
        var type = s_Types[m_NodeType];
        var name = m_NodeName;
        GenLuaBase(props, type, name);
        GenLuaLogic(props, type, name);
    }

    private void GenLuaBase(List<GameObject> props, string type, string name)
    {
        var fileName = type + name + "Base.lua";
        var fullPath = currentSystemRoot + Path.AltDirectorySeparatorChar + fileName;

        EditorUtil.CheckPath(currentSystemRoot);
        var fragment = GetBaseCodeFragment(props);
        var code = string.Format(BASE_CODE, type, name, fragment);

        File.WriteAllText(fullPath, code, new UTF8Encoding(false));
    }

    private bool IsCommon(Transform trans)
    {
        return trans.name.EndsWith("_Common");
    }
    private string GetBaseCodeFragment(List<GameObject> props)
    {
        var builder = new StringBuilder();
        foreach (var prop in props)
        {
            var propFullName = prop.name;
            var param = EditorUtil.TranslateUIElemete(propFullName);
            var type = param[0];
            var name = param[1];

            if (s_GenCodeType.Contains(type))
            {
                var com = c_TypeMap[type];
                var tmp = "self.{0}Com = self.{0}:GetComponent(typeof(UI.{1}))";
                builder.AppendLine();
                builder.Append("\t" + string.Format(tmp, propFullName, com));
            }
            else if (type.Equals("glst"))
            {
                const string tmp =
@"    self.{0} = require 'ui.base.{1}'.new()
    self.{0}.container = self
    self.{0}.isEasyMode = false
	self.{0}.itemType = 'ui.{2}'
	self.{0}:Create(self.{3})";

                const string tmp1 =
@"    self.{0} = require 'ui.base.UIList'.new()
    self.{0}.container = self
    self.{0}.isEasyMode = true
	self.{0}:Create(self.{3}) ";

                builder.AppendLine();
                var listName = propFullName.Replace("glst", "list");
                var listType = propFullName.Contains("_Simple") ? "UISimpleList" : "UIList";
                var trans = prop.transform.GetChild(0);
                var itemSystem = IsCommon(trans) ? "common" : systemName;
                var itemType = itemSystem + "." + trans.name.Replace("gitem", "Item");

                var codeTmp = propFullName.Contains("_Easy") ? tmp1 : tmp;
                builder.AppendFormat(codeTmp, listName, listType, itemType, propFullName);
            }
            else if (type.Equals("gviw"))
            {
                const string tmp =
@"    self.{0} = require 'ui.base.UIScrollView'.new()
    self.{0}.types = {1}
    self.{0}:Create(self.{2})";

                builder.AppendLine();

                var sb = new StringBuilder();
                sb.Append("{");
                var types = new List<string>();
                var count = prop.transform.childCount;
                for (int i = 0; i < count; i++)
                {
                    var child = prop.transform.GetChild(i);
                    var typeName = child.name.Replace("gview", "View");
                    var item = string.Format("[{0}]='ui.{1}.{2}'", i, systemName, typeName);
                    types.Add(item);
                }
                sb.Append(string.Join(",", types));
                sb.Append("}");

                var viewName = propFullName.Replace("gviw", "view");
                var typesCode = sb.ToString();
                builder.AppendFormat(tmp, viewName, typesCode, propFullName);
            }
            else if (type.Equals("gctnr"))
            {
                const string tmp =
@"    self.{0} = require 'ui.{1}'.new()
    self.{0}:Create(self.{2})";

                builder.AppendLine();
                var containerName = propFullName + "Custom";
                var containerTypePath = systemName + "." + propFullName.Replace("gctnr", "Container");
                builder.AppendFormat(tmp, containerName, containerTypePath, propFullName);
            }
        }
        return builder.ToString();
    }

    private void GenLuaLogic(List<GameObject> props, string type, string name)
    {
        var fileName = type + name + ".lua";
        var fullPath = currentSystemRoot + Path.AltDirectorySeparatorChar + fileName;

        if (File.Exists(fullPath))
        {
            // if (!EditorUtility.DisplayDialog("提示", fileName + "文件已经存在，是否要覆盖？", "是", "否"))
            return;
        }

        EditorUtil.CheckPath(currentSystemRoot);
        var fragment = GetLogicFragment(props);
        var easyListCode = GetEasyListCode(props, type, name);
        var code = string.Format(LOGIC_CODE, type, name, systemName, fragment, easyListCode);
        File.WriteAllText(fullPath, code, new UTF8Encoding(false));
    }

    private string GetLogicFragment(List<GameObject> props)
    {
        if (m_ButtonNames.Count == 0) return string.Empty;
        var builder = new StringBuilder();
        const string tmp1 = "if btn_name == '{0}' then";
        const string tmp2 = "elseif btn_name == '{0}' then";
        for (int i = 0; i < m_ButtonNames.Count; i++)
        {
            var tmp = i == 0 ? tmp1 : tmp2;
            builder.AppendLine("\t" + string.Format(tmp, m_ButtonNames[i]));
        }
        builder.Append("\tend");
        return builder.ToString();
    }

    private string GetEasyListCode(List<GameObject> props, string type, string name)
    {
        var hasEasyList = false;
        foreach (var prop in props)
        {
            var propName = prop.name;
            if (propName.Contains("glst") && propName.Contains("_Easy"))
            {
                hasEasyList = true;
                break;
            }
        }
        if (hasEasyList)
        {
            var tmp = @"function {0}{1}:OnItemInit(list, item, index) 
end

function {0}{1}:OnItemClick(list, item, index) 
end";
            return string.Format(tmp, type, name);
        }
        return string.Empty;
    }

    private void GenCsharp()
    {
        #region C# code gen

        // var props = m_Desc.GetAllGameObjects();
        // var builderDefine = new StringBuilder();
        // var builderInit = new StringBuilder();
        // var builderButton = new StringBuilder();
        // var tabInit = "        ";

        // if (props.Count > 0)
        // {
        //     foreach (var prop in props)
        //     {
        //         var propFullName = prop.name;
        //         var param = EditorUtil.TranslateUIElemete(propFullName);
        //         var propType = param[0];

        //         if (!c_TypeMap.ContainsKey(propType)) continue;

        //         var uitype = c_TypeMap[propType];
        //         var lineDefine = string.Format("    private {0} {1};", uitype, propFullName);
        //         builderDefine.AppendLine(lineDefine);

        //         var lineInit = string.Empty;
        //         var temp = "{0} = GetObject<{1}>(\"{0}\");";
        //         lineInit = tabInit + string.Format(temp, propFullName, uitype);
        //         builderInit.AppendLine(lineInit);
        //     }

        //     foreach (var prop in props)
        //     {
        //         var propFullName = prop.name;
        //         var param = EditorUtil.TranslateUIElemete(propFullName);
        //         var propType = param[0];

        //         if (!c_TypeMap.ContainsKey(propType)) continue;

        //         var uitype = c_TypeMap[propType];
        //         switch (propType)
        //         {
        //             case "btn":
        //                 builderInit.AppendLine(string.Format("{0}{1}.onClick.AddListener(() => OnButtonClick({1}));", tabInit, propFullName));
        //                 if (builderButton.Length == 0)
        //                 {
        //                     // builderButton.AppendLine(string.Format("if(button == {0}){", propFullName));
        //                     builderButton.AppendLine("        if (button == " + propFullName + "){");
        //                 }
        //                 else if (builderButton.Length > 0)
        //                 {
        //                     // builderButton.AppendLine(string.Format("}else if(button == {0}){", propFullName));
        //                     builderButton.AppendLine("        }else if(button == " + propFullName + "){");
        //                 }
        //                 break;
        //             case "ipt":
        //                 builderInit.AppendLine(string.Format("{0}{1}.onEndEdit.AddListener(txt => OnInputFieldEndEdit({1}, txt));", tabInit, propFullName));
        //                 builderInit.AppendLine(string.Format("{0}{1}.onValueChanged.AddListener(txt => OnInputFieldValueChange({1}, txt));", tabInit, propFullName));
        //                 break;
        //             case "tog":
        //                 builderInit.AppendLine(string.Format("{0}{1}.onValueChanged.AddListener(isOn => OnToggleValueChange({1}, isOn));", tabInit, propFullName));
        //                 break;
        //             case "scb":
        //                 builderInit.AppendLine(string.Format("{0}{1}.onValueChanged.AddListener(value => OnScrollbarValueChange({1}, value));", tabInit, propFullName));
        //                 break;
        //             case "sld":
        //                 builderInit.AppendLine(string.Format("{0}{1}.onValueChanged.AddListener(value => OnSliderValueChange({1}, value));", tabInit, propFullName));
        //                 break;
        //             case "dop":
        //                 builderInit.AppendLine(string.Format("{0}{1}.onValueChanged.AddListener(value => OnDropdownValueChange({1}, value));", tabInit, propFullName));
        //                 break;
        //             case "skr":
        //                 builderInit.AppendLine(string.Format("{0}{1}.onValueChanged.AddListener(value => OnScrollRectValueChange({1}, value));", tabInit, propFullName));
        //                 break;
        //             //TODO
        //             case "glst":
        //                 break;
        //             case "gtgb":
        //                 break;
        //             case "gviw":
        //                 break;
        //         }
        //     }
        //     builderInit.AppendFormat("{0}OnCreate();", tabInit);
        // }

        // var classNameBase = "GUIContainer";
        // m_Builder.AppendLine("/* 此文件为自动生成，不要手动修改！ */");
        // m_Builder.AppendLine();
        // m_Builder.AppendLine("using UnityEngine;");
        // m_Builder.AppendLine("using UnityEngine.UI;");
        // m_Builder.AppendLine();
        // m_Builder.AppendLine(string.Format("public partial class {0} : {1}", usedName, classNameBase));
        // m_Builder.AppendLine("{");
        // if (builderDefine.Length > 0)
        //     m_Builder.AppendLine(builderDefine.ToString());
        // m_Builder.AppendLine("    protected override void Awake()");
        // m_Builder.AppendLine("    {");
        // if (builderInit.Length > 0)
        //     m_Builder.AppendLine(builderInit.ToString());
        // m_Builder.AppendLine("    }");
        // m_Builder.Append("}");
        // File.WriteAllText(string.Format("{0}/{1}.cs", pathCodeGen, usedName), m_Builder.ToString(), UnicodeEncoding.UTF8);

        // var fullPath = string.Format("{0}/{1}.cs", pathCodeGenLogic, usedName);
        // if (!File.Exists(fullPath))
        // {
        //     m_Builder.Remove(0, m_Builder.Length);
        //     m_Builder.AppendLine("using UnityEngine;");
        //     m_Builder.AppendLine("using UnityEngine.UI;");
        //     m_Builder.AppendLine(string.Format("public partial class {0} : {1}", usedName, classNameBase));
        //     m_Builder.AppendLine("{");
        //     m_Builder.AppendLine(GConst.CodeTemp_FuncsLogic);
        //     if (builderButton.Length > 0)
        //     {
        //         builderButton.Append("\t\t}");
        //         m_Builder.AppendLine("\tprotected override void OnButtonClick(Button button)\n\t{");
        //         m_Builder.Append(builderButton.ToString());
        //         m_Builder.AppendLine("\t}");
        //     }
        //     m_Builder.AppendLine("}");

        //     File.WriteAllText(fullPath, m_Builder.ToString(), UnicodeEncoding.UTF8);
        // }

        #endregion
    }

}