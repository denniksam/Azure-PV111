using Azure_PV111.Models.Home.ImageSearch.ImageOrm;

namespace Azure_PV111.Models.Home.ImageSearch
{
    public class HomeImageSearchViewModel
    {
        public ImageSearchResponse? SearchResponse { get; set; }
        public String? ErrorMessage { get; set; }
        public int page { get; set; }
        public int offset { get; set; }
    }
}
