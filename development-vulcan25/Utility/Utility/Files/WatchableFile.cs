using System;
using System.ComponentModel;
using System.IO;
using System.Runtime.Serialization;
using System.Security.Permissions;
using Vulcan.Utility.ComponentModel;

namespace Vulcan.Utility.Files
{
    [DataContract(IsReference = true)]
    public abstract class WatchableFile : VulcanNotifyPropertyChanged, IDisposable
    {
        private readonly FileSystemWatcher _fileWatcher;

        public event EventHandler<FileSystemEventArgs> FileChangedEvent;

        [EnvironmentPermissionAttribute(SecurityAction.LinkDemand, Unrestricted = true)]
        protected WatchableFile(bool isReadOnly)
        {
            if (!isReadOnly)
            {
                _fileWatcher = new FileSystemWatcher();
                _fileWatcher.NotifyFilter = NotifyFilters.LastWrite;
            }
        }

        [EnvironmentPermissionAttribute(SecurityAction.LinkDemand, Unrestricted = true)]
        protected WatchableFile(string path, bool isReadOnly)
            : this(isReadOnly)
        {
            SetWatchedPath(path);
        }

        #region Methods

        [EnvironmentPermissionAttribute(SecurityAction.LinkDemand, Unrestricted = true)]
        public void SetWatchedPath(string path)
        {
            if (_fileWatcher != null)
            {
                _fileWatcher.Path = Path.GetDirectoryName(path);
                _fileWatcher.Filter = Path.GetFileName(path);
                EnableFileChangedEvents = true;
            }
        }

        [EnvironmentPermissionAttribute(SecurityAction.LinkDemand, Unrestricted = true)]
        public void SetChangedHandler(FileSystemEventHandler handler)
        {
            if (_fileWatcher != null)
            {
                _fileWatcher.Changed += handler;
            }
        }

        [EnvironmentPermissionAttribute(SecurityAction.LinkDemand, Unrestricted = true)]
        public void RemoveChangedHandler(FileSystemEventHandler handler)
        {
            if (_fileWatcher != null)
            {
                _fileWatcher.Changed -= handler;
            }
        }

        #endregion

        #region Properties

        [Browsable(false)]
        public DateTime LastWriteTime { get; set; }

        [Browsable(false)]
        public bool IsSaving { get; set; }

        [Browsable(false)]
        public bool EnableFileChangedEvents
        {
            [EnvironmentPermissionAttribute(SecurityAction.LinkDemand, Unrestricted = true)]
            get
            {
                if (_fileWatcher != null)
                {
                    return _fileWatcher.EnableRaisingEvents;
                }
                return false;
            }

            [EnvironmentPermissionAttribute(SecurityAction.LinkDemand, Unrestricted = true)]
            set
            {
                if (_fileWatcher != null)
                {
                    _fileWatcher.EnableRaisingEvents = value;
                }
            }
        }

        #endregion

        public void FileChanged(FileSystemEventArgs e)
        {
            // We're using an event since I want a design where the event logic 
            // can hook into a UI. Accessing a UI could require 
            // that I pass in a reference to the main Dispatcher, 
            // along with potentially other objects.
            // To keep things simpler, I'm adding a layer of indirection.
            if (FileChangedEvent != null)
            {
                FileChangedEvent(this, e);
            }
        }

        #region IDisposable

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                _fileWatcher.Dispose();
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        #endregion
    }
}
