using System;
using System.Globalization;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Xabe.FileLock
{
    public class FileLock : IDisposable
    {
        private readonly string _path;
        private const string Extension = "lock";
        private readonly CancellationTokenSource _canceller;

        private DateTime ReleaseDate
        {
            get => GetDateTime(_path);
            set => File.WriteAllText(_path, value.Ticks.ToString(), Encoding.UTF8);
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

        private void RefreshLockTime(TimeSpan lockTime)
        {
            var refreshTime = (int)(lockTime.TotalMilliseconds * 0.9);
            while (!_canceller.Token.IsCancellationRequested)
            {
                Thread.Sleep(refreshTime);
                AddLockTime(TimeSpan.FromMilliseconds(refreshTime));
            }
        }

        public void AddLockTime(TimeSpan lockTime)
        {
            ReleaseDate = ReleaseDate + lockTime;
        }

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

        public void Dispose()
        {
            _canceller.Cancel();
            File.Delete(_path);
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
