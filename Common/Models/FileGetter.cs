using System;
using System.Threading.Tasks;
using System.IO;
using MessagePack;

namespace WorkNet.Common.Models
{
    [MessagePack.Union(0, typeof(FileBytes))]
    public interface FileGetter
    {
        Task WriteTo(string filename);
    }

    [MessagePackObject(keyAsPropertyName: true)]
    public class FileBytes : FileGetter
    {
        byte[] bytes;
        public readonly bool zippedFolder;
        public FileBytes(byte[] b, bool zf = false)
        {
            bytes = b;
            zippedFolder = zf;
        }
        public Task WriteTo(string filename)
        {
            return File.WriteAllBytesAsync(filename, bytes);
        }
    }
}