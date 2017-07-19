using System;
using System.Collections.Generic;

namespace CollabEdit.DocumentModel
{
    public class TextDocument
    {
        private string _text;

        public TextDocument() : this(string.Empty, new DocumentMetadata())
        {

        }
        public TextDocument(string text, DocumentMetadata metadata)
        {
            if (text == null) throw new ArgumentNullException(nameof(text));
            if (metadata == null)
                Metadata = new DocumentMetadata();
            else
                Metadata = metadata;

            _text = text;
        }
        public DocumentMetadata Metadata { get; }
        public string Text
        {
            get { return _text; }
            set
            {
                if (value == null) throw new ArgumentNullException(nameof(value));
                _text = value;
            }
        }
    }

}