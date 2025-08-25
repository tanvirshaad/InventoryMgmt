using InventoryMgmt.BLL.Services;
using InventoryMgmt.MVC.Models;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace InventoryMgmt.MVC.Controllers
{
    public class HomeController : Controller
    {
        private readonly InventoryService _inventoryService;

        public HomeController(InventoryService inventoryService)
        {
            _inventoryService = inventoryService;
        }

        public async Task<IActionResult> Index()
        {
            var model = new HomeIndexViewModel
            {
                LatestInventories = await _inventoryService.GetLatestInventoriesAsync(10),
                PopularInventories = await _inventoryService.GetMostPopularInventoriesAsync(5)
            };

            return View(model);
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> Search(string q)
        {
            if (string.IsNullOrWhiteSpace(q))
            {
                return View("SearchResults", new List<object>());
            }

            var results = await _inventoryService.SearchInventoriesAsync(q);
            ViewBag.SearchTerm = q;

            return View("SearchResults", results);
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
