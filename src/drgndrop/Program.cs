using drgndrop;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.DataProtection.KeyManagement;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Net.Http.Headers;
using SevenZip;
using Tomlyn;

namespace drgndrop
{
    class Program
    {
        public static string AppName = "Drgndrop";
        public static string DomainName = "localhost:5132";
        public static string UploadPath = Path.Combine("C:", "drgndrop", "uploads");
        public static string TempPath = Path.Combine("C:", "drgndrop", "temp");
        public static string LibPath = Path.Combine("C:", "drgndrop", "lib");
        public static long MaxFileSize = 50L * 1024 * 1024;

        public static string Title = "";

        public static void Main(string[] args)
        {
            Initialize();

            SevenZipBase.SetLibraryPath(Path.Combine(LibPath, "7z", "x64", "7za.dll"));

            var builder = WebApplication.CreateBuilder(args);
            builder.Services.AddRazorPages();
            builder.Services.AddServerSideBlazor();

            builder.Services.AddSignalR(hubOptions =>
            {
                hubOptions.MaximumReceiveMessageSize = 64000;
            });

            //DI
            builder.Services.AddScoped<IClipboardService, ClipboardService>();

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

            app.MapGet("/src", async (HttpContext ctx) =>
            {
                ctx.Response.Redirect("https://github.com/BttrDrgn/drgndrop", true);
            });

            app.MapGet("/files/{path}", async (HttpContext ctx, string path) =>
            {
                try
                {
                    string filePath = Path.Combine(UploadPath, path);
                    bool exists = File.Exists(filePath);

                    if(!exists)
                    {
                        return Results.Text("File not found");
                    }

                    var url = ctx.Request.GetDisplayUrl();
                    var splitUrl = url.Split('?');
                    bool isArchive = Utils.Is7zArchive(filePath);

                    //Drgnfile drgnfile = Drgnfile.Load(filePath);

                    if (isArchive)
                    {
                        SevenZipExtractor extractor;

                        if (splitUrl.Length > 1)
                        {
                            var key = Utils.GetQuery(in splitUrl[1], "key");
                            extractor = new SevenZipExtractor(filePath, key);
                        }
                        else
                        {
                            extractor = new SevenZipExtractor(filePath);
                        }

                        var tempPath = Path.Combine(TempPath, $".{Utils.GenID()}.tmp");
                        FileStream temp = new FileStream(tempPath, FileMode.Create, FileAccess.ReadWrite);
                        File.SetAttributes(temp.SafeFileHandle, FileAttributes.Hidden);
                        await extractor.ExtractFileAsync(0, temp);
                        temp.Close();

                        return Results.Stream(File.OpenRead(tempPath), Utils.GetMIMEType(filePath));
                    }
                    else
                    {
                        return Results.Stream(File.OpenRead(filePath), Utils.GetMIMEType(Path.GetFileName(filePath)));
                    }
                }
                catch (Exception ex)
                {
                    return Results.Text(ex.Message);
                }
            });

            app.Run();
        }

        private static void Initialize()
        {
            string path = "config.toml";
            if (File.Exists(path))
            {
                TOML toml = new TOML(path);
                if (toml.Table == null) return;

                DomainName = toml.Get("host", "domain", "localhost:5132");
                AppName = toml.Get("host", "appname", "Drgndrop");

                MaxFileSize = toml.Get("file", "maxfilesize", 50L);
                MaxFileSize *= (long)Math.Round(Math.Pow(1024, 2));

                UploadPath = toml.Get("file", "uploadpath", Path.Combine("C:", "drgndrop", "upload"));
                TempPath = toml.Get("file", "temppath", Path.Combine("C:", "drgndrop", "temp"));
                LibPath = toml.Get("file", "libppath", Path.Combine("C:", "drgndrop", "lib"));
            }
            else
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

                writer.Close();
            }
        }
    }
}