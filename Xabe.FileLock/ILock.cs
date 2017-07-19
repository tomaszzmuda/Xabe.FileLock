using System;
using System.Threading.Tasks;

namespace Xabe.FileLock
{
    /// <summary>
    ///     Implementetion of FileLock
    /// </summary>
    public interface ILock: IDisposable
    {
        /// <summary>
        ///     Extend lock by certain amount of time
        /// </summary>
        /// <param name="lockTime">How much time add to lock</param>
        Task AddTime(TimeSpan lockTime);

        /// <summary>
        ///     Acquire lock.
        /// </summary>
        /// <param name="releaseDate">Date after that lock is released</param>
        /// <returns>File lock. False if lock already exists.</returns>
        Task<bool> TryAcquire(DateTime releaseDate);

        /// <summary>
        ///     Acquire lock.
        /// </summary>
        /// <param name="lockTime">Amount of time after that lock is released</param>
        /// <param name="refreshContinuously">Specify if FileLock should automatically refresh lock.</param>
        /// <returns>File lock. False if lock already exists.</returns>
        Task<bool> TryAcquire(TimeSpan lockTime, bool refreshContinuously = false);
    }
}
