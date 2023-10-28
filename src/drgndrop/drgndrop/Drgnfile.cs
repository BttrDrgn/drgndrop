
using MessagePack;

namespace drgndrop
{
    //Ex:

    // 1698259224
    // .png
    // $2a$12$Y4YlamrY6U.e/b/jRh6QG.X/w7m/XMd08bP4bBNzJ/J1ZG8jEM0py
    // 0x00 0x00 0x00...

    [MessagePackObject]
    public class Drgnfile
    {
        [Key(0)] public string Name { get; set; }
        [Key(1)] public long Creation { get; set; }
        [Key(2)] public string PasswordHash { get; set; } //Bcrypt hash
        [Key(3)] public byte[] Data { get; set; }

        public Drgnfile(string name, string passwordHash)
        {
            Name = name;
            PasswordHash = passwordHash;

            Creation = DateTimeOffset.Now.ToUnixTimeSeconds();
        }

        public Drgnfile(string name, long creation, string passwordHash, byte[] data)
        {
            Name = name;
            Creation = creation;
            PasswordHash = passwordHash;
            Data = data;
        }

        public void Write(Stream stream)
        {
            List<byte> totalStream = new();
            byte[] buffer = new byte[32];
            int read;

            while ((read = stream.Read(buffer, 0, buffer.Length)) > 0)
            {
                totalStream.AddRange(buffer.Take(read));
            }

            Data = totalStream.ToArray();
        }

        public void Save(string file)
        {
            FileStream fs = new FileStream(file, FileMode.Create, FileAccess.Write);
            MessagePackSerializer.Serialize(fs, this);
            fs.Close();
        }

        public Drgnfile Load(string file)
        {
            FileStream fs = new FileStream(file, FileMode.Open, FileAccess.Read);
            var retn = MessagePackSerializer.Deserialize<Drgnfile>(fs);
            fs.Close();
            return retn;
        }
    }
}
