﻿using System;
using System.IO;
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
        private readonly CancellationTokenSource _cancellationTokenSource = new CancellationTokenSource();
        private readonly LockModel _content;
        private readonly string _path;

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
            _cancellationTokenSource.Cancel();
            if(File.Exists(_path))
                File.Delete(_path);
        }

        /// <summary>
        ///     Extend lock by certain amount of time
        /// </summary>
        /// <param name="lockTime">How much time add to lock</param>
        public async Task AddTime(TimeSpan lockTime)
        {
            await _content.TrySetReleaseDate(await _content.GetReleaseDate() + lockTime);
        }

        /// <inheritdoc />
        public async Task<bool> TryAcquire(DateTime releaseDate)
        {
            if(File.Exists(_path) &&
               await _content.GetReleaseDate() > DateTime.UtcNow)
                return false;

            return await _content.TrySetReleaseDate(releaseDate);
        }

        /// <inheritdoc />
        public async Task<bool> TryAcquire(TimeSpan lockTime, bool refreshContinuously = false)
        {
            if(!File.Exists(_path))
            {
                if(!await _content.TrySetReleaseDate(DateTime.Now + lockTime))
                {
                    return false;
                }
                return true;
            }
            if(File.Exists(_path) &&
               await _content.GetReleaseDate() > DateTime.UtcNow)
                return false;
            if(!await _content.TrySetReleaseDate(DateTime.Now + lockTime))
            {
                return false;
            }

            if(refreshContinuously)
            {
                var refreshTime = (int) (lockTime.TotalMilliseconds * 0.9);
                Task.Run(async () =>
                {
                    while(!_cancellationTokenSource.IsCancellationRequested)
                    {
                        await AddTime(TimeSpan.FromMilliseconds(refreshTime));
                        await Task.Delay(refreshTime);
                    }
                }, _cancellationTokenSource.Token);
            }
            return true;
        }

        private string GetLockFileName(string path)
        {
            return Path.ChangeExtension(path, Extension);
        }
    }
}
