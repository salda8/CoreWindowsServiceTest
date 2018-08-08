using System;
using MongoDbGenericRepository.Models;

namespace Complaya
{
    public class DocumentToArchive : IDocument
    {
        public byte[] Data { get; set; }
        public Guid Id { get; set; }
        public int Version { get; set; }
    }
}