using System.IO;
using UnityEditor;
using UnityEngine;

namespace TypeSafe.Editor
{
    internal static class Embedded
    {
        private static Texture2D _settingsLogoDark;
        private static Texture2D _settingsLogoLight;
        private static Texture2D _welcomeLogoDark;
        private static Texture2D _welcomeLogoLight;
        private static Texture2D _bgDark;
        private static Texture2D _bgLight;
        private static Texture2D _resourcesDemo;
        private static Texture2D _usageDemo;

        public static Texture2D SettingsLogoDark
        {
            get
            {
                if (_settingsLogoDark == null)
                {
                    _settingsLogoDark = ReadTexture("Settings_DarkBG.png");
                }

                return _settingsLogoDark;
            }
        }

        public static Texture2D SettingsLogoLight
        {
            get
            {
                if (_settingsLogoLight == null)
                {
                    _settingsLogoLight = ReadTexture("Settings_LightBG.png");
                }

                return _settingsLogoLight;
            }
        }

        public static Texture2D WelcomeLogoDark
        {
            get
            {
                if (_welcomeLogoDark == null)
                {
                    _welcomeLogoDark = ReadTexture("Welcome_DarkBG.png");
                }

                return _welcomeLogoDark;
            }
        }

        public static Texture2D WelcomeLogoLight
        {
            get
            {
                if (_welcomeLogoLight == null)
                {
                    _welcomeLogoLight = ReadTexture("Welcome_LightBG.png");
                }

                return _welcomeLogoLight;
            }
        }

        public static Texture2D BGDark
        {
            get
            {
                if (_bgDark == null)
                {
                    _bgDark = ReadTexture("BG_Dark.png");
                    _bgDark.wrapMode = TextureWrapMode.Repeat;
                }

                return _bgDark;
            }
        }

        public static Texture2D BGLight
        {
            get
            {
                if (_bgLight == null)
                {
                    _bgLight = ReadTexture("BG_Light.png");
                    _bgLight.wrapMode = TextureWrapMode.Repeat;
                }

                return _bgLight;
            }
        }

        public static Texture2D ResourcesDemo
        {
            get
            {
                if (_resourcesDemo == null)
                {
                    _resourcesDemo = ReadTexture("ResourcesDemo.png");
                }

                return _resourcesDemo;
            }
        }

        public static Texture2D UsageDemo
        {
            get
            {
                if (_usageDemo == null)
                {
                    _usageDemo = ReadTexture("UsageDemo.png");
                }

                return _usageDemo;
            }
        }

        private static Texture2D ReadTexture(string path)
        {
	        string assetPath = Path.Combine(Path.Combine(PathUtility.GetTypeSafeEditorPath(), "UI"), path);
	        TSLog.Log(LogCategory.Trace, string.Format("Loading Embedded Texture: {0} from {1}", path, assetPath));

	        return AssetDatabase.LoadAssetAtPath<Texture2D>(assetPath);
        }
    }
}
