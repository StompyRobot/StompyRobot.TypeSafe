using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;

namespace TypeSafe.Editor
{
    internal static class TypeSafeUtil
    {
        private static readonly IList<string> TypeSafeClassNames = new ReadOnlyCollection<string>(new[]
        {
            Strings.ResourcesTypeName,
            Strings.LayersTypeName,
            Strings.LayerMaskTypeName,
            Strings.SortingLayersTypeName,
            Strings.TagsTypeName,
            Strings.ScenesTypeName,
            Strings.InputTypeName,
            Strings.AudioMixersName,
            Strings.AnimatorsName
        });

        private static bool? _isEnabled;
        private static bool _codeRefreshCompleted;

        public static bool IsEnabled()
        {
            if (_isEnabled.HasValue)
            {
                return _isEnabled.Value;
            }

            _isEnabled = !Environment.GetCommandLineArgs().Contains(Strings.DisableCommandLineParam);

            if (!_isEnabled.Value)
            {
                TSLog.Log(LogCategory.Trace, "TypeSafe is disabled via command line.");
                return false;
            }

            // Force TypeSafe to be disabled if the assembly is missing (likely the user has deleted TypeSafe)
            if (!File.Exists(typeof (TypeSafeUtil).Assembly.Location))
            {
                TSLog.Log(LogCategory.Trace, "TypeSafe assembly is missing, IsEnabled() = false");
                _isEnabled = false;
            }

            return _isEnabled.Value;
        }

        public static bool IsCodeRefreshInProgress()
        {
            return !_codeRefreshCompleted;
        }

        public static void ReportCodeRefreshStarted()
        {
            TSLog.Log(LogCategory.Trace, "Code Refresh Started");
            _codeRefreshCompleted = false;
        }

        public static void ReportCodeRefreshCompleted()
        {
            if (_codeRefreshCompleted)
            {
                return;
            }

            TSLog.Log(LogCategory.Trace, "Code Refresh Completed");

            _codeRefreshCompleted = true;
        }

        /// <summary>
        /// Get a list of base class names used by TypeSafe
        /// </summary>
        public static IList<string> GetTypeSafeClassNames()
        {
            return TypeSafeClassNames;
        }

