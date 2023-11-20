using Azure_PV111.Middleware;
using Azure_PV111.Models;
using Azure_PV111.Models.Home.Search;
using Azure_PV111.Models.Home.Search.WebOrm;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using System.Text.Json;

namespace Azure_PV111.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IConfiguration _configuration;

        public HomeController(ILogger<HomeController> logger, IConfiguration configuration)
        {
            _logger = logger;
            _configuration = configuration;
        }

        public IActionResult Index()
        {
            ViewData["config"] =
                _configuration
                .GetSection("Search")
                .GetSection("Endpoint")
                .Value;

            return View();
        }

        public IActionResult Privacy()
        {
            DataMiddleware.Data.Add("Privacy visited");
            return View();
        }
        
        public IActionResult Search(String? search)
        {
            HomeSearchViewModel model = new();

            if( ! String.IsNullOrEmpty(search))  // є пошуковий запит
            {
                String? endpoint = _configuration.GetSection("Search").GetSection("Endpoint").Value;
                String? key = _configuration.GetSection("Search").GetSection("Key").Value;
                String? location = _configuration.GetSection("Search").GetSection("Location").Value;
                if(endpoint != null && key != null && location != null)
                {
                    endpoint += "v7.0/search?q=" + Uri.EscapeDataString(search);
                    using HttpClient httpClient = new();
                    httpClient.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", key);
                    String content = httpClient.GetStringAsync(endpoint).Result;
                    
                    model.WebSearchResponse =
                        JsonSerializer.Deserialize<WebSearchResponse>(content);

                    model.ErrorMessage = content;
                }
                else
                {
                    model.ErrorMessage = "Config load error";
                }
            }
            
            return View(model);
        }

        public ViewResult Data()
        {
            ViewData["data"] = DataMiddleware.Data;
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}