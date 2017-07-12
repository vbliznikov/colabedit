using System;
using System.Text;

namespace VersionControl
{
    public class Commit<T> : IEquatable<Commit<T>>
    {
        internal Commit(T value, CommitMetadata metadata, params Commit<T>[] parents)
        {
            if (value == null) throw new ArgumentException("Value may not be null");
            Value = value;
            Metadata = metadata ?? CommitMetadata.Default;
            Parent = parents.Length > 0 ? parents[0] : null;

            var mergeLength = parents.Length > 0 ? parents.Length - 1 : 0;
            MergeParents = new Commit<T>[mergeLength];
            if (mergeLength > 0)
                for (int i = 1; i < parents.Length; i++)
                    MergeParents[i - 1] = parents[i];
        }
        public Commit<T> Parent { get; internal set; }
        public Commit<T>[] MergeParents { get; internal set; }

        public T Value { get; internal set; }

        public CommitMetadata Metadata { get; internal set; }

        public bool Equals(Commit<T> other)
        {
            return other.GetHashCode() == this.GetHashCode();
        }

        override public int GetHashCode()
        {
            int hashCode = Value.GetHashCode() ^ Metadata.GetHashCode();
            if (Parent != null)
                hashCode ^= Parent.GetHashCode();
            foreach (var parent in MergeParents)
                hashCode ^= parent.GetHashCode();
            return hashCode;
        }

        override public string ToString()
        {
            var builder = new StringBuilder();
            builder.AppendFormat("id: {0}\n", GetHashCode());
            builder.AppendFormat("parents: {0}", Parent != null ? Parent.GetHashCode().ToString() : "");
            foreach (var parent in MergeParents)
                builder.AppendFormat(";{0}", parent.GetHashCode());
            builder.Append("\n");
            builder.AppendFormat("Comment:{0}\n", this.Metadata.Comment);
            builder.AppendFormat("Value:{0}", Value);

            builder.AppendLine();
            return builder.ToString();
        }
    }
}
