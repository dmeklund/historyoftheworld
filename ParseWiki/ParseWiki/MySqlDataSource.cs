using System.Threading.Tasks;
using MySql.Data.MySqlClient;

namespace ParseWiki
{
    public class MySqlDataSource : IDataSource
    {
        private readonly string _connstr;
        public MySqlDataSource(string connstr)
        {
            _connstr = connstr;
        }
        
        public async Task SaveEvent(int id, string title, string eventtype, DateRange range, Coord coord)
        {
            await using var conn = new MySqlConnection(_connstr);
            await conn.OpenAsync();
            var cmd = conn.CreateCommand();
            cmd.CommandText =
                "INSERT INTO events (id, title, eventtype, startyear, startmonth, startday, starthour, startminute, endyear, endmonth, endday, endhour, endminute, lat, lng)" +
                "VALUES (@id, @title, @eventtype, @startyear, @startmonth, @startday, @starthour, @startminute, @endyear, @endmonth, @endday, @endhour, @endminute, @lat, @lng)";
            cmd.Parameters.AddWithValue("@id", id);
            cmd.Parameters.AddWithValue("@title", title);
            cmd.Parameters.AddWithValue("@eventtype", eventtype);
            cmd.Parameters.AddWithValue("@startyear", range.StartTime.YearWithEpoch);
            cmd.Parameters.AddWithValue("@startmonth", range.StartTime.Month);
            cmd.Parameters.AddWithValue("@startday", range.StartTime.Day);
            cmd.Parameters.AddWithValue("@starthour", range.StartTime.Hour);
            cmd.Parameters.AddWithValue("@startminute", range.StartTime.Minute);
            cmd.Parameters.AddWithValue("@endyear", range.EndTime.YearWithEpoch);
            cmd.Parameters.AddWithValue("@endmonth", range.EndTime.Month);
            cmd.Parameters.AddWithValue("@endday", range.EndTime.Day);
            cmd.Parameters.AddWithValue("@endhour", range.EndTime.Hour);
            cmd.Parameters.AddWithValue("@endminute", range.EndTime.Minute);
            cmd.Parameters.AddWithValue("@lat", coord.Latitude);
            cmd.Parameters.AddWithValue("@lng", coord.Longitude);
            await cmd.ExecuteNonQueryAsync();
        }

        public async Task SaveLocation(int id, string title, Coord coord)
        { 
            await using var conn = new MySqlConnection(_connstr);
            await conn.OpenAsync();
            var cmd = conn.CreateCommand();
            cmd.CommandText =
                "INSERT INTO locations (id, title, lat, lng)" +
                "VALUES (@id, @title, @lat, @lng)";
            cmd.Parameters.AddWithValue("@id", id);
            cmd.Parameters.AddWithValue("@title", title);
            cmd.Parameters.AddWithValue("@lat", coord.Latitude);
            cmd.Parameters.AddWithValue("@lng", coord.Longitude);
            await cmd.ExecuteNonQueryAsync();
        }

        public async void TruncateEvents()
        {
            await using var conn = new MySqlConnection(_connstr);
            await conn.OpenAsync();
            var cmd = conn.CreateCommand();
            cmd.CommandText = "TRUNCATE TABLE events";
            await cmd.ExecuteNonQueryAsync();
        }

        public async void TruncateLocations()
        {
            await using var conn = new MySqlConnection(_connstr);
            await conn.OpenAsync();
            var cmd = conn.CreateCommand();
            cmd.CommandText = "TRUNCATE TABLE locations";
            await cmd.ExecuteNonQueryAsync();
        }
    }
}