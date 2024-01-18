using Microsoft.VisualBasic;

namespace drgndrop
{
    public class Command
    {
        public string Name { get; set; }
        public string Help { get; set; }
        public Action<Cmd, string[]> Callback { get; set; }

        public Command(string name, string help, Action<Cmd, string[]> callback)
        {
            Name = name;
            Help = help;
            Callback = callback;
        }
    }

    public class Cmd
    {
        public User? User { get; set; }
        public List<string> Output { get; set; } = new List<string>();
        public string Buffer { get; set; }
        public static List<Command> Commands = new List<Command>();

        public Cmd(User? user)
        {
            User = user;
            Output.Add($"{Program.AppName} admin console");
            Output.Add($"logged in as {user.Name}");
        }

        public void Execute()
        {
            WriteLine($"> {Buffer}");

            string cmd = "";

            var split = Buffer.Split(" ");
            if (split.Length > 0) cmd = split[0];
            else split = new string[1] { Buffer };

            bool found = false;
            foreach(var c in Commands)
            {
                if(c.Name == cmd)
                {
                    c.Callback.Invoke(this, split);
                    found = true;
                    break;
                }
            }

            if(!found) WriteLine($"err: command \"{cmd}\" not found");

            Buffer = "";
        }

        public void WriteLine(string msg)
        {
            this.Output.Add(msg);
        }

        public static void RegisterCommand(string name, string help, Action<Cmd, string[]> callback)
        {
            Commands.Add(new Command(name, help, callback));
        }

