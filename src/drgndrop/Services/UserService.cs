namespace drgndrop.Services
{
    public class UserService
    {
        public User? CurrentUser { get; set; } = null;
        public bool LoggedIn => CurrentUser != null;
    }
}
