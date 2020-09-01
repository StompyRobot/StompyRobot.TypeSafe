using UnityEditorInternal;

namespace TypeSafe.Editor.DataSources
{
    internal class TagsDataSource : ITypeSafeDataSource
    {
        public TypeSafeDataUnit GetTypeSafeDataUnit()
        {
            var unit = new TypeSafeDataUnit(TypeSafeUtil.GetFinalClassName(Strings.TagsTypeName), typeof (string));
            var tags = InternalEditorUtility.tags;

            foreach (var tag in tags)
            {
                var ignore = string.IsNullOrEmpty(tag) || tag.Trim().Length == 0;

                TSLog.Log(LogCategory.Scanner, string.Format("Tag: {0}, (ignore={1})", tag, ignore));

                if (!ignore)
                {
                    unit.Data.Add(new TypeSafeDataEntry(tag, new object[] {tag}));
                }
            }

            unit.EnableAllProperty = true;
            unit.FileName = "Tags";

            return unit;
        }
    }
}
