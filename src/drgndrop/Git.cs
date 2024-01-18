using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace drgndrop
{
    public class Author
    {
        public string Name { get; set; }
        public string URL { get; set; }
        public string AvatarURL { get; set; }
    }

    public class Commit
    {
        public string SHA { get; set; }
        public string Message { get; set; }
        public Author Author { get; set; }
    }

    public class Git
    {
        public static string Repo = "BttrDrgn/drgndrop";
        public static string ApiUrl => $"https://api.github.com/repos/{Repo}";
        public static List<Commit> Commits = new List<Commit>();
        public static string LocalSHA => ThisAssembly.Git.Sha;

        public static async Task GatherCommits()
        {
            Commits.Clear();

            using (HttpClient client = new HttpClient())
            {
                // Set the user agent to avoid 403 Forbidden responses
                client.DefaultRequestHeaders.UserAgent.ParseAdd(Program.AppName);

                // Make the request to the GitHub API
                HttpResponseMessage response = await client.GetAsync($"{ApiUrl}/commits?per_page=10");

                if (response.IsSuccessStatusCode)
                {
                    // Read and parse the JSON response
                    JArray array = JArray.Parse(await response.Content.ReadAsStringAsync());
                    foreach(var entry in array)
                    {
                        Commits.Add(new Commit()
                        {
                            SHA = entry["sha"].ToString(),
                            Message = entry["commit"]["message"].ToString(),
                            Author = new Author()
                            {
                                Name = entry["author"]["login"].ToString(),
                                URL = entry["author"]["url"].ToString(),
                                AvatarURL = entry["author"]["avatar_url"].ToString(),
                            }
                        });
                    }
                }
                else
                {
                    // Handle error responses
                    Logger.ErrorLog($"{response.StatusCode} - {response.ReasonPhrase}");
                }
            }
        }
    }
}
