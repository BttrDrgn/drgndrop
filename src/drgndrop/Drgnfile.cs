using System.Runtime.Serialization.Formatters.Binary;
using Crypt = BCrypt.Net.BCrypt;
using System.Text.Json;

namespace drgndrop
{
    public class DrgnfileInfo
    {
        public string Name { get; set; }
        public long Creation { get; set; }
        public long Size { get; set; }
        public string PasswordHash { get; set; }
        public bool IsEncrypted => PasswordHash != "";
    }

    public class Drgnfile
    {
        public DrgnfileInfo Info;

        public Drgnfile(string name, string password, long size)
        {
            Info = new DrgnfileInfo();

            Info.Name = name;
            Info.PasswordHash = Crypt.EnhancedHashPassword(password);
            Info.Size = size;
            Info.Creation = DateTimeOffset.Now.ToUnixTimeSeconds();
        }

        public async Task Save(string file, Stream data)
        {
            Directory.CreateDirectory(file);

            FileStream infoFs = new FileStream(Path.Combine(file, "info"), FileMode.Create, FileAccess.ReadWrite);
            JsonSerializer.Serialize(infoFs, Info);
            infoFs.Close();

            FileStream dataFs = new FileStream(Path.Combine(file, "data"), FileMode.Create, FileAccess.ReadWrite);

            data.Position = 0;
            byte[] buffer = new byte[data.Length];
            while (await data.ReadAsync(buffer, 0, (int)data.Length) is int read && read > 0)
            {
                await dataFs.WriteAsync(buffer, 0, read);
            }

            dataFs.Close();
        }

        public static DrgnfileInfo? Load(string file, Stream dataStream)
        {
            DrgnfileInfo? info = Load(file);
            dataStream = File.OpenRead(Path.Combine(file, "data"));
            return info;
        }

        public static DrgnfileInfo? Load(string file)
        {
            DrgnfileInfo? info = JsonSerializer.Deserialize<DrgnfileInfo>(File.OpenRead(Path.Combine(file, "info")));
            return info;
        }
    }
}
