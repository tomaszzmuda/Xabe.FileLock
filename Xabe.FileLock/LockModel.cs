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

        internal async Task SetReleaseDate(DateTime date)
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

        internal async Task<DateTime> GetReleaseDate(string path = "")
        {
            using(var fs = new FileStream(string.IsNullOrWhiteSpace(path) ? _path : path, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                using(var sr = new StreamReader(fs, Encoding.UTF8))
                {
                    string text = await sr.ReadToEndAsync();
                    long ticks = long.Parse(text);
                    return new DateTime(ticks, DateTimeKind.Utc);
                }
            }
        }
    }
}
