using System;
using System.Collections.Generic;

namespace CollabEdit.VersionControl.Operations
{
    public class StringMergeHandler : MergeHandler<string>
    {
        private IMergeHandler<string> _strategy;
        public StringMergeHandler() : base(StringComparer.Ordinal)
        {
            _strategy = new CharBasedMerge();
        }
        protected override string DoMerge(string origin, string left, string right, ConflictResolutionOptions options)
        {
            return _strategy.Merge(origin, left, right, options);
        }
    }
}