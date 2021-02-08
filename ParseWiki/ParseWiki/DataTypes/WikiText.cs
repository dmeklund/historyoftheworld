namespace ParseWiki.DataTypes
{
    public class WikiText
    {
        internal string Title { get; }
        internal int Id { get; }
        internal int NamespaceId { get; }
        internal string Text { get; }

        public WikiText(
            string title,
            int id,
            string text,
            int namespaceid)
        {
            Title = title;
            Id = id;
            Text = text;
            NamespaceId = namespaceid;
        }
    }
}
