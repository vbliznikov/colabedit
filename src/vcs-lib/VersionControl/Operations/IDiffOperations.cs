using System.Collections.Generic;

namespace DiffMatchPatch
{
    public interface IDiffOperations
    {
        List<Diff> diff_main(string text1, string text2);
        List<Diff> diff_main(string text1, string text2, bool checklines);
    
    
        void diff_cleanupSemantic(List<Diff> diffs);
        void diff_cleanupSemanticLossless(List<Diff> diffs);
        void diff_cleanupEfficiency(List<Diff> diffs);
        void diff_cleanupMerge(List<Diff> diffs);
    
        int diff_xIndex(List<Diff> diffs, int loc);
    
        string diff_prettyHtml(List<Diff> diffs);
    
        string diff_text1(List<Diff> diffs);
        string diff_text2(List<Diff> diffs);
    
        int diff_levenshtein(List<Diff> diffs);
    
        string diff_toDelta(List<Diff> diffs);
        List<Diff> diff_fromDelta(string text1, string delta);
    }
}