using System;

namespace CollabEdit.Model
{
    public class FileSystemInfoDto : IComparable
    {
        public string Path { get; set; }
        public string Name { get; set; }
        public bool IsFile { get; set; }
        public string Content { get; set; }

        int IComparable.CompareTo(object obj)
        {
            var other = obj as FileSystemInfoDto;
            if (other == null) return 1;

            if (IsFile && !other.IsFile) return 1;
            if (!IsFile && other.IsFile) return -1;

            return Name.CompareTo(other.Name);
        }
    }
}