        public static void Initialize()
        {
            {
                RegisterCommand("help", "usage: help [cmd:string]; prints all commands if no arg supplied", (cmd, args) =>
                {
                    if (args.Count() == 1)
                    {
                        cmd.WriteLine("Commands list:");

                        foreach (var c in Commands)
                        {
                            cmd.WriteLine($"{c.Name}");
                        }
                    }
                    else
                    {
                        foreach(var c in Commands)
                        {
                            if(c.Name == args[1])
                            {
                                cmd.WriteLine($"? {c.Help}");
                                break;
                            }
                        }
                    }
                });
                
                RegisterCommand("pw", "usage: pw (user:string) (oldpass:string) (newpass:string); change passwords", (cmd, args) =>
                {
                    if (args.Count() == 4)
                    {
                        User? user = Database.GetUserByName(args[1]);
                        if (user == null)
                        {
                            cmd.WriteLine($"err: no user found by name \"{args[1]}\"");
                            return;
                        }

                        if (!user.PasswordCheck(args[2]))
                        {
                            cmd.WriteLine($"err: oldpass did not match");
                            return;
                        }

                        if (user.ChangePassword(args[2], args[3]))
                        {
                            cmd.WriteLine($"info: password for \"{args[1]}\" has been changed");

                        }
                        else
                        {
                            cmd.WriteLine($"err: oldpass did not match");
                        }
                    }
                    else
                    {
                        cmd.WriteLine("usage: pw (user:string) (oldpass:string) (newpass:string)");
                    }
                });
                
                RegisterCommand("cls", "usage: cls; clears the command output buffer", (cmd, _) =>
                {
                    cmd.Output = new List<string>();
                });
                
                RegisterCommand("group", "usage: group (user:string) (group:string); changes the group of a user, has limitations", (cmd, args) =>
                {
                    if (args.Count() == 3)
                    {
                        User? user = Database.GetUserByName(args[1]);

                        if (user == null)
                        {
                            cmd.WriteLine($"err: no user found by name \"{args[1]}\"");
                            return;
                        }

                        bool limitations = cmd.User.GUID != Database.ADMIN_GUID;

                        if (limitations && user.GUID == cmd.User.GUID)
                        {
                            cmd.WriteLine($"err: unable to change your own group");
                            return;
                        }

                        Group newGroup;

                        switch (args[2].ToLower())
                        {
                            case "user":
                                newGroup = Group.User;
                                break;

                            case "donor":
                            case "donator":
                                newGroup = Group.Donor;
                                break;

                            case "mod":
                            case "moderator":
                                newGroup = Group.Moderator;
                                break;

                            case "admin":
                                newGroup = Group.Admin;
                                break;

                            case "owner":
                                newGroup = Group.Owner;
                                break;

                            default:
                                cmd.WriteLine($"err: unable to determine which group to be set. options: user, donor, donator, mod, moderator, admin, owner");
                                return;
                        }

                        bool isInGroup = user.IsInGroup(newGroup);

                        if(isInGroup)
                        {
                            user.UnsetGroup(newGroup);

                            if (user.Group == 0)
                            {
                                cmd.WriteLine($"info: user \"{user.Name}\" has been removed from all groups and set back to \"{Group.User}\"");
                                user.SetGroup(Group.User);
                            }
                            else cmd.WriteLine($"info: user \"{user.Name}\" has been removed from group \"{newGroup}\"");
                        }
                        else
                        {
                            user.SetGroup(newGroup);
                            cmd.WriteLine($"info: user \"{user.Name}\" has been added to group \"{newGroup}\"");
                        }

                        

                        user.Update();
                    }
                    else
                    {
                        cmd.WriteLine("usage: group (user:string) (group:int)");
                    }
                });
                
                RegisterCommand("keys", "usage: keys (user:string); prints all keys and information that the user has", (cmd, args) =>
                {
                    if (args.Count() == 2)
                    {
                        User? user = Database.GetUserByName(args[1]);
                        if (user == null)
                        {
                            cmd.WriteLine($"err: no user found by name \"{args[1]}\"");
                            return;
                        }

                        var keys = Database.GetKeys(user);
                        cmd.WriteLine($"Value | Created | Used");
                        foreach (var key in keys)
                        {
                            cmd.WriteLine($"{key.Value} : {key.Creation} : {!string.IsNullOrEmpty(key.UsedBy)}");
                        }
                    }
                    else
                    {
                        cmd.WriteLine("usage: keys (user:string)");
                    }
                });
                
                RegisterCommand("genkey", "usage: genkey (user:string) [count:int]; generates keys for the user", (cmd, args) =>
                {
                    if (args.Count() >= 2)
                    {
                        int count = 1;
                        if(args.Count() == 3)
                        {
                            if(!int.TryParse(args[2], out count))
                            {
                                cmd.WriteLine($"err: failed to parse {args[2]} as int");
                            }
                        }
                        User? user = Database.GetUserByName(args[1]);
                        if (user == null)
                        {
                            cmd.WriteLine($"err: no user found by name \"{args[1]}\"");
                            return;
                        }

                        cmd.WriteLine($"Keys created for \"{user.Name}\":");
                        List<InviteKey> keys = new List<InviteKey>();
                        for(int i = 0; i < count; ++i)
                        {
                            keys.Add(user.CreateInviteKey());
                            cmd.WriteLine($"{keys[i].Value}");
                        }
                    }
                    else
                    {
                        cmd.WriteLine("usage: genkey (user:string)");
                    }
                });
                
                RegisterCommand("del", "usage: del (fileid:string)", (cmd, args) =>
                {
                    if(args.Count() == 2)
                    {
                        string fileId = args[1];
                        bool deleted = false;
                        string filePath = Path.Combine(Program.UploadPath, fileId);

                        if (Directory.Exists(filePath))
                        {
                            try
                            {
                                Directory.Delete(filePath, true);
                                deleted = true;
                            }
                            catch (Exception e) 
                            {
                                cmd.WriteLine($"err: {e.Message}");
                            }
                        }
                        else
                        {
                            cmd.WriteLine($"warn: unable to find file with id \"{fileId}\"");
                        }

                        var users = Database.GetUsers();
                        foreach (var user in users)
                        {
                            if (user.Uploads == null) continue;

                            foreach(var upload in user.Uploads)
                            {
                                if(upload.ID == fileId)
                                {
                                    user.Uploads.Remove(upload);
                                    cmd.WriteLine($"Deleted id \"{upload.ID}\" from user \"{user.Name}\"");
                                    deleted = true;
                                    user.Update();
                                    break;
                                }
                            }
                        }

                        if (deleted) cmd.WriteLine($"File \"{fileId}\" has been deleted");
                        else cmd.WriteLine($"err: unable to find both file with id or in user uploads");
                    }
                    else
                    {
                        cmd.WriteLine("usage: del (fileid:string)");
                    }
                });
            }
        }
    }
}
