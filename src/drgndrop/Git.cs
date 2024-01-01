using System.Text.Json;

namespace drgndrop
{
    public class Author
    {
        public string Name { get; set; }
        public string Email { get; set; }
    }

    public class Commit
    {
        public string Message { get; set; }
        public string Author { get; set; }
        public string SHA { get; set; }
    }

    public class Git
    {
        public static string Repo = "BttrDrgn/drgndrop";
        public static string ApiUrl => $"https://api.github.com/repos/{Repo}";
        public static List<string> Commits = new List<string>();

        public static async Task GatherCommits()
        {
            Commits.Clear();

            using (HttpClient client = new HttpClient())
            {
                // Set the user agent to avoid 403 Forbidden responses
                client.DefaultRequestHeaders.UserAgent.ParseAdd("YourAppName");

                // Make the request to the GitHub API
                HttpResponseMessage response = await client.GetAsync($"{ApiUrl}/commits?per_page=10");

                if (response.IsSuccessStatusCode)
                {
                    // Read and parse the JSON response
                    string jsonContent = await response.Content.ReadAsStringAsync();
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
