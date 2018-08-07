using System.Collections.Generic;

public class DocumentTypeConfiguration{
    public HashSet<string> ToSendToVirtualUser { get; set; }
    public HashSet<string> ToArchive{get;set;}
}
