using Azure_PV111.Middleware;
using Azure_PV111.Models;
using Azure_PV111.Models.Home.ImageSearch;
using Azure_PV111.Models.Home.ImageSearch.ImageOrm;
using Azure_PV111.Models.Home.Search;
using Azure_PV111.Models.Home.Search.WebOrm;
using Azure_PV111.Models.Home.SpellCheck;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Cosmos;
using System.Diagnostics;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Nodes;

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

        private static String FilesPath => 
            (System.IO.Directory.Exists(@"C:\home\site\"))
                ? @"C:\home\site\Files\"
                : "./Files/";

        public ViewResult Files()
        {
            String filename;
            if (System.IO.Directory.Exists(@"C:\home\site\"))
            {
                filename = @"C:\home\site\";
            }
            else
            {
                filename = "./";
            }

            filename += "Files/";
            if (System.IO.Directory.Exists(filename))
            {
                ViewData["dir-exists"] = "Exists";
            }
            else
            {
                ViewData["dir-exists"] = "Not Exists ";
                try
                {
                    System.IO.Directory.CreateDirectory(filename);
                    ViewData["dir-exists"] += "Created";
                }
                catch(Exception ex)
                {
                    ViewData["dir-exists"] += ex.Message;
                }
            }

            String[] files = System.IO.Directory.GetFiles(filename);
            for (int i = 0; i < files.Length; i += 1)
            {
                files[i] = Path.GetFileName(files[i]);
            }
            ViewData["files"] = files;

            // Перевіряємо наявність повідомлення
            if (HttpContext.Session.Keys.Contains("file-message"))
            {
                ViewData["file-message"] = 
                    HttpContext.Session.GetString("file-message");

                HttpContext.Session.Remove("file-message");
            }
            /*
            filename += "file.txt";

            if (System.IO.File.Exists(filename))
            {
                ViewData["exists"] = "Exists. " +
                   System.IO.File.ReadAllText(filename) ;
            }
            else
            {
                ViewData["exists"] = "Not Exists. ";
                try
                {
                    System.IO.File.WriteAllText(filename, "Test Line 2");
                    ViewData["exists"] += "Created";
                }
                catch(Exception ex)
                {
                    ViewData["exists"] += ex.Message;
                }
            }
            */
            return View();
        }
        public IActionResult FileDeleter(String filename)
        {
            String file = FilesPath + filename;

            if (System.IO.File.Exists(file))
            {
                System.IO.File.Delete(file);
                HttpContext.Session.SetString("file-message", "Видалено успішно");
                return RedirectToAction(nameof(Files));
            }
            return NotFound();
        }

        public IActionResult FileDownloader(String filename)
        {
            String file = FilesPath + filename;

            if (System.IO.File.Exists(file))
            {
                return File(
                    System.IO.File.ReadAllBytes(file),
                    System.Net.Mime.MediaTypeNames.Application.Octet,
                    filename
                );
            }
            return NotFound();
        }

        [HttpPost]
        public RedirectToActionResult FileUploader(IFormFile uploaded)
        {
            if(uploaded != null && uploaded.Length > 0)
            {                
                using Stream stream = 
                    System.IO.File.OpenWrite(
                        FilesPath + uploaded.FileName);

                uploaded.CopyTo(stream);
                HttpContext.Session.SetString("file-message", "Завантажено успішно");            
            }
            else
            {
                HttpContext.Session.SetString("file-message", "Помилка завантаження");

            }
            return RedirectToAction(nameof(Files));
        }

        public async Task<ViewResult> DbAsync()
        {
            

            return View();
        }

        public ViewResult Translator()
        {
            using HttpClient client = new();
            ViewData["langs"] =
                JsonSerializer.Deserialize<JsonNode>(
                    client
                    .GetStringAsync("https://api.cognitive.microsofttranslator.com/languages?api-version=3.0")
                    .Result
                );
            return View();
        }

        public IActionResult Privacy()
        {
            DataMiddleware.Data.Add("Privacy visited");
            return View();
        }
        
        public IActionResult Search(String? search, int? page)
        {
            HomeSearchViewModel model = new();

            if( ! String.IsNullOrEmpty(search))  // є пошуковий запит
            {
                page ??= 1;  // page = page ?? 1;
                int count = 20;
                int offset = (page.Value - 1) * count;

                model.offset = offset;
                model.page = page.Value;

                String? endpoint = _configuration.GetSection("Search").GetSection("Endpoint").Value;
                String? key = _configuration.GetSection("Search").GetSection("Key").Value;
                String? location = _configuration.GetSection("Search").GetSection("Location").Value;
                if(endpoint != null && key != null && location != null)
                {
                    endpoint += $"v7.0/search?mkt=uk-UA&count={count}&offset={offset}&q=" + Uri.EscapeDataString(search);
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

        public ViewResult ImageSearch(String? search)
        {
            HomeImageSearchViewModel model = new();
            if (!String.IsNullOrEmpty(search))  // є пошуковий запит
            {
                String? endpoint = _configuration.GetSection("Search").GetSection("Endpoint").Value;
                String? key = _configuration.GetSection("Search").GetSection("Key").Value;
                String? location = _configuration.GetSection("Search").GetSection("Location").Value;
                if (endpoint != null && key != null && location != null)
                {
                    endpoint += $"v7.0/images/search?mkt=uk-UA&count={10}&offset={0}&q=" + Uri.EscapeDataString(search);
                    using HttpClient httpClient = new();
                    httpClient.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", key);
                    String content = httpClient.GetStringAsync(endpoint).Result;
                    
                    model.SearchResponse =
                        JsonSerializer.Deserialize<ImageSearchResponse>(content);

                    model.ErrorMessage = content;                    
                }
                else
                {
                    model.ErrorMessage = "Config load error";
                }
            }
            return View(model);
        }

        public ViewResult NewsSearch(String? search)
        {
            if (!String.IsNullOrEmpty(search))  // є пошуковий запит
            {
                String? endpoint = _configuration.GetSection("Search").GetSection("Endpoint").Value;
                String? key = _configuration.GetSection("Search").GetSection("Key").Value;
                String? location = _configuration.GetSection("Search").GetSection("Location").Value;
                if (endpoint != null && key != null && location != null)
                {
                    endpoint += $"v7.0/news/search?textDecorations=true&textFormat=HTML&q=" + Uri.EscapeDataString(search);
                    using HttpClient httpClient = new();
                    httpClient.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", key);
                    String content = httpClient.GetStringAsync(endpoint).Result;

                    ViewData["SearchResponse"] = content;
                    ViewData["Search"] = JsonSerializer.Deserialize<JsonNode>(content);
                    //  JsonSerializer.Deserialize<ImageSearchResponse>(content);
                    // &textDecorations=true&textFormat=html
                    // model.ErrorMessage = content;
                }
                else
                {
                    ViewData["SearchResponse"] = "Config load error";
                }
            }
             return View();
        }

        public ViewResult SpellCheck(String? phrase)
        {
            HomeSpellCheckViewModel model = new();

            if (!String.IsNullOrEmpty(phrase))
            {
                String? endpoint = _configuration.GetSection("Search").GetSection("Endpoint").Value;
                String? key = _configuration.GetSection("Search").GetSection("Key").Value;
                String? location = _configuration.GetSection("Search").GetSection("Location").Value;
                if (endpoint != null && key != null && location != null)
                {
                    endpoint += $"v7.0/spellcheck?mkt=en-us&mode=proof";
                    using HttpClient httpClient = new();
                    httpClient.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", key);
                    var postContent = new StringContent(
                        $"text={phrase}", 
                        System.Text.Encoding.UTF8, 
                        "application/x-www-form-urlencoded");

                    String content = 
                        httpClient
                        .PostAsync(endpoint, postContent)
                        .Result
                        .Content
                        .ReadAsStringAsync()
                        .Result;

                    model.SpellCheckResponse =
                        JsonSerializer
                        .Deserialize<SpellCheckResponse>(content);

                    model.ErrorMessage = content;
                }
                else
                {
                    model.ErrorMessage = "Config error";
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