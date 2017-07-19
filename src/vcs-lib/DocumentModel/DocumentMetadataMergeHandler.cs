using System;
using CollabEdit.VersionControl.Operations;

namespace CollabEdit.DocumentModel
{
    public class DocumentMetadataMergeHandler : IMergeHandler<DocumentMetadata>
    {
        private DictionaryMergeHandler<string, EditState> mergeHandler = new DictionaryMergeHandler<string, EditState>();
        public DocumentMetadata Merge(DocumentMetadata origin, DocumentMetadata left, DocumentMetadata right, ConflictResolutionOptions options)
        {
            return new DocumentMetadata(mergeHandler.Merge(origin, left, right, options));
        }
    }
}