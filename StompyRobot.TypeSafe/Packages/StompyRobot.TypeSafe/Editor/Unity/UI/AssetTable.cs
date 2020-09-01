using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace TypeSafe.Editor.Unity.UI
{
    class AssetTable
    {
        public Vector2 ScrollPosition;

        public Action<string> Added;
        public Action<string> Removed;

        private readonly List<Type> _validAssetTypes;

        public AssetTable(IEnumerable<Type> validAssetTypes)
        {
            _validAssetTypes = validAssetTypes.ToList();
        }

        protected void OnAdded(string guid)
        {
	        if (Added != null)
	        {
		        Added.Invoke(guid);
	        }
        }

        protected void OnRemoved(string guid)
        {
	        if (Removed != null)
	        {
		        Removed.Invoke(guid);
	        }
        }

        public void Draw(IList<string> entries)
        {
            var rect = EditorGUILayout.BeginVertical(Styles.NamingPreviewTable, GUILayout.Height(85));
            ScrollPosition = EditorGUILayout.BeginScrollView(ScrollPosition, Styles.NoPaddingNoMargin);

            for (var i = 0; i < entries.Count; i++)
            {
                var rowRect = EditorGUILayout.BeginHorizontal(i % 2 == 0 ? Styles.NamingPreviewRowEven : Styles.NamingPreviewRowOdd);

                var assetPath = AssetDatabase.GUIDToAssetPath(entries[i]);
                var name = Path.GetFileNameWithoutExtension(assetPath);
                GUILayout.Label(name, "OL Label");

                if (GUILayout.Button(new GUIContent("", "Remove"), "OL Minus", GUILayout.ExpandWidth(false)))
                {
                    OnRemoved(entries[i]);
                    break;
                }

                EditorGUILayout.EndHorizontal();

                if (Event.current.type == EventType.MouseDown && rowRect.Contains(Event.current.mousePosition))
                {
                    EditorGUIUtility.PingObject(AssetDatabase.LoadAssetAtPath(assetPath, typeof(UnityEngine.Object)));
                }
            }

            EditorGUILayout.EndScrollView();
            EditorGUILayout.EndVertical();

            var evt = Event.current;
            switch (evt.type)
            {
                case EventType.DragUpdated:
                case EventType.DragPerform:
                {
                    if (!rect.Contains(evt.mousePosition))
                    {
                        break;
                    }

                    if (!DragAndDrop.objectReferences.All(p => AcceptsType(p.GetType())))
                    {
                        DragAndDrop.visualMode = DragAndDropVisualMode.Rejected;
                        break;
                    }

                    DragAndDrop.visualMode = DragAndDropVisualMode.Copy;

                    if (evt.type == EventType.DragPerform)
                    {
                        DragAndDrop.AcceptDrag();

                        foreach (var path in DragAndDrop.paths)
                        {
                            OnAdded(AssetDatabase.AssetPathToGUID(path));
                        }
                    }

                    break;
                }
            }
        }

        bool AcceptsType(Type t)
        {
            foreach (var validType in _validAssetTypes)
            {
                if (validType.IsAssignableFrom(t))
                {
                    return true;
                }
            }

            return false;
        }
    }
}