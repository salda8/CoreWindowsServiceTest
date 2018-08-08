using System;
using MongoDbGenericRepository.Models;

namespace Complaya
{
    public class ParsedDocument
    {
        public string DocumentType { get; set; }
        public bool Success { get; set; }
        public string Data { get; set; }

    }

    public class DocumentToArchive : IDocument
    {
        public byte[] Data { get; set; }
        public Guid Id { get; set; }
        public int Version { get; set; }
    }
}