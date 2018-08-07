using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.Extensions.Options;

namespace Complaya
{
    public class FolderWatcher : IDisposable
    {
        public FileSystemWatcher FileWatcher { get; set; }
        public FolderWatcherConfiguration Configuration { get; set; }

        public List<string> FilesAdded { get; set; } = new List<string>();

        public FolderWatcher(IOptions<FolderWatcherConfiguration> configuration, Serilog.ILogger logger)
        {
            FileWatcher = new FileSystemWatcher();
            Configuration = configuration.Value;

            this.logger = logger;
            var filter = Configuration.Filter;
            if (!string.IsNullOrWhiteSpace(filter))
            {
                FileWatcher.Filter = filter;
            }

            var path = Configuration.Path;
            if (!string.IsNullOrWhiteSpace(path))
            {
                FileWatcher.Path = path;
            }

            FileWatcher.Created += Created;
            FileWatcher.Deleted += Deleted;

        }

        private void Deleted(object sender, FileSystemEventArgs e)
        {
            if (FilesAdded.Remove(e.FullPath))
            {
                logger.Information($"File removed {e.Name}.");
            }

        }

        private void Created(object sender, FileSystemEventArgs e)
        {
            FilesAdded.Add(e.FullPath);
            logger.Information($"New file added {e.Name}.");
        }

        public void AddNotifyFilter(NotifyFilters notifyFilters)
        {
            FileWatcher.NotifyFilter = notifyFilters;
        }
        private readonly Serilog.ILogger logger;


        #region IDisposable Support
        private bool disposedValue = false;

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    FileWatcher.Dispose();
                    FileWatcher = null;

                }
                disposedValue = true;
            }
        }


        public void Dispose()
        {

            Dispose(true);

        }
        #endregion


    }


}
