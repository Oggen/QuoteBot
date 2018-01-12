using System;
using System.Collections.Generic;
using System.Text;

namespace QuotableLambda
{
    public class DataService
    {
        private readonly IDbService _dbService;
        private readonly Random _random;

        public DataService(IDbService dbService)
        {
            _dbService = dbService;
            _random = new Random();
        }

        public Quote GetRandomQuote(string quotee = null)
        {
            var quotes = quotee == null ? _dbService.GetAllQuotes() : _dbService.GetAllQuotesByQuotee(quotee);
            if (quotes.Count == 0)
            {
                return null;
            }
            else
            {
                return quotes[_random.Next(quotes.Count - 1)];
            }
        }

        public bool AddQuote(Quote quote)
        {
            return _dbService.AddQuote(quote);
        }

        public bool DeleteQuote(string quotee, string quoteText)
        {
            return _dbService.DeleteQuote(quotee, quoteText);
        }
    }
}
