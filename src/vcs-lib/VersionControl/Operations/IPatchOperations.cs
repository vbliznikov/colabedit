using System;
using System.Collections.Generic;

namespace DiffMatchPatch
{
    public interface IPatchOperations
    {
        List<Patch> patch_make(string text1, string text2);
        List<Patch> patch_make(List<Diff> diffs);

        List<Patch> patch_make(string text1, string text2, List<Diff> diffs);

        List<Patch> patch_make(string text1, List<Diff> diffs);
        List<Patch> patch_deepCopy(List<Patch> patches);
    
        Object[] patch_apply(List<Patch> patches, string text);
    
        string patch_addPadding(List<Patch> patches);
        void patch_splitMax(List<Patch> patches);
    
        string patch_toText(List<Patch> patches);
        List<Patch> patch_fromText(string textline);
    }
}