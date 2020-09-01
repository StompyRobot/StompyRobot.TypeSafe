using UnityEditor;

namespace TypeSafe.Editor
{
    /// <summary>
    /// Class containing all interaction from the user to TypeSafe (menus etc)
    /// </summary>
    static class MenuItems
    {
        [MenuItem("Window/TypeSafe/Open Settings", false, 0)]
        public static void OpenSettingsWindow()
        {
            TypeSafeApi.OpenSettingsWindow();
        }

        [MenuItem("Window/TypeSafe/Open Guide", false, 1)]
        public static void OpenWelcomeWindow()
        {
            TypeSafeApi.OpenWelcomeWindow();
        }
        
        [MenuItem("Assets/TypeSafe Refresh", false, 0)]
        public static void Refresh()
        {
            TypeSafeApi.QueueRefresh();
        }

        [MenuItem("Assets/TypeSafe Refresh", true)]
        public static bool RefreshValidate()
        {
            if (TypeSafeApi.IsBusy)
            {
                return false;
            }

            return true;
        }
    }
}
