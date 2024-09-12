namespace drgndrop
{
    public class NewsEntry
    {
        public string Title { get; set; }
        public string Content { get; set; }
        public DateTime Creation { get; set; }
        public DateTime Edited { get; set; }

        public NewsEntry(string title, string content, DateTime creation, DateTime edited)
        {
            Title = title;
            Content = content;
            Creation = creation;
            Edited = edited;
        }
    };
}
