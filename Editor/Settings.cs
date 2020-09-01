using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace TypeSafe.Editor
{
    internal class Settings : ScriptableObject
    {
        private const string PatchOldVersion = "  m_Script: {fileID: 454367454, guid: 1b23d7222dc4f54439109010ec8c7eb2, type: 3}";
        private const string PatchNewVersion = "  m_Script: {fileID: 11500000, guid: 775148651662f65429083cade6d818ab, type: 3}";

        private static Settings _instance;
        public bool AutoRebuild = true;
        public string ClassNamePrefix = "SR";
        public string ClassNameSuffix = "";
        public bool EnableWaiting = true;
        public bool EnableWhitelist = false;
        public bool HasShownWelcome = false;
        public bool IncludeNonActiveScenes = false;
        public float MinimumBuildTime = 3.5f;
        public string Namespace = "";
        public string OutputDirectory = Strings.DefaultOutputPath;
        public List<string> ResourceFolderFilter = new List<string>();
        public bool TriggerOnInputChange = true;
        public bool TriggerOnLayerChange = true;
        public bool TriggerOnResourceChange = true;
        public bool TriggerOnSceneChange = true;
        public bool TriggerOnAssetChange = true;

        public List<string> Whitelist = new List<string>();
        public List<string> DisabledDataSources = new List<string>();

        /// <summary>
        /// List of AudioMixers GUIDs
        /// </summary>
        public List<string> AudioMixers = new List<string>();

        /// <summary>
        /// List of AnimatorControllers GUIDs
        /// </summary>
        public List<string> Animators = new List<string>();

        public static Settings Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = GetInstance();
                }

                return _instance;
            }
        }

        private static Settings GetInstance()
        {
            if (!TypeSafeUtil.IsEnabled())
            {
                throw new InvalidOperationException(
                    "Attempted to create settings instance while TypeSafe is disabled. Something should be checking TypeSafeUtil.IsEnabled() before running.");
            }

            var settingsPath = PathUtility.GetSettingsPath();

            TSLog.Log(LogCategory.Trace, string.Format("Checking for settings asset at {0}", settingsPath));

            var fileExists = File.Exists(settingsPath);

            var instance = AssetDatabase.LoadAssetAtPath<Settings>(settingsPath);
            if (instance == null && fileExists)
            {
                UpgradeFromOldVersion(settingsPath);

                TSLog.Log(LogCategory.Trace, "Settings asset exists, but AssetDatabase doesn't know it. Likely project reimport in progress.");
                return null;
            }

            if (instance == null)
            {
                TSLog.Log(LogCategory.Trace, string.Format("Settings asset not found at {0} (fileExists={1}, instance=null)", settingsPath, fileExists));

                // Create instance
                instance = CreateInstance<Settings>();

                TSLog.Log(LogCategory.Info, string.Format("Creating settings asset at {0}", settingsPath));

                try
                {
                    string parentDirectory = Path.GetDirectoryName(settingsPath);
                    if(parentDirectory != null && !Directory.Exists(parentDirectory))
                    {
                        Directory.CreateDirectory(parentDirectory);
                    }

                    AssetDatabase.CreateAsset(instance, settingsPath);
                }
                catch (Exception e)
                {
                    TSLog.LogError(LogCategory.Info, "Error creating settings asset.");
                    TSLog.LogError(LogCategory.Info, e.ToString());
                }
            }

            return instance;
        }

        private static void UpgradeFromOldVersion(string settingsPath)
        {
            // Check file to see if it's an older version.
            string[] fileContents = File.ReadAllLines(settingsPath);
            for (var i = 0; i < fileContents.Length; i++)
            {
                if (fileContents[i].Equals(PatchOldVersion))
                {
                    TSLog.Log(LogCategory.Info, "Upgrading settings file from old version.");
                    fileContents[i] = PatchNewVersion;

                    string backupPath = null;
                    try
                    {
                        string backupDirectory = PathUtility.GetBackupDirectory();
                        if (!Directory.Exists(backupDirectory))
                        {
                            Directory.CreateDirectory(backupDirectory);
                        }

                        // Backup existing settings
                        backupPath = Path.Combine(backupDirectory, "Settings.asset.backup");
                        File.WriteAllLines(backupPath, fileContents);

                        // Delete old settings
                        AssetDatabase.DeleteAsset(settingsPath);
                        AssetDatabase.SaveAssets();

                        // Write new settings
                        File.WriteAllLines(settingsPath, fileContents);
                        AssetDatabase.Refresh(ImportAssetOptions.Default);
                        TSLog.Log(LogCategory.Info, "Upgrade complete.");

                        // Delete backup
                        File.Delete(backupPath);
                    }
                    catch (Exception e)
                    {
                        TSLog.LogError(LogCategory.Info, "Error when trying to upgrade from old settings file: " + e);

                        if (backupPath != null)
                        {
                            TSLog.LogError(LogCategory.Info, "Backup was written to " + backupPath);
                        }
                    }

                    break;
                }
            }
        }

        public void Save()
        {
            EditorUtility.SetDirty(this);
        }
    }
}
