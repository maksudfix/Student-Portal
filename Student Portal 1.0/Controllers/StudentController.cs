using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Student_Portal_1._0.Data;
using Student_Portal_1._0.Models;
using Student_Portal_1._0.Models.ViewModels;

namespace Student_Portal_1._0.Controllers
{
    [Authorize]
    public class StudentController : Controller
    {
        private readonly AppDbContext _context;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly string[] _allowedExtensions = { ".jpg", ".jpeg", ".png", ".gif" };

        public StudentController(
            AppDbContext context,
            IWebHostEnvironment webHostEnvironment,
            UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _webHostEnvironment = webHostEnvironment;
            _userManager = userManager;
        }

        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Index()
        {
            var studentList = await _context.studentInfos.ToListAsync();
            return View(studentList);
        }

        [HttpGet]
        public async Task<IActionResult> Details()
        {
            if (User.IsInRole("Admin"))
            {
                return RedirectToAction(nameof(Index));
            }

            var loggedInUser = await _userManager.GetUserAsync(User);
            if (loggedInUser == null)
            {
                return Challenge();
            }

            var studentProfile = await _context.studentInfos
                .FirstOrDefaultAsync(s => s.ApplicationUserId == loggedInUser.Id || s.Email == loggedInUser.Email);

            if (studentProfile == null)
            {
                return RedirectToAction(nameof(Create));
            }

            var dashboardViewModel = new StudentDashboardViewModel
            {
                StudentInfo = studentProfile
            };

            return View(dashboardViewModel);
        }

        [HttpGet]
        public async Task<IActionResult> ClassRoutine()
        {
            if (User.IsInRole("Admin"))
            {
                return RedirectToAction("Index", "Course");
            }

            var loggedInUser = await _userManager.GetUserAsync(User);
            if (loggedInUser == null)
            {
                return Challenge();
            }

            var studentProfile = await _context.studentInfos
                .FirstOrDefaultAsync(s => s.ApplicationUserId == loggedInUser.Id || s.Email == loggedInUser.Email);

            if (studentProfile == null)
            {
                TempData["ErrorMessage"] = "Student profile not found. Please create your profile first.";
                return RedirectToAction(nameof(Create));
            }

            var scheduleList = await _context.CourseSchedules
                .Include(cs => cs.Course)
                .Include(cs => cs.Enrollments)
                .Where(cs => cs.Enrollments!.Any(e => e.StudentInfoId == studentProfile.Id))
                .OrderBy(cs => cs.DayOfWeek)
                .ThenBy(cs => cs.StartTime)
                .ToListAsync();

            return View(scheduleList);
        }

        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(StudentInfo model, IFormFile? imageFile)
        {
            if (ModelState.IsValid)
            {
                if (imageFile != null && imageFile.Length > 0)
                {
                    model.Image = await ProcessImageUpload(imageFile);
                    if (model.Image == null)
                    {
                        ModelState.AddModelError("Image", "Only image files (.jpg, .png, .gif) are allowed.");
                        return View(model);
                    }
                }

                if (!User.IsInRole("Admin"))
                {
                    var loggedInUser = await _userManager.GetUserAsync(User);
                    if (loggedInUser != null)
                    {
                        model.ApplicationUserId = loggedInUser.Id;
                    }
                }

                _context.studentInfos.Add(model);
                await _context.SaveChangesAsync();

                return User.IsInRole("Admin")
                    ? RedirectToAction(nameof(Index))
                    : RedirectToAction(nameof(Details));
            }

            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int? id)
        {
            StudentInfo? targetStudent;

            if (id == null)
            {
                var loggedInUser = await _userManager.GetUserAsync(User);
                if (loggedInUser == null)
                {
                    return Challenge();
                }

                targetStudent = await _context.studentInfos
                    .FirstOrDefaultAsync(s => s.ApplicationUserId == loggedInUser.Id || s.Email == loggedInUser.Email);
            }
            else
            {
                if (!User.IsInRole("Admin"))
                {
                    return Forbid();
                }

                targetStudent = await _context.studentInfos.FindAsync(id);
            }

            if (targetStudent == null)
            {
                return NotFound();
            }

            return View(targetStudent);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, StudentInfo model, IFormFile? imageFile)
        {
            if (id != model.Id)
            {
                return NotFound();
            }

            if (!User.IsInRole("Admin"))
            {
                var loggedInUser = await _userManager.GetUserAsync(User);
                if (loggedInUser == null)
                {
                    return Challenge();
                }

                var existingUserStudent = await _context.studentInfos
                    .AsNoTracking()
                    .FirstOrDefaultAsync(s => s.ApplicationUserId == loggedInUser.Id || s.Email == loggedInUser.Email);

                if (existingUserStudent == null || existingUserStudent.Id != id)
                {
                    return Forbid();
                }
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var existingStudent = await _context.studentInfos
                        .AsNoTracking()
                        .FirstOrDefaultAsync(s => s.Id == id);

                    if (existingStudent == null)
                    {
                        return NotFound();
                    }

                    model.ApplicationUserId = existingStudent.ApplicationUserId;

                    if (imageFile != null && imageFile.Length > 0)
                    {
                        string? newImagePath = await ProcessImageUpload(imageFile);
                        if (newImagePath == null)
                        {
                            ModelState.AddModelError("Image", "Only image files (.jpg, .png, .gif) are allowed.");
                            return View(model);
                        }

                        DeleteOldImage(existingStudent.Image);
                        model.Image = newImagePath;
                    }
                    else
                    {
                        model.Image = existingStudent.Image;
                    }

                    _context.Update(model);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!StudentExists(model.Id))
                    {
                        return NotFound();
                    }
                    throw;
                }

