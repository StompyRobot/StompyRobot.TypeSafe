using System.CodeDom;
using TypeSafe.Editor.Data;

namespace TypeSafe.Editor.Compiler
{
    internal class ResourceCompiler
    {
        public string ClassName = "SRResources";
        public string Namespace = "";

        public CodeCompileUnit Compile(ResourceDatabase database)
        {
            var unit = new CodeCompileUnit();
            unit.UserData.Add(Strings.CompileUnitUserDataKey, "Resources");

            CompilerUtil.WriteHeader(unit);

            var ns = new CodeNamespace(Namespace);
            unit.Namespaces.Add(ns);

            var container = CompilerUtil.CreateStaticType(ClassName);
            CompilerUtil.AddTypeSafeTag(container);

            WriteFolder(container, database.RootFolder);

            ns.Types.Add(container);

            return unit;
        }

        private void WriteFolder(CodeTypeDeclaration type, ResourceFolder folder)
        {
            ResourceCompilerUtil.WriteResources(type, folder);

            foreach (var f in folder.Folders)
            {
                var c = CompilerUtil.CreateStaticType(CompilerUtil.GetSafeName(f.Name));
                WriteFolder(c, f);

                type.Members.Add(c);
            }

            ResourceCompilerUtil.CreateGetContentsMethod(type);
            ResourceCompilerUtil.CreateGetContentsRecursiveMethod(type, folder);

            ResourceCompilerUtil.WriteGetContentsGenericMethod(type);
            ResourceCompilerUtil.WriteGetContentsRecursiveGenericMethod(type);

            ResourceCompilerUtil.WriteUnloadAllMethod(type);
            ResourceCompilerUtil.WriteUnloadAllRecursiveMethod(type);
        }
    }
}
