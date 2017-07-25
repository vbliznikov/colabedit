using System;
using System.Collections.Generic;
using System.Linq;

using Google.DiffMatchPatch;

namespace CollabEdit.VersionControl.Operations
{
    public class CharBasedMerge : IMergeHandler<string>
    {
        public string Merge(string origin, string left, string right, ConflictResolutionOptions options = ConflictResolutionOptions.RaiseException)
        {
            var diffOps = new DiffOperations();

            var mergedScript = diffOps.GetDifference(origin, left)
                .MergeWith(diffOps.GetDifference(origin, right));

            return mergedScript.ToText();
        }
    }
}