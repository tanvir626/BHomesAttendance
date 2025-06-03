using BHomesAttendance.Data;
using BHomesAttendance.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Data;
using System.Data.OleDb;
using System.Diagnostics;
using System.Runtime.InteropServices;


namespace BHomesAttendance.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly ApplicationDbContext _context;

        public HomeController(ILogger<HomeController> logger, ApplicationDbContext context)
        {
            _logger = logger;
            _context = context;
        }

        public IActionResult Index()
        {
            return View();
        }


        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }


        #region Data Pushing Module        
        public IActionResult MainPage()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> MainFunction()
        {
            try
            {

                string connStr = $@"Provider=Microsoft.ACE.OLEDB.12.0;Data Source=\\192.168.5.2\Bhomes_Shared\att2000.mdb;Persist Security Info=False;";
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

                // Join USERINFO and CHECKINOUT
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

                var attendanceEntities = attendanceRows.Select(a => new Tbl_Employee_Attendance
                {
                    fp_id = a.fp_id?.ToString(),
                    io_time = (DateTime)a.io_time,
                    fp_date = ((DateTime)a.io_time).Date,
                    fp_time = a.fp_time
                }).ToList();

                // Get existing records from DB
                var existingKeys = _context.Tbl_Employee_Attendance
                    .Select(e => new { e.fp_id, e.io_time })
                    .ToList()
                    .Select(e => (e.fp_id, e.io_time))
                    .ToHashSet();

                var uniqueEntities = attendanceEntities
                    .Where(a => !existingKeys.Contains((a.fp_id, a.io_time)))
                    .ToList();

                if (uniqueEntities.Any())
                {
                    _context.Tbl_Employee_Attendance.AddRange(uniqueEntities);
                    await _context.SaveChangesAsync();
                }

                TempData["UploadSuccess"] = "Data processed from local file.";
            }
            catch (Exception ex)
            {
                TempData["UploadSuccess"] = $"Error: {ex.Message}";
            }

            return RedirectToAction("MainPage");
        }
        #endregion

    }
}
