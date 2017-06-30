using System;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Xabe.FileLock
{
    /// <summary>
    ///     Providing file locks
    /// </summary>
    public class FileLock: ILock
    {
        private const string Extension = "lock";
        private readonly CancellationTokenSource _canceller;
        private readonly string _path;

        private FileLock()
        {
        }

        private FileLock(FileInfo fileToLock, TimeSpan lockTime, bool refreshContinuously)
        {
            _path = GetLockFileName(fileToLock);
            ReleaseDate = DateTime.UtcNow + lockTime;
            _canceller = new CancellationTokenSource();
            if(refreshContinuously)
            {
                Task.Run(() => RefreshLockTime(lockTime), _canceller.Token);
            }
        }

        private FileLock(FileInfo fileToLock, DateTime releaseDate)
        {
            _path = GetLockFileName(fileToLock);
            ReleaseDate = releaseDate;
            _canceller = new CancellationTokenSource();
        }

        private DateTime ReleaseDate { get => GetDateTime(_path); set => File.WriteAllText(_path, value.Ticks.ToString(), Encoding.UTF8); }

        /// <summary>
        ///     Stop refreshing lock and delete lock file
        /// </summary>
        public void Dispose()
        {
            _canceller.Cancel();
            File.Delete(_path);
        }

        /// <summary>
        ///     Extend lock by certain amount of time
        /// </summary>
        /// <param name="lockTime">How much time add to lock</param>
        public void AddTime(TimeSpan lockTime)
        {
            ReleaseDate = ReleaseDate + lockTime;
        }

        private void RefreshLockTime(TimeSpan lockTime)
        {
            var refreshTime = (int) (lockTime.TotalMilliseconds * 0.9);
            while(!_canceller.Token.IsCancellationRequested)
            {
                Task.Delay(refreshTime);
                AddTime(TimeSpan.FromMilliseconds(refreshTime));
            }
        }

        /// <summary>
        ///     Acquire lock.
        /// </summary>
        /// <param name="fileToLock">File to lock</param>
        /// <param name="releaseDate">Date after that lock is released</param>
        /// <returns>File lock. Null if lock already exists.</returns>
        public static ILock Acquire(FileInfo fileToLock, DateTime releaseDate)
        {
            if(!File.Exists(GetLockFileName(fileToLock)))
            {
                return new FileLock(fileToLock, releaseDate);
            }

            DateTime lockReleaseDate = GetDateTime(GetLockFileName(fileToLock));
            if(lockReleaseDate > DateTime.UtcNow)
            {
                return null;
            }
            return new FileLock(fileToLock, releaseDate);
        }

        /// <summary>
        ///     Acquire lock.
        /// </summary>
        /// <param name="fileToLock">File to lock</param>
        /// <param name="lockTime">Amount of time after that lock is released</param>
        /// <param name="refreshContinuously">Specify if FileLock should automatically refresh lock.</param>
        /// <returns>File lock. Null if lock already exists.</returns>
        public static ILock Acquire(FileInfo fileToLock, TimeSpan lockTime, bool refreshContinuously = false)
        {
            if(!File.Exists(GetLockFileName(fileToLock)))
            {
                return new FileLock(fileToLock, lockTime, refreshContinuously);
            }

            DateTime releaseDate = GetDateTime(GetLockFileName(fileToLock));
            if(releaseDate > DateTime.UtcNow)
            {
                return null;
            }
            return new FileLock(fileToLock, lockTime, refreshContinuously);
        }


        private static DateTime GetDateTime(string path)
        {
            var text = File.ReadAllText(path);
            var ticks = long.Parse(text);
            return new DateTime(ticks);
        }

        private static string GetLockFileName(FileInfo file)
        {
            return Path.ChangeExtension(file.FullName, Extension);
        }
    }
}
