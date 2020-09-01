using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using TypeSafe.Editor.Data;
using UnityEngine;
using Object = UnityEngine.Object;

namespace TypeSafe.Editor.Compiler
{
    internal static class ResourceCompilerUtil
    {
        internal const string RecursiveLookupCacheName = "__ts_internal_recursiveLookupCache";
        internal const string CollectionMemberName = "__ts_internal_resources";

        public static void WriteResources(CodeTypeDeclaration type, ResourceFolder folder)
        {
            var containerType = new CodeTypeReference(GetIResourceType(), 1);
            var createExpression = new CodeArrayCreateExpression(containerType);

            if (folder.Resources.Count > 0)
            {
                for (var i = 0; i < folder.Resources.Count; i++)
                {
                    var rd = folder.Resources[i];

                    // Add it to the array initializers
                    createExpression.Initializers.Add(GetResourceCreateExpression(rd));

                    WriteResourceProperty(type, rd, i);
                }
            }

            var containerField = new CodeMemberField(GetIResourceIListType(), CollectionMemberName)
            {
                InitExpression = new CodeObjectCreateExpression(GetIResourceReadOnlyCollectionType(), createExpression),
                Attributes = MemberAttributes.Private | MemberAttributes.Static
            };

            type.Members.Add(containerField);
        }

        public static void CreateGetContentsMethod(CodeTypeDeclaration type)
        {
            var method = new CodeMemberMethod
            {
                Name = Strings.GetResourcesMethodName,
                ReturnType = GetIResourceIListType(),
                Attributes = MemberAttributes.Public | MemberAttributes.Static
            };

            method.Comments.AddRange(CompilerUtil.CreateDocsComment(Strings.GetContentsCommentSummary,
                Strings.GetContentsCommentReturns));

            method.Statements.Add(
                new CodeMethodReturnStatement(new CodeFieldReferenceExpression(null, CollectionMemberName)));

            type.Members.Add(method);
        }

        public static void CreateGetContentsRecursiveMethod(CodeTypeDeclaration type, ResourceFolder folder)
        {
            var cache = new CodeMemberField(GetIResourceIListType(), RecursiveLookupCacheName)
            {
                Attributes = MemberAttributes.Static | MemberAttributes.Private
            };

            type.Members.Add(cache);

            var method = new CodeMemberMethod
            {
                Attributes = MemberAttributes.Static | MemberAttributes.Public,
                Name = Strings.GetResourcesRecursiveMethodName,
                ReturnType = GetIResourceIListType()
            };

            method.Comments.AddRange(CompilerUtil.CreateDocsComment(Strings.GetContentsRecursiveCommentSummary,
                Strings.GetContentsRecursiveCommentReturns));

            method.Statements.Add(
                new CodeConditionStatement(
                    new CodeBinaryOperatorExpression(
                        new CodeFieldReferenceExpression(null, RecursiveLookupCacheName),
                        CodeBinaryOperatorType.IdentityInequality,
                        new CodePrimitiveExpression(null)
                        ),
                    new CodeMethodReturnStatement(new CodeFieldReferenceExpression(null, RecursiveLookupCacheName))));

            // Create list
            method.Statements.Add(new CodeVariableDeclarationStatement(GetIResourceListType(), "tmp",
                new CodeObjectCreateExpression(GetIResourceListType())));

            // Add any resources from this folder
            method.Statements.Add(new CodeMethodInvokeExpression(new CodeVariableReferenceExpression("tmp"), "AddRange",
                new CodeMethodInvokeExpression(new CodeMethodReferenceExpression(null, Strings.GetResourcesMethodName))));

            // Add any resources from subfolders
            foreach (var f in folder.Folders)
            {
                var name = CompilerUtil.GetSafeName(f.Name, true);

                // Skip if this folder hasn't been added to the type for some reason
                if (!CompilerUtil.IsDuplicate(name, type))
                {
                    continue;
                }

                // Add any resources from this folder
                method.Statements.Add(new CodeMethodInvokeExpression(new CodeVariableReferenceExpression("tmp"),
                    "AddRange",
                    new CodeMethodInvokeExpression(
                        new CodeMethodReferenceExpression(new CodeSnippetExpression(name),
                            Strings.GetResourcesRecursiveMethodName))));
            }

            method.Statements.Add(
                new CodeAssignStatement(new CodeFieldReferenceExpression(null, RecursiveLookupCacheName),
                    new CodeVariableReferenceExpression("tmp")));

            method.Statements.Add(
                new CodeMethodReturnStatement(new CodeFieldReferenceExpression(null, RecursiveLookupCacheName)));

            type.Members.Add(method);
        }

