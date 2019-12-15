/*
 * instruction:	add instruction here...
 * author:		lizhixiong-pc
 * create time:	Tuesday, August 6th 2019, 6:43:35 pm
 */

using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections.Generic;
namespace Gear.Runtime.UI
{
    [RequireComponent(typeof(ToggleGroup))]
    public class GToggleBar : GUIBehaviour
    {
        private Action<int> m_OnSelect;
        private ToggleGroup m_Group;
        private List<Toggle> m_Toggles = new List<Toggle>();
        private bool m_Silence = false;
        protected override void Awake()
        {
            m_Group = GetComponent<ToggleGroup>();
            GetComponentsInChildren<Toggle>(true, m_Toggles);
            foreach (var tog in m_Toggles)
            {
                tog.onValueChanged.AddListener(isOn => OnToggle(tog));
                if (tog.group == null)
                    tog.group = m_Group;
            }
            m_Group.allowSwitchOff = false;
        }

        private void OnToggle(Toggle tog)
        {
            // Debug.Log(string.Format("togname={0}, togindex={1}, isOn={2}", tog.name, m_Toggles.IndexOf(tog), tog.isOn));
            if (!m_Silence && tog.isOn && m_OnSelect != null)
                m_OnSelect(m_Toggles.IndexOf(tog));
        }

        public void Register(Action<int> onSelect)
        {
            m_OnSelect = onSelect;
        }

        public void Unregister()
        {
            if (m_OnSelect != null)
                m_OnSelect = null;
        }

        public void Select(int index, bool silence = false)
        {
            if (index < 0 || index >= m_Toggles.Count)
            {
                Debug.LogError("toggle bar index out of range!");
                return;
            }
            m_Silence = silence;
            var tog = m_Toggles[index];
            tog.isOn = true;
            m_Silence = false;
        }

        protected override void OnDestroy()
        {
            Unregister();
            foreach (var tog in m_Toggles)
                tog.onValueChanged.RemoveAllListeners();
        }
    }
}