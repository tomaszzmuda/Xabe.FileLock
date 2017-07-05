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
        private Timer _timer;

        private FileLock()
        {
        }

        /// <summary>
        ///     Creates reference to file lock on target file
        /// </summary>
        /// <param name="fileToLock"></param>
        public FileLock(FileInfo fileToLock)
        {
            _path = GetLockFileName(fileToLock);
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
        public async void AddTime(TimeSpan lockTime)
        {
            await SetReleaseDate(await GetReleaseDate() + lockTime);
        }

        /// <inheritdoc />
        public async Task<bool> TryAcquire(DateTime releaseDate)
        {
            try
            {
                if(File.Exists(_path) &&
                   await GetDateTime(_path) > DateTime.UtcNow)
                    return false;
            }
            catch(Exception)
            {
                return false;
            }

            await SetReleaseDate(releaseDate);
            return true;
        }

        /// <inheritdoc />
        public async Task<bool> TryAcquire(TimeSpan lockTime, bool refreshContinuously = false)
        {
            try
            {
                if(File.Exists(_path) &&
                   await GetDateTime(_path) > DateTime.UtcNow)
                    return false;
                await SetReleaseDate(DateTime.UtcNow + lockTime);
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

        private async Task<DateTime> GetReleaseDate()
        {
            return await GetDateTime(_path);
        }

        private async Task SetReleaseDate(DateTime date)
        {
            using(var fs = new FileStream(_path, FileMode.OpenOrCreate, FileAccess.Write, FileShare.Read))
            {
                using(var sr = new StreamWriter(fs, Encoding.UTF8))
                {
                    await sr.WriteAsync(date.Ticks.ToString());
                }
            }
        }

        private static async Task<DateTime> GetDateTime(string path)
        {
            using(var fs = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            {
                using(var sr = new StreamReader(fs, Encoding.UTF8))
                {
                    string text = await sr.ReadToEndAsync();
                    long ticks = long.Parse(text);
                    return new DateTime(ticks);
                }
            }
        }

        private static string GetLockFileName(FileInfo file)
        {
            return Path.ChangeExtension(file.FullName, Extension);
        }
    }
}
