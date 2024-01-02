namespace drgndrop
{
    public class DrgnfileInfo
    {
        public string ID { get; set; }
        public string Name { get; set; }
        public long Creation { get; set; }
        public long Size { get; set; }
        public string PasswordHash { get; set; }
        public bool IsEncrypted => !String.IsNullOrEmpty(PasswordHash);
    }
}
