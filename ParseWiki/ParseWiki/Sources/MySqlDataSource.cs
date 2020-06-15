using System.Collections.Generic;
using System.Threading.Tasks;
using MySql.Data.MySqlClient;
using ParseWiki.DataTypes;
using ParseWiki.Extractors;
using ParseWiki.Sinks;
using ParseWiki.Translators;

namespace ParseWiki.Sources
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

        public async Task SaveTitle(int id, string title)
        {
            await using var conn = new MySqlConnection(_connstr);
            await conn.OpenAsync();
            var cmd = conn.CreateCommand();
            cmd.CommandText = "INSERT INTO titles (id, title) VALUES (@id, @title)";
            cmd.Parameters.AddWithValue("@id", id);
            cmd.Parameters.AddWithValue("@title", title);
            await cmd.ExecuteNonQueryAsync();
        }

        internal async Task<int?> GetIdByTitle(string title)
        {
            await using var conn = new MySqlConnection(_connstr);
            await conn.OpenAsync();
            var cmd = conn.CreateCommand();
            cmd.CommandText = "SELECT id FROM titles1 WHERE title=@title";
            cmd.Parameters.AddWithValue("@title", title);
            var result = await cmd.ExecuteScalarAsync();
            return (int?)result;
        }

        public ISink<string> GetTitleSink()
        {
            return new TitleSink(this);
        }

        public IExtractor<string, int?> GetTitleToIdExtractor()
        {
            return new TitleToIdExtractor(this);
        }

        private class TitleSink : ISink<string>
        {
            private readonly MySqlDataSource _source;
            internal TitleSink(MySqlDataSource source)
            {
                _source = source;
            }
            public async Task Save(int id, string item)
            {
                await _source.SaveTitle(id, item);
            }
        }

        private class TitleToIdExtractor : IExtractor<string, int?>
        {
            private readonly MySqlDataSource _src;
            
            internal TitleToIdExtractor(MySqlDataSource src)
            {
                _src = src;
            }

            public Task<int?> Extract(string title)
            {
                return _src.GetIdByTitle(title);
            }
        }
    }
}