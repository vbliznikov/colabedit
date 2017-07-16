using System;

namespace CollabEdit.VersionControl.Operations
{
    public class DumbMergehandler<TValue> : IMergeHandler<TValue>
    {
        public TValue Merge(TValue origin, TValue left, TValue right,
            ConflictResolutionOptions options = ConflictResolutionOptions.RaiseException)
        {
            return MergeUtils.Merge(origin, left, right, options);
        }
    }
}