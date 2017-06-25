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
    public class FileLock: IDisposable
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

        private DateTime ReleaseDate { get => GetDateTime(_path); set => File.WriteAllText(_path, value.Ticks.ToString(), Encoding.UTF8); }

        /// <summary>
        ///     Stop refreshing lock and delete lock file
        /// </summary>
        public void Dispose()
        {
            _canceller.Cancel();
            File.Delete(_path);
        }

        private void RefreshLockTime(TimeSpan lockTime)
        {
            var refreshTime = (int) (lockTime.TotalMilliseconds * 0.9);
            while(!_canceller.Token.IsCancellationRequested)
            {
                Task.Delay(refreshTime);
                AddLockTime(TimeSpan.FromMilliseconds(refreshTime));
            }
        }

        /// <summary>
        ///     Extend lock by certain amount of time
        /// </summary>
        /// <param name="lockTime"></param>
        public void AddLockTime(TimeSpan lockTime)
        {
            ReleaseDate = ReleaseDate + lockTime;
        }

        /// <summary>
        ///     Acquire lock.
        /// </summary>
        /// <param name="fileToLock">File to lock</param>
        /// <param name="lockTime">Amount of time after that lock is released</param>
        /// <param name="refreshContinuously">Specify if FileLock should automatically refresh lock.</param>
        /// <returns>File lock. Null if lock already exists.</returns>
        public static FileLock Acquire(FileInfo fileToLock, TimeSpan lockTime, bool refreshContinuously = false)
        {
            if(!File.Exists(GetLockFileName(fileToLock)))
            {
                return new FileLock(fileToLock, lockTime, refreshContinuously);
            }

            var releaseDate = GetDateTime(GetLockFileName(fileToLock));
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
