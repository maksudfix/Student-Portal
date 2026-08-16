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
    public class ClassRoutineController : Controller
    {
        private readonly AppDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public ClassRoutineController(AppDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        [HttpGet]
        public async Task<IActionResult> Index(int? studentId)
        {
            bool isUserAdmin = User.IsInRole("Admin");

            if (isUserAdmin && !studentId.HasValue)
            {
                var allStudentsOverviewList = await _context.studentInfos
                    .AsNoTracking()
                    .Select(s => new StudentRoutineOverviewViewModel
                    {
                        StudentInfoId = s.Id,
                        StudentId = s.StudentId,
                        StudentName = s.Name ?? "Unknown",
                        Department = s.Department ?? "N/A",
                        Email = s.Email ?? "N/A",
                        TotalEnrolledClasses = _context.StudentCourseEnrollments.Count(e => e.StudentInfoId == s.Id)
                    })
                    .ToListAsync();

                return View("AdminStudentList", allStudentsOverviewList);
            }

            StudentInfo? targetStudent = null;

            if (isUserAdmin && studentId.HasValue)
            {
                targetStudent = await _context.studentInfos
                    .AsNoTracking()
                    .FirstOrDefaultAsync(s => s.Id == studentId.Value);
            }
            else
            {
                var loggedInUser = await _userManager.GetUserAsync(User);
                if (loggedInUser == null)
                {
                    return Challenge();
                }

                targetStudent = await _context.studentInfos
                    .AsNoTracking()
                    .FirstOrDefaultAsync(s => s.Email == loggedInUser.Email || s.StudentId.ToString() == loggedInUser.UserName);
            }

            if (targetStudent == null)
            {
                if (isUserAdmin)
                {
                    TempData["ErrorMessage"] = "Selected student record was not found.";
                    return RedirectToAction(nameof(Index));
                }

                ViewBag.ErrorMessage = "No student record linked to your user account. Please contact an Administrator.";
                return View("ErrorNotice");
            }

            var studentEnrollments = await _context.StudentCourseEnrollments
                .AsNoTracking()
                .Include(e => e.CourseSchedule)
                    .ThenInclude(cs => cs!.Course)
                .Where(e => e.StudentInfoId == targetStudent.Id && e.CourseSchedule != null)
                .OrderBy(e => e.CourseSchedule!.DayOfWeek)
                .ThenBy(e => e.CourseSchedule!.StartTime)
                .ToListAsync();

            var routineItemsList = studentEnrollments
                .Where(e => e.CourseSchedule != null)
                .Select(e => new ClassRoutineItemViewModel
                {
                    EnrollmentId = e.Id,
                    CourseScheduleId = e.CourseScheduleId,
                    CourseCode = e.CourseSchedule!.Course?.CourseCode ?? "N/A",
                    CourseTitle = e.CourseSchedule.Course?.Title ?? "Unassigned",
                    Section = e.CourseSchedule.Section ?? "N/A", // <-- Mapped from CourseSchedule
                    DayOfWeek = e.CourseSchedule.DayOfWeek.ToString(),
                    StartTime = e.CourseSchedule.StartTime,
                    EndTime = e.CourseSchedule.EndTime,
                    RoomNo = e.CourseSchedule.RoomNo ?? "TBD"
                }).ToList();

            var routineViewModel = new StudentClassRoutineViewModel
            {
                StudentInfoId = targetStudent.Id,
                StudentId = targetStudent.StudentId,
                StudentName = targetStudent.Name ?? "Unknown",
                Department = targetStudent.Department ?? "N/A",
                RoutineItems = routineItemsList
            };

            return View("Index", routineViewModel);
        }

        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> EditStudentRoutine(int studentInfoId)
        {
            var targetStudent = await _context.studentInfos
                .AsNoTracking()
                .FirstOrDefaultAsync(s => s.Id == studentInfoId);

            if (targetStudent == null)
            {
                TempData["ErrorMessage"] = "Student not found.";
                return RedirectToAction(nameof(Index));
            }

            var currentlyEnrolledScheduleIds = await _context.StudentCourseEnrollments
                .AsNoTracking()
                .Where(e => e.StudentInfoId == studentInfoId)
                .Select(e => e.CourseScheduleId)
                .ToListAsync();

            var allAvailableSchedules = await _context.CourseSchedules
                .AsNoTracking()
                .Include(cs => cs.Course)
                .OrderBy(cs => cs.Course!.CourseCode)
                .ThenBy(cs => cs.DayOfWeek)
                .ToListAsync();

            ViewBag.Student = targetStudent;
            ViewBag.EnrolledScheduleIds = currentlyEnrolledScheduleIds;

            return View(allAvailableSchedules);
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditStudentRoutine(int studentInfoId, List<int> selectedScheduleIds)
        {
            selectedScheduleIds ??= new List<int>();

            var targetStudent = await _context.studentInfos.FindAsync(studentInfoId);
            if (targetStudent == null)
            {
                TempData["ErrorMessage"] = "Student profile not found.";
                return RedirectToAction(nameof(Index));
            }

            var currentEnrollmentRecords = await _context.StudentCourseEnrollments
                .Where(e => e.StudentInfoId == studentInfoId)
                .ToListAsync();

            var existingScheduleIdSet = currentEnrollmentRecords.Select(e => e.CourseScheduleId).ToHashSet();

            var enrollmentsToRemove = currentEnrollmentRecords.Where(e => !selectedScheduleIds.Contains(e.CourseScheduleId)).ToList();
            _context.StudentCourseEnrollments.RemoveRange(enrollmentsToRemove);

            var schedulesToAdd = selectedScheduleIds.Where(id => !existingScheduleIdSet.Contains(id)).ToList();
            foreach (var scheduleId in schedulesToAdd)
            {
                _context.StudentCourseEnrollments.Add(new StudentCourseEnrollment
                {
                    StudentInfoId = studentInfoId,
                    CourseScheduleId = scheduleId
                });
            }

            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = $"Routine updated successfully for {targetStudent.Name ?? "Student"}!";

            return RedirectToAction(nameof(Index));
        }
    }
}