using System.Threading.Tasks;
using System.Collections.Generic;
using Amazon.Lambda.Core;
using Amazon.Lambda.APIGatewayEvents;
using HipChat.Net.Models.Request;
using HipChat.Net.Models.Response;
using Newtonsoft.Json;
using System.Linq;
using System;
using System.Text;

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
            RoomMessage roomMessage;
            try
            {
                roomMessage = JsonConvert.DeserializeObject<RoomMessage>(request.Body);
            }
            catch (Exception e)
            {
                return new APIGatewayProxyResponse
                {
                    Body = "Bad request",
                    StatusCode = 400
                };
            }
            var message = roomMessage.Item.Message.MessageText.Trim();
            string command, text;
            if (message.Split(' ').Length > 1)
            {
                command = message.Split(' ')[1].ToLower();
                text = string.Join(" ", message.Split(' ').Skip(2).ToArray());
            }
            else
            {
                command = "";
                text = "";
            }

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
                if (string.IsNullOrWhiteSpace(text))
                {
                    responseText = _dataService.GetRandomQuote()?.ToString() ?? "There are no quotes in the system. Add some!";
                }
                else
                {
                    responseText = _dataService.GetRandomQuote(text)?.ToString() ?? $"{text} has no quotes. Must be pretty boring.";
                }
            }
            else if (command == "search")
            {
                if (string.IsNullOrWhiteSpace(text))
                {
                    responseText = "Usage: search <searchTerm>";
                }
                else
                {
                    responseText = _dataService.GetRandomMatchingQuote(text)?.ToString() ?? "No matching quotes found.";
                }
            }
            else if (command == "dispute")
            {
                responseText = string.IsNullOrWhiteSpace(text) ? _lexService.PostText(command, roomMessage.Item.Message.From.MentionName) : _lexService.PostText(text, roomMessage.Item.Message.From.MentionName);
            }
            else if (command == "leaderboard")
            {
                int count = 3;
                if (string.IsNullOrWhiteSpace(text) || int.TryParse(text, out count))
                {
                    var leaderboard = _dataService.GetLeaderboard(count);
                    var sb = new StringBuilder();
                    sb.AppendLine("Leaderboard");
                    sb.AppendLine("Most Quoted:");
                    foreach (var entry in leaderboard.MostQuoted)
                    {
                        sb.AppendLine($"{entry.Item1}: {entry.Item2}");
                    }
                    sb.AppendLine("Most Quotes Added:");
                    foreach (var entry in leaderboard.MostReported)
                    {
                        sb.AppendLine($"{entry.Item1}: {entry.Item2}");
                    }
                    sb.AppendLine("Most Narcissistic:");
                    foreach (var entry in leaderboard.MostNarcissistic)
                    {
                        sb.AppendLine($"{entry.Item1}: {entry.Item2}");
                    }
                    responseText = sb.ToString();
                }
                else
                {
                    responseText = "Usage: leaderboard [count]";
                }
            }
            else
            {
                var sb = new StringBuilder();
                sb.AppendLine("Commands");
                sb.AppendLine("add \"<quote>\" <quotee>");
                sb.AppendLine("random [quotee]");
                sb.AppendLine("search <searchTerm>");
                sb.AppendLine("dispute");
                sb.AppendLine("leaderboard [count]");
                responseText = sb.ToString();
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
            if (split.Length < 3 || split[0].Length != 0 || split[split.Length - 1].Length == 0) return null;
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