        /// <summary>
        /// Check if TypeSafe should be scanning or compiling with the current Unity Editor state.
        /// </summary>
        /// <returns>True if operating is permitted, otherwise false.</returns>
        public static bool ShouldBeOperating()
        {
            if (EditorApplication.isCompiling || EditorApplication.isPlayingOrWillChangePlaymode ||
                IsCodeRefreshInProgress())
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// Given a long error message in the format "{0} - {1}", return only {0}.
        /// </summary>
        public static string GetShortErrorMessage(string errorMessage)
        {
            return errorMessage.Split(new[] {" - "}, StringSplitOptions.None)[0];
        }

        /// <summary>
        /// Copy a collection of file paths to the deploy path and refresh the Unity asset database for the target files.
        /// This will check for file changes before copying to reduce unnecessary recompiles.
        /// </summary>
        /// <param name="paths">Collection of paths to deploy.</param>
        /// <param name="fileCount">Number of files that have been updated.</param>
        /// <param name="dryRun">If true, skip the actual copying of the file, just check for differences</param>
        /// <returns>True if successfully deployed.</returns>
        public static bool DeployBuildArtifacts(ICollection<string> paths, out int fileCount, bool dryRun = false)
        {
            TSLog.Log(LogCategory.Trace, "Deploying Build Artifacts");

            fileCount = 0;
            var error = false;

            var deployDirectory = PathUtility.GetDeployDirectory();
            TSLog.Log(LogCategory.Trace, "Deploy Directory: " + deployDirectory);

            if (!Directory.Exists(deployDirectory) && !dryRun)
            {
                try
                {
                    TSLog.Log(LogCategory.Trace, string.Format("Creating deploy directory @ {0}", deployDirectory));

                    Directory.CreateDirectory(deployDirectory);
                }
                catch (Exception e)
                {
                    TSLog.LogError(LogCategory.Info, string.Format("Error creating deploy directory @ {0}", deployDirectory));
                    TSLog.LogError(LogCategory.Info, e.ToString());
                    return false;
                }
            }

            try
            {
                AssetDatabase.StartAssetEditing();

                foreach (var s in paths)
                {
                    var destPath = deployDirectory + "/" + Path.GetFileName(s);

                    if (File.Exists(destPath) && PathUtility.GetFileHash(destPath) == PathUtility.GetFileHash(s))
                    {
                        if (!dryRun)
                        {
                            TSLog.Log(LogCategory.Trace, string.Format("Ignoring File {0} -> {1} (no changes)", s, destPath));
                        }

                        continue;
                    }

                    ++fileCount;

                    if (dryRun)
                    {
                        continue;
                    }

                    TSLog.Log(LogCategory.Trace, string.Format("Copying File {0} -> {1}", s, destPath));

                    try
                    {
                        File.Copy(s, destPath, true);
                        AssetDatabase.ImportAsset(PathUtility.PathRelativeTo(destPath, Environment.CurrentDirectory));
                    }
                    catch (Exception exception)
                    {
                        TSLog.LogError(LogCategory.Info, string.Format("Error copying file {0} -> {1}", s, destPath));
                        TSLog.LogError(LogCategory.Info, exception.ToString());
                        error = true;
                    }
                }
            }
            finally
            {
                AssetDatabase.StopAssetEditing();
            }

            return !error;
        }

        /// <summary>
        /// Remove all generated files from the Unity project directory
        /// </summary>
        public static void Clean()
        {
            TSLog.Log(LogCategory.Trace, "Cleaning Deploy Directory");

            var directory = PathUtility.GetDeployDirectory();

            TSLog.Log(LogCategory.Trace, "Deploy Directory: " + directory);

            foreach (var file in Directory.GetFiles(directory, "*.Generated.*"))
            {
                if (file.EndsWith(".meta"))
                {
                    continue;
                }

                var path = PathUtility.PathRelativeTo(file, Environment.CurrentDirectory);

                TSLog.Log(LogCategory.Trace, string.Format("Deleting {0}", file));

                try
                {
                    if (!FileContains(path, Strings.TypeSafeInternalTagFieldName))
                    {
                        TSLog.LogWarning(LogCategory.Trace, "File does not contain TypeSafe tag, skipped deletion.");
                        continue;
                    }

                    if (!AssetDatabase.DeleteAsset(path))
                    {
                        TSLog.LogError(LogCategory.Info,
	                        string.Format("Error deleting {0} with AssetDatabase. Attempting File.Delete", path));
                        File.Delete(path);
                    }
                }
                catch (Exception e)
                {
                    TSLog.LogError(LogCategory.Info, string.Format("Error deleting {0}", path));
                    TSLog.LogError(LogCategory.Info, e.ToString());
                }
            }
        }

        public static bool FileContains(string path, string check)
        {
            using (var fileStream = File.OpenRead(path))
            {
                using (var reader = new StreamReader(fileStream))
                {
                    string f;

                    while (reader.Peek() >= 0)
                    {
                        f = reader.ReadLine();
                        if (f.Contains(check))
                        {
                            return true;
                        }
                    }
                }
            }

            return false;
        }

        /// <summary>
        /// Determine if a type was generated by TypeSafe.
        /// </summary>
        /// <param name="type">Type to check</param>
        /// <returns>True if generated by TypeSafe, otherwise false.</returns>
        public static bool IsTypeSafeGenerated(Type type)
        {
            if (type == null)
            {
                throw new ArgumentNullException("type");
            }

            // Check for the type tag that is added by TypeSafe at compile time.
            if (
                type.GetMember(Strings.TypeSafeInternalTagFieldName, BindingFlags.NonPublic | BindingFlags.Static)
                    .Length == 0)
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// Perform a full validation check on a type name, including checking for conflicts with existing non-TypeSafe
        /// classes.
        /// </summary>
        /// <param name="className">The 'base' class name, e.g. 'Resources'</param>
        /// <param name="ns">The namespace that the class name will be placed in.</param>
        /// <param name="prefix">A prefix to apply to <paramref name="className" />, or null</param>
        /// <param name="suffix">A suffix to apply to <paramref name="className" />, or null</param>
        /// <param name="errorMessage">String will be populated with the error message if validation fails.</param>
        /// <returns>True if validation succeeds, otherwise false.</returns>
        public static bool ValidateTypeName(string className, string ns, string prefix, string suffix,
            out string errorMessage)
        {
            if (ns == null)
            {
                ns = "";
            }

            if (prefix == null)
            {
                prefix = "";
            }

            if (suffix == null)
            {
                suffix = "";
            }

            var fullName = string.Format("{0}{1}{2}", prefix, className, suffix);

            if (!ValidateTypeName(fullName, out errorMessage))
            {
                return false;
            }

            try
            {
                errorMessage = Strings.Error_ConflictsWithExistingType;

                if (string.IsNullOrEmpty(ns))
                {
                    if (!TestTypeNameConflicts(fullName))
                    {
                        return false;
                    }

                    fullName = "UnityEngine." + fullName;

                    if (!TestTypeNameConflicts(fullName))
                    {
                        return false;
                    }
                }
                else
                {
                    fullName = ns + "." + fullName;

                    if (!TestTypeNameConflicts(fullName))
                    {
                        return false;
                    }
                }

                errorMessage = "";
                return true;
            }
            catch (Exception e)
            {
                errorMessage = e.Message;
                return false;
            }
        }

        public static bool TestTypeNameConflicts(string typeName)
        {
            var type = FindType(typeName);

            if (type == null)
            {
                return true;
            }

            if (IsTypeSafeGenerated(type))
            {
                return true;
            }

            return false;
        }

        /// <summary>
        /// Remove any characters that are forbidden in a type name.
        /// </summary>
        /// <param name="n">String to filter</param>
        /// <returns></returns>
        public static string FilterTypeName(string n)
        {
            var regex = "[^0-9a-zA-Z_]+";

            var safeName = Regex.Replace(n, regex, "");

            // Remove numbers from the start
            while (safeName.Length > 0 && char.IsNumber(safeName[0]))
            {
                safeName = safeName.Substring(1);
            }

            return safeName;
        }

        /// <summary>
        /// Remove any characters that are forbidden in a namespace name.
        /// </summary>
        /// <param name="n">String to filter</param>
        /// <returns></returns>
        public static string FilterNamespaceName(string n)
        {
            var regex = "[^\\.0-9a-zA-Z_]+";

            var safeName = Regex.Replace(n, regex, "");

            // Remove numbers and periods from the start
            while (safeName.Length > 0 && char.IsNumber(safeName[0]) || safeName[0] == '.')
            {
                safeName = safeName.Substring(1);
            }

            // Remove periods from the end
            while (safeName.Length > 0 && safeName[safeName.Length - 1] == '.')
            {
                safeName = safeName.Substring(0, safeName.Length - 1);
            }

            return safeName;
        }

        /// <summary>
        /// Perform validation on a string as a type name.
        /// </summary>
        /// <param name="n">Type name to validate</param>
        /// <param name="errorMessage">String to be filled with the error message if validation failed.</param>
        /// <returns>True if validation succeeded, overwise false. <paramref name="errorMessage" /> contains the error if false.</returns>
        public static bool ValidateTypeName(string n, out string errorMessage)
        {
            var permittedCharacters = "^[0-9a-zA-Z_]+$";

            if (!Regex.Match(n, permittedCharacters).Success)
            {
                errorMessage = Strings.Error_InvalidCharactersTypeName;
                return false;
            }

            if (char.IsDigit(n.First()))
            {
                errorMessage = Strings.Error_NameMustNotStartWithNumber;
                return false;
            }

            errorMessage = "";
            return true;
        }

        /// <summary>
        /// Perform validation on a string as a namespace name.
        /// </summary>
        /// <param name="n">Namespace to validate</param>
        /// <param name="errorMessage">String to be filled with the error message if validation failed.</param>
        /// <returns>True if validation succeeded, overwise false. <paramref name="errorMessage" /> contains the error if false.</returns>
        public static bool ValidateNamespaceName(string n, out string errorMessage)
        {
            var permittedCharacters = "^[0-9a-zA-Z_\\.]+$";

            if (!Regex.Match(n, permittedCharacters).Success)
            {
                errorMessage = Strings.Error_InvalidCharactersNamespace;
                return false;
            }

            if (n.First() == '.' || n.Last() == '.')
            {
                errorMessage = Strings.Error_NameMustNotStartOrEndWithPeriod;
                return false;
            }

            if (char.IsDigit(n.First()))
            {
                errorMessage = Strings.Error_NameMustNotStartWithNumber;
                return false;
            }

            errorMessage = "";
            return true;
        }

        /// <summary>
        /// Find a type by name in any assemblies loaded in the current AppDomain
        /// </summary>
        /// <param name="qualifiedTypeName">Type name to search for</param>
        /// <returns>The found <c>Type</c>, or null if not found.</returns>
        public static Type FindType(string qualifiedTypeName)
        {
            var t = Type.GetType(qualifiedTypeName);

            if (t != null)
            {
                return t;
            }

            foreach (var asm in AppDomain.CurrentDomain.GetAssemblies())
            {
                t = asm.GetType(qualifiedTypeName);
                if (t != null)
                {
                    return t;
                }
            }

            return null;
        }

        /// <summary>
        /// Checks that the current Unity version matches the version this assembly was compiled for. Will print a warning if
        /// mismatch.
        /// </summary>
        /// <returns>True if matches, otherwise false.</returns>
        public static bool EnsureCorrectUnityVersion()
        {
            var unityVersion = Application.unityVersion;

            if (unityVersion.StartsWith("4.") || unityVersion.StartsWith("5.") &&
                !(unityVersion.StartsWith("5.4.") || unityVersion.StartsWith("5.5.") ||
                  unityVersion.StartsWith("5.6.") || unityVersion.StartsWith("5.7.")))
            {
                TSLog.LogWarning(LogCategory.Info,
                    string.Format(Strings.Warning_VersionMismatch));
                return false;
            }

            return true;
        }

        public static string GetFinalClassName(string baseName)
        {
            return string.Format("{0}{1}{2}", Settings.Instance.ClassNamePrefix, baseName, Settings.Instance.ClassNameSuffix);
        }

        /// <summary>
        /// Returns true if TypeSafe has performed a generation and the artifacts exist in the project.
        /// </summary>
        public static bool HasGeneratedBefore()
        {
            return File.Exists(Path.Combine(PathUtility.GetDeployDirectory(), "Resources.Generated.cs"));
        }

        public static IEnumerable<ITypeSafeDataSource> GetCustomDataSources()
        {
            return GetInterfaceInstances<ITypeSafeDataSource>();
        }

        private static IEnumerable<Assembly> GetReflectionAssemblies()
        {
            // Workaround for exception when calling GetTypes() on Accessibility.dll
            return AppDomain.CurrentDomain.GetAssemblies().Where(p => !p.FullName.StartsWith("Accessibility"));
        }

        public static IEnumerable<Type> GetCustomDataSourcesTypes()
        {
            var interfaceType = typeof(ITypeSafeDataSource);

            var types = GetReflectionAssemblies().SelectMany(p => p.GetTypes().Where(t => interfaceType.IsAssignableFrom(t) && !t.IsAbstract));

            return types;
        }

        private static IEnumerable<T> GetInterfaceInstances<T>() where T : class
        {
            var interfaceType = typeof (T);

            var types = GetReflectionAssemblies().SelectMany(p => p.GetTypes().Where(t => interfaceType.IsAssignableFrom(t) && !t.IsAbstract));

            foreach (var type in types)
            {
                TSLog.Log(LogCategory.Trace, string.Format("Creating Data Source: {0}", type.FullName));

                T dataSource = null;

                try
                {
                    dataSource = (T) Activator.CreateInstance(type);
                }
                catch (Exception e)
                {
                    TSLog.LogError(LogCategory.Info, string.Format("Error creating instance ({0})", type.FullName));
                    TSLog.LogError(LogCategory.Info, e.ToString());
                }

                if (dataSource == null)
                {
                    TSLog.LogError(LogCategory.Info, string.Format("Error creating instance ({0})", type.FullName));
                    continue;
                }

                yield return dataSource;
            }
        }

        public static bool ContainsTypeSafeTrackedAsset(IList<string> paths)
        {
            foreach (var path in paths)
            {
                var guid = AssetDatabase.AssetPathToGUID(path);

                if (Settings.Instance.AudioMixers.Contains(guid) || Settings.Instance.Animators.Contains(guid))
                {
                    return true;
                }
            }

            return false;
        }

        public static bool CheckForRemovedAssets()
        {
            var removed = false;

            if (CheckGuidListForRemovedAssets(Settings.Instance.AudioMixers))
            {
                removed = true;
            }

            if (CheckGuidListForRemovedAssets(Settings.Instance.Animators))
            {
                removed = true;
            }

            if (removed)
            {
                Settings.Instance.Save();
            }
            return removed;
        }

        private static bool CheckGuidListForRemovedAssets(IList<string> list)
        {
            var removed = false;
            for (var i = list.Count - 1; i >= 0; i--)
            {
                var asset = AssetDatabase.GUIDToAssetPath(list[i]);
                if (string.IsNullOrEmpty(asset) || !File.Exists(asset))
                {
                    list.RemoveAt(i);
                    removed = true;
                }
            }

            return removed;
        }
    }
}