        public static void WriteGetContentsGenericMethod(CodeTypeDeclaration type)
        {
            var method = GetGenericResourceListMethod(Strings.GetResourcesMethodName);

            method.Comments.AddRange(CompilerUtil.CreateDocsComment(Strings.GetContentsGenericCommentSummary,
                Strings.GetContentsGenericCommentReturns));
            method.Statements.Add(
                new CodeMethodReturnStatement(
                    new CodeSnippetExpression(
                        "global::TypeSafe.TypeSafeUtil.GetResourcesOfType<TResource>(GetContents())")));

            type.Members.Add(method);
        }

        public static void WriteGetContentsRecursiveGenericMethod(CodeTypeDeclaration type)
        {
            var method = GetGenericResourceListMethod(Strings.GetResourcesRecursiveMethodName);
            method.Attributes = MemberAttributes.Public | MemberAttributes.Static;

            method.Comments.AddRange(
                CompilerUtil.CreateDocsComment(Strings.GetContentsGenericRecursiveCommentSummary,
                    Strings.GetContentsGenericRecursiveCommentReturns));

            method.Statements.Add(
                new CodeMethodReturnStatement(
                    new CodeSnippetExpression(
                        "global::TypeSafe.TypeSafeUtil.GetResourcesOfType<TResource>(GetContentsRecursive())")));

            type.Members.Add(method);
        }

        public static void WriteUnloadAllMethod(CodeTypeDeclaration type)
        {
            var method = new CodeMemberMethod {Name = Strings.UnloadAllMethodName};
            method.Attributes = MemberAttributes.Public | MemberAttributes.Static;

            method.Comments.AddRange(CompilerUtil.CreateDocsComment(Strings.UnloadAllCommentSummary));

            method.Statements.Add(
                new CodeMethodInvokeExpression(
                    new CodeTypeReferenceExpression(new CodeTypeReference("TypeSafe.TypeSafeUtil")
                    {
                        Options = CodeTypeReferenceOptions.GlobalReference
                    }), "UnloadAll", new CodeSnippetExpression("GetContents()")));

            type.Members.Add(method);
        }

        public static void WriteUnloadAllRecursiveMethod(CodeTypeDeclaration type)
        {
            var method = new CodeMemberMethod {Name = Strings.UnloadAllRecursiveMethodName};

            method.Comments.AddRange(CompilerUtil.CreateDocsComment(Strings.UnloadAllRecursiveCommentSummary));

            method.Statements.Add(
                new CodeMethodInvokeExpression(
                    new CodeTypeReferenceExpression(new CodeTypeReference("TypeSafe.TypeSafeUtil")
                    {
                        Options = CodeTypeReferenceOptions.GlobalReference
                    }), "UnloadAll", new CodeSnippetExpression("GetContentsRecursive()")));

            type.Members.Add(method);
        }

