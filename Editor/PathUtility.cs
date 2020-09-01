using System;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using UnityEditor;
using UnityEngine;

namespace TypeSafe.Editor
{
    internal class PathUtility
    {
        public static bool IsAssetResource(string path)
        {
            var components = path.Split('/');

            if (components.Any(IsFolderFiltered))
            {
                return false;
            }

            return components.Any(p => p.ToLower() == "resources");
        }

        public static bool IsFolderFiltered(string folderName)
        {
            if (Strings.ConstExcludedFolders.Contains(folderName, StringComparer.InvariantCultureIgnoreCase))
            {
                return true;
            }

            return false;
        }

        public static bool IsInBlacklist(string path)
        {
            return Settings.Instance.ResourceFolderFilter.Any(
                p => path.ToLowerInvariant().Contains(p.ToLowerInvariant()));
        }

        public static bool IsInWhitelist(string path)
        {
            return Settings.Instance.Whitelist.Any(p => path.ToLowerInvariant().Contains(p.ToLowerInvariant()));
        }

        public static string GetBuildTempDirectory()
        {
            return Path.Combine(Path.Combine(Environment.CurrentDirectory, "Temp"), "TypeSafe");
        }

        public static string GetBackupDirectory()
        {
            return Path.Combine(Path.Combine(Environment.CurrentDirectory, "Library"), "TypeSafe");
        }

        public static string GetLogFilePath()
        {
            return Path.Combine(Path.Combine(Environment.CurrentDirectory, "Temp"), "TypeSafe.log");
        }

        public static string GetDeployDirectory(string custom = null)
        {
            var path = custom != null ? custom : Settings.Instance.OutputDirectory;

            path = path.Replace(Strings.TypeSafePathKey, GetTypeSafeProjectPath());

            if (!Path.IsPathRooted(path) && !path.StartsWith("Assets/"))
            {
                path = Path.Combine("Assets", path);
            }

            path = Path.GetFullPath(path);

            return path;
        }

        public static string PathRelativeTo(string path, string relativeTo)
        {
            var pathUri = new Uri(path);

            // Folders must end in a slash
            if (!relativeTo.EndsWith(Path.DirectorySeparatorChar.ToString()))
            {
                relativeTo += Path.DirectorySeparatorChar;
            }
            var folderUri = new Uri(relativeTo);

            return
                Uri.UnescapeDataString(folderUri.MakeRelativeUri(pathUri)
                                                .ToString()
                                                .Replace('/', Path.DirectorySeparatorChar));
        }

        public static string GetSettingsPath()
        {
            return "Assets/TypeSafe/Editor/Settings.asset";
        }

        public static string GetTypeCachePath()
        {
            return "Assets/TypeSafe/Editor/Cache.asset";
        }

		private static string _rootPath;

		private static string RootPath
		{
			get
			{
				if (_rootPath != null)
				{
					return _rootPath;
				}
				string rootAsset = AssetDatabase.FindAssets("PathUtility t:Script").FirstOrDefault();
				if (rootAsset == null)
				{
					throw new Exception();
				}

				string assetPath = AssetDatabase.GUIDToAssetPath(rootAsset);
				string rootPath = Path.GetDirectoryName(Path.GetDirectoryName(assetPath)).Replace('\\', '/');
				TSLog.Log(LogCategory.Trace, string.Format("Root Path: {0}", rootPath));

				return _rootPath = rootPath;
            }
		}

        public static string GetTypeSafeEditorPath()
        {
	        return Path.Combine(RootPath, "Editor");
        }

        public static string GetTypeSafePath()
        {
	        return RootPath;
        }

        /// <summary>
        /// Get path in the current unity project where TypeSafe will place output.
        /// </summary>
        /// <returns></returns>
        public static string GetTypeSafeProjectPath()
        {
            return Path.Combine(Application.dataPath, "TypeSafe");
        }

        public static string GetFileHash(string filePath)
        {
            using (var md5 = MD5.Create())
            {
                using (var stream = File.OpenRead(filePath))
                {
                    return BitConverter.ToString(md5.ComputeHash(stream));
                }
            }
        }

        public static void EnsureDirectoryExists(string path)
        {
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
        }

        /// <summary>
        /// Determines if a path is part of TypeSafe
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public static bool IsTypeSafePath(string path)
        {
            if (path.StartsWith(GetTypeSafePath()))
            {
                return true;
            }

            return false;
        }
    }
}
