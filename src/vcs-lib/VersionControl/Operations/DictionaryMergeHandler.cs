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
            if (origin == null) throw new ArgumentNullException(nameof(origin));
            if (left == null) throw new ArgumentNullException(nameof(left));
            if (right == null) throw new ArgumentException(nameof(right));

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

            // Inserted only left
            var insLeft = lEditScript.InsertedKeys.Except(rEditScript.InsertedKeys);
            foreach (var key in insLeft)
                mergedDictionary.Add(key, left[key]);

            //Inserted only right
            var insRight = rEditScript.InsertedKeys.Except(lEditScript.InsertedKeys);
            foreach (var key in insRight)
                mergedDictionary.Add(key, right[key]);

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
                .Where(key => !MergeUtils.CompareEqual(origin[key], left[key]));
            var delConflictRight = lEditScript.DeletedKeys.Intersect(rEditScript.CommonKeys)
                .Where(key => !MergeUtils.CompareEqual(origin[key], right[key]));

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
                    if (delConflictLeft.Count() > 0 || delConflictRight.Count() > 0)
                        throw new MergeOperationException("There are some keys deleted  at one side and modified at other.");
                    break;
            }


            return mergedDictionary;
        }
    }
}