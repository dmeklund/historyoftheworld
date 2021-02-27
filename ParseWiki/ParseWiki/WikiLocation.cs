namespace ParseWiki
{
    public class WikiLocation
    {
        public Coord Coordinate { get; set; }
        public long Id { get; set; }
        public string Title { get; set; }

        public WikiLocation(long id, string title, Coord coord)
        {
            Id = id;
            Title = title;
            Coordinate = coord;
        }

        public override string ToString()
        {
            return $"{Title} ({Id}): {Coordinate}";
        }
    }
}