using System.Diagnostics;
using BHomesAttendance.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Data;
using System.Data.OleDb;
using System.Runtime.InteropServices;


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
                if (Path.GetExtension(myfile.FileName).Equals(".mdb", StringComparison.OrdinalIgnoreCase))
                {
                    string fileName = "Bhome101.mdb";
                    var directoryPath = Path.Combine("wwwroot", "uploads");
                    if (!Directory.Exists(directoryPath))
                    {
                        Directory.CreateDirectory(directoryPath);
                    }
                    var filePath = Path.Combine(directoryPath, fileName);
                    using (var stream = System.IO.File.Create(filePath))
                    {
                        await myfile.CopyToAsync(stream);
                    }
                    TempData["UploadSuccess"] = "File uploaded";

                    // Read tables from the uploaded .mdb file
                    string connStr = $@"Provider=Microsoft.ACE.OLEDB.12.0;Data Source={filePath};Persist Security Info=False;";
                    var userInfoList = new List<Dictionary<string, object>>();
                    var checkInOutList = new List<Dictionary<string, object>>();

                    using (var conn = new OleDbConnection(connStr))
                    {
                        conn.Open();

                        // Read USERINFO table
                        using (var cmd = new OleDbCommand("SELECT USERID,BADGENUMBER FROM USERINFO", conn))
                        using (var reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                var row = new Dictionary<string, object>();
                                for (int i = 0; i < reader.FieldCount; i++)
                                    row[reader.GetName(i)] = reader.GetValue(i);
                                userInfoList.Add(row);
                            }
                        }

                        // Read CHECKINOUT table
                        using (var cmd = new OleDbCommand("SELECT USERID,CHECKTIME,sn FROM CHECKINOUT", conn))
                        using (var reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                var row = new Dictionary<string, object>();
                                for (int i = 0; i < reader.FieldCount; i++)
                                    row[reader.GetName(i)] = reader.GetValue(i);
                                checkInOutList.Add(row);
                            }
                        }
                    }

                    var attendanceRows = (from u in userInfoList
                                          join c in checkInOutList
                                          on u["USERID"] equals c["USERID"]
                                          select new
                                          {
                                              fp_id = u["BADGENUMBER"],
                                              io_time = c["CHECKTIME"],
                                              fp_date = ((DateTime)c["CHECKTIME"]).ToString("M/d/yyyy"),
                                              fp_time = ((DateTime)c["CHECKTIME"]).ToString("h:mm:ss tt"),
                                          }).ToList();
                    //Adding to 

                }
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
