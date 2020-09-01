using TypeSafe.Editor.Unity;

namespace TypeSafe.Editor
{
    /// <summary>
    /// Methods exposed for public access to allow for invoking TypeSafe via editor scripts
    /// </summary>
    public static class TypeSafeApi
    {
        /// <summary>
        /// Accessors for public TypeSafe settings
        /// </summary>
        public static class Settings
        {
            /// <summary>
            /// Enable automatic rebuild when changes are detected to the project.
            /// </summary>
            public static bool AutoRebuild
            {
                get { return Editor.Settings.Instance.AutoRebuild; }
                set
                {
                    TSLog.Log(LogCategory.Trace, string.Format("TypeSafeApi.Settings.AutoRebuild={0}", value));
                    Editor.Settings.Instance.AutoRebuild = value;
                    Editor.Settings.Instance.Save();
                }
            }

            /// <summary>
            /// Include scenes listed in the build settings window that are 'unchecked'
            /// </summary>
            public static bool IncludeNonActiveScenes
            {
                get { return Editor.Settings.Instance.IncludeNonActiveScenes; }
                set
                {
                    TSLog.Log(LogCategory.Trace, string.Format("TypeSafeApi.Settings.IncludeNonActiveScenes={0}", value));
                    Editor.Settings.Instance.IncludeNonActiveScenes = value;
                    Editor.Settings.Instance.Save();
                }
            }
        }

        /// <summary>
        /// True if TypeSafe is currently scanning or compiling.
        /// </summary>
        public static bool IsBusy
        {
	        get { return TypeSafeController.Instance.State != States.Idle; }
        }

        /// <summary>
        /// Queue a new scan/compile process. Will do nothing and print a warning if <c>IsBusy</c> is true.
        /// </summary>
        public static void QueueRefresh()
        {
            TSLog.Log(LogCategory.Trace, "TypeSafeApi.QueueRefresh");
            TypeSafeController.Instance.Queue(true);
        }

        /// <summary>
        /// If a TypeSafe scan is currently in progress, cancel it.
        /// </summary>
        public static void Cancel()
        {
            TSLog.Log(LogCategory.Trace, "TypeSafeApi.Cancel");
            TypeSafeController.Instance.Cancel();
        }

        /// <summary>
        /// Format a string with the class prefix/suffix specified in the Settings window.
        /// </summary>
        /// <param name="baseName"></param>
        /// <returns>A string in the format {prefix}{baseName}{suffix}</returns>
        public static string FormatTypeName(string baseName)
        {
            return TypeSafeUtil.GetFinalClassName(baseName);
        }
        
        /// <summary>
        /// Open the TypeSafe settings window.
        /// </summary>
        public static void OpenSettingsWindow()
        {
            SettingsWindow.Open();
        }  
              
        /// <summary>
        /// Open the TypeSafe welcome window.
        /// </summary>
        public static void OpenWelcomeWindow()
        {
            WelcomeWindow.Open();
        }

        /// <summary>
        /// Clear the internal cache of custom asset types
        /// </summary>
        public static void ClearAssetTypeCache()
        {
            AssetTypeCache.ClearCache();
        }
    }
}
