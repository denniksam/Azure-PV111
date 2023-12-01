using Newtonsoft.Json;

namespace Azure_PV111.Models.Home.Db
{
    public class Test
    {
        [JsonProperty(PropertyName = "id")]
        public string Id { get; set; }

        [JsonProperty(PropertyName = "partitionKey")]
        public string PartitionKey { get; set; }

        public string Data { get; set; }
    }
}
