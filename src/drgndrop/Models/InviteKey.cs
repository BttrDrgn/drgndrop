namespace drgndrop
{
    public class InviteKey
    {
        public int Id { get; set; }
        public string Value { get; set; }
        public string Creator { get; set; }
        public long Creation { get; set; }
        public string UsedBy { get; set; }

        public void UseKey(User? user)
        {
            UsedBy = user.GUID;
            Update();
        }

        public void Update()
        {
            Database.InviteKeys.Update(this);
        }
    }
}
