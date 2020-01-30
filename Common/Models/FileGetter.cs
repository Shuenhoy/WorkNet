using System;
using System.Threading.Tasks;
using System.IO;
using MessagePack;
using System.IO.Compression;

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
        public byte[] bytes;
        public bool zippedFolder;
        [SerializationConstructor]
        public FileBytes()
        {

        }
        public FileBytes(byte[] b, bool zf = false)
        {
            bytes = b;
            zippedFolder = zf;
        }
        public async Task WriteTo(string filename)
        {
            Directory.CreateDirectory(Path.GetDirectoryName(filename));
            await File.WriteAllBytesAsync(filename, bytes);

            if (zippedFolder)
            {
                ZipFile.ExtractToDirectory(filename, Path.GetDirectoryName(filename) + Path.GetFileNameWithoutExtension(filename));
                File.Delete(filename);
            }
        }
    }
}