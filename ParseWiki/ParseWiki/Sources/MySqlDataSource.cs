using System;
using System.Collections.Generic;
using System.Text.Json;
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

        public async Task<Dictionary<string, int>> GetAllTitleToIds()
        {
            await using var conn = new MySqlConnection(_connstr);
            await conn.OpenAsync();
            var cmd = conn.CreateCommand();
            cmd.CommandText = "SELECT id, title FROM titles2";
            var reader = await cmd.ExecuteReaderAsync();
            var result = new Dictionary<string, int>();
            while (await reader.ReadAsync())
            {
                var id = reader.GetInt32(0);
                var title = reader.GetString(1);
                result[title] = id;
            }

            return result;
        }
        
        internal async Task SaveWikiEvent(WikiEvent wEvent)
        {
            await using var conn = new MySqlConnection(_connstr);
            await conn.OpenAsync();
            var cmd = conn.CreateCommand();
            var sentenceJson = JsonSerializer.Serialize(wEvent.Sentence);
            // Console.WriteLine($"Saving wiki event for sentence: {wEvent.Sentence}");
            cmd.CommandText =
                "INSERT INTO nlpevents (sentence, startyear, startmonth, startday, starthour, startminute, endyear, endmonth, endday, endhour, endminute, lat, lng, pageid)" +
                "VALUES (@sentence, @startyear, @startmonth, @startday, @starthour, @startminute, @endyear, @endmonth, @endday, @endhour, @endminute, @lat, @lng, @pageid)";
            cmd.Parameters.AddWithValue("@sentence", sentenceJson);
            cmd.Parameters.AddWithValue("@startyear", wEvent.Date.StartTime.YearWithEpoch);
            cmd.Parameters.AddWithValue("@startmonth", wEvent.Date.StartTime.Month);
            cmd.Parameters.AddWithValue("@startday", wEvent.Date.StartTime.Day);
            cmd.Parameters.AddWithValue("@starthour", wEvent.Date.StartTime.Hour);
            cmd.Parameters.AddWithValue("@startminute", wEvent.Date.StartTime.Minute);
            cmd.Parameters.AddWithValue("@endyear", wEvent.Date.EndTime.YearWithEpoch);
            cmd.Parameters.AddWithValue("@endmonth", wEvent.Date.EndTime.Month);
            cmd.Parameters.AddWithValue("@endday", wEvent.Date.EndTime.Day);
            cmd.Parameters.AddWithValue("@endhour", wEvent.Date.EndTime.Hour);
            cmd.Parameters.AddWithValue("@endminute", wEvent.Date.EndTime.Minute);
            cmd.Parameters.AddWithValue("@lat", wEvent.Location.Coordinate.Latitude);
            cmd.Parameters.AddWithValue("@lng", wEvent.Location.Coordinate.Longitude);
            cmd.Parameters.AddWithValue("@pageid", wEvent.PageId);
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

        public async void TruncateWikiEvents()
        {
            await using var conn = new MySqlConnection(_connstr);
            await conn.OpenAsync();
            var cmd = conn.CreateCommand();
            cmd.CommandText = "TRUNCATE TABLE nlpevents";
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
            cmd.CommandText = "INSERT INTO titles2 (id, title) VALUES (@id, @title)";
            cmd.Parameters.AddWithValue("@id", id);
            cmd.Parameters.AddWithValue("@title", title);
            try
            {
                await cmd.ExecuteNonQueryAsync();
            }
            catch (Exception e)
            {
                Console.WriteLine($"Couldn't save title to database: {e.Message}");
            }
        }

        internal async Task<WikiLocation> GetLocationById(int id)
        {
            await using var conn = new MySqlConnection(_connstr);
            await conn.OpenAsync();
            var cmd = conn.CreateCommand();
            cmd.CommandText = "SELECT title, lat, lng FROM locations1 WHERE id=@id";
            cmd.Parameters.AddWithValue("@id", id);
            var reader = await cmd.ExecuteReaderAsync();
            if (await reader.ReadAsync())
            {
                return new WikiLocation(
                    id,
                    reader.GetString(0),
                    new Coord(reader.GetFloat(1), reader.GetFloat(2))
                );
            }
            return null;
        }
        
        internal async Task<WikiLocation> GetLocationByTitle(string title)
        {
            await using var conn = new MySqlConnection(_connstr);
            await conn.OpenAsync();
            var cmd = conn.CreateCommand();
            cmd.CommandText = "SELECT id, lat, lng FROM locations1 WHERE title=@title";
            cmd.Parameters.AddWithValue("@title", title);
            var reader = await cmd.ExecuteReaderAsync();
            if (await reader.ReadAsync())
            {
                return new WikiLocation(
                    reader.GetInt32(0),
                    title,
                    new Coord(reader.GetFloat(1), reader.GetFloat(2))
                );
            }
            return null;
        }

        internal async Task<int?> GetIdByTitle(string title)
        {
            try
            {
                await using var conn = new MySqlConnection(_connstr);
                await conn.OpenAsync();
                var cmd = conn.CreateCommand();
                cmd.CommandText = "SELECT id FROM titles2 WHERE title=@title";
                cmd.Parameters.AddWithValue("@title", title);
                var result = await cmd.ExecuteScalarAsync();
                return (int?) result;
            }
            catch (TimeoutException)
            {
                return null;
            }
            catch (MySqlException)
            {
                return null;
            }
        }

        public ISink<WikiEvent> GetWikiEventSink()
        {
            return new WikiEventSink(this);
        }

        public ISink<string> GetTitleSink()
        {
            return new TitleSink(this);
        }

        public ITranslator<string, int?> GetTitleToIdTranslator()
        {
            return new TitleToIdTranslator(this);
        }

        public ITranslator<int, WikiLocation> GetIdToLocationTranslator()
        {
            return new IdToLocationTranslator(this);
        }

        public ITranslator<string, WikiLocation> GetTitleToLocationTranslator()
        {
            return new TitleToLocationTranslator(this);
        }

        private class IdToLocationTranslator : ITranslator<int, WikiLocation>
        {
            private readonly MySqlDataSource _source;
            internal IdToLocationTranslator(MySqlDataSource source)
            {
                _source = source;
            }
            
            public Task<WikiLocation> Translate(int id)
            {
                return _source.GetLocationById(id);
            }
        }

        private class TitleToLocationTranslator : ITranslator<string, WikiLocation>
        {
            private readonly MySqlDataSource _source;

            internal TitleToLocationTranslator(MySqlDataSource source)
            {
                _source = source;
            }

            public Task<WikiLocation> Translate(string title)
            {
                return _source.GetLocationByTitle(title);
            }
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

        private class TitleToIdTranslator : ITranslator<string, int?>
        {
            private readonly MySqlDataSource _src;
            
            internal TitleToIdTranslator(MySqlDataSource src)
            {
                _src = src;
            }

            public async Task<int?> Translate(string title)
            {
                return await _src.GetIdByTitle(title);
            }
        }

        private class WikiEventSink : ISink<WikiEvent>
        {
            private readonly MySqlDataSource _source;
            internal WikiEventSink(MySqlDataSource source)
            {
                _source = source;
            }
            public async Task Save(int id, WikiEvent item)
            {
                await _source.SaveWikiEvent(item);
            }
        }
    }
}