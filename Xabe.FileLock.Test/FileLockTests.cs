using System;
using System.Globalization;
using System.IO;
using Xunit;

namespace Xabe.FileLock.Test
{ 
    public class FileLockTests
    {
        private readonly TimeSpan timeVariable = TimeSpan.FromSeconds(5);
        private const string Extension = "lock";

        [Fact]
        public void BasicLock()
        {
            var file = new FileInfo(Path.GetTempFileName());
            FileLock.Acquire(file, TimeSpan.FromHours(1));
            
            Assert.True(File.Exists(Path.ChangeExtension(file.FullName, Extension)));
            var fileDate = new DateTime(long.Parse(File.ReadAllText(Path.ChangeExtension(file.FullName, Extension))));
            Assert.True(fileDate - DateTime.UtcNow - TimeSpan.FromHours(1) < timeVariable);
        }

        [Fact]
        public void BasicLockWithAddTime()
        {
            var file = new FileInfo(Path.GetTempFileName());
            var fileLock = FileLock.Acquire(file, TimeSpan.FromHours(1));
            fileLock.AddLockTime(TimeSpan.FromHours(1));

            Assert.True(File.Exists(Path.ChangeExtension(file.FullName, Extension)));
            var fileDate = new DateTime(long.Parse(File.ReadAllText(Path.ChangeExtension(file.FullName, Extension))));
            Assert.True(fileDate - DateTime.UtcNow - TimeSpan.FromHours(2) < timeVariable);
        }

        [Fact]
        public void AcquireSecondLock()
        {
            var file = new FileInfo(Path.GetTempFileName());
            FileLock.Acquire(file, TimeSpan.FromHours(1));

            var fileLock = FileLock.Acquire(file, TimeSpan.FromHours(1));
            Assert.Null(fileLock);
        }

        [Fact]
        public void Dispose()
        {
            var file = new FileInfo(Path.GetTempFileName());
            using(var fileLock = FileLock.Acquire(file, TimeSpan.FromHours(1)))
            {
                Assert.True(File.Exists(Path.ChangeExtension(file.FullName, Extension)));
            }

            Assert.False(File.Exists(Path.ChangeExtension(file.FullName, Extension)));
        }
    }
}
