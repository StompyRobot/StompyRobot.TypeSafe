using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace TypeSafe.Editor.Compiler
{
    internal class DataUnitCompiler
    {
        public string Namespace { get; set; }

        public CodeCompileUnit Compile(TypeSafeDataUnit dataUnit)
        {
            var compileUnit = new CodeCompileUnit();
            compileUnit.UserData.Add(Strings.CompileUnitUserDataKey, dataUnit.FileName);

            CompilerUtil.WriteHeader(compileUnit);

            var ns = new CodeNamespace(Namespace);
            compileUnit.Namespaces.Add(ns);

            var className = CompilerUtil.GetSafeName(dataUnit.ClassName, false, false);

            if (className != dataUnit.ClassName)
            {
                TSLog.LogWarning(LogCategory.Compile,
	                string.Format("Class name was modified to conform to C# standards ({0} -> {1})", dataUnit.ClassName, className));
            }

            var container = CompilerUtil.CreateStaticType(className);
            CompilerUtil.AddTypeSafeTag(container);

            PopulateClass(dataUnit, container);

            ns.Types.Add(container);
            return compileUnit;
        }

        private static void PopulateClass(TypeSafeDataUnit dataUnit, CodeTypeDeclaration container)
        {
            foreach (var nestedUnit in dataUnit.NestedUnits)
            {
                string className;

                if (!CompilerUtil.GetSafeNameAndVerifyNotDuplicate(nestedUnit.ClassName, container, out className))
                {
                    continue;
                }

                if (className == container.Name)
                {
                    TSLog.LogWarning(LogCategory.Compile, Strings.Warning_NameCannotBeSameAsParent);
                    className = "_" + className;

                    // Check for duplicates again after having added the '_'
                    if (!CompilerUtil.GetSafeNameAndVerifyNotDuplicate(className, container, out className))
                    {
                        continue;
                    }
                }

                var nestedContainer = CompilerUtil.CreateStaticType(className);
                PopulateClass(nestedUnit, nestedContainer);

                container.Members.Add(nestedContainer);
            }

            PopulateClassMembers(dataUnit, container);
        }

        private static void PopulateClassMembers(TypeSafeDataUnit unit, CodeTypeDeclaration container)
        {
            // Create the list type that is used to store the internal array (not used when All list is disabled)
            var valueListType = new CodeTypeReference(typeof (IList<>))
            {
                TypeArguments = {unit.DataType}
            };

            // Set up the default type for members
            var dataType = new CodeTypeReference(unit.DataType);

            // Use global references
            CompilerUtil.SetTypeGlobalReference(dataType);
            CompilerUtil.SetTypeGlobalReference(valueListType);

            // Expression to make the internal list (not used when All list is disabled)
            var arrayExpression = new CodeArrayCreateExpression(dataType);

            //TSLog.Log(LogCategory.Trace, $"[DataUnitCompiler] Populating {container.Name}");

            foreach (var c in unit.Data)
            {
                // Can't override type if using the All property method (since can't store non-compatible types in the list)
                if (unit.EnableAllProperty && c.OverrideType != null)
                {
                    throw new InvalidOperationException("Cannot override type when using All list.");
                }

                var name = c.PropertyName;
                string memberName;

                if (
                    !CompilerUtil.GetSafeNameAndVerifyNotDuplicate(name, container, out memberName,
                        !c.OverrideRestrictedNames))
                {
                    continue;
                }

                // Create the expression to initialize this member
                var entryExpression = GetCreateExpression(c, c.OverrideType != null ? c.OverrideType : unit.DataType);
                var isObsolete = !string.IsNullOrEmpty(c.ObsoleteWarning);

                //TSLog.Log(LogCategory.Trace, $"[DataUnitCompiler] Member (name={memberName}, isObsolete={isObsolete})");

                // Exclude obsolete members from the _all array
                if (unit.EnableAllProperty && !isObsolete)
                {
                    // Add the initializer expression to the internal _all array
                    arrayExpression.Initializers.Add(entryExpression);
                }

                CodeTypeMember member;

                // Create a field if one of the following criteria matches:
                // - Entry is a primitive or string. We duplicate the data (include in _all array and const field) so that const uses are faster
                // - All property is disabled, so we don't have an internal _all array to access
                // - Entry has an obsolete warning and so isn't included in the internal _all array.
                if (!unit.EnableAllProperty || unit.DataType.IsPrimitive || unit.DataType == typeof (string) ||
                    isObsolete)
                {
                    //TSLog.Log(LogCategory.Trace, $"[DataUnitCompiler] Member handled as field");

                    var entryTypeReference = dataType;
                    var attributes = MemberAttributes.Public;

                    if (c.OverrideType != null)
                    {
                        entryTypeReference = new CodeTypeReference(c.OverrideType);
                    }

                    if (CompilerUtil.IsPrimitiveType(unit.DataType))
                    {
                        attributes |= MemberAttributes.Const;
                    }
                    else
                    {
                        attributes |= MemberAttributes.Static;
                        CompilerUtil.SetTypeGlobalReference(entryTypeReference);
                    }

                    // Duplicate data and create a field for data entry
                    member = new CodeMemberField
                    {
                        Name = memberName,
                        Type = entryTypeReference,
                        Attributes = attributes,
                        InitExpression = entryExpression
                    };

                    if (isObsolete)
                    {
                        //TSLog.Log(LogCategory.Trace, $"[DataUnitCompiler] Adding obsolete warning: {c.ObsoleteWarning}");
                        member.CustomAttributes.Add(CompilerUtil.GetObsoleteAttribute(c.ObsoleteWarning, false));
                    }
                }
                else
                {
                    //TSLog.Log(LogCategory.Trace, $"[DataUnitCompiler] Member handled as property");

                    // Otherwise create a property getter to access the internal _all array element
                    member = new CodeMemberProperty
                    {
                        Name = memberName,
                        Type = dataType,
                        HasSet = false,
                        HasGet = true,
                        Attributes = MemberAttributes.Static | MemberAttributes.Public,
                        GetStatements =
                        {
                            new CodeMethodReturnStatement(new CodeArrayIndexerExpression(
                                new CodeVariableReferenceExpression("__all"),
                                new CodePrimitiveExpression(arrayExpression.Initializers.Count - 1)))
                        }
                    };
                }

                container.Members.Add(member);
            }

            if (unit.EnableAllProperty)
            {
                var all = new CodeMemberField(valueListType, "__all")
                {
                    InitExpression =
                        new CodeObjectCreateExpression(
                            new CodeTypeReference(typeof (ReadOnlyCollection<>),
                                CodeTypeReferenceOptions.GlobalReference)
                            {
                                TypeArguments = {dataType}
                            }, arrayExpression),
                    Attributes = MemberAttributes.Private | MemberAttributes.Static
                };

                container.Members.Add(all);

                var allPublic = new CodeMemberProperty
                {
                    Name = "All",
                    Type = valueListType,
                    GetStatements =
                    {
                        new CodeMethodReturnStatement(new CodeFieldReferenceExpression {FieldName = "__all"})
                    },
                    HasGet = true,
                    Attributes = MemberAttributes.Public | MemberAttributes.Static
                };

                container.Members.Add(allPublic);
            }
        }

        private static CodeExpression GetCreateExpression(TypeSafeDataEntry c, Type t)
        {
            if (t.IsPrimitive)
            {
                if (c.Parameters.Length == 0 || c.Parameters.Length > 1 || !c.Parameters[0].GetType().IsPrimitive ||
                    c.Parameters[0].GetType() != t)
                {
                    throw new ArgumentException("Primitive DataType must have a single parameter of the same type");
                }

                return new CodePrimitiveExpression(c.Parameters[0]);
            }

            if (t == typeof (string))
            {
                if (c.Parameters.Length == 0 || c.Parameters.Length > 1 || !(c.Parameters[0] is string))
                {
                    throw new ArgumentException("String DataType must have a single string parameter");
                }

                return new CodePrimitiveExpression(c.Parameters[0]);
            }

            if (!c.Parameters.All(p => p.GetType().IsPrimitive || p is string))
            {
                throw new ArgumentException("Parameters must be all primitive types");
            }

            var dataType = new CodeTypeReference(t);
            CompilerUtil.SetTypeGlobalReference(dataType);

            return new CodeObjectCreateExpression(dataType,
                c.Parameters.Select(p => new CodePrimitiveExpression(p)).Cast<CodeExpression>().ToArray());
        }
    }
}
