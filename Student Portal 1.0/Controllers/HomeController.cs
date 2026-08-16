using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Student_Portal_1._0.Data;
namespace Student_Portal_1._0.Controllers
{
    [Authorize]
    public class HomeController : Controller
    {
        private readonly AppDbContext _context;

        public HomeController(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            if (User.Identity?.IsAuthenticated == true)
            {
                var userName = User.Identity.Name;

                if (int.TryParse(userName, out int studentId))
                {
                    var student = await _context.studentInfos
                        .FirstOrDefaultAsync(s => s.StudentId == studentId);

                    if (student != null)
                    {
                        ViewBag.StudentName = student.Name;
                        ViewBag.StudentId = student.StudentId;
                    }
                }
            }

            return View();
        }
    }
}