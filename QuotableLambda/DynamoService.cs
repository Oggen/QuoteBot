using System;
using System.Collections.Generic;
using System.Text;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;
using System.Linq;

namespace QuotableLambda
{
    public class DynamoService : IDbService
    {
        private readonly AmazonDynamoDBClient _db;
        private const string TableName = "Quotes";

        public DynamoService()
        {
            _db = new AmazonDynamoDBClient(Amazon.RegionEndpoint.USEast1);
        }

        public List<Quote> GetAllQuotes()
        {
            var result = _db.ScanAsync(new ScanRequest(TableName)).Result;
            return result.Items.Select(item => ParseQuote(item)).ToList();
        }

        public List<Quote> GetAllQuotesByQuotee(string quotee)
        {
            var result =_db.QueryAsync(new QueryRequest
            {
                TableName = TableName,
                ExpressionAttributeValues = { [":quotee"] = new AttributeValue(quotee) },
                KeyConditionExpression = "Quotee = :quotee"
            }).Result;
            return result.Items.Select(item => ParseQuote(item)).ToList();
        }

        public bool AddQuote(Quote quote)
        {
            return _db.PutItemAsync(new PutItemRequest
            {
                TableName = TableName,
                Item = PrepareQuote(quote)
            }).Result.HttpStatusCode == System.Net.HttpStatusCode.OK;
            
        }

        private Quote ParseQuote(Dictionary<string, AttributeValue> item)
        {
            var quote = new Quote();
            foreach (var entry in item)
            {
                switch (entry.Key)
                {
                    case "Quote": quote.QuoteText = entry.Value.S; break;
                    case "Quotee": quote.Quotee = entry.Value.S; break;
                    case "AddedBy": quote.AddedBy = entry.Value.S; break;
                }
            }
            return quote;
        }

        private Dictionary<string, AttributeValue> PrepareQuote(Quote quote)
        {
            var item = new Dictionary<string, AttributeValue>();
            if (quote.AddedBy != null)
            {
                item.Add("AddedBy", new AttributeValue(quote.AddedBy));
            }
            if (quote.Quotee != null)
            {
                item.Add("Quotee", new AttributeValue(quote.Quotee));
            }
            if (quote.QuoteText != null)
            {
                item.Add("Quote", new AttributeValue(quote.QuoteText));
            }
            return item;
        }
    }
}
