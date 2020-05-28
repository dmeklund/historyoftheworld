using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace DateLocNLP
{
    public class NlpProcessor
    {
        private readonly string _urlPlusQuery;
        
        public NlpProcessor()
        {
            var nlpBaseAddress = "http://localhost:9000";
            var options = new NlpProperties();
            // options.annotators = "tokenize, ssplit, pos, lemma, ner, parse, coref";
            // options.annotators = "tokenize, ssplit, pos, ner, coref";
            options.annotators = "openie, coref";
            options.outputformat = "text";
            var jsonOptions = JsonSerializer.Serialize(options);
            var qstringProperties = new Dictionary<string, string> {{"properties", jsonOptions}};
            var qString = ToQueryString(qstringProperties);
            _urlPlusQuery = nlpBaseAddress + qString;
        }
        
        public async Task<NlpResult> ProcessText(string text)
        {
            var content = new StringContent(text);
            content.Headers.Clear();
            content.Headers.Add("Content-Type", "application/x-www-form-urlencoded");
            var client = new HttpClient();
            var response = await client.PostAsync(_urlPlusQuery, content);
            if (response.StatusCode != System.Net.HttpStatusCode.OK)
            {
                throw new ApplicationException("Subject-Object tuple extraction returned an unexpected response from the subject-object service");
            }
            var jsonResult = await response.Content.ReadAsStringAsync();
            var result = JsonSerializer.Deserialize<NlpResult>(jsonResult);
            return result;
        }
        
        
        private static string ToQueryString(Dictionary<string,string> args)
        {
            var sb = new StringBuilder("?");
            var first = true;
            foreach (var (key, value) in args)
            {
                if (!first)
                {
                    sb.Append("&");
                }
                sb.AppendFormat("{0}={1}", Uri.EscapeDataString(key), Uri.EscapeDataString(value));
                first = false;
            }
            return sb.ToString();
        }
    }
}