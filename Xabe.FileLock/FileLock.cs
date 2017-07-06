using System;
using System.IO;
using System.Text;
using System.Threading;

namespace Xabe.FileLock
{
    /// <summary>
    ///     Providing file locks
    /// </summary>
    public class FileLock: ILock
    {
        private const string Extension = "lock";
        private readonly object _fileLock = new object();
        private readonly string _path;
        private Timer _timer;

        private FileLock()
        {
        }

        /// <summary>
        ///     Creates reference to file lock on target file
        /// </summary>
        /// <param name="fileToLock">File which we want lock</param>
        public FileLock(FileInfo fileToLock)
        {
            _path = GetLockFileName(fileToLock);
        }

        /// <summary>
        ///     Creates reference to file lock on target file
        /// </summary>
        /// <param name="path"></param>
        public FileLock(string path)
        {
            _path = GetLockFileName(new FileInfo(path));
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
            SetReleaseDate(GetReleaseDate() + lockTime);
        }

        /// <inheritdoc />
        public bool TryAcquire(DateTime releaseDate)
        {
            try
            {
                if(File.Exists(_path) &&
                   GetDateTime(_path) > DateTime.UtcNow)
                    return false;
            }
            catch(Exception)
            {
                return false;
            }

            SetReleaseDate(releaseDate);
            return true;
        }

        /// <inheritdoc />
        public bool TryAcquire(TimeSpan lockTime, bool refreshContinuously = false)
        {
            if(!File.Exists(_path))
            {
                SetReleaseDate(DateTime.UtcNow + lockTime);
                return true;
            }
            try
            {
                if(File.Exists(_path) &&
                   GetDateTime(_path) > DateTime.UtcNow)
                    return false;
                SetReleaseDate(DateTime.UtcNow + lockTime);
            }
            catch(Exception)
            {
                return false;
            }
            if(refreshContinuously)
            {
                var refreshTime = (int) (lockTime.TotalMilliseconds * 0.9);
                _timer = new Timer(state => AddTime(TimeSpan.FromMilliseconds(refreshTime)), null, 0, refreshTime);
            }
            return true;
        }

        private DateTime GetReleaseDate()
        {
            return GetDateTime(_path);
        }

        private void SetReleaseDate(DateTime date)
        {
            lock(_fileLock)
            {
                using(var fs = new FileStream(_path, FileMode.OpenOrCreate, FileAccess.Write, FileShare.Read))
                {
                    using(var sr = new StreamWriter(fs, Encoding.UTF8))
                    {
                        sr.Write(date.Ticks.ToString());
                    }
                }
            }
        }

        private DateTime GetDateTime(string path)
        {
            lock(_fileLock)
            {
                using(var fs = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                {
                    using(var sr = new StreamReader(fs, Encoding.UTF8))
                    {
                        string text = sr.ReadToEnd();
                        long ticks = long.Parse(text);
                        return new DateTime(ticks);
                    }
                }
            }
        }

        private static string GetLockFileName(FileInfo file)
        {
            return Path.ChangeExtension(file.FullName, Extension);
        }
    }
}
