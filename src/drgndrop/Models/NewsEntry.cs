namespace drgndrop
{
    public class NewsEntry
    {
        public string Title { get; set; }
        public string ShortDescription { get; set; }
        public string Content { get; set; }
        public DateTime Creation { get; set; }
        public DateTime Edited { get; set; }

        public NewsEntry(string title, string shortDescription, string content, DateTime creation, DateTime edited)
        {
            Title = title;
            Content = content;
            ShortDescription = shortDescription;
            Creation = creation;
            Edited = edited;
        }

        public static NewsEntry Parse(FileInfo file)
        {
            var lines = File.ReadAllLines(file.FullName);
            string currentSection = string.Empty;
            var contentBuilder = new List<string>();

            string title = "";
            string shortDescription = "";
            string content = "";
            DateTime creation = file.CreationTimeUtc;
            DateTime edited = file.LastWriteTimeUtc;

            foreach (var line in lines)
            {
                if (string.IsNullOrWhiteSpace(line))
                    continue;

                if (line.StartsWith("[header]"))
                {
                    currentSection = "header";
                    continue;
                }

                if (line.StartsWith("[content]"))
                {
                    currentSection = "content";
                    continue;
                }

                if (currentSection == "header")
                {
                    var keyValue = line.Split(new[] { '=' }, 2);
                    if (keyValue.Length == 2)
                    {
                        var key = keyValue[0].Trim();
                        var value = keyValue[1].Trim().Trim('"');

                        if (key == "Title") title = value;
                        else if (key == "Short") shortDescription = value;
                    }
                }
                else if (currentSection == "content")
                {
                    contentBuilder.Add(line);
                }
            }

            content = string.Join(Environment.NewLine, contentBuilder);

            return new NewsEntry(title, shortDescription, content, creation, edited);
        }
    };
}
