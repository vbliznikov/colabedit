using System;
using System.Collections.Generic;
using CollabEdit.VersionControl.Operations;

namespace CollabEdit.DocumentModel
{
    public class TextDocumentMergeHandler : MergeHandler<TextDocument>
    {
        public TextDocumentMergeHandler() : base(new TextDocumentEqulityComparer())
        {

        }
        IMergeHandler<string> textMergeHandler = new StringMergeHandler();
        IMergeHandler<DocumentMetadata> metadataHandler = new DocumentMetadataMergeHandler();

        protected override TextDocument DoMerge(TextDocument origin, TextDocument left, TextDocument right, ConflictResolutionOptions options)
        {
            string text = textMergeHandler.Merge(origin.Text, left.Text, right.Text, options);
            var meta = metadataHandler.Merge(origin.Metadata, left.Metadata, right.Metadata, options);

            return new TextDocument(text, meta);
        }
    }
}