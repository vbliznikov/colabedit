using System;
using System.Linq;
using System.Collections.Generic;

namespace CollabEdit.VersionControl.Operations
{
    public class DictionaryMergeHandler<TKey, TValue> : IMergeHandler<IDictionary<TKey, TValue>>
    {
        public IDictionary<TKey, TValue> Merge(IDictionary<TKey, TValue> origin, IDictionary<TKey, TValue> left,
            IDictionary<TKey, TValue> right, ConflictResolutionOptions options)
        {
            // Empty dictionaries left and right
            if (left.Count == 0 && right.Count == 0)
                return left;

            var mergedDictionary = new Dictionary<TKey, TValue>();

            var lEditScript = KeysEditScript<TKey>.From(origin.Keys, left.Keys);
            var rEditScript = KeysEditScript<TKey>.From(origin.Keys, right.Keys);
            var resultScript = lEditScript.Merge(rEditScript);

            // Chack that the key was changed either left or right, otherwise apply ConflictResolutinOptions
            foreach (TKey key in resultScript.CommonKeys)
            {
                var originValue = origin[key];
                var leftValue = left[key];
                var rightValue = right[key];

                var mergedValue = MergeUtils.Merge<TValue>(originValue, leftValue, rightValue, options);
                mergedDictionary.Add(key, mergedValue);
            }

            var commonInsertedKeys = lEditScript.InsertedKeys.Intersect(rEditScript.InsertedKeys);
            // Check that the same values was inserted, otherwise apply ConflictResolutionOptions
            foreach (TKey key in commonInsertedKeys)
            {
                var leftValue = left[key];
                var rightValue = right[key];

                var mergedValue = MergeUtils.Merge<TValue>(leftValue, rightValue, options);
                mergedDictionary.Add(key, mergedValue);
            }

            var delConflictLeft = rEditScript.DeletedKeys.Intersect(lEditScript.CommonKeys)
                .Where(key => !EqualityComparer<TKey>.Equals(origin[key], left[key]));
            var delConflictRight = lEditScript.DeletedKeys.Intersect(rEditScript.CommonKeys)
                .Where(key => !EqualityComparer<TKey>.Equals(origin[key], right[key]));

            switch (options)
            {
                case ConflictResolutionOptions.TakeRight:
                    foreach (var key in delConflictRight)
                        mergedDictionary.Add(key, right[key]);
                    break;
                case ConflictResolutionOptions.TakeLeft:
                    foreach (var key in delConflictLeft)
                        mergedDictionary.Add(key, left[key]);
                    break;
                default:
                    throw new MergeOperationException(string.Format("The key[{0}] was deleted at right but was changed at left.", key));
            }


            return mergedDictionary;
        }
    }
}