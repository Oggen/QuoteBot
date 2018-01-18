using System;
using System.Collections.Generic;
using System.Text;
using Amazon.Lex;
using Amazon.Lex.Model;

namespace QuotableLambda
{
    public class LexService
    {
        private readonly AmazonLexClient _lex;
        private readonly DataService _dataService;

        public LexService(DataService dataService)
        {
            _lex = new AmazonLexClient(Amazon.RegionEndpoint.USEast1);
            _dataService = dataService;
        }

        public string PostText(string text, string mentionName)
        {
            var result = _lex.PostTextAsync(new PostTextRequest
            {
                BotName = "QuoteBotDispute",
                BotAlias = "Initial",
                InputText = text,
                UserId = mentionName + mentionName,
                SessionAttributes = { ["mentionName"] = mentionName }
            }).Result;
            if (result.DialogState == DialogState.ReadyForFulfillment)
            {
                return _dataService.DeleteQuote(result.SessionAttributes["quotee"], result.SessionAttributes["quote"])
                    ? "Quote deleted." : "Problem deleting from database.";
            }
            else
            {
                return result.Message;
            }
        }
    }
}
