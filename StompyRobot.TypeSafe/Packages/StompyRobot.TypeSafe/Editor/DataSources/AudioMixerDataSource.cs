using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine.Audio;

namespace TypeSafe.Editor.DataSources
{
    internal class AudioMixerDataSource : ITypeSafeDataSource
    {
        private class ExposedAudioParameter
        {
            private static readonly Type WrappedType =
                typeof(EditorApplication).Assembly.GetType("UnityEditor.Audio.ExposedAudioParameter");

            private static readonly FieldInfo GuidField = WrappedType.GetField("guid", BindingFlags.Instance | BindingFlags.Public);
            private static readonly FieldInfo NameField = WrappedType.GetField("name", BindingFlags.Instance | BindingFlags.Public);

            public GUID guid
            {
	            get { return (GUID) GuidField.GetValue(_target); }
            }

            public string name
            {
	            get { return (string) NameField.GetValue(_target); }
            }

            private object _target;

            public ExposedAudioParameter(object target)
            {
                if (!WrappedType.IsInstanceOfType(target))
                {
                    throw new ArgumentException("Target was not an instance of " + WrappedType.FullName);
                }

                _target = target;
            }
        }

        private class AudioMixerControllerWrapper
        {
            public static readonly Type WrappedType =
                typeof(EditorApplication).Assembly.GetType("UnityEditor.Audio.AudioMixerController");

            private static readonly PropertyInfo ExposedParametersProperty = WrappedType.GetProperty("exposedParameters", BindingFlags.Instance | BindingFlags.Public);
            private static readonly PropertyInfo SnapshotsProperty = WrappedType.GetProperty("snapshots", BindingFlags.Instance | BindingFlags.Public);

            public ExposedAudioParameter[] exposedParameters
            {
                get
                {
                    var parameters = (Array)ExposedParametersProperty.GetValue(_target, null);
                    return parameters.Cast<object>().Select(p => new ExposedAudioParameter(p)).ToArray();
                }
            }

            public AudioMixerSnapshot[] snapshots
            {
                get
                {
                    var snapshots = (Array)SnapshotsProperty.GetValue(_target, null);
                    return snapshots.Cast<AudioMixerSnapshot>().ToArray();
                }
            }

            public AudioMixer Mixer
            {
	            get { return _target; }
            }

            private readonly AudioMixer _target;

            public AudioMixerControllerWrapper(AudioMixer target)
            {
                if (!WrappedType.IsInstanceOfType(target))
                {
                    throw new ArgumentException("Target was not an instance of " + WrappedType.FullName);
                }

                _target = target;
            }
        }

        public TypeSafeDataUnit GetTypeSafeDataUnit()
        {
            var rootUnit = new TypeSafeDataUnit(TypeSafeUtil.GetFinalClassName(Strings.AudioMixersName)
                , typeof(float), new List<TypeSafeDataEntry>(), false, "AudioMixers");

            var mixers = Settings.Instance.AudioMixers;

            foreach (var guid in mixers)
            {
                var assetPath = AssetDatabase.GUIDToAssetPath(guid);
                var asset = AssetDatabase.LoadAssetAtPath<AudioMixer>(assetPath);

                TSLog.Log(LogCategory.Trace, string.Format("AudioMixer: {0}", assetPath), asset);
                rootUnit.NestedUnits.Add(CreateMixerDataUnit(new AudioMixerControllerWrapper(asset)));
            }

            return rootUnit;
        }

        private TypeSafeDataUnit CreateMixerDataUnit(AudioMixerControllerWrapper wrapper)
        {
            var unit = new TypeSafeDataUnit(wrapper.Mixer.name, typeof(string));

            var parametersUnit = new TypeSafeDataUnit("Parameters", typeof(string));
            foreach (var param in wrapper.exposedParameters)
            {
                TSLog.Log(LogCategory.Trace, string.Format("Parameter: {0}", param.name));

                if (parametersUnit.Data.Any(p => p.PropertyName == param.name))
                {
                    TSLog.LogWarning(LogCategory.Compile, string.Format("[AudioMixer] Duplicate parameter name ({0}).", param.name), wrapper.Mixer);
                    continue;
                }

                parametersUnit.Data.Add(new TypeSafeDataEntry(param.name, new object[] {param.name}));
            }

            var snapshotsUnit = new TypeSafeDataUnit("Snapshots", typeof(string));
            foreach (var snapshot in wrapper.snapshots)
            {
                TSLog.Log(LogCategory.Trace, string.Format("Snapshot: {0}", snapshot.name));

                if (snapshotsUnit.Data.Any(p => p.PropertyName == snapshot.name))
                {
                    TSLog.LogWarning(LogCategory.Compile, string.Format("[AudioMixer] Duplicate snapshot name ({0}).", snapshot.name), wrapper.Mixer);
                    continue;
                }

                snapshotsUnit.Data.Add(new TypeSafeDataEntry(snapshot.name, new object[] {snapshot.name}));
            }

            unit.NestedUnits.Add(parametersUnit);
            unit.NestedUnits.Add(snapshotsUnit);

            return unit;
        }
    }
}