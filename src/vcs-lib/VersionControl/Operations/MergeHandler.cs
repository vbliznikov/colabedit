using System;
using System.Collections.Generic;

namespace CollabEdit.VersionControl.Operations
{
    public abstract class MergeHandler<T> : IMergeHandler<T>
    {
        protected MergeHandler(IEqualityComparer<T> comparer)
        {
            Comparer = comparer;
        }

        public IEqualityComparer<T> Comparer { get; }
        public T Merge(T origin, T left, T right, ConflictResolutionOptions options)
        {
            if (Comparer.Equals(left, right)) return right;
            if (Comparer.Equals(origin, left)) return right;
            if (Comparer.Equals(origin, right)) return left;

            return DoMerge(origin, left, right, options);
        }

        protected abstract T DoMerge(T origin, T left, T right, ConflictResolutionOptions options);
    }
}