using System;
using System.Collections.Generic;

namespace CollabEdit.DocumentModel
{
    public class TextDocumentEqulityComparer : IEqualityComparer<TextDocument>
    {
        public bool Equals(TextDocument x, TextDocument y)
        {
            if (x == null && y == null) return true;
            if (x == null && y != null) return false;
            if (y == null && x != null) return false;

            return x.GetHashCode() == y.GetHashCode();
        }

        public int GetHashCode(TextDocument obj)
        {
            int hashCode = typeof(TextDocument).GetHashCode();
            if (obj == null) return hashCode;

            hashCode ^= obj.Text.GetHashCode() ^ obj.Metadata.GetHashCode();
            return hashCode;
        }
    }
}