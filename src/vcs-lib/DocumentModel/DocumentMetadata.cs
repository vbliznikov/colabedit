using System;
using System.Linq;
using System.Collections.Generic;

using CollabEdit.VersionControl;

namespace CollabEdit.DocumentModel
{

    public class DocumentMetadata : Dictionary<string, EditState>, ITwoWayMergable<DocumentMetadata>
    {
        public DocumentMetadata Merge(DocumentMetadata commonBase, DocumentMetadata other)
        {
            var merged = new DocumentMetadata();

            var combinedKeys = new HashSet<string>(commonBase.Keys);
            combinedKeys.UnionWith(this.Keys);
            combinedKeys.UnionWith(other.Keys);

            foreach (var key in combinedKeys)
            {
                if (!commonBase.ContainsKey(key))
                {
                    //Insert left
                    if (this.ContainsKey(key) && !other.ContainsKey(key))
                    {
                        merged.Add(key, this[key]);
                        continue;
                    }
                    //Insert Right
                    if (other.ContainsKey(key) && !this.ContainsKey(key))
                    {
                        merged.Add(key, other[key]);
                        continue;
                    }
                    // Identical insert both sides
                    if (this[key].Equals(other[key]))
                    {
                        merged.Add(key, this[key]);
                        continue;
                    }
                    // Different value insertions at left and right
                    throw new MergeOperationException(string.Format("Conflicting insertions of Key[{1}]", key));
                }

                if (!this.ContainsKey(key))
                {
                    // Delete both sides
                    if (!other.ContainsKey(key))
                        continue;

                    // Delete from left
                    if (other[key].Equals(commonBase[key]))
                        continue;
                    else
                    {
                        // Delete at left, change at right
                        throw new MergeOperationException(string.Format("Conflicting changes at Key[{0}]", key));
                    }
                }

                if (!other.ContainsKey(key))
                {
                    // Delete right
                    if (this[key].Equals(commonBase[key]))
                        continue;
                    else
                    {
                        // Delete at right, change at left
                        throw new MergeOperationException(string.Format("Conflicting changes at Key[{0}]", key));
                    }
                }
                var baseValue = commonBase[key];
                var leftValue = this[key];
                var rightValue = other[key];

                if (leftValue.Equals(rightValue))
                {
                    // The same change left and right or No operation
                    merged.Add(key, leftValue);
                    continue;
                }

                if (leftValue.Equals(baseValue))
                {
                    // No changes left
                    merged.Add(key, rightValue);
                    continue;
                }

                if (rightValue.Equals(baseValue))
                {
                    // No change right
                    merged.Add(key, leftValue);
                    continue;
                }

                // Change at right and change at left
                throw new MergeOperationException(string.Format("Conflicting changes at Key[{0}]", key));
            }

            return merged;
        }

        private Diff<KeyValuePair<string, EditState>> Compare(DocumentMetadata commonBase, DocumentMetadata other)
        {
            var diff = new Diff<KeyValuePair<string, EditState>>();
            var commonKeys = new HashSet<string>(commonBase.Keys);
            commonKeys.UnionWith(other.Keys);
            var sortedKeys = commonKeys.ToList();
            sortedKeys.Sort();
            foreach (var key in sortedKeys)
            {
                if (!commonBase.ContainsKey(key))
                {
                    diff.Add(new EditOperation<KeyValuePair<string, EditState>>
                    (
                        Operation.Insert,
                        new KeyValuePair<string, EditState>(key, other[key]
                    )));
                }
                if (!other.ContainsKey(key))
                {
                    diff.Add(new EditOperation<KeyValuePair<string, EditState>>
                    (
                        Operation.Delete,
                        new KeyValuePair<string, EditState>(key, commonBase[key]
                    )));
                }

                if (commonBase[key].Equals(other[key]))
                {
                    diff.Add(new EditOperation<KeyValuePair<string, EditState>>
                    (
                        Operation.Equal,
                        new KeyValuePair<string, EditState>(key, other[key]
                    )));
                }
                // Key changed
                diff.Add(new EditOperation<KeyValuePair<string, EditState>>
                   (
                       Operation.Delete,
                       new KeyValuePair<string, EditState>(key, commonBase[key]
                   )));
                diff.Add(new EditOperation<KeyValuePair<string, EditState>>
                   (
                       Operation.Insert,
                       new KeyValuePair<string, EditState>(key, other[key]
                   )));
            }

            return diff;
        }
    }

    public class Diff<T> : List<EditOperation<T>>
    {

    }

    public enum Operation
    {
        Delete, Insert, Equal
    }

    public class EditOperation<T> : IEquatable<EditOperation<T>>
    {
        public EditOperation(Operation operation, T value)
        {
            Operation = operation;
            Value = value;
        }
        Operation Operation { get; }
        T Value { get; }

        public bool Equals(EditOperation<T> other)
        {
            if (other == null) return false;

            if (this.Value == null)
                return other.Value == null && this.Operation.Equals(other.Operation);
            else
                return this.Operation.Equals(other.Operation) && this.Value.Equals(other.Value);
        }

        override public int GetHashCode()
        {
            int hashCode = Operation.GetHashCode();
            if (Value != null) hashCode ^= Value.GetHashCode();

            return hashCode;
        }
    }
}