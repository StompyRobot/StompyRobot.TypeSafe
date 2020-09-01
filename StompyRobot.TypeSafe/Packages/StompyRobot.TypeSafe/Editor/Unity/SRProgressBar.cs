using System;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace TypeSafe.Editor
{
    internal static class SRProgressBar
    {
        private static readonly MethodInfo _displayMethod;
        private static readonly MethodInfo _clearMethod;
        private static readonly bool _asyncSupported;

        static SRProgressBar()
        {
            var t = typeof (EditorUtility).Assembly.GetType("UnityEditor.AsyncProgressBar");

            if (t != null)
            {
                _displayMethod = t.GetMethod("Display", BindingFlags.Static | BindingFlags.Public, null,
                    new[] {typeof (string), typeof (float)}, null);

                _clearMethod = t.GetMethod("Clear", BindingFlags.Static | BindingFlags.Public, null, new Type[0], null);
            }

            if (_displayMethod == null || _clearMethod == null)
            {
                TSLog.LogWarning(LogCategory.Trace, string.Format("[TypeSafe] Error finding AsyncProgressBar ({0})", t));
                _asyncSupported = false;
            }
            else
            {
                _asyncSupported = true;
            }
        }

        public static void Display(string title, string info, float progress)
        {
            progress = Mathf.Clamp01(progress);

            if (!_asyncSupported)
            {
                EditorUtility.DisplayProgressBar(title, info, progress);
                return;
            }

#if UNITY_5
            _displayMethod.Invoke(null, new object[] {title + " - " + info, progress});
#else
			_displayMethod.Invoke(null, new object[] {info, progress});
#endif
        }

        public static void Clear()
        {
            if (!_asyncSupported)
            {
                EditorUtility.ClearProgressBar();
                return;
            }

            _clearMethod.Invoke(null, new object[0]);
        }
    }
}
