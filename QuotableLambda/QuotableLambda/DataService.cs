using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

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
                return quotes[_random.Next(quotes.Count)];
            }
        }

        public Quote GetRandomMatchingQuote(string searchTerm)
        {
            var regex = new Regex(searchTerm.ToLower());
            var matchingQuotes = _dbService.GetAllQuotes().Where(q => regex.IsMatch(q.QuoteText.ToLower())).ToList();
            if (matchingQuotes.Count == 0)
            {
                return null;
            }
            else
            {
                return matchingQuotes[_random.Next(matchingQuotes.Count)];
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

        public Leaderboard GetLeaderboard(int topCount)
        {
            var quotes = _dbService.GetAllQuotes();
            return new Leaderboard
            {
                MostQuoted = quotes.GroupBy(quote => quote.Quotee)
                    .Select(group => new Tuple<string, int>(group.Key, group.Count()))
                    .OrderByDescending(tuple => tuple.Item2)
                    .Take(topCount)
                    .ToArray(),
                MostReported = quotes.GroupBy(quote => quote.AddedBy)
                    .Select(group => new Tuple<string, int>(group.Key, group.Count()))
                    .OrderByDescending(tuple => tuple.Item2)
                    .Take(topCount)
                    .ToArray(),
                MostNarcissistic = quotes.Where(quote => quote.AddedBy == quote.Quotee)
                    .GroupBy(quote => quote.AddedBy)
                    .Select(group => new Tuple<string, int>(group.Key, group.Count()))
                    .OrderByDescending(tuple => tuple.Item2)
                    .Take(topCount)
                    .ToArray()
            };
        }
    }
}
