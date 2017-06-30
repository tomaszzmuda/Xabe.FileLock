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
        private readonly string _path;
        private CancellationTokenSource _canceller;

        private FileLock()
        {
        }

        public FileLock(FileInfo fileToLock)
        {
            _path = GetLockFileName(fileToLock);
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

        /// <inheritdoc />
        public bool TryAcquire(DateTime releaseDate)
        {
            if(File.Exists(_path) &&
               GetDateTime(_path) > DateTime.UtcNow)
            {
                return false;
            }

            ReleaseDate = releaseDate;
            _canceller = new CancellationTokenSource();
            return true;
        }

        /// <inheritdoc />
        public bool TryAcquire(TimeSpan lockTime, bool refreshContinuously = false)
        {
            if(File.Exists(_path) &&
               GetDateTime(_path) > DateTime.UtcNow)
            {
                return false;
            }
            ReleaseDate = DateTime.UtcNow + lockTime;
            _canceller = new CancellationTokenSource();
            if(refreshContinuously)
            {
                Task.Run(() => RefreshLockTime(lockTime), _canceller.Token);
            }
            return true;
        }

        public void Release()
        {
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
