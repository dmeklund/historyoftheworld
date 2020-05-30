namespace ParseWiki
{
    public class WikiLocation
    {
        public Coord Coordinate { get; set; }
        public int Id { get; set; }
        public string Title { get; set; }

        public WikiLocation(int id, string title, Coord coord)
        {
            Id = id;
            Title = title;
            Coordinate = coord;
        }
    }
}