using System;
using System.Collections.Generic;

namespace CollabEdit.VersionControl.Operations
{
    public class StringMergeHandler : MergeHandler<string>
    {
        public StringMergeHandler() : base(StringComparer.Ordinal) { }
        protected override string DoMerge(string origin, string left, string right, ConflictResolutionOptions options)
        {
            throw new NotImplementedException();
        }
    }
}