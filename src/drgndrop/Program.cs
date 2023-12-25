using drgndrop.Services;
using Microsoft.AspNetCore.Http.Extensions;
using SevenZip;
using System.IO;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;
using Crypt = BCrypt.Net.BCrypt;

namespace drgndrop
{
    class Program
    {

#if DEBUG
        public static string Http = "http";
#else
        public static string Http = "https";
#endif

        public static string AppName = "Drgndrop";
        public static string DomainName = "localhost:5132";
        public static string UploadPath = Path.Combine("C:", "drgndrop", "uploads");
        public static string TempPath = Path.Combine("C:", "drgndrop", "temp");
        public static string LibPath = Path.Combine("C:", "drgndrop", "lib");
        public static string RepoPath = Path.Combine("C:", "drgndrop", "repo");
        public static string DatabasePath = Path.Combine("C:", "drgndrop", "db");
        public static long MaxFileSize = 50L * 1024 * 1024;
        public static string CommitSHA = "";

        public static string Title = "";

        public static string[] Args;

        public static void Main(string[] args)
        {
            Args = args;
            Initialize(Args);

            var builder = WebApplication.CreateBuilder(Args);
            builder.Services.AddRazorPages();
            builder.Services.AddServerSideBlazor();

            //DI
            builder.Services.AddScoped<IClipboardService, ClipboardService>();
            builder.Services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();

            builder.Services.AddScoped<UserService>();

            var app = builder.Build();

            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            //app.UseHttpsRedirection();

            app.UseStaticFiles();
            app.UseRouting();

            app.MapBlazorHub();
            app.MapFallbackToPage("/_Host");

            app.MapGet("/", async (HttpContext ctx) =>
            {
                ctx.Response.Redirect("/upload", true);
            });

            app.MapGet("/src", async (HttpContext ctx) =>
            {
                ctx.Response.Redirect($"https://github.com/{Git.Repo}", true);
            });

            app.MapGet("/dl/{path}", async (HttpContext ctx, string path) =>
            {

                string filePath = Path.Combine(UploadPath, path);

                bool exists = Directory.Exists(filePath);

                bool isDiscord = false;

                if ( ctx.Request.Headers.TryGetValue("User-Agent", out var userAgent) )
                {
                    isDiscord = userAgent.ToString().ToLower().Contains("discord");
                }

                if (!exists)
                {
                    if (isDiscord)
                    {
                        //Discord embed for not found maybe
                        return Results.Text("File not found", statusCode: 404);
                    }
                    else
                    {
                        return Results.Text("File not found", statusCode: 404);
                    }
                }

                var tempPath = Path.Combine(TempPath, $".{path}.tmp");

                try
                {
                    DrgnfileInfo? drgnfile = Drgnfile.Load(filePath);
                    if(drgnfile == null) return Results.Text("File not found", statusCode: 404);

                    var url = ctx.Request.GetDisplayUrl();
                    var splitUrl = url.Split('?');
                    var key = splitUrl.Length > 1 ? Utils.GetQuery(in splitUrl[1], "key") : "";
                    var passwordCheck = drgnfile.IsEncrypted ? Crypt.EnhancedVerify(key, drgnfile.PasswordHash) : true;
                    string mimeType = Utils.GetMIMEType(drgnfile.Name);
                    bool isMedia = Utils.IsMedia(mimeType);

                    if (isDiscord)
                    {
                        string metaTags = "";

                        if (drgnfile.IsEncrypted)
                        {
                            if (passwordCheck)
                            {
                                if (isMedia) goto render;
                                metaTags = Utils.GenerateFileMetaTags(path, key, drgnfile);
                            }
                            else
                            {
                                metaTags = Utils.GenerateFileMetaTags(path);
                            }
                        }
                        else
                        {
                            metaTags = Utils.GenerateFileMetaTags(path, drgnfile: drgnfile);
                        }

                        var template = new StreamReader(File.OpenRead("wwwroot/metaembed.html")).ReadToEnd();
                        template = template.Replace("@MetaTags", metaTags.ToString());

                        return Results.Text(metaTags, "text/html");
                    }
                    else
                    {
                        if(!passwordCheck)
                        {
                            return Results.Text("Wrong password", statusCode: 401);
                        }
                    }

                render:

                    if (!File.Exists(tempPath))
                    {
                        SevenZipExtractor extractor;
                        var dataPath = Path.Combine(filePath, "data");
                        if (drgnfile.IsEncrypted && key != "") extractor = new SevenZipExtractor(dataPath, key);
                        else extractor = new SevenZipExtractor(dataPath);

                        FileStream temp = new FileStream(tempPath, FileMode.Create, FileAccess.ReadWrite, FileShare.ReadWrite);
                        File.SetAttributes(temp.SafeFileHandle, FileAttributes.Hidden);
                        await extractor.ExtractFileAsync(0, temp);
                        await temp.DisposeAsync();
                    }

                    ctx.Response.Headers.Add("Content-Disposition", $"inline; filename=\"{drgnfile.Name}\"");

                    Results.StatusCode(200);
                    return Results.Stream(new FileStream(tempPath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite), mimeType, enableRangeProcessing: true);
                }
                catch (Exception ex)
                {
                    return Results.Text(ex.Message);
                }
            });

            Console.Write("\n\n\n");

            app.Run();
        }