                return User.IsInRole("Admin")
                    ? RedirectToAction(nameof(Index))
                    : RedirectToAction(nameof(Details));
            }

            return View(model);
        }

        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var targetStudent = await _context.studentInfos
                .FirstOrDefaultAsync(m => m.Id == id);

            if (targetStudent == null)
            {
                return NotFound();
            }

            return View(targetStudent);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var studentInfoRecord = await _context.studentInfos.FindAsync(id);
            if (studentInfoRecord != null)
            {
                DeleteOldImage(studentInfoRecord.Image);

                var matchingUserAccount = !string.IsNullOrEmpty(studentInfoRecord.ApplicationUserId)
                    ? await _userManager.FindByIdAsync(studentInfoRecord.ApplicationUserId)
                    : await _userManager.Users.FirstOrDefaultAsync(u => u.Email == studentInfoRecord.Email);

                _context.studentInfos.Remove(studentInfoRecord);
                await _context.SaveChangesAsync();

                if (matchingUserAccount != null)
                {
                    await _userManager.DeleteAsync(matchingUserAccount);
                }
            }

            return RedirectToAction(nameof(Index));
        }

        private async Task<string?> ProcessImageUpload(IFormFile imageFile)
        {
            string fileExtension = Path.GetExtension(imageFile.FileName).ToLower();
            if (!_allowedExtensions.Contains(fileExtension))
            {
                return null;
            }

            string uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "images/students");
            Directory.CreateDirectory(uploadsFolder);

            string uniqueFileName = $"{Guid.NewGuid()}_{imageFile.FileName}";
            string filePath = Path.Combine(uploadsFolder, uniqueFileName);

            using (var fileStream = new FileStream(filePath, FileMode.Create))
            {
                await imageFile.CopyToAsync(fileStream);
            }

            return "/images/students/" + uniqueFileName;
        }

        private void DeleteOldImage(string? imagePath)
        {
            if (string.IsNullOrEmpty(imagePath))
            {
                return;
            }

            string fullFilePath = Path.Combine(_webHostEnvironment.WebRootPath, imagePath.TrimStart('/'));
            if (System.IO.File.Exists(fullFilePath))
            {
                System.IO.File.Delete(fullFilePath);
            }
        }

        private bool StudentExists(int id)
        {
            return _context.studentInfos.Any(e => e.Id == id);
        }
    }
}