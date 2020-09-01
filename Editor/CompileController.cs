using System;
using System.CodeDom;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using Microsoft.CSharp;
using TypeSafe.Editor.Compiler;
using TypeSafe.Editor.Data;
using Object = UnityEngine.Object;

namespace TypeSafe.Editor
{
    internal enum CompileModes
    {
        SourceFiles,
        Dll
    }

    internal class CompileController
    {
        private Thread _thread;
        private bool _logErrors = true;
        public ICollection<string> Output { get; private set; }
        public ResourceDatabase ResourceDatabase { get; set; }
        public IEnumerable<TypeSafeDataUnit> DataUnits { get; set; }

        public bool LogErrors
        {
	        get { return _logErrors; }
        }

        public bool IsDone
        {
            get { return _thread == null || !_thread.IsAlive; }
        }

        public bool WasSuccessful { get; private set; }
        public bool UseThread { get; set; }

        public void Compile()
        {
            if (_thread != null)
            {
                throw new InvalidOperationException("Compile is already running");
            }

            WasSuccessful = false;

            var compileParameters = new CompileParameters
            {
                DataUnits = DataUnits,
                ResourceDatabase = ResourceDatabase,
                LogErrors = LogErrors,
                OnComplete = OnComplete
            };

            if (UseThread)
            {
                _thread = new Thread(o => { DoCompile(o as CompileParameters); });

                _thread.Start(compileParameters);
            }
            else
            {
                DoCompile(compileParameters);
            }
        }

        public void Abort()
        {
            if (_thread != null && _thread.IsAlive)
            {
                _thread.Abort();
            }
        }

        private void OnComplete(bool success, ICollection<string> outputPaths)
        {
            WasSuccessful = success;
            Output = outputPaths;

            _thread = null;
        }

        private static void DoCompile(CompileParameters p)
        {
            using (var provider = new CSharpCodeProvider())
            {
                var compileUnits = new List<CodeCompileUnit>();

                if (p.ResourceDatabase != null)
                {
                    var compiler = new ResourceCompiler();

                    compiler.ClassName = TypeSafeUtil.GetFinalClassName(Strings.ResourcesTypeName);
                    compiler.Namespace = Settings.Instance.Namespace;

                    try
                    {
                        var u = compiler.Compile(p.ResourceDatabase);
                        compileUnits.Add(u);
                    }
                    catch (Exception e)
                    {
                        TSLog.LogError(LogCategory.Compile, "Exception occured while compiling resources unit.");
                        TSLog.LogError(LogCategory.Compile, e.ToString());
                    }
                }

                if (p.DataUnits != null)
                {
                    var compiler = new DataUnitCompiler();
                    compiler.Namespace = Settings.Instance.Namespace;

                    foreach (var dataUnit in p.DataUnits)
                    {
                        try
                        {
                            compileUnits.Add(compiler.Compile(dataUnit));
                        }
                        catch (Exception e)
                        {
                            TSLog.LogError(LogCategory.Compile, string.Format("Exception occured while compiling data unit {0}", dataUnit.ClassName));
                            TSLog.LogError(LogCategory.Compile, e.ToString());
                        }
                    }
                }

                var results = new List<string>();

                var didSucceed = false;

                switch (p.CompileMode)
                {
                    case CompileModes.Dll:

                        string path;
                        didSucceed = CompileToDll(provider, compileUnits, p, out path);
                        results.Add(path);
                        break;

                    case CompileModes.SourceFiles:

                        IList<string> r;
                        didSucceed = CompileToSourceFiles(provider, compileUnits, p, out r);
                        results.AddRange(r);
                        break;
                }

                if (p.OnComplete != null)
                {
                    p.OnComplete(didSucceed, results);
                }
            }
        }

