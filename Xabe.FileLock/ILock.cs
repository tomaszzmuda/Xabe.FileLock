using System;

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
        void AddTime(TimeSpan lockTime);
    }
}
