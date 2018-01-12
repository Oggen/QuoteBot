using System.Threading.Tasks;
using System.Collections.Generic;
using Amazon.Lambda.Core;
using Amazon.Lambda.APIGatewayEvents;
using HipChat.Net.Models.Request;
using HipChat.Net.Models.Response;
using Newtonsoft.Json;
using System.Linq;
using System;

[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.Json.JsonSerializer))]
namespace QuotableLambda
{
    public class Function
    {
        private readonly DataService _dataService;
        private readonly LexService _lexService;

        public Function()
        {
            _dataService = new DataService(new DynamoService());
            _lexService = new LexService(_dataService);
        }

        public APIGatewayProxyResponse FunctionHandler(APIGatewayProxyRequest request, ILambdaContext context)
        {
            var roomMessage = JsonConvert.DeserializeObject<RoomMessage>(request.Body);
            var message = roomMessage.Item.Message.MessageText.Trim();
            var command = message.Split(' ')[1].ToLower();
            var text = string.Join(" ", message.Split(' ').Skip(2).ToArray());

            string responseText;

            if (command == "add")
            {
                var quote = ParseQuote(text);
                if (quote == null)
                {
                    responseText = "Usage: add \"<quote>\" <quotee>";
                }
                else
                {
                    quote.AddedBy = $"@{roomMessage.Item.Message.From.MentionName}";
                    quote.AddedOn = DateTime.UtcNow;
                    responseText = _dataService.AddQuote(quote) ? "Quote added." : "Trouble adding quote.";
                }
            }
            else if (command == "random")
            {
                Quote quote;

                if (string.IsNullOrWhiteSpace(text))
                {
                    quote = _dataService.GetRandomQuote();
                }
                else
                {
                    quote = _dataService.GetRandomQuote(text);
                }

                responseText = quote?.ToString() ?? "No matching quote found.";
            }
            else if (command == "dispute")
            {
                responseText = string.IsNullOrWhiteSpace(text) ? _lexService.PostText(command, roomMessage.Item.Message.From.MentionName) : _lexService.PostText(text, roomMessage.Item.Message.From.MentionName);
            }
            else
            {
                responseText = "Commands:\nadd \"<quote>\" <quotee>\nrandom [quotee]";
            }

            return new APIGatewayProxyResponse
            {
                Body = JsonConvert.SerializeObject(new SendNotification
                {
                    Format = MessageFormat.Text,
                    Message = responseText,
                    Notify = false
                }),
                StatusCode = 200
            };
        }

        private Quote ParseQuote(string input)
        {
            var split = input.Split('"');
            if (split.Length < 3 || split[0].Length != 0) return null;
            var quotee = split[split.Length - 1].Trim();
            split[split.Length - 1] = "";
            var quote = string.Join("\"", split);
            return new Quote
            {
                Quotee = quotee,
                QuoteText = quote
            };
        }
    }
}
