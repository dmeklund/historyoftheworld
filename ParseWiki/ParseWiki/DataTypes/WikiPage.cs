namespace ParseWiki.DataTypes
{
    public class WikiPage
    {
        public string Title { get; }
        public string Text { get; }

        public WikiPage(string title, string text)
        {
            Title = title;
            Text = text;
        }
    }
}