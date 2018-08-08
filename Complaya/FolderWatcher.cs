using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.Extensions.Options;

namespace Complaya
{
    public class FolderWatcher : IDisposable
    {
        public FileSystemWatcher FileWatcher { get; private set; }
        public FolderWatcherConfiguration Configuration { get; }

        public List<string> FilesAdded { get; set; } = new List<string>();

        public FolderWatcher(IOptions<FolderWatcherConfiguration> configuration, Serilog.ILogger logger)
        {
            Configuration = configuration.Value;

            this.logger = logger;
            var filter = Configuration.Filter;
            var path = Configuration.Path;
            if (string.IsNullOrWhiteSpace(filter) || string.IsNullOrWhiteSpace(path))
            {
                throw new ArgumentNullException("Missing configuration.");
            }
            FileWatcher = new FileSystemWatcher(path, filter);

            FileWatcher.Created += OnCreated;
            FileWatcher.Deleted += OnDeleted;
            FileWatcher.Changed += OnChanged;
            FileWatcher.Error += OnError;

            FileWatcher.EnableRaisingEvents = true;
            ProcessExistingFiles();

        }

        private void ProcessExistingFiles()
        {
            if (Configuration.ShouldProcessAlreadyExistingFiles)
            {
                FilesAdded.AddRange(Directory.GetFiles(Configuration.Path, Configuration.Filter));
            }
        }

        public virtual void OnError(object sender, ErrorEventArgs e)
        {
            logger.Error($"The FileSystemWatcher has detected an error", e.GetException());
        }

        public virtual void OnChanged(object sender, FileSystemEventArgs e)
        {
            WatcherChangeTypes wct = e.ChangeType;
            logger.Information("File {0} {1}", e.FullPath, wct.ToString());
        }

        public virtual void OnDeleted(object sender, FileSystemEventArgs e)
        {
            if (FilesAdded.Remove(e.FullPath))
            {
                logger.Information($"File removed {e.Name}.");
            }

        }

        public virtual void OnCreated(object sender, FileSystemEventArgs e)
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
