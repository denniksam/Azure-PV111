using Microsoft.AspNetCore.Mvc;

namespace Azure_PV111.Models.Home.Db
{
    public class ProducerFormModel
    {
        // [FromForm(Name = "db-producer")]
        [FromBody]
        public String Name { get; set; } = null!;
    }
}
