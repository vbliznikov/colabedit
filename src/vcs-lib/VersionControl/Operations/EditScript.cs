using System;
using System.Linq;
using System.Collections.Generic;
using System.Collections;

namespace CollabEdit.VersionControl.Operations
{
    public class OrderedSequenceEditScript<T> : List<DiffSegnment<T>>
    {

    }

    public class KeysEditScript<TKey> : IEnumerable<DiffSegnment<TKey>>
    {
        public KeysEditScript(IEnumerable<TKey> commonKeys, IEnumerable<TKey> deletedKeys, IEnumerable<TKey> insertedKeys)
        {
            CommonKeys = commonKeys;
            DeletedKeys = deletedKeys;
            InsertedKeys = insertedKeys;
        }

        public IEnumerable<TKey> CommonKeys { get; }
        public IEnumerable<TKey> DeletedKeys { get; }
        public IEnumerable<TKey> InsertedKeys { get; }

        public static KeysEditScript<TKey> From(IEnumerable<TKey> leftKeys, IEnumerable<TKey> rightKeys)
        {
            var commonKeys = leftKeys.Intersect(rightKeys);
            var deletedKeys = leftKeys.Except(commonKeys);
            var insertedKeys = rightKeys.Except(commonKeys);

            return new KeysEditScript<TKey>(commonKeys, deletedKeys, insertedKeys);
        }

        public KeysEditScript<TKey> Merge(KeysEditScript<TKey> other)
        {
            var resultCommonKeys = CommonKeys.Intersect(other.CommonKeys);
            var resultDeletedKeys = DeletedKeys.Union(other.DeletedKeys);
            var resultInsertedKeys = InsertedKeys.Union(other.InsertedKeys);

            return new KeysEditScript<TKey>(resultCommonKeys, resultDeletedKeys, resultInsertedKeys);
        }

        public IEnumerator<DiffSegnment<TKey>> GetEnumerator()
        {
            var resultingSequence =
                CommonKeys.Select(key => new DiffSegnment<TKey>(EditOperation.Equl, key)).Concat(
                    DeletedKeys.Select(key => new DiffSegnment<TKey>(EditOperation.Delete, key))
                ).Concat(
                    InsertedKeys.Select(key => new DiffSegnment<TKey>(EditOperation.Insert, key))
                );
            return resultingSequence.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }
    }

    public class DiffSegnment<T> : IEquatable<DiffSegnment<T>>
    {
        public DiffSegnment(EditOperation operation, T value)
        {
            Operation = operation;
            Value = value;
        }
        public EditOperation Operation { get; }
        public T Value { get; }

        public bool Equals(DiffSegnment<T> other)
        {
            if (other == null) return false;

            if (Value == null)
            {
                if (other.Value == null)
                    return Operation.Equals(other.Operation);
                else
                    return false;
            }
            if (other.Value == null) return false;

            return Operation.Equals(other.Operation) && Value.Equals(other.Value);
        }
    }

    public enum EditOperation
    {
        Equl,
        Delete,
        Insert
    }
}