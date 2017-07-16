using System;

namespace CollabEdit.VersionControl.Operations
{
    public static class MergeUtils
    {
        public static TValue Merge<TValue>(TValue origin, TValue left, TValue right,
            ConflictResolutionOptions options = ConflictResolutionOptions.RaiseException)
        {
            if (CompareEqual(left, right)) return right;
            if (CompareEqual(origin, left)) return right;
            if (CompareEqual(origin, right)) return left;

            return ResolveConflict<TValue>(left, right, options);
        }

        public static TValue Merge<TValue>(TValue left, TValue right,
            ConflictResolutionOptions options = ConflictResolutionOptions.RaiseException)
        {
            if (CompareEqual(left, right))
                return right;
            else
                return ResolveConflict(left, right, options);
        }

        public static TValue ResolveConflict<TValue>(TValue left, TValue right, ConflictResolutionOptions options)
        {
            switch (options)
            {
                case ConflictResolutionOptions.TakeLeft:
                    return left;
                case ConflictResolutionOptions.TakeRight:
                    return right;
                default:
                    throw new MergeOperationException("There is conflicting change and no other resolution option was provided.");
            }
        }

        public static bool CompareEqual<T>(T left, T right)
        {
            if (left == null && right == null)
                return true;
            if (left == null && right != null)
                return false;
            if (right == null && left != null)
                return false;

            if (left is IEquatable<T>)
                return ((IEquatable<T>)left).Equals(right);
            else
                return Object.Equals(left, right);
        }
    }
}