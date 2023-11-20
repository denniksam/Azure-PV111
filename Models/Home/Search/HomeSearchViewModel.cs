using Azure_PV111.Models.Home.Search.WebOrm;

namespace Azure_PV111.Models.Home.Search
{
    public class HomeSearchViewModel
    {
        public WebSearchResponse? WebSearchResponse { get; set; }

        public String? ErrorMessage { get; set; }
    }
}