        private static void Initialize(string[] args)
        {
            string path = "config.toml";
            if (!File.Exists(path))
            {
                StreamWriter writer = new StreamWriter(path);

                writer.WriteLine($"[host]");
                writer.WriteLine($"appname = \"Drgndrop\"");
                writer.WriteLine($"domain = \"localhost:5132\"");

                writer.WriteLine($"[file]");
                writer.WriteLine($"maxfilesize = 50 # in MB");
                writer.WriteLine($"uploadpath = \"C:/drgndrop/uploads/\"");
                writer.WriteLine($"temppath = \"C:/drgndrop/temp/\"");
                writer.WriteLine($"libpath = \"C:/drgndrop/lib/\"");
                writer.WriteLine($"repopath = \"C:/drgndrop/repo/\"");
                writer.WriteLine($"dbpath = \"C:/drgndrop/database/\"");

                writer.WriteLine($"[database]");
                writer.WriteLine($"password = \"\"");
                writer.WriteLine($"shared = true");

                writer.Close();
            }

            TOML toml = new TOML(path);
            if (toml.Table == null) return;

            DomainName = toml.Get("host", "domain", "localhost:5132");
            AppName = toml.Get("host", "appname", "Drgndrop");

            MaxFileSize = toml.Get("file", "maxfilesize", 50L);
            MaxFileSize *= (long)Math.Round(Math.Pow(1024, 2));

            UploadPath = toml.Get("file", "uploadpath", Path.Combine("C:", "drgndrop", "upload"));
            TempPath = toml.Get("file", "temppath", Path.Combine("C:", "drgndrop", "temp"));
            LibPath = toml.Get("file", "libpath", Path.Combine("C:", "drgndrop", "lib"));
            RepoPath = toml.Get("file", "repopath", Path.Combine("C:", "drgndrop", "repo"));
            DatabasePath = toml.Get("file", "dbpath", Path.Combine("C:", "drgndrop", "database"));

            if(GetArg("--delete-database") == "confirm")
            {
                if(File.Exists(Path.Combine(DatabasePath, "database.db")))
                {
                    File.Delete(Path.Combine(DatabasePath, "database.db"));
                }
            }

            Database.Initialize(
                Path.Combine(DatabasePath, "database.db"),
                toml.Get("database", "password", ""),
                toml.Get("database", "shared", true)
            );

            SevenZipBase.SetLibraryPath(Path.Combine(LibPath, "7z", "x64", "7za.dll"));

            Utils.IvokeInterval(TimeSpan.FromMinutes(30), FlushTempCache, true);
            Utils.IvokeInterval(TimeSpan.FromMinutes(30), FlushNullData, true);

            Git.GatherCommits();

            if (Directory.Exists(Path.Combine(RepoPath, ".git")))
            {
            }
        }

        public static void FlushTempCache()
        {
            foreach (var file in Directory.GetFiles(TempPath))
            {
                if (!Utils.IsFileLocked(file))
                {
                    File.Delete(file);
                }
            }
        }

        public static void FlushNullData()
        {
            foreach(var folder in Directory.GetDirectories(UploadPath))
            {
                foreach(var file in Directory.GetFiles(folder))
                {
                    if(file.EndsWith("data") && !Utils.IsFileLocked(file))
                    {
                        var info = new FileInfo(Path.Combine(UploadPath, folder, file));
                        if (info.Length <= 0)
                        {
                            Directory.Delete(Path.Combine(UploadPath, folder), true);
                            break;
                        }
                    }
                }
            }
        }

        public static string? GetArg(string arg)
        {
            for(int i = 0; i < Args.Length; ++i)
            {
                if (Args[i].StartsWith("--") && Args[i] == arg)
                {
                    if(Args.Length > i + 1)
                    {
                        if (Args[i + 1].StartsWith("--"))
                        {
                            return Args[i + 1];
                        }
                    }
                    return "";
                }
            }

            return null;
        }
    }
}