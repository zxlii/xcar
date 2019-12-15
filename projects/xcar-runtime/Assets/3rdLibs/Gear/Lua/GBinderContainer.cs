
using UnityEngine;
using UnityEngine.UI;
using XLua;
using System;
using System.Collections.Generic;

namespace Gear.Runtime.UI
{
    [CSharpCallLua]
    public delegate void DelegateStringBool(LuaTable t, string s, bool b);
    [CSharpCallLua]
    public delegate void DelegateStringFloat(LuaTable t, string s, float b);
    [CSharpCallLua]
    public delegate void DelegateStringInt(LuaTable t, string s, int i);
    public class GBinderContainer : GBinder
    {
        private Action<LuaTable, string> m_OnButtonClick;
        private DelegateStringFloat m_OnSliderValueChange;
        private DelegateStringFloat m_OnScrollbarValueChange;
        private DelegateStringBool m_OnToggleValueChange;
        private Action<LuaTable, string, string> m_OnInputFieldEndEdit;
        private Action<LuaTable, string, string> m_OnInputFieldValueChanged;
        private DelegateStringInt m_OnDropdownValueChange;
        private Action<LuaTable> m_OnClickAnyWhere;
        private Action<LuaTable> m_OnClickDefaultBackground;

        public override void Bind(LuaTable table)
        {
            base.Bind(table);
            Inject(true);
            RestoreLuaEvents();
            SetUIEvents(true);
        }

        private void Inject(bool isAdd)
        {
            var desc = GetComponent<GDescriptor>();
            if (desc == null) return;
            foreach (var obj in desc.GetAllGameObjects())
                Table.Set(obj.name, isAdd ? obj : null);
        }

        private void RestoreLuaEvents()
        {
            m_OnButtonClick = Table.Get<Action<LuaTable, string>>("OnButtonClick");
            m_OnToggleValueChange = Table.Get<DelegateStringBool>("OnToggleValueChange");
            m_OnSliderValueChange = Table.Get<DelegateStringFloat>("OnSliderValueChange");
            m_OnScrollbarValueChange = Table.Get<DelegateStringFloat>("OnScrollbarValueChange");
            m_OnInputFieldEndEdit = Table.Get<Action<LuaTable, string, string>>("OnInputFieldEndEdit");
            m_OnInputFieldValueChanged = Table.Get<Action<LuaTable, string, string>>("OnInputFieldValueChanged");
            m_OnDropdownValueChange = Table.Get<DelegateStringInt>("OnDropdownValueChange");
            m_OnClickAnyWhere = Table.Get<Action<LuaTable>>("OnClickAnyWhere");
            m_OnClickDefaultBackground = Table.Get<Action<LuaTable>>("OnClickDefaultBackground");
        }

        private void SetUIEvents(bool isAdd)
        {
            var root = transform;
            var queue = new Queue<Transform>();
            queue.Enqueue(root);
            while (queue.Count > 0)
            {
                var node = queue.Dequeue();
                RegisterUGUIEvent(node, isAdd);
                if (node != root)
                {
                    // if (!node.gameObject.activeSelf)
                    //     continue;
                    if (node.GetComponent<GDescriptor>() != null)
                        continue;
                }

                foreach (Transform child in node)
                    queue.Enqueue(child);
            }
        }

        private void RegisterUGUIEvent(Transform node, bool isAdd)
        {
            var nodeName = node.name;
            if (nodeName.Length < 3) return;
            var prefix = nodeName.Substring(0, 3);
            if (prefix.Equals("btn"))
            {
                var btn = node.GetComponent<Button>();
                if (btn == null)
                    btn = node.gameObject.AddComponent<Button>();

                btn.onClick.RemoveAllListeners();
                if (isAdd)
                    btn.onClick.AddListener(delegate () { OnClick(btn); });
            }
            else if (prefix.Equals("tog"))
            {
                var tog = node.GetComponent<Toggle>();
                if (tog == null)
                    tog = node.gameObject.AddComponent<Toggle>();

                tog.onValueChanged.RemoveAllListeners();
                if (isAdd)
                    tog.onValueChanged.AddListener(delegate (bool value) { m_OnToggleValueChange(Table, tog.name, value); });
            }
            else if (prefix.Equals("sld"))
            {
                var sld = node.GetComponent<Slider>();
                if (sld == null)
                    sld = node.gameObject.AddComponent<Slider>();

                sld.onValueChanged.RemoveAllListeners();
                if (isAdd)
                    sld.onValueChanged.AddListener(delegate (float value) { m_OnSliderValueChange(Table, sld.name, value); });
            }
            else if (prefix.Equals("scb"))
            {
                var scb = node.GetComponent<Scrollbar>();
                if (scb == null)
                    scb = node.gameObject.AddComponent<Scrollbar>();

                scb.onValueChanged.RemoveAllListeners();
                if (isAdd)
                    scb.onValueChanged.AddListener(delegate (float value) { m_OnScrollbarValueChange(Table, scb.name, value); });
            }
            else if (prefix.Equals("dop"))
            {
                var dop = node.GetComponent<Dropdown>();
                if (dop == null)
                    dop = node.gameObject.AddComponent<Dropdown>();

                dop.onValueChanged.RemoveAllListeners();
                if (isAdd)
                    dop.onValueChanged.AddListener(delegate (int value) { m_OnDropdownValueChange(Table, dop.name, value); });
            }
            else if (prefix.Equals("ipt"))
            {
                var ipt = node.GetComponent<InputField>();
                if (ipt == null)
                    ipt = node.gameObject.AddComponent<InputField>();

                ipt.onEndEdit.RemoveAllListeners();
                ipt.onValueChanged.RemoveAllListeners();
                if (isAdd)
                {
                    ipt.onEndEdit.AddListener(delegate (string value) { m_OnInputFieldEndEdit?.Invoke(Table, ipt.name, value); });
                    ipt.onValueChanged.AddListener(delegate (string value) { m_OnInputFieldValueChanged(Table, ipt.name, value); });
                }
            }
        }

        private void OnClick(Button btn)
        {
            if (m_OnClickAnyWhere != null)
                m_OnClickAnyWhere(Table);

            if (btn.name.Equals("btnPanelBackground_Default"))
            {
                if (m_OnClickDefaultBackground != null)
                    m_OnClickDefaultBackground(Table);
            }
            else
                m_OnButtonClick(Table, btn.name);
        }

        protected override void Unbind()
        {
            SetUIEvents(false);
            m_OnButtonClick = null;
            m_OnToggleValueChange = null;
            m_OnSliderValueChange = null;
            m_OnScrollbarValueChange = null;
            m_OnDropdownValueChange = null;
            m_OnInputFieldEndEdit = null;
            m_OnInputFieldValueChanged = null;
            m_OnClickAnyWhere = null;
            m_OnClickDefaultBackground = null;
            // Inject(false);
            base.Unbind();
        }
    }
}