        private static bool CompileToDll(CodeDomProvider provider, IEnumerable<CodeCompileUnit> compileUnits,
            CompileParameters p, out string path)
        {
            TSLog.Log(LogCategory.Trace, "Compiling to DLL");

            var compilerParameters = new CompilerParameters();
            compilerParameters.ReferencedAssemblies.Add(typeof (int).Assembly.Location);
            compilerParameters.ReferencedAssemblies.Add(typeof (Object).Assembly.Location);
            compilerParameters.ReferencedAssemblies.Add("TypeSafe.dll");

            if (p.ResourceDatabase != null)
            {
                // Include references to assemblies used in resources

                var list = GetReferencedAssemblies(p.ResourceDatabase);

                foreach (var s in list)
                {
                    if (!compilerParameters.ReferencedAssemblies.Contains(s))
                    {
                        compilerParameters.ReferencedAssemblies.Add(s);
                    }
                }
            }

            for (var i = 0; i < compilerParameters.ReferencedAssemblies.Count; i++)
            {
                TSLog.Log(LogCategory.Trace,
                    string.Format("Referencing Assembly: {0}", compilerParameters.ReferencedAssemblies[i]));
            }

            PathUtility.EnsureDirectoryExists(PathUtility.GetBuildTempDirectory());
            compilerParameters.OutputAssembly = PathUtility.GetBuildTempDirectory() + "/" + Strings.DllName;
            //compilerParameters.TempFiles.KeepFiles = true;

            TSLog.Log(LogCategory.Trace, "Compile starting...");

            var result = provider.CompileAssemblyFromDom(compilerParameters, compileUnits.ToArray());

            if (p.LogErrors)
            {
                for (var i = 0; i < result.Errors.Count; i++)
                {
                    var error = result.Errors[i];

                    if (error.IsWarning)
                    {
                        TSLog.LogWarning(LogCategory.Compile, error.ToString());
                    }
                    else
                    {
                        TSLog.LogError(LogCategory.Compile, error.ToString());
                    }
                }
            }

            path = result.PathToAssembly;

            return result.NativeCompilerReturnValue == 0;
        }

        private static bool CompileToSourceFiles(CodeDomProvider provider, IEnumerable<CodeCompileUnit> compileUnits,
            CompileParameters p, out IList<string> paths)
        {
            TSLog.Log(LogCategory.Trace, "Compiling to Source Files");

            PathUtility.EnsureDirectoryExists(PathUtility.GetBuildTempDirectory());

            var codeGeneratorOptions = new CodeGeneratorOptions();
            codeGeneratorOptions.VerbatimOrder = true;

            var outputPath = PathUtility.GetBuildTempDirectory();

            var outputPaths = new List<string>();

            foreach (var c in compileUnits)
            {
                var found = false;

                foreach (var key in c.UserData.Keys)
                {
                    var s = key as string;
                    if (s != null && s == Strings.CompileUnitUserDataKey)
                    {
                        found = true;
                        break;
                    }
                }

                if (!found)
                {
                    TSLog.LogError(LogCategory.Compile, "Compile unit does not contain name user data key");
                    continue;
                }

                var name = (string) c.UserData[Strings.CompileUnitUserDataKey];

                var fileName = string.Format(Strings.SourceFileNameFormat, name);

                var o = Path.Combine(outputPath, fileName);

                using (var fs = File.Open(o, FileMode.Create, FileAccess.Write))
                {
                    using (var tw = new StreamWriter(fs))
                    {
                        provider.GenerateCodeFromCompileUnit(c, tw, codeGeneratorOptions);
                    }
                }

                // Read the newly generated file
                var file = File.ReadAllLines(o);

                // It's not worth the bother to do this. sealed and private constructor achieve the same effect.
                /*// Replace sealed classes with static ones. (CodeDOM doesn't have a way of creating static classes by default)
				for (var i = 0; i < file.Length; i++) {

					var s = file[i];

					if (s.Contains("public sealed ")) {
						file[i] = s.Replace("public sealed ", "public static ");
						break;
					}

				}*/

                // Write with default header removed
                File.WriteAllLines(o, file.Skip(10).ToArray());

                outputPaths.Add(o);
            }

            paths = outputPaths;

            return true;
        }

        private static List<string> GetReferencedAssemblies(ResourceDatabase db)
        {
            var list = new List<string>();

            GetAssemblies(db.RootFolder, list);

            return list;
        }

        private static void GetAssemblies(ResourceFolder folder, List<string> list)
        {
            foreach (var rd in folder.Resources)
            {
                var location = rd.Type.Assembly.Location;

                if (!list.Contains(location))
                {
                    list.Add(location);
                }
            }

            foreach (var resourceFolder in folder.Folders)
            {
                GetAssemblies(resourceFolder, list);
            }
        }

        private class CompileParameters
        {
            public readonly CompileModes CompileMode = CompileModes.SourceFiles;
            public Action<bool, ICollection<string>> OnComplete;
            public ResourceDatabase ResourceDatabase { get; set; }
            public IEnumerable<TypeSafeDataUnit> DataUnits { get; set; }
            public bool LogErrors { get; set; }
        }
    }
}
