using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using ParseWiki.DataTypes;

namespace ParseWiki.Sinks
{
    public class PageWriterSink : ISink<PageXml>
    {
        private string _path;
        private int _numchars;
        private Dictionary<char, char> _validChars;
        
        public PageWriterSink(string path, int numchars)
        {
            _path = path;
            _numchars = numchars;
            InitValidChars();
        }

        private void InitValidChars()
        {
            _validChars = new Dictionary<char, char>();
            for (char val = 'a'; val <= 'z'; ++val)
            {
                _validChars[val] = val;
                _validChars[char.ToUpper(val)] = char.ToUpper(val);
            }

            for (char val = '0'; val <= '9'; ++val)
            {
                _validChars[val] = val;
            }

            _validChars['('] = '(';
            _validChars[')'] = ')';
            _validChars['_'] = '_';
            _validChars['-'] = '-';
        }
        
        public async Task Save(long id, PageXml item)
        {
            var dirname = SanitizeString(
                item.Title.Substring(0, Math.Min(3, item.Title.Length)), 
                3
            ).ToLowerInvariant();
            var dirpath = Path.Join(_path, dirname);
            Directory.CreateDirectory(dirpath);
            var filename = SanitizeString(item.Title, 0) + " " + item.Id.ToString() + ".xml";
            var filepath = Path.Join(dirpath, filename);
            await using var outputStream = File.CreateText(filepath);
            await outputStream.WriteAsync(item.RawXml);
        }

        private string SanitizeString(string input, int minLength)
        {
            var output = new char[Math.Max(input.Length, minLength)];
            for (var index = 0; index < input.Length; ++index)
            {
                output[index] = _validChars.GetValueOrDefault(input[index], '_');
            }
            for (var index = input.Length; index < minLength; ++index)
            {
                output[index] = '_';
            }
            return new string(output);
        }
    }
}