﻿using Azure_PV111.Models.Home.Search.WebOrm;

namespace Azure_PV111.Models.Home.Search
{
    public class HomeSearchViewModel
    {
        public WebSearchResponse? WebSearchResponse { get; set; }
        public int page { get; set; }
        public int offset { get; set; }

        public String? ErrorMessage { get; set; }
    }
}
