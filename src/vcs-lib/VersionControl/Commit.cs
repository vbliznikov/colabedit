using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CollabEdit.VersionControl
{
    public class Commit<TValue, TMeta> : IEquatable<Commit<TValue, TMeta>>
    {
        internal Commit(TValue value, TMeta metadata, params Commit<TValue, TMeta>[] parents)
        {
            if (value == null) throw new ArgumentException("Value may not be null");
            Value = value;
            Metadata = metadata;
            Previous = parents.Length > 0 ? parents[0] : null;
            Parents = parents.Where(element => element != null);
        }
        public Commit<TValue, TMeta> Previous { get; internal set; }
        public IEnumerable<Commit<TValue, TMeta>> Parents { get; internal set; }

        public TValue Value { get; internal set; }

        public TMeta Metadata { get; internal set; }

        public bool Equals(Commit<TValue, TMeta> other)
        {
            return other.GetHashCode() == this.GetHashCode();
        }

        override public int GetHashCode()
        {
            int hashCode = Value.GetHashCode();
            if (Metadata != null)
                hashCode ^= Metadata.GetHashCode();

            foreach (var parent in Parents)
                hashCode ^= parent.GetHashCode();
            return hashCode;
        }

        override public string ToString()
        {
            var builder = new StringBuilder();
            builder.AppendFormat("id: {0}\n", GetHashCode());
            builder.Append("parents: ");
            bool first = true;
            foreach (var parent in Parents)
            {
                if (!first) builder.Append(";");
                builder.AppendFormat("{0}", parent.GetHashCode());
                first = false;
            }
            builder.Append("\n");
            builder.AppendFormat("Metada:{0}\n", this.Metadata.ToString());
            builder.AppendFormat("Value:{0}", Value);

            builder.AppendLine();
            return builder.ToString();
        }
    }
}
