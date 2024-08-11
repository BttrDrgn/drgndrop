using LiteDB;
using Microsoft.AspNetCore.Http.HttpResults;
using System;
using System.Xml.Linq;
using Crypt = BCrypt.Net.BCrypt;


namespace drgndrop
{
    public class Database
    {
        public static LiteDatabase DB;
        public static ILiteCollection<User> Users;
        public static ILiteCollection<InviteKey> InviteKeys;
        public const string ADMIN_GUID = "00000000-0000-0000-0000-000000000000";

        public static void Initialize(string file, string password = "", bool shared = true)
        {
            bool initialSetup = false;
            if(!File.Exists(Path.Combine(Program.DatabasePath, "database.db")))
            {
                initialSetup = true;
            }
            
            string dbInit = $"Filename={file}";

            if (password != "") dbInit += $";Password={password}";
            if (shared) dbInit += $";connection=shared";

            DB = new LiteDatabase(dbInit);

            Users = DB.GetCollection<User>("users");
            InviteKeys = DB.GetCollection<InviteKey>("invitekeys");

            if (initialSetup)
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Logger.InfoLog("This is the initial setup of the database.");
                Console.ResetColor();

                if (GetUserByName("admin") == null)
                {
                    CreateAdminUser("changeme");

                    Logger.InfoLog("A user has been created under the name \"admin\" and the password is \"changeme\"");

                    Console.ForegroundColor = ConsoleColor.Red;
                    Logger.WarningLog("It is highly recommended you change the password prior to making the interface public!");
                    Console.ResetColor();

                }
            }
        }

        //USERS
        public static User? GetUserByName(string name)
        {
            List<User> tempUsers = Users.Query().Where(x => x.Name == name.ToLower()).ToList();
            if (tempUsers.Count == 1) return tempUsers[0];
            return null;
        }

        public static User? GetUserBySession(string session)
        {
            List<User> tempUsers = Users.Query().Where(x => x.SessionToken == session).ToList();
            if (tempUsers.Count == 1)
            {
                if (tempUsers[0].GUID != ADMIN_GUID)
                {
                    return tempUsers[0];
                }
            }
            return null;
        }

        public static User? GetUserByGUID(string guid)
        {
            List<User> tempUsers = Users.Query().Where(x => x.GUID == guid.ToLower()).ToList();
            if (tempUsers.Count == 1) return tempUsers[0];
            return null;
        }

        public static bool CreateAdminUser(string password)
        {
            string name = "admin";
            if (GetUserByName(name) == null)
            {
                User newUser = new User
                {
                    Name = name,
                    Password = Crypt.EnhancedHashPassword(password),
                    GUID = ADMIN_GUID,
                    SessionToken = Guid.NewGuid().ToString(),
                    InvitedBy = ADMIN_GUID,
                    Creation = DateTimeOffset.Now.ToUnixTimeSeconds(),
                    Group = Group.Admin,
                    Uploads = new List<DrgnfileInfo>(),
                };

                Users.Insert(newUser);
                Users.EnsureIndex(x => x.Id);
                return true;
            }
            return false;
        }

        public static bool CreateUser(string name, string password, InviteKey inviteKey)
        {
            name = name.ToLower();

            if (GetUserByName(name) == null)
            {
                User newUser = new User
                {
                    Name = name,
                    InvitedBy = inviteKey.Creator,
                    Password = Crypt.EnhancedHashPassword(password),
                    GUID = Guid.NewGuid().ToString(),
                    SessionToken = Guid.NewGuid().ToString(),
                    Creation = DateTimeOffset.Now.ToUnixTimeSeconds(),
                    Group = Group.User,
                    Uploads = new List<DrgnfileInfo>(),
                };

                Users.Insert(newUser);
                Users.EnsureIndex(x => x.Id);

                inviteKey.UsedBy = newUser.GUID;
                inviteKey.Update();

                return true;
            }
            return false;
        }

        public static List<User> GetUsers()
        {
            return Users.Query().ToList();
        }

        //INVITE KEYS
        public static InviteKey CreateInviteKey(User? user)
        {
            InviteKey newKey = new InviteKey()
            {
                Value = Guid.NewGuid().ToString(),
                Creator = user.GUID,
                Creation = DateTimeOffset.Now.ToUnixTimeSeconds(),
                UsedBy = "",
            };

            InviteKeys.Insert(newKey);
            InviteKeys.EnsureIndex(x => x.Id);

            return newKey;
        }

        public static List<InviteKey> GetKeys(User? user)
        {
            List<InviteKey> keys = new List<InviteKey>();
            if (user != null)
            {
                keys = InviteKeys.Query().Where(x => x.Creator == user.GUID).ToList();
            }
            return keys;
        }

        public static InviteKey? GetKey(string key)
        {
            List<InviteKey> tempKeys = InviteKeys.Query().Where(x => x.Value == key).ToList();
            if (tempKeys.Count == 1) return tempKeys[0];
            return null;
        }
    }
}
