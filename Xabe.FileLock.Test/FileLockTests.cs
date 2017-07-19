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
        public async void AcquireSecondLock()
        {
            var file = new FileInfo(Path.GetTempFileName());
            await new FileLock(file).TryAcquire(TimeSpan.FromHours(1));

            bool fileLock = await new FileLock(file).TryAcquire(TimeSpan.FromHours(1));
            Assert.False(fileLock);
        }

        [Fact]
        public async void AcquireSecondLockAfterRelease()
        {
            var file = new FileInfo(Path.GetTempFileName());
            ILock fileLock = new FileLock(file);
            await fileLock.TryAcquire(TimeSpan.FromSeconds(1));
            Thread.Sleep(1500);
            await fileLock.TryAcquire(TimeSpan.FromSeconds(10));

            Assert.NotNull(fileLock);
        }

        [Fact]
        public async void BasicLock()
        {
            var file = new FileInfo(Path.GetTempFileName());
            await new FileLock(file).TryAcquire(TimeSpan.FromHours(1));

            Assert.True(File.Exists(Path.ChangeExtension(file.FullName, Extension)));
            var fileDate = new DateTime(long.Parse(File.ReadAllText(Path.ChangeExtension(file.FullName, Extension))));
            Assert.True(fileDate - DateTime.UtcNow - TimeSpan.FromHours(1) < _timeVariable);
        }

        [Fact]
        public async void BasicLockToDate()
        {
            var file = new FileInfo(Path.GetTempFileName());
            await new FileLock(file).TryAcquire(DateTime.UtcNow + TimeSpan.FromHours(1));

            Assert.True(File.Exists(Path.ChangeExtension(file.FullName, Extension)));
            var fileDate = new DateTime(long.Parse(File.ReadAllText(Path.ChangeExtension(file.FullName, Extension))));
            Assert.True(fileDate - DateTime.UtcNow - TimeSpan.FromHours(1) < _timeVariable);
        }

        [Fact]
        public async void BasicLockWithAddTime()
        {
            var file = new FileInfo(Path.GetTempFileName());
            var fileLock = new FileLock(file);
            await fileLock.TryAcquire(TimeSpan.FromHours(1));
            await fileLock.AddTime(TimeSpan.FromHours(1));

            Assert.True(File.Exists(Path.ChangeExtension(file.FullName, Extension)));
            var fileDate = new DateTime(long.Parse(File.ReadAllText(Path.ChangeExtension(file.FullName, Extension))));
            Assert.True(fileDate - DateTime.UtcNow - TimeSpan.FromHours(2) < _timeVariable);
        }

        [Fact]
        public async void CannotWriteToFile()
        {
            var file = new FileInfo(Path.GetTempFileName());
            ILock fileLock = new FileLock(file);
            if(await fileLock.TryAcquire(TimeSpan.FromHours(1)))
                using(fileLock)
                {
                    string pathToLock = Path.ChangeExtension(file.FullName, Extension);
                    using(FileStream stream = File.Open(pathToLock, FileMode.Open, FileAccess.Write, FileShare.ReadWrite))
                    {
                        await Assert.ThrowsAsync<IOException>(async () => await fileLock.AddTime(TimeSpan.FromHours(1)));
                    }
                }
        }

        [Fact]
        public async void Dispose()
        {
            var file = new FileInfo(Path.GetTempFileName());
            ILock fileLock = new FileLock(file);
            if(await fileLock.TryAcquire(TimeSpan.FromHours(1)))
                using(fileLock)
                {
                    Assert.True(File.Exists(Path.ChangeExtension(file.FullName, Extension)));
                }

            Assert.False(File.Exists(Path.ChangeExtension(file.FullName, Extension)));
        }

        [Fact]
        public async void Many()
        {
            int i = 100;
            var result = true;
            for(int j = 0; j < i; j++)
            {
                var file = new FileInfo(Path.GetTempFileName());
                if(!await new FileLock(file).TryAcquire(TimeSpan.FromHours(1)) &&
                   !File.Exists(Path.ChangeExtension(file.FullName, Extension)))
                {
                    result = false;
                    break;
                }
            }

            Assert.True(result);
        }

        [Fact]
        public async void ManyProcess()
        {
            var file = new FileInfo(Path.GetTempFileName());
            var fileLock = new FileLock(file);
            await fileLock.TryAcquire(TimeSpan.FromHours(1));
            fileLock.AddTime(TimeSpan.FromHours(1));

            var fileDate = new DateTime(long.Parse(File.ReadAllText(Path.ChangeExtension(file.FullName, Extension))));
            Assert.True(fileDate - DateTime.UtcNow - TimeSpan.FromHours(2) < _timeVariable);
        }
    }
}
