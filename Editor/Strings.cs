using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using TypeSafe.Editor.Compiler;
using UnityEditor;

namespace TypeSafe.Editor
{
    internal static class Strings
    {
        public const string CommentHeaderPrefix =
            "------------------------------------------------------------------------------";

        public const string CommentHeaderSuffix =
            "------------------------------------------------------------------------------";

        public static readonly string[] CommentHeader =
        {
            @" _______   _____ ___ ___   _   ___ ___ ",
            @"|_   _\ \ / / _ \ __/ __| /_\ | __| __|",
            @"  | |  \ V /|  _/ _|\__ \/ _ \| _|| _| ",
            @"  |_|   |_| |_| |___|___/_/ \_\_| |___|"
        };

        public static readonly string[] CommentHeaderText =
        {
            "This file has been generated automatically by TypeSafe.",
            "Any changes to this file may be lost when it is regenerated.",
            "https://www.stompyrobot.uk/tools/typesafe"
        };

        public const string DisableCommandLineParam = "-tsDisableInit";

        public const string SettingsInspectorWarningText =
            "TypeSafe settings are designed to be edited from the TypeSafe Settings window.\n" +
            "Click below to open the settings window, or press Override to view the raw settings data.";

        public const string CacheInspectorWarningText =
            "This file contains a cache of resource types in your project.\n" +
            "You can exclude this file from your source control to reduce noise in your change sets.";

        public const string SettingsExcludeFoldersBrief =
            "Resources containing these strings in their path will be excluded from the scan process.";

        public const string SettingsWindowTitle = "TypeSafe Settings";

        public const string LogPrefix = "[TypeSafe] ";

        public const string DllName = "TypeSafe.Generated.dll";

        public const string SourceFileNameFormat = "{0}.Generated.cs";
        public const string CompileUnitUserDataKey = "SRName";

        public const string TypeSafePathKey = "$TYPESAFE$";
        public const string DefaultOutputPath = TypeSafePathKey + "/usr";

        public const string ResourcesTypeName = "Resources";
        public const string LayersTypeName = "Layers";
        public const string LayerMaskTypeName = "LayerMask";
        public const string SortingLayersTypeName = "SortingLayers";
        public const string TagsTypeName = "Tags";
        public const string ScenesTypeName = "Scenes";
        public const string InputTypeName = "Input";
        public const string AudioMixersName = "AudioMixers";
        public const string AnimatorsName = "Animators";

        public const string GetResourcesMethodName = "GetContents";
        public const string GetResourcesRecursiveMethodName = "GetContentsRecursive";

        public const string UnloadAllMethodName = "UnloadAll";
        public const string UnloadAllRecursiveMethodName = "UnloadAllRecursive";

        public const string ClearCacheMethodName = "ClearCache";

        public const string TypeSafeInternalTagFieldName = "_tsInternal";

        public static readonly IList<string> ReservedNames = new ReadOnlyCollection<string>(new[]
        {
            GetResourcesMethodName,
            GetResourcesRecursiveMethodName,
            UnloadAllMethodName,
            UnloadAllRecursiveMethodName,
            ClearCacheMethodName,
            "All",
            "__all",
            "None",
            ResourceCompilerUtil.CollectionMemberName,
            ResourceCompilerUtil.RecursiveLookupCacheName,
            TypeSafeInternalTagFieldName
        });

        public static readonly IList<string> ConstExcludedFolders = new ReadOnlyCollection<string>(new[]
        {
            "Editor"
        });

        public const string Version = AssemblyVersionInformation.Version;

        public const string Error_ConflictsWithExistingType = "Name Conflict - An existing type already has this name";

        public const string Error_NameMustNotStartWithNumber = "Name Error - Must not start with a number";

        public const string Error_NameMustNotStartOrEndWithPeriod = "Name Error - Must not start or end with a period";

        public const string Error_NameMustNotBeEmpty =
            "Name Error - Must not be empty";

        public const string Error_InvalidCharactersNamespace =
            "Invalid Characters - Only numbers, letters, underscores and periods permitted";

        public const string Error_InvalidCharactersTypeName =
            "Invalid Characters - Only numbers, letters and underscores permitted";

        public const string Warning_FolderNameCannotBeSameAsParent =
            "Folder name should not be the same as parent folder. ({0})";

        public const string Warning_ResourceNameCannotBeSameAsParent =
            "Resource name should not be the same as parent folder.";

        public const string Warning_NameCannotBeSameAsParent =
            "Member name should not be the same as parent.";

        public const string Warning_VersionMismatch =
            "This version of TypeSafe was compiled for a different version of Unity (expected 5.4 or newer). Download the package from the asset store to get a build for this version of Unity.";

        public const string Warning_NonActiveScene =
            "This scene is not enabled for production builds. This usage will fail at runtime.";

        public const string SettingsRateBoxContents =
            "If you like TypeSafe, please consider leaving a rating on the Asset Store.";

        public const string WebsiteUrl = "http://tiny.cc/typesafewebsite";
        public const string GithubUrl = "https://github.com/StompyRobot/StompyRobot.TypeSafe";
        public const string DocumentationUrl = "http://tiny.cc/typesafedocs";
        public const string AssetStoreUrl = "http://tiny.cc/typesafe";
        public const string SupportUrl = "http://tiny.cc/typesafesupport";

        public const string EditorPrefs_BoolWelcomeWindowShown = "TYPESAFE_WELCOME_SHOWN";
        public const string EditorPrefs_StringChangeLogVersion = "TYPESAFE_CHANGELOG";

        public static string LinkColour
        {
            get
            {
                if (EditorGUIUtility.isProSkin)
                {
                    return "#7C8CB9";
                }

                return "#0032E6";
            }
        }

        public const string GetContentsCommentSummary =
            "Return a read-only list of all resources in this folder.\n" +
            "This method has a very low performance cost, no need to cache the result.";

        public const string GetContentsCommentReturns =
            "A list of resource objects in this folder.";

        public const string GetContentsRecursiveCommentSummary =
            "Return a list of all resources in this folder and all sub-folders.\n" +
            "The result of this method is cached, so subsequent calls will have very low performance cost.";

        public const string GetContentsRecursiveCommentReturns =
            "A list of resource objects in this folder and sub-folders.";

        public const string GetContentsGenericCommentSummary =
            "Return an iterator of all resources in this folder of type <typeparamref name=\"TResource\"> (does not include sub-folders)\n" +
            "This method does not cache the result, so you should cache the result yourself if you will use it often. Convert to a list first if it will be iterated over multiple time.";

        public const string GetContentsGenericCommentReturns =
            "A list of <typeparamref>TResource</typeparamref> objects in this folder.";

        public const string GetContentsGenericRecursiveCommentSummary =
            "Return a iterator of all resources in this folder of type <typeparamref name=\"TResource\">, including sub-folders.\n" +
            "This method does not cache the result, so you should cache the result yourself if you will use it often. Convert to a list first if it will be iterated over multiple time.";

        public const string GetContentsGenericRecursiveCommentReturns =
            "A list of <typeparamref>TResource</typeparamref> objects in this folder and sub-folders.";

        public const string UnloadAllCommentSummary =
            "Call Unload() on every loaded resource in this folder.";

        public const string UnloadAllRecursiveCommentSummary =
            "Call Unload() on every loaded resource in this folder and subfolders.";

        public const string ClearCacheCommentSummary =
            "Clears any internal lists of assets that were cached by <see cref=\"GetContentsRecursive\"/>.";
    }
}
