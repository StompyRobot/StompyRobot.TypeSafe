using System;
using System.Reflection;
using UnityEditorInternal;

namespace TypeSafe.Editor.Unity
{
    internal static class InternalEditorUtilityWrapper
    {
        private static PropertyInfo _sortingLayerNamesProperty;
        private static PropertyInfo _sortingLayerUniqueIdsProperty;
        private static readonly bool _isInited = false;

        public static string[] sortingLayerNames
        {
            get
            {
                if (!_isInited)
                {
                    Init();
                }

                if (_sortingLayerNamesProperty == null)
                {
                    return new string[0];
                }

                return (string[]) (_sortingLayerNamesProperty.GetValue(null, null));
            }
        }

        public static int[] sortingLayerUniqueIDs
        {
            get
            {
                if (!_isInited)
                {
                    Init();
                }

                if (_sortingLayerUniqueIdsProperty == null)
                {
                    return new int[0];
                }

                return (int[]) (_sortingLayerUniqueIdsProperty.GetValue(null, null));
            }
        }

        private static void Init()
        {
            try
            {
                _sortingLayerNamesProperty = typeof (InternalEditorUtility).GetProperty("sortingLayerNames",
                    BindingFlags.Static | BindingFlags.NonPublic);
                _sortingLayerUniqueIdsProperty = typeof (InternalEditorUtility).GetProperty("sortingLayerUniqueIDs",
                    BindingFlags.Static | BindingFlags.NonPublic);
            }
            catch (Exception e)
            {
                TSLog.LogError(LogCategory.Info,
                    "Error accessing sorting layers. See TypeSafe.log for more information.");
                TSLog.LogError(LogCategory.Trace, e.ToString());
            }
        }
    }
}
