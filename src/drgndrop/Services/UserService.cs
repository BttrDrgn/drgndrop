namespace drgndrop.Services
{
    public class UserService
    {
        public User? CurrentUser { get; set; } = null;
        public bool LoggedIn => CurrentUser != null;

        public void Login(string sessionToken)
        {
            CurrentUser = Database.GetUserBySession(sessionToken);
        }
    }
}
