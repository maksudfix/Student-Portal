using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Student_Portal_1._0.Data;
using Student_Portal_1._0.Models;
using Student_Portal_1._0.Models.ViewModels;

namespace Student_Portal_1._0.Controllers
{
    [Authorize]
    public class ExamRoutineController : Controller
    {
        private readonly AppDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public ExamRoutineController(AppDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var loggedInUser = await _userManager.GetUserAsync(User);
            if (loggedInUser == null)
            {
                return Challenge();
            }

            var studentProfile = await _context.studentInfos
                .AsNoTracking()
                .FirstOrDefaultAsync(s => s.Email == loggedInUser.Email || s.StudentId.ToString() == loggedInUser.UserName);

            if (studentProfile == null && !User.IsInRole("Admin"))
            {
                return NotFound("Student profile not found. Please contact administration.");
            }

            var enrolledCourseScheduleIds = new List<int>();

            if (studentProfile != null)
            {
                enrolledCourseScheduleIds = await _context.StudentCourseEnrollments
                    .AsNoTracking()
                    .Where(e => e.StudentInfoId == studentProfile.Id)
                    .Select(e => e.CourseScheduleId)
                    .ToListAsync();
            }

            var examListItems = await _context.ExamRoutines
                .AsNoTracking()
                .Include(er => er.CourseSchedule)
                    .ThenInclude(cs => cs!.Course)
                .Where(er => User.IsInRole("Admin") || enrolledCourseScheduleIds.Contains(er.CourseScheduleId))
                .OrderBy(er => er.ExamDate)
                .ThenBy(er => er.StartTime)
                .Select(er => new ExamScheduleItem
                {
                    ExamRoutineId = er.Id,
                    CourseCode = er.CourseSchedule != null && er.CourseSchedule.Course != null ? er.CourseSchedule.Course.CourseCode : "N/A",
                    CourseTitle = er.CourseSchedule != null && er.CourseSchedule.Course != null ? er.CourseSchedule.Course.Title : "Unassigned",
                    Section = er.CourseSchedule != null ? er.CourseSchedule.Section : "N/A",
                    ExamDate = er.ExamDate,
                    StartTime = er.StartTime,
                    EndTime = er.EndTime,
                    RoomNo = er.RoomNo
                })
                .ToListAsync();

            var examRoutineViewModel = new ExamRoutineViewModel
            {
                StudentId = studentProfile?.StudentId ?? 0,
                StudentName = studentProfile?.Name ?? (User.IsInRole("Admin") ? "Administrator View" : "Unknown"),
                Department = studentProfile?.Department ?? "N/A",
                ExamSchedules = examListItems
            };

            return View(examRoutineViewModel);
        }

        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create()
        {
            await PopulateCourseSchedulesDropDownListAsync();
            return View();
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ExamRoutine examRoutine)
        {
            ModelState.Remove(nameof(ExamRoutine.CourseSchedule));

            if (examRoutine.StartTime >= examRoutine.EndTime)
            {
                ModelState.AddModelError("EndTime", "End time must be later than start time.");
            }

            if (ModelState.IsValid)
            {
                _context.ExamRoutines.Add(examRoutine);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "Exam schedule published successfully!";
                return RedirectToAction(nameof(Index));
            }

            await PopulateCourseSchedulesDropDownListAsync(examRoutine.CourseScheduleId);
            return View(examRoutine);
        }

        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(int id)
        {
            var targetExamRoutine = await _context.ExamRoutines.FindAsync(id);
            if (targetExamRoutine == null)
            {
                return NotFound();
            }

            await PopulateCourseSchedulesDropDownListAsync(targetExamRoutine.CourseScheduleId);
            return View(targetExamRoutine);
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, ExamRoutine examRoutine)
        {
            if (id != examRoutine.Id)
            {
                return NotFound();
            }

            ModelState.Remove(nameof(ExamRoutine.CourseSchedule));

            if (examRoutine.StartTime >= examRoutine.EndTime)
            {
                ModelState.AddModelError("EndTime", "End time must be later than start time.");
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(examRoutine);
                    await _context.SaveChangesAsync();

                    TempData["SuccessMessage"] = "Exam routine updated successfully!";
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!await ExamRoutineExistsAsync(examRoutine.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
            }

            await PopulateCourseSchedulesDropDownListAsync(examRoutine.CourseScheduleId);
            return View(examRoutine);
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var targetExamRoutine = await _context.ExamRoutines.FindAsync(id);
            if (targetExamRoutine != null)
            {
                _context.ExamRoutines.Remove(targetExamRoutine);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "Exam routine removed successfully!";
            }

            return RedirectToAction(nameof(Index));
        }

        #region Helpers

        private async Task PopulateCourseSchedulesDropDownListAsync(object? selectedSchedule = null)
        {
            var schedulesQueryList = await _context.CourseSchedules
                .AsNoTracking()
                .Include(cs => cs.Course)
                .OrderBy(cs => cs.Course!.CourseCode)
                .ThenBy(cs => cs.Section)
                .Select(cs => new
                {
                    cs.Id,
                    DisplayText = cs.Course != null
                        ? $"{cs.Course.CourseCode} - {cs.Course.Title} [Sec: {cs.Section}]"
                        : $"[Sec: {cs.Section}]"
                })
                .ToListAsync();

            ViewBag.Courses = new SelectList(schedulesQueryList, "Id", "DisplayText", selectedSchedule);
        }

        private async Task<bool> ExamRoutineExistsAsync(int id)
        {
            return await _context.ExamRoutines.AnyAsync(e => e.Id == id);
        }

        #endregion
    }
}