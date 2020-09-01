using UnityEditor;
using UnityEngine;

namespace TypeSafe.Editor.Unity
{
    internal static class TypeSafeGUI
    {
        private static GUIStyle _style;

        public static void DrawWelcomeLogo()
        {
            var logo = EditorGUIUtility.isProSkin ? Embedded.WelcomeLogoDark : Embedded.WelcomeLogoLight;

            DrawLogo(logo);
        }

        public static void DrawSettingsLogo()
        {
            var logo = EditorGUIUtility.isProSkin ? Embedded.SettingsLogoDark : Embedded.SettingsLogoLight;

            DrawLogo(logo);
        }

        public static void DrawLogo(Texture2D logo)
        {
            if (logo == null)
            {
                TSLog.LogWarning(LogCategory.Trace, "Logo is null");
                return;
            }

            var rect = EditorGUILayout.BeginVertical();

            GUILayout.Space(20);

            EditorGUILayout.BeginHorizontal();

            GUILayout.FlexibleSpace();

            GUI.DrawTexture(
                GUILayoutUtility.GetRect(logo.width, logo.width, logo.height, logo.height, GUILayout.ExpandHeight(false),
                    GUILayout.ExpandWidth(false)),
                logo);

            GUILayout.FlexibleSpace();

            EditorGUILayout.EndHorizontal();

            GUILayout.Space(15);

            EditorGUILayout.EndVertical();

            var size = EditorStyles.miniLabel.CalcSize(new GUIContent(Strings.Version));
            GUI.Label(new Rect(rect.xMax - size.x, rect.yMax - size.y, size.x, size.y), Strings.Version,
                EditorStyles.miniLabel);
        }

        public static void BeginDrawBackground()
        {
            if (_style == null)
            {
                _style = new GUIStyle();
                _style.margin = _style.padding = new RectOffset(0, 0, 0, 0);
            }

            var bg = EditorGUIUtility.isProSkin ? Embedded.BGDark : Embedded.BGLight;

            var rect = EditorGUILayout.BeginVertical(_style);

            DrawTextureTiled(rect, bg);
        }

        public static void EndDrawBackground()
        {
            EditorGUILayout.EndVertical();
        }

        public static void DrawScreenshot(Texture2D tex)
        {
            EditorGUILayout.Space();

            EditorGUILayout.BeginHorizontal();

            GUILayout.FlexibleSpace();

            EditorGUILayout.BeginHorizontal(Styles.Screenshot);

            GUI.DrawTexture(
                GUILayoutUtility.GetRect(tex.width, tex.width, tex.height, tex.height, GUILayout.ExpandHeight(false),
                    GUILayout.ExpandWidth(false)),
                tex);

            EditorGUILayout.EndHorizontal();

            GUILayout.FlexibleSpace();

            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space();
        }

        public static void DrawTextureTiled(Rect rect, Texture2D tex)
        {
            GUI.BeginGroup(rect);

            var tilesX = Mathf.Max(1, Mathf.CeilToInt(rect.width/tex.width));
            var tilesY = Mathf.Max(1, Mathf.CeilToInt(rect.height/tex.height));

            for (var x = 0; x < tilesX; x++)
            {
                for (var y = 0; y < tilesY; y++)
                {
                    var pos = new Rect(x*tex.width, y*tex.height, tex.width, tex.height);
                    pos.x += rect.x;
                    pos.y += rect.y;

                    GUI.DrawTexture(pos, tex, ScaleMode.ScaleAndCrop);
                }
            }

            GUI.EndGroup();
        }

        public static bool ClickableLabel(string text, GUIStyle style)
        {
            var rect = EditorGUILayout.BeginVertical(Styles.NoPaddingNoMargin);

            GUILayout.Label(text, style);

            EditorGUILayout.EndVertical();

            if (Event.current.type == EventType.MouseUp && rect.Contains(Event.current.mousePosition))
            {
                return true;
            }

            return false;
        }
    }
}
