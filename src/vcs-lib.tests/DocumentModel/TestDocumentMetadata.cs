using System;
using System.IO;
using NUnit.Framework;

namespace CollabEdit.DocumentModel.Tests
{
    [TestFixture]
    public class TestDocumentMetadata
    {
        [Test]
        public void Test_EmtyInstanceEqulity()
        {
            var meta1 = new DocumentMetadata();
            var meta2 = new DocumentMetadata();

            Assert.That(meta1, Is.EqualTo(meta2), "Instances created by default constructor should be equal");
        }
    }
}