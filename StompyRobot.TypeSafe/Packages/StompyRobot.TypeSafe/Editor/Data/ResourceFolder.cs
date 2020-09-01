using System.Collections.Generic;
using System.Text;
using TypeSafe.Editor.Data;

namespace TypeSafe.Editor
{
    internal class ResourceFolder
    {
        private readonly List<ResourceFolder> _folders = new List<ResourceFolder>();
        private readonly List<ResourceDefinition> _resources = new List<ResourceDefinition>();

        public ResourceFolder(string name, string path)
        {
            Name = name;
            Path = path;
        }

        public string Name { get; private set; }
        public string Path { get; private set; }

        public IList<ResourceFolder> Folders
        {
            get { return _folders; }
        }

        public IList<ResourceDefinition> Resources
        {
            get { return _resources; }
        }

        public override string ToString()
        {
            return ToString(0, true);
        }

        private string ToString(int indentLevel, bool isLast)
        {
            var sb = new StringBuilder();

            for (var i = 0; i < indentLevel; i++)
            {
                sb.Append("  ");
            }

            var indent = sb.ToString();

            sb.Length = 0;

            sb.Append(indent);

            if (isLast)
            {
                sb.Append("\\-");
            }
            else
            {
                sb.Append("|-");
            }

            sb.AppendLine(Name);

            for (var i = 0; i < Folders.Count; i++)
            {
                var folder = Folders[i];

                var l = i == Folders.Count - 1 && Resources.Count == 0;

                sb.Append(folder.ToString(indentLevel + 1, l));
            }

            for (var i = 0; i < Resources.Count; i++)
            {
                var l = i == Resources.Count - 1;

                sb.Append(indent);

                if (l)
                {
                    sb.Append("\\-");
                }
                else
                {
                    sb.Append("|-");
                }

                sb.AppendLine(Resources[i].ToString());
            }

            return sb.ToString();
        }
    }
}
