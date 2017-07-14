using System;

namespace CollabEdit.VersionControl
{
    public class CommitMetadata : IEquatable<CommitMetadata>
    {
        public static CommitMetadata Default = new CommitMetadata(string.Empty);

        public CommitMetadata(string comment)
        {
            if (comment == null) throw new ArgumentNullException(nameof(comment));
            Comment = comment;
        }
        public string Comment { get; internal set; }

        override public int GetHashCode()
        {
            return this.Comment.GetHashCode();
        }

        public bool Equals(CommitMetadata other)
        {
            if (other == null) return false;

            return other.GetHashCode() == this.GetHashCode();
        }
    }
}
