using UnityEditor;
using UnityEngine;

namespace TypeSafe.Editor.Unity
{
    internal static class Styles
    {
        static Styles()
        {
            SettingsHeaderBoxStyle = new GUIStyle("OL Title");
            SettingsHeaderBoxStyle.padding = new RectOffset(0, 0, 0, 0);
            SettingsHeaderBoxStyle.margin = new RectOffset(0, 0, 0, 0);
            SettingsHeaderBoxStyle.clipping = TextClipping.Clip;
            SettingsHeaderBoxStyle.overflow = new RectOffset(0, 0, 0, 0);
            //SettingsHeaderBoxStyle.border = new RectOffset(1, 1, 1, 1);
            SettingsHeaderBoxStyle.fixedHeight = 0.5f;

            NamingPreviewGroupStyle = new GUIStyle();
            NamingPreviewGroupStyle.padding = new RectOffset(2, 2, 0, 2);
            NamingPreviewGroupStyle.margin = new RectOffset(2, 2, 0, 2);

            NamingPreviewHeaderGroupStyle = new GUIStyle();
            NamingPreviewHeaderGroupStyle.padding = new RectOffset(0, 0, 0, 0);
            NamingPreviewHeaderGroupStyle.margin = new RectOffset(2, 2, 2, 0);

            NamingPreviewRowEven = new GUIStyle("OL EntryBackEven");
            NamingPreviewRowEven.margin = new RectOffset(0, 0, 0, 0);
            NamingPreviewRowEven.padding = new RectOffset(4, 4, 4, 4);

            NamingPreviewRowOdd = new GUIStyle("OL EntryBackOdd");
            NamingPreviewRowOdd.margin = new RectOffset(0, 0, 0, 0);
            NamingPreviewRowOdd.padding = new RectOffset(4, 4, 4, 4);

            NamingPreviewTable = new GUIStyle("OL Box");
            NamingPreviewTable.margin = new RectOffset(4, 4, 4, 4);
            NamingPreviewTable.stretchWidth = true;
            NamingPreviewTable.stretchHeight = false;
            NamingPreviewTable.border = new RectOffset(2, 2, 2, 2);
            NamingPreviewTable.padding = new RectOffset(1, 1, 1, 1);
            //NamingPreviewTable.padding = new RectOffset(0, 0, 0, 0);

            var previewStyle = new GUIStyle(EditorStyles.label);
            previewStyle.alignment = TextAnchor.MiddleLeft;
            previewStyle.margin.left = 0;
            previewStyle.margin.right = 0;
            previewStyle.padding.left = 0;
            previewStyle.padding.right = 0;
            previewStyle.stretchWidth = false;

            NamingPreviewNamespaceLabel = new GUIStyle(previewStyle);
            NamingPreviewNamespaceLabel.alignment = TextAnchor.MiddleRight;
            NamingPreviewNamespaceLabel.normal.textColor = NamespaceColor;

            NamingPreviewSeperatorLabel = new GUIStyle(previewStyle);
            NamingPreviewSeperatorLabel.alignment = TextAnchor.MiddleCenter;

            NamingPreviewPrefixLabel = new GUIStyle(previewStyle);
            NamingPreviewPrefixLabel.normal.textColor = PrefixColour;

            NamingPreviewSuffixLabel = new GUIStyle(previewStyle);
            NamingPreviewSuffixLabel.normal.textColor = SuffixColour;

            NamingPreviewClassNameLabel = new GUIStyle(previewStyle);
            NamingPreviewClassNameLabel.fontStyle = FontStyle.Bold;

            NamingHeaderTextBox = new GUIStyle(EditorStyles.textField);
            NamingHeaderTextBox.margin.left = 0;
            NamingHeaderTextBox.margin.right = 0;
            NamingHeaderTextBox.stretchWidth = true;

            NamingHeaderNamespaceTextBox = new GUIStyle(NamingHeaderTextBox);
            NamingHeaderNamespaceTextBox.alignment = TextAnchor.MiddleRight;

            var baseHeaderLabel = new GUIStyle(EditorStyles.label);
            baseHeaderLabel.stretchWidth = true;
            baseHeaderLabel.alignment = TextAnchor.MiddleLeft;
            baseHeaderLabel.padding = NamingHeaderTextBox.padding;
            baseHeaderLabel.margin = NamingHeaderTextBox.margin;

            NamingHeaderNamespaceLabel = new GUIStyle(baseHeaderLabel);
            NamingHeaderNamespaceLabel.normal.textColor = NamespaceColor;

            NamingHeaderPrefixLabel = new GUIStyle(baseHeaderLabel);
            NamingHeaderPrefixLabel.normal.textColor = PrefixColour;

            NamingHeaderSuffixLabel = new GUIStyle(baseHeaderLabel);
            NamingHeaderSuffixLabel.normal.textColor = SuffixColour;

            HeaderLabel = new GUIStyle(EditorStyles.largeLabel);
            HeaderLabel.fontSize = 20;
            HeaderLabel.fontStyle = FontStyle.Normal;
            HeaderLabel.margin = new RectOffset(5, 5, 6, 5);

            ParagraphLabel = new GUIStyle(EditorStyles.label);
            //ParagraphLabel.fontSize = 24;
            ParagraphLabel.margin = new RectOffset(5, 5, 5, 5);
            ParagraphLabel.wordWrap = true;
            ParagraphLabel.richText = true;

            Screenshot = new GUIStyle();
            Screenshot.margin = new RectOffset(0, 0, 0, 0);
            Screenshot.padding = new RectOffset(2, 2, 2, 2);
            Screenshot.border = new RectOffset(2, 2, 2, 2);

            WelcomeTextBox = new GUIStyle("OL box");
            WelcomeTextBox.margin = new RectOffset(0, 0, 0, 0);
            WelcomeTextBox.border = new RectOffset(2, 2, 2, 2);
            WelcomeTextBox.padding = new RectOffset(1, 1, 1, 1);
            //NamingPreviewTable.padding = new RectOffset(0, 0, 0, 0);

            NoPaddingNoMargin = new GUIStyle();
            NoPaddingNoMargin.margin = new RectOffset(0, 0, 0, 0);
            NoPaddingNoMargin.padding = new RectOffset(0, 0, 0, 0);

            MiniLabelWrapped = new GUIStyle(EditorStyles.miniLabel);
            MiniLabelWrapped.wordWrap = true;
            MiniLabelWrapped.richText = true;
        }

