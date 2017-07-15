using System;
using System.IO;
using System.Threading;

namespace Xabe.FileLock
{
    /// <summary>
    ///     Providing file locks
    /// </summary>
    public class FileLock: ILock
    {
        private const string Extension = "lock";
        private readonly LockModel _content;
        private readonly string _path;
        private Timer _timer;

        private FileLock()
        {
        }

        /// <summary>
        ///     Creates reference to file lock on target file
        /// </summary>
        /// <param name="fileToLock">File we want lock</param>
        public FileLock(FileInfo fileToLock): this(fileToLock.FullName)
        {
        }

        /// <summary>
        ///     Creates reference to file lock on target file
        /// </summary>
        /// <param name="path">Path to file we want lock</param>
        public FileLock(string path)
        {
            _path = GetLockFileName(path);
            _content = new LockModel(_path);
        }

        /// <summary>
        ///     Stop refreshing lock and delete lock file
        /// </summary>
        public void Dispose()
        {
            _timer?.Dispose();
            if(File.Exists(_path))
                File.Delete(_path);
        }

        /// <summary>
        ///     Extend lock by certain amount of time
        /// </summary>
        /// <param name="lockTime">How much time add to lock</param>
        public void AddTime(TimeSpan lockTime)
        {
            _content.ReleaseDate += lockTime;
        }

        /// <inheritdoc />
        public bool TryAcquire(DateTime releaseDate)
        {
            if(File.Exists(_path) &&
               _content.ReleaseDate > DateTime.UtcNow)
                return false;

            _content.ReleaseDate = releaseDate;
            return true;
        }

        /// <inheritdoc />
        public bool TryAcquire(TimeSpan lockTime, bool refreshContinuously = false)
        {
            if(!File.Exists(_path))
            {
                _content.ReleaseDate = DateTime.UtcNow + lockTime;
                return true;
            }
            if(File.Exists(_path) &&
               _content.ReleaseDate > DateTime.UtcNow)
                return false;
            _content.ReleaseDate = DateTime.UtcNow + lockTime;

            if(refreshContinuously)
            {
                var autoEvent = new AutoResetEvent(false);
                var refreshTime = (int) (lockTime.TotalMilliseconds * 0.9);
                _timer = new Timer(state => AddTime(TimeSpan.FromMilliseconds(refreshTime)), autoEvent, 0, refreshTime);
            }
            return true;
        }

        private string GetLockFileName(string path)
        {
            return Path.ChangeExtension(path, Extension);
        }
    }
}
