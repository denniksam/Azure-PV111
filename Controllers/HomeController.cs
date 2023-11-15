using Azure_PV111.Middleware;
using Azure_PV111.Models;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace Azure_PV111.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Privacy()
        {
            DataMiddleware.Data.Add("Privacy visited");
            return View();
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