        public static Color PrefixColour
        {
            get
            {
                if (EditorGUIUtility.isProSkin)
                {
                    return new Color32(127, 201, 122, 255);
                }

                return new Color32(107, 0, 229, 255);
            }
        }

        public static Color SuffixColour
        {
            get
            {
                if (EditorGUIUtility.isProSkin)
                {
                    return new Color32(124, 140, 185, 255);
                }

                return new Color32(0, 50, 230, 255);
            }
        }

        public static Color NamespaceColor
        {
            get
            {
                if (EditorGUIUtility.isProSkin)
                {
                    return new Color32(201, 201, 126, 255);
                }

                return new Color32(229, 19, 0, 255);
            }
        }

        public static GUIStyle SettingsHeaderBoxStyle { get; private set; }
        public static GUIStyle NamingPreviewGroupStyle { get; private set; }
        public static GUIStyle NamingPreviewHeaderGroupStyle { get; private set; }
        public static GUIStyle NamingPreviewRowEven { get; private set; }
        public static GUIStyle NamingPreviewRowOdd { get; private set; }
        public static GUIStyle NamingPreviewTable { get; private set; }
        public static GUIStyle NamingPreviewNamespaceLabel { get; private set; }
        public static GUIStyle NamingPreviewPrefixLabel { get; private set; }
        public static GUIStyle NamingPreviewClassNameLabel { get; private set; }
        public static GUIStyle NamingPreviewSeperatorLabel { get; private set; }
        public static GUIStyle NamingPreviewSuffixLabel { get; private set; }
        public static GUIStyle NamingHeaderNamespaceLabel { get; private set; }
        public static GUIStyle NamingHeaderPrefixLabel { get; private set; }
        public static GUIStyle NamingHeaderSuffixLabel { get; private set; }
        public static GUIStyle NamingHeaderNamespaceTextBox { get; private set; }
        public static GUIStyle NamingHeaderTextBox { get; private set; }
        public static GUIStyle HeaderLabel { get; private set; }
        public static GUIStyle ParagraphLabel { get; private set; }
        public static GUIStyle Screenshot { get; private set; }
        public static GUIStyle WelcomeTextBox { get; private set; }
        public static GUIStyle NoPaddingNoMargin { get; private set; }
        public static GUIStyle MiniLabelWrapped { get; private set; }
    }
}
