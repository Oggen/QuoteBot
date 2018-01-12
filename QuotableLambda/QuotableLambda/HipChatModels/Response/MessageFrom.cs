using Newtonsoft.Json;

namespace HipChat.Net.Models.Response
{
  [JsonObject]
  public class MessageFrom : Entity
  {
    [JsonProperty("mention_name")]
    public string MentionName { get; set; }
    [JsonProperty("name")]
    public string DisplayName { get; set; }
    [JsonProperty("id")]
    public int UserId { get; set; }
  }
}
