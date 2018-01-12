using System.Collections.Generic;

namespace QuotableLambda
{
    public interface IDbService
    {
        List<Quote> GetAllQuotes();

        List<Quote> GetAllQuotesByQuotee(string quotee);

        bool AddQuote(Quote quote);

        bool DeleteQuote(string quotee, string quoteText);
    }
}