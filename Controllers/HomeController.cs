using System.Diagnostics;
using BHomesAttendance.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BHomesAttendance.Controllers
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
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        [Authorize]
        public IActionResult MainPage()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> MainFunction(IFormFile myfile)
        {
            try
            {
                if (Path.GetExtension(myfile.FileName) == ".mdb")
                {
                    string a = myfile.FileName;
                    a = "Bhome101.mdb";
                    // Example: Save the file to wwwroot/uploads
                    var filePath = Path.Combine("wwwroot/uploads", a);
                    using (var stream = System.IO.File.Create(filePath))
                    {
                        await myfile.CopyToAsync(stream);
                    }
                    TempData["UploadSuccess"] = "File uploaded";
                }
                // Redirect or return a view as needed
                return RedirectToAction("MainPage");
            }
            catch
            {
                TempData["UploadSuccess"] = false;
            }
            return RedirectToAction("MainPage");
        }
    }
}
