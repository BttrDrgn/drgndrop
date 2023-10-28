using drgndrop;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Net.Http.Headers;
using SevenZip;

namespace drgndrop
{
    class Program
    {

#if DEBUG
        public static string DomainName = "localhost:5132";
#else
        public static string DomainName = "drgndrop.me";
#endif

        public static string UploadPath = Path.Combine("C:", "drgndrop", "uploads");
        public static string TempPath = Path.Combine("C:", "drgndrop", "temp");
        public static string LibPath = Path.Combine("C:", "drgndrop", "lib");
        public static int MaxFileSize = 100 * 1024 * 1024;
        public static SevenZipCompressor Compressor = null;

        public static void Main(string[] args)
        {
            SevenZipBase.SetLibraryPath(Path.Combine(LibPath, "7z", "x64", "7za.dll"));
            Compressor = new SevenZipCompressor();

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

                    if (isArchive && splitUrl.Length > 1)
                    {
                        var key = Utils.GetQuery(in splitUrl[1], "key");
                        SevenZipExtractor extractor = new SevenZipExtractor(filePath, key);

                        var tempPath = Path.Combine(TempPath, $".{Utils.GenID()}.tmp");
                        FileStream temp = new FileStream(tempPath, FileMode.Create, FileAccess.ReadWrite);
                        File.SetAttributes(temp.SafeFileHandle, FileAttributes.Hidden);
                        await extractor.ExtractFileAsync(0, temp);
                        temp.Close();

                        var res = Results.Stream(File.OpenRead(tempPath), Utils.GetMIMEType(Path.GetFileName(filePath)));
                        return res;
                    }
                    else
                    {
                        SevenZipExtractor extractor = new SevenZipExtractor(filePath);

                        var tempPath = Path.Combine(TempPath, $".{Utils.GenID()}.tmp");
                        FileStream temp = new FileStream(tempPath, FileMode.Create, FileAccess.ReadWrite);
                        File.SetAttributes(temp.SafeFileHandle, FileAttributes.Hidden);
                        await extractor.ExtractFileAsync(0, temp);
                        temp.Close();

                        var res = Results.Stream(File.OpenRead(tempPath), Utils.GetMIMEType(Path.GetFileName(filePath)));
                        return res;
                    }
                }
                catch (Exception ex)
                {
                    return Results.Text(ex.Message);
                }
            });

            app.Run();
        }
    }
}