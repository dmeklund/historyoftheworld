using System.Threading.Tasks;
using Amazon;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DocumentModel;
using Amazon.Runtime;
using Amazon.Runtime.CredentialManagement;
using ParseWiki.DataTypes;
using ParseWiki.Sinks;

namespace ParseWiki.Sources
{
    public class DynamoDbSource
    {
        private readonly AmazonDynamoDBClient _client;
        private static readonly string _tableName = "enwiki-20200401-pages-articles";

        public DynamoDbSource()
        {
            // var credentials = new StoredProfileAWSCredentials("hotw");
            // var credentials = new Basi
            AWSConfigs.AWSRegion = "us-east-1";
            AWSConfigs.AWSProfileName = "hotw";
            _client = new AmazonDynamoDBClient();
        }

        public async Task SaveWikiXml(WikiPageLazyLoadId page)
        {
            var table = Table.LoadTable(_client, _tableName);
            var doc = new Document
            {
                ["Title"] = page.Title,
                ["Xml"] = page.Text
            };
            await table.PutItemAsync(doc);
        }

        // private class WikiXmlSink : ISink<WikiPageLazyLoadId>
        // {
        //     private readonly DynamoDbSource _source;
        //
        //     internal WikiXmlSink(DynamoDbSource source)
        //     {
        //         _source = source;
        //     }
        //     public Task Save(int id, WikiPageLazyLoadId item)
        //     {
        //     }
        // }
    }
}