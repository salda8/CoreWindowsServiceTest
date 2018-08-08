using System;
using System.Collections.Generic;
using System.IO;

namespace Complaya
{
    public interface IFolderWatcher : IDisposable
    {
        void Start();
        void OnError(object sender, ErrorEventArgs e);
        void OnChanged(object sender, FileSystemEventArgs e);
        void OnDeleted(object sender, FileSystemEventArgs e);
        void OnCreated(object sender, FileSystemEventArgs e);
        void AddNotifyFilter(NotifyFilters notifyFilters);

        IEnumerable<string> FilesAdded{get;}

    }


}
