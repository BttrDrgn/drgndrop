using Crypt = BCrypt.Net.BCrypt;

namespace drgndrop
{
    public enum Group
    {
        User = 0,
        Moderator,
        Admin,
        Owner,
        Count
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

        public void Update()
        {
            Database.Users.Update(this);
        }
    }
}
