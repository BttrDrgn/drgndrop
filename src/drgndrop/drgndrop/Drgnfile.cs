namespace drgndrop
{
    //Ex:

    // 1698259224
    // .png
    // $2a$12$Y4YlamrY6U.e/b/jRh6QG.X/w7m/XMd08bP4bBNzJ/J1ZG8jEM0py
    // 0x00 0x00 0x00...

    public class Drgnfile
    {
        public long Creation { get; set; }
        public string Extension { get; set; }
        public string PasswordHash { get; set; } //Bcrypt hash
        public byte[] Data { get; set; }

        public void Save(string file)
        {

        }

        public void Load(string file)
        {

        }
    }
}
