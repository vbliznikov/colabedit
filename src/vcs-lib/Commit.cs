using System;
using System.Text;

namespace VersionControl
{
    public class Commit<TValue, TMeta> : IEquatable<Commit<TValue, TMeta>>
    {
        internal Commit(TValue value, TMeta metadata, params Commit<TValue, TMeta>[] parents)
        {
            if (value == null) throw new ArgumentException("Value may not be null");
            Value = value;
            Metadata = metadata;
            Parent = parents.Length > 0 ? parents[0] : null;

            var mergeLength = parents.Length > 0 ? parents.Length - 1 : 0;
            MergeParents = new Commit<TValue, TMeta>[mergeLength];
            if (mergeLength > 0)
                for (int i = 1; i < parents.Length; i++)
                    MergeParents[i - 1] = parents[i];
        }
        public Commit<TValue, TMeta> Parent { get; internal set; }
        public Commit<TValue, TMeta>[] MergeParents { get; internal set; }

        public TValue Value { get; internal set; }

        public TMeta Metadata { get; internal set; }

        public bool Equals(Commit<TValue, TMeta> other)
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
            builder.AppendFormat("Metada:{0}\n", this.Metadata.ToString());
            builder.AppendFormat("Value:{0}", Value);

            builder.AppendLine();
            return builder.ToString();
        }
    }
}
