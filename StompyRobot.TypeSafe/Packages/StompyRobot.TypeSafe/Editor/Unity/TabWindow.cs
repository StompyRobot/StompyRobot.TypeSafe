using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace TypeSafe.Editor.Unity
{
    internal class TabWindow : EditorWindow
    {
        [SerializeField] private readonly List<ITab> _tabs = new List<ITab>();
        [SerializeField] private int _currentTab = -1;
        private bool _tabIsOpen;
        protected GUIStyle BoxStyleOverride;

        protected virtual void OnEnable()
        {
            if (_tabs.Count == 0)
            {
                OnRegisterTabs();
            }
        }

        protected virtual void OnRegisterTabs() {}

        protected virtual void OnDisable()
        {
            if (_currentTab >= 0)
            {
                _tabs[_currentTab].OnExit();
            }
            _tabIsOpen = false;
        }

        protected virtual void OnGUI()
        {
            if (_currentTab == -1 && _tabs.Count > 0)
            {
                SetActiveTab(0);
            }

            if (!_tabIsOpen && _tabs.Count > 0)
            {
                _tabs[_currentTab].OnEnter();
                _tabIsOpen = true;
            }

            var rect = EditorGUILayout.BeginVertical(BoxStyleOverride != null ? BoxStyleOverride : GUI.skin.box);
            --rect.width;

            var height = 18;

            var canChange = _currentTab >= 0 && _tabs[_currentTab].CanExit;

            EditorGUI.BeginChangeCheck();
            EditorGUI.BeginDisabledGroup(!canChange);

            for (var i = 0; i < _tabs.Count; ++i)
            {
                var xStart = Mathf.RoundToInt(i*rect.width/_tabs.Count);
                var xEnd = Mathf.RoundToInt((i + 1)*rect.width/_tabs.Count);

                var pos = new Rect(rect.x + xStart, rect.y, xEnd - xStart, height);

                if (GUI.Toggle(pos, _currentTab == i, new GUIContent(_tabs[i].TabName), EditorStyles.toolbarButton) &&
                    _currentTab != i)
                {
                    SetActiveTab(i);
                }
            }

            GUILayoutUtility.GetRect(10f, height);

            EditorGUI.EndDisabledGroup();

            if (EditorGUI.EndChangeCheck())
            {
                GUIUtility.keyboardControl = 0;
            }

            if (_currentTab >= 0)
            {
                _tabs[_currentTab].OnGUI();
            }

            EditorGUILayout.EndVertical();
        }

        protected void SetActiveTab(int index)
        {
            if (index == _currentTab)
            {
                return;
            }

            if (index < 0 || index >= _tabs.Count)
            {
                return;
            }

            if (_currentTab >= 0)
            {
                _tabs[_currentTab].OnExit();
            }

            _currentTab = index;

            _tabs[_currentTab].OnEnter();
            _tabIsOpen = true;
        }

        protected void AddTab(ITab t)
        {
            _tabs.Add(t);
        }
    }
}
