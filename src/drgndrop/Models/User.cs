using Crypt = BCrypt.Net.BCrypt;

namespace drgndrop
{
    [Flags] public enum Group
    {
        User = 1 << 0,
        Donor = 1 << 1,
        Moderator = 1 << 2,
        Admin = 1 << 3,
        Owner = 1 << 4,
    }

    public class User
    {
        public int Id { get; set; }
        public Group Group { get; set; }
        public string InvitedBy { get; set; }
        public string Name { get; set; }
        public string Password { get; set; }
        public string GUID { get; set; }
        public string SessionToken { get; set; }
        public List<DrgnfileInfo> Uploads { get; set; }
        public long Creation { get; set; }

        public bool PasswordCheck(string pass)
        {
            return Crypt.EnhancedVerify(pass, Password);
        }

        public bool ChangePassword(string oldPass, string newPass)
        {
            if (PasswordCheck(oldPass))
            {
                Password = Crypt.EnhancedHashPassword(newPass);
                SessionToken = Guid.NewGuid().ToString();
                Update();

                return true;
            }

            return false;
        }

        public InviteKey CreateInviteKey()
        {
            var newKey = Database.CreateInviteKey(this);
            Update();
            return newKey;
        }

        public bool OwnsFile(string id)
        {
            foreach (var file in Uploads)
            {
                if (file.ID == id) return true;
            }
            return false;
        }

        public void Update()
        {
            Database.Users.Update(this);
        }

        public bool IsInGroup(Group group)
        {
            return (Group & group) == group;
        }

        public void SetGroup(Group group)
        {
            Group |= group;
        }

        public void UnsetGroup(Group group)
        {
            Group &= ~group;
        }
    }
}
