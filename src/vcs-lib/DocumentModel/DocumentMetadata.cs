using System;
using System.Linq;
using System.Collections.Generic;

namespace CollabEdit.DocumentModel
{

    public class DocumentMetadata : Dictionary<string, EditState>, IEquatable<DocumentMetadata>
    {
        public DocumentMetadata() { }

        public DocumentMetadata(IDictionary<string, EditState> dictionary) : base(dictionary)
        {

        }
        public bool Equals(DocumentMetadata other)
        {
            if (other == null) return false;

            return this.GetHashCode().Equals(other.GetHashCode());
        }

        override public int GetHashCode()
        {
            int hashCode = GetType().GetHashCode();
            if (Count == 0) return hashCode;

            foreach (var pair in this)
            {
                if (pair.Key != null)
                    hashCode ^= pair.Key.GetHashCode();
                else
                    hashCode ^= typeof(string).GetHashCode();

                hashCode ^= pair.Value.GetHashCode();
            }

            return hashCode;
        }
    }
}