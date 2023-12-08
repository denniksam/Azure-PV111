using Newtonsoft.Json;

namespace Azure_PV111.Models.Home.Db
{
    public class ProductDataModel
    {
        public const String DataType = "Product";


        [JsonProperty(PropertyName = "id")]
        public Guid Id { get; set; }

        [JsonProperty(PropertyName = "name")]
        public String Name { get; set; } = null!;

        [JsonProperty(PropertyName = "year")]
        public int Year { get; set; }


        [JsonProperty(PropertyName = "type")]
        public String Type => DataType;

    }
}
