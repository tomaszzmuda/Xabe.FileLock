using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace Xabe.FileLock
{
    internal class LockModel
    {
        private readonly string _path;

        public LockModel(string path)
        {
            _path = path;
        }

        internal async Task<bool> TrySetReleaseDate(DateTime date)
        {
            try
            {
                using(var fs = new FileStream(_path, FileMode.OpenOrCreate, FileAccess.Write, FileShare.None))
                {
                    using(var sr = new StreamWriter(fs, Encoding.UTF8))
                    {
                        await sr.WriteAsync(date.ToUniversalTime()
                                                .Ticks.ToString());
                    }
                }
            }
            catch(Exception)
            {
                return false;
            }
            return true;
        }

        internal async Task<DateTime> GetReleaseDate(string path = "")
        {
            try
            {
                using(var fs = new FileStream(string.IsNullOrWhiteSpace(path) ? _path : path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                {
                    using(var sr = new StreamReader(fs, Encoding.UTF8))
                    {
                        string text = await sr.ReadToEndAsync();
                        long ticks = long.Parse(text);
                        return new DateTime(ticks, DateTimeKind.Utc);
                    }
                }
            }
            catch(Exception)
            {
                return DateTime.MaxValue;
            }
        }
    }
}
