using System;
using System.IO;
using System.Threading;
using Xunit;

namespace Xabe.FileLock.Test
{
    public class FileLockTests
    {
        private readonly TimeSpan _timeVariable = TimeSpan.FromSeconds(5);
        private const string Extension = "lock";

        [Fact]
        public void AcquireSecondLock()
        {
            var file = new FileInfo(Path.GetTempFileName());
            new FileLock(file).TryAcquire(TimeSpan.FromHours(1));

            var fileLock = new FileLock(file).TryAcquire(TimeSpan.FromHours(1));
            Assert.False(fileLock);
        }

        [Fact]
        public void AcquireSecondLockAfterRelease()
        {
            var file = new FileInfo(Path.GetTempFileName());
            ILock fileLock = new FileLock(file);
            fileLock.TryAcquire(TimeSpan.FromSeconds(1));
            Thread.Sleep(1500);
            fileLock.TryAcquire(TimeSpan.FromSeconds(10));

            Assert.NotNull(fileLock);
        }

        [Fact]
        public void BasicLock()
        {
            var file = new FileInfo(Path.GetTempFileName());
            new FileLock(file).TryAcquire(TimeSpan.FromHours(1));

            Assert.True(File.Exists(Path.ChangeExtension(file.FullName, Extension)));
            var fileDate = new DateTime(long.Parse(File.ReadAllText(Path.ChangeExtension(file.FullName, Extension))));
            Assert.True(fileDate - DateTime.UtcNow - TimeSpan.FromHours(1) < _timeVariable);
        }

        [Fact]
        public void BasicLockToDate()
        {
            var file = new FileInfo(Path.GetTempFileName());
            new FileLock(file).TryAcquire(DateTime.UtcNow + TimeSpan.FromHours(1));

            Assert.True(File.Exists(Path.ChangeExtension(file.FullName, Extension)));
            var fileDate = new DateTime(long.Parse(File.ReadAllText(Path.ChangeExtension(file.FullName, Extension))));
            Assert.True(fileDate - DateTime.UtcNow - TimeSpan.FromHours(1) < _timeVariable);
        }

        [Fact]
        public void BasicLockWithAddTime()
        {
            var file = new FileInfo(Path.GetTempFileName());
            var fileLock = new FileLock(file);
            fileLock.TryAcquire(TimeSpan.FromHours(1));
            fileLock.AddTime(TimeSpan.FromHours(1));

            Assert.True(File.Exists(Path.ChangeExtension(file.FullName, Extension)));
            var fileDate = new DateTime(long.Parse(File.ReadAllText(Path.ChangeExtension(file.FullName, Extension))));
            Assert.True(fileDate - DateTime.UtcNow - TimeSpan.FromHours(2) < _timeVariable);
        }

        [Fact]
        public void Dispose()
        {
            var file = new FileInfo(Path.GetTempFileName());
            ILock fileLock = new FileLock(file);
            if(fileLock.TryAcquire(TimeSpan.FromHours(1)))
            {
                using(fileLock)
                {
                    Assert.True(File.Exists(Path.ChangeExtension(file.FullName, Extension)));
                }
            }

            Assert.False(File.Exists(Path.ChangeExtension(file.FullName, Extension)));
        }
    }
}
