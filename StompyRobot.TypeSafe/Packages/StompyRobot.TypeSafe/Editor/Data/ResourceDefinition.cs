using System;

namespace TypeSafe.Editor.Data
{
    internal class ResourceDefinition
    {
        public ResourceDefinition(string name, string path, string fullPath, Type type)
        {
            if (string.IsNullOrEmpty(name))
            {
                throw new ArgumentException("name must not be null or empty", "name");
            }

            Name = name;
            Path = path;
            FullPath = fullPath;
            Type = type;
        }

        public string FullPath { get; private set; }
        public string Path { get; private set; }
        public string Name { get; private set; }
        public Type Type { get; private set; }

        public override string ToString()
        {
            return string.Format("{0} ({1})", Name, Type.Name);
        }
    }
}
