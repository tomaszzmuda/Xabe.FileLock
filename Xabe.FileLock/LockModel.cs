using System;
using System.IO;
using System.Text;

namespace Xabe.FileLock
{
    internal class LockModel
    {
        private readonly string _path;

        public LockModel(string path)
        {
            _path = path;
        }

        internal DateTime ReleaseDate { get => GetDateTime(_path); set => SetReleaseDate(value); }

        private void SetReleaseDate(DateTime date)
        {
            lock(this)
            {
                using(var fs = new FileStream(_path, FileMode.OpenOrCreate, FileAccess.Write, FileShare.None))
                {
                    using(var sr = new StreamWriter(fs, Encoding.UTF8))
                    {
                        sr.Write(date.ToUniversalTime()
                                     .Ticks);
                    }
                }
            }
        }

        private DateTime GetDateTime(string path)
        {
            lock(this)
            {
                using(var fs = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read))
                {
                    using(var sr = new StreamReader(fs, Encoding.UTF8))
                    {
                        string text = sr.ReadToEnd();
                        long ticks = long.Parse(text);
                        return new DateTime(ticks, DateTimeKind.Utc);
                    }
                }
            }
        }
    }
}
