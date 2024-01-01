using Microsoft.AspNetCore.Components;

namespace drgndrop.Services
{
    public class UserService
    {
        public User? CurrentUser { get; set; } = null;
        public bool LoggedIn => CurrentUser != null;
        public List<Action> LoginCallbacks { get; set; } = new List<Action>();
        public List<Action> LogoutCallbacks { get; set; } = new List<Action>();

        public void Login(string sessionToken)
        {
            CurrentUser = Database.GetUserBySession(sessionToken);
            if (CurrentUser != null) RunLoginCallbacks();
        }

        public void Logout()
        {
            CurrentUser = null;
            RunLogoutCallbacks();
        }

        public void RegisterLoginCallback(Action callback)
        {
            LoginCallbacks.Add(callback);
        }

        public void RunLoginCallbacks()
        {
            foreach (var cb in LoginCallbacks)
            {
                cb.Invoke();
            }
        }

        public void RegisterLogoutCallback(Action callback)
        {
            LogoutCallbacks.Add(callback);
        }

        public void RunLogoutCallbacks()
        {
            foreach (var cb in LogoutCallbacks)
            {
                cb.Invoke();
            }
        }
    }
}
