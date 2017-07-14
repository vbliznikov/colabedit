namespace CollabEdit.VersionControl
{
    public static class MergeUtils
    {
        public static TValue Merge<TValue>(TValue origin, TValue left, TValue right,
            ConflictResolutionOptions options = ConflictResolutionOptions.RaiseException)
        {
            if (left == null && right == null) return left;
            if (origin == null)
            {
                if (left == null) return right;
                if (right == null) return left;
            }

            if (left.Equals(right)) return left;
            if (left.Equals(origin)) return right;
            if (right.Equals(origin)) return left;

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
    }

    public enum ConflictResolutionOptions
    {
        RaiseException, TakeLeft, TakeRight
    }
}