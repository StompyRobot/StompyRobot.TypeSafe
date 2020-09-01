using System;
using System.CodeDom;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Globalization;
using System.Reflection;
using System.Text.RegularExpressions;
using Microsoft.CSharp;

namespace TypeSafe.Editor.Compiler
{
    internal class CompilerUtil
    {
        private static readonly CodeDomProvider _provider = new CSharpCodeProvider();

        public static bool GetSafeNameAndVerifyNotDuplicate(string name, CodeTypeDeclaration targetType,
            out string finalName, bool checkReserved = true)
        {
            try
            {
                finalName = GetSafeName(name, false, checkReserved);

                if (IsDuplicate(finalName, targetType))
                {
                    TSLog.LogError(LogCategory.Compile, string.Format("Name {0} conflicts with existing member.", finalName));
                    return false;
                }
            }
            catch(Exception e)
            {
                TSLog.LogError(LogCategory.Trace, e.ToString());
                TSLog.LogError(LogCategory.Compile, string.Format("Failed to create a C# safe-name for `{0}`", name));
                finalName = null;
                return false;
            }

            return true;
        }

        public static string GetSafeName(string name, bool suppressWarning = false, bool checkReserved = true)
        {
            var n = GetSafeNameInternal(name);

            if (checkReserved && IsReservedName(n))
            {
                if (!suppressWarning)
                {
                    TSLog.LogWarning(LogCategory.Compile,
	                    string.Format("\"{0}\" is reserved by TypeSafe. Will have underscore prefixed. [{1}] -> [_{2}]", n, name, n));
                }

                n = "_" + n;
            }

            return n;
        }

        public static bool IsDuplicate(string name, CodeTypeDeclaration type)
        {
            foreach (var m in type.Members)
            {
                var member = m as CodeTypeMember;

                if (member.Name == name)
                {
                    return true;
                }
            }

            return false;
        }

        public static bool IsSafeName(string name)
        {
            return _provider.IsValidIdentifier(name);
        }

        public static bool IsReservedName(string name)
        {
            return Strings.ReservedNames.Contains(name);
        }

        private static bool IsIdentifierStartCharacter(char c)
        {
            return (c >= 'a' && c <= 'z') || (c >= 'A' && c <= 'Z') || c == '_' || c == '@' || Char.IsLetter(c);
        }

        private static bool IsIdentifierPartCharacter(char c)
        {
            return (c >= 'a' && c <= 'z') || (c >= 'A' && c <= 'Z') || c == '_' || (c >= '0' && c <= '9') ||
                   Char.IsLetter(c);
        }

        private static string GetSafeNameInternal(string name)
        {
            var safeName = name.Trim();

            // RegEx to conform to C# spec
            // Mono can't handle \p{Cf}
            safeName = Regex.Replace(safeName, @"[^\p{Lu}\p{Ll}\p{Lt}\p{Lm}\p{Lo}\p{Nl}\p{Mn}\p{Mc}\p{Nd}\p{Pc}\s]+", "");
            safeName = Regex.Replace(safeName, @"[\s]+", "_");

            safeName = ReduceUnicodeNumbers(safeName);
            safeName = ReduceNonMonoCharacters(safeName);

            if (safeName.Length > 0 && !IsIdentifierStartCharacter(safeName[0]))
            {
                safeName = "_" + safeName;
            }

            safeName = _provider.CreateValidIdentifier(safeName);

            if (!IsSafeName(safeName))
            {
                throw new Exception(string.Format("Failed to create a safe name ({0} -> {1})", name, safeName));
            }

            return safeName;
        }

        /// <summary>
        /// Transform any Unicode numbers to ASCII digits
        /// </summary>
        private static string ReduceUnicodeNumbers(string safeName)
        {
            var charArray = safeName.ToCharArray();

            for (var i = 0; i < charArray.Length; i++)
            {
                // Convert unicode digits to ASCII digits. Mono doesn't support non-ascii digits (MS C# does, but Unity doesn't use that)
                if (char.IsDigit(charArray[i]))
                {
                    var digitValue = CharUnicodeInfo.GetDigitValue(charArray[i]);

                    if (digitValue < 0 || digitValue > 9)
                    {
                        throw new Exception("Digit value was not between 0 and 9 when creating safe name.");
                    }

                    var transformedDigit = (char) (48 + digitValue);
                    charArray[i] = transformedDigit;
                }
            }

            return new string(charArray);
        }

        /// <summary>
        /// Remove any Unicode characters that aren't supported by Mono
        /// </summary>
        private static string ReduceNonMonoCharacters(string safeName)
        {
            var charArray = new List<char>(safeName.ToCharArray());

            for (var i = charArray.Count-1; i > 0; i--)
            {
                if (!IsIdentifierPartCharacter(charArray[i]))
                {
                    charArray.RemoveAt(i);
                }
            }

            return new string(charArray.ToArray());
        }

        public static void WriteHeader(CodeCompileUnit u)
        {
            var ns = new CodeNamespace("");

            ns.Comments.Add(new CodeCommentStatement(Strings.CommentHeaderPrefix));

            for (var i = 0; i < Strings.CommentHeader.Length; i++)
            {
                ns.Comments.Add(new CodeCommentStatement(Strings.CommentHeader[i]));
            }

            ns.Comments.Add(new CodeCommentStatement(""));

            for (var i = 0; i < Strings.CommentHeaderText.Length; i++)
            {
                ns.Comments.Add(new CodeCommentStatement(Strings.CommentHeaderText[i]));
            }

            ns.Comments.Add(new CodeCommentStatement(""));

            ns.Comments.Add(new CodeCommentStatement(string.Format("TypeSafe Version: {0}", Strings.Version)));

            ns.Comments.Add(new CodeCommentStatement(""));
            ns.Comments.Add(new CodeCommentStatement(Strings.CommentHeaderSuffix));

            u.Namespaces.Add(ns);
        }

        /// <summary>
        /// Create the standard static type container
        /// </summary>
        public static CodeTypeDeclaration CreateStaticType(string name)
        {
            var t = new CodeTypeDeclaration(name);
            t.IsClass = true;
            t.TypeAttributes = TypeAttributes.Public | TypeAttributes.Sealed;

            // Add a private constructor so it can't be instantiated
            t.Members.Add(new CodeConstructor {Attributes = MemberAttributes.Private});

            return t;
        }

        /// <summary>
        /// Tag a class declaration with the TypeSafe internal tag, used for determining if a type was created by TypeSafe in
        /// the editor.
        /// </summary>
        /// <param name="type"></param>
        public static void AddTypeSafeTag(CodeTypeDeclaration type)
        {
            // Don't add if the tag is already part of the class
            for (var i = 0; i < type.Members.Count; i++)
            {
                if (type.Members[i].Name == Strings.TypeSafeInternalTagFieldName)
                {
                    return;
                }
            }

            var field = new CodeMemberField(typeof (string), Strings.TypeSafeInternalTagFieldName);
            field.InitExpression = new CodePrimitiveExpression(Strings.Version);
            field.Attributes = MemberAttributes.Const | MemberAttributes.Private;

            type.Members.Add(field);
        }

        /// <summary>
        /// Get a CodeTypeReference object for type <paramref name="t" /> with every reference using the global:: syntax to
        /// prevent
        /// naming conflicts.
        /// </summary>
        /// <param name="t"></param>
        public static CodeTypeReference GetGlobalScopeTypeReference(Type t)
        {
            var typeRef = new CodeTypeReference(t);

            SetTypeGlobalReference(typeRef);

            return typeRef;
        }

        /// <summary>
        /// Set every reference in <paramref name="typeRef" /> to use a global reference.
        /// </summary>
        /// <param name="typeRef"></param>
        public static void SetTypeGlobalReference(CodeTypeReference typeRef)
        {
            // Make sure it is a global reference in case of name conflicts
            typeRef.Options |= CodeTypeReferenceOptions.GlobalReference;

            for (var i = 0; i < typeRef.TypeArguments.Count; i++)
            {
                SetTypeGlobalReference(typeRef.TypeArguments[i]);
            }
        }

        public static CodeCommentStatement[] CreateDocsComment(string summary, string @return = null)
        {
            var list = new List<CodeCommentStatement>();

            list.Add(new CodeCommentStatement("<summary>", true));

            var lines = summary.Split('\n');

            for (var i = 0; i < lines.Length; i++)
            {
                list.Add(new CodeCommentStatement(lines[i], true));
            }

            list.Add(new CodeCommentStatement("</summary>", true));

            if (!string.IsNullOrEmpty(@return))
            {
                list.Add(new CodeCommentStatement(string.Format("<returns>{0}</returns>", @return), true));
            }

            return list.ToArray();
        }

        public static CodeAttributeDeclaration GetObsoleteAttribute(string warning, bool error)
        {
            var type = new CodeTypeReference(typeof (ObsoleteAttribute), CodeTypeReferenceOptions.GlobalReference);

            return new CodeAttributeDeclaration(type, new CodeAttributeArgument(new CodePrimitiveExpression(warning)),
                new CodeAttributeArgument(new CodePrimitiveExpression(error)));
        }

        public static bool IsPrimitiveType(Type t)
        {
            return t.IsPrimitive || t == typeof (string);
        }
    }
}