        private static CodeMemberMethod GetGenericResourceListMethod(string name)
        {
            var method = new CodeMemberMethod
            {
                Name = name,
                ReturnType =
                    new CodeTypeReference(typeof (List<>), CodeTypeReferenceOptions.GlobalReference)
                    {
                        TypeArguments =
                        {
                            new CodeTypeReference("global::TypeSafe.Resource",
                                new CodeTypeReference("TResource", CodeTypeReferenceOptions.GenericTypeParameter))
                        }
                    },
                TypeParameters =
                {
                    new CodeTypeParameter("TResource")
                    {
                        Constraints = {CompilerUtil.GetGlobalScopeTypeReference(typeof (Object))}
                    }
                },
                Attributes = MemberAttributes.Public | MemberAttributes.Static
            };

            return method;
        }

        private static void WriteResourceProperty(CodeTypeDeclaration type, ResourceDefinition rd, int index)
        {
            string name;

            if (!CompilerUtil.GetSafeNameAndVerifyNotDuplicate(rd.Name, type, out name))
            {
                return;
            }

            var resourceType = GetResourceType(rd.Type);

            var attributes = MemberAttributes.Static;

            if (rd.Type.IsNotPublic)
            {
                attributes |= MemberAttributes.Assembly;
            }
            else
            {
                attributes |= MemberAttributes.Public;
            }

            // Create a property to access the array
            var propertyExpression = new CodeMemberProperty
            {
                Name = name,
                Type = resourceType,
                HasSet = false,
                HasGet = true,
                Attributes = attributes,
                GetStatements =
                {
                    new CodeMethodReturnStatement(new CodeCastExpression(resourceType,
                        new CodeArrayIndexerExpression(
                            new CodeVariableReferenceExpression(CollectionMemberName),
                            new CodePrimitiveExpression(index))))
                }
            };

            type.Members.Add(propertyExpression);
        }

        private static CodeExpression GetResourceCreateExpression(ResourceDefinition rd)
        {
            var resourceType = GetResourceType(rd.Type);

            // Create an create expression for the resource object
            var resourceCreateExpression = new CodeObjectCreateExpression(resourceType,
                new CodePrimitiveExpression(rd.Name),
                new CodePrimitiveExpression(rd.Path));

            return resourceCreateExpression;
        }

        /// <summary>
        /// Get a CodeTypeReference object to the TypeSafe.IResource class
        /// </summary>
        public static CodeTypeReference GetIResourceType()
        {
            return new CodeTypeReference("global::TypeSafe.IResource");
        }

        /// <summary>
        /// Get a CodeTypeReference object to the TypeSafe.Resource[T] class where T is <paramref name="t" />.
        /// </summary>
        public static CodeTypeReference GetResourceType(Type t)
        {
            if (t == typeof (GameObject))
            {
                return new CodeTypeReference("global::TypeSafe.PrefabResource");
            }

            return new CodeTypeReference("global::TypeSafe.Resource", CompilerUtil.GetGlobalScopeTypeReference(t));
        }

        /// <summary>
        /// Get a CodeTypeReference object to the ReadOnlyCollection type with IResource as the param type
        /// </summary>
        public static CodeTypeReference GetIResourceReadOnlyCollectionType()
        {
            var r = new CodeTypeReference(typeof (ReadOnlyCollection<>), CodeTypeReferenceOptions.GlobalReference);
            r.TypeArguments.Add(GetIResourceType());
            return r;
        }

        /// <summary>
        /// Get a CodeTypeReference object to the IList interface with IResource as the param type
        /// </summary>
        public static CodeTypeReference GetIResourceIListType()
        {
            var r = new CodeTypeReference(typeof (IList<>), CodeTypeReferenceOptions.GlobalReference);
            r.TypeArguments.Add(GetIResourceType());
            return r;
        }

        /// <summary>
        /// Get a CodeTypeReference object to the List[IResource] type
        /// </summary>
        public static CodeTypeReference GetIResourceListType()
        {
            var r = new CodeTypeReference(typeof (List<>), CodeTypeReferenceOptions.GlobalReference);
            r.TypeArguments.Add(GetIResourceType());
            return r;
        }
    }
}
