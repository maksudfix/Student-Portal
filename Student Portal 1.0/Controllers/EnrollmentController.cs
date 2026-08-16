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
    public class EnrollmentController : Controller
    {
        private readonly AppDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public EnrollmentController(AppDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        private async Task<StudentInfo?> GetCurrentStudentInfoAsync(ApplicationUser currentUser)
        {
            StudentInfo? targetStudent = null;

            targetStudent = await _context.studentInfos
                .FirstOrDefaultAsync(s => s.ApplicationUserId == currentUser.Id);

            if (targetStudent == null && !string.IsNullOrEmpty(currentUser.Email))
            {
                targetStudent = await _context.studentInfos
                    .FirstOrDefaultAsync(s => s.Email == currentUser.Email);
            }

            if (targetStudent == null && !string.IsNullOrEmpty(currentUser.UserName))
            {
                if (int.TryParse(currentUser.UserName, out int parsedStudentId))
                {
                    targetStudent = await _context.studentInfos
                        .FirstOrDefaultAsync(s => s.StudentId == parsedStudentId);
                }
            }

            return targetStudent;
        }

        [HttpGet]
        public async Task<IActionResult> MyRoutine()
        {
            if (User.IsInRole("Admin"))
            {
                return RedirectToAction("Index", "ClassRoutine");
            }

            var loggedInUser = await _userManager.GetUserAsync(User);
            if (loggedInUser == null)
            {
                return Challenge();
            }

            var studentProfile = await GetCurrentStudentInfoAsync(loggedInUser);

            if (studentProfile == null)
            {
                TempData["ErrorMessage"] = "Student profile not found. Please create or update your profile first.";
                return RedirectToAction("Create", "Student");
            }

            var studentEnrollmentsList = await _context.StudentCourseEnrollments
                .Include(e => e.CourseSchedule!)
                    .ThenInclude(cs => cs.Course)
                .Where(e => e.StudentInfoId == studentProfile.Id)
                .ToListAsync();

            var routineViewModel = new ClassRoutineViewModel
            {
                StudentId = studentProfile.StudentId,
                StudentName = studentProfile.Name ?? loggedInUser.UserName ?? "Student",
                Department = studentProfile.Department ?? "General",
                Semester = studentProfile.Semester ?? "N/A"
            };

            foreach (var enrollmentRecord in studentEnrollmentsList)
            {
                var scheduleSlot = enrollmentRecord.CourseSchedule;
                if (scheduleSlot == null || scheduleSlot.Course == null)
                {
                    continue;
                }

                var slotViewModel = new RoutineSlot
                {
                    EnrollmentId = enrollmentRecord.Id,
                    CourseScheduleId = scheduleSlot.Id,
                    CourseCode = scheduleSlot.Course.CourseCode ?? string.Empty,
                    CourseTitle = scheduleSlot.Course.Title ?? string.Empty,
                    StartTime = scheduleSlot.StartTime,
                    EndTime = scheduleSlot.EndTime,
                    RoomNo = scheduleSlot.RoomNo ?? "TBD"
                };

                if (routineViewModel.Timetable.ContainsKey(scheduleSlot.DayOfWeek))
                {
                    routineViewModel.Timetable[scheduleSlot.DayOfWeek].Add(slotViewModel);
                }
                else
                {
                    routineViewModel.Timetable[scheduleSlot.DayOfWeek] = new List<RoutineSlot> { slotViewModel };
                }
            }

            foreach (var dayKey in routineViewModel.Timetable.Keys.ToList())
            {
                routineViewModel.Timetable[dayKey] = routineViewModel.Timetable[dayKey]
                    .OrderBy(slot => slot.StartTime)
                    .ToList();
            }

            return View(routineViewModel);
        }

        [HttpGet]
        public async Task<IActionResult> SelectCourses()
        {
            var loggedInUser = await _userManager.GetUserAsync(User);
            if (loggedInUser == null)
            {
                return Challenge();
            }

            var availableCoursesList = await _context.Courses
                .Include(c => c.Schedules!)
                    .ThenInclude(s => s.Enrollments)
                .Where(c => c.Schedules != null && c.Schedules.Any())
                .ToListAsync();

            var enrolledScheduleIdsList = new List<int>();
            string currentStudentName = loggedInUser.UserName ?? "Student";

            var studentProfile = await GetCurrentStudentInfoAsync(loggedInUser);

            if (studentProfile != null)
            {
                currentStudentName = studentProfile.Name ?? loggedInUser.UserName ?? $"Student #{studentProfile.StudentId}";

                enrolledScheduleIdsList = await _context.StudentCourseEnrollments
                    .Where(e => e.StudentInfoId == studentProfile.Id)
                    .Select(e => e.CourseScheduleId)
                    .ToListAsync();
            }

            var selectionViewModel = new CourseSelectionViewModel
            {
                StudentName = currentStudentName,
                Courses = availableCoursesList,
                EnrolledScheduleIds = enrolledScheduleIdsList
            };

            return View(selectionViewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Enroll(int scheduleId)
        {
            var loggedInUser = await _userManager.GetUserAsync(User);
            if (loggedInUser == null)
            {
                return Challenge();
            }

            var studentProfile = await GetCurrentStudentInfoAsync(loggedInUser);

            if (studentProfile == null)
            {
                TempData["ErrorMessage"] = "Student profile not found. Please complete your profile registration.";
                return RedirectToAction("Create", "Student");
            }

            var targetSchedule = await _context.CourseSchedules
                .Include(s => s.Course)
                .Include(s => s.Enrollments)
                .FirstOrDefaultAsync(s => s.Id == scheduleId);

            if (targetSchedule == null)
            {
                TempData["ErrorMessage"] = "Course schedule not found.";
                return RedirectToAction(nameof(SelectCourses));
            }

            if (targetSchedule.Enrollments != null && targetSchedule.Enrollments.Count >= targetSchedule.Capacity)
            {
                TempData["ErrorMessage"] = "This schedule slot is full!";
                return RedirectToAction(nameof(SelectCourses));
            }

            bool alreadyInSameSlot = await _context.StudentCourseEnrollments
                .AnyAsync(e => e.StudentInfoId == studentProfile.Id && e.CourseScheduleId == scheduleId);

            if (alreadyInSameSlot)
            {
                TempData["ErrorMessage"] = "You are already enrolled in this class slot.";
                return RedirectToAction(nameof(SelectCourses));
            }

            bool alreadyInCourse = await _context.StudentCourseEnrollments
                .Include(e => e.CourseSchedule)
                .AnyAsync(e => e.StudentInfoId == studentProfile.Id &&
                           e.CourseSchedule != null &&
                           e.CourseSchedule.CourseId == targetSchedule.CourseId);

            if (alreadyInCourse)
            {
                TempData["ErrorMessage"] = "You are already enrolled in another section of this course.";
                return RedirectToAction(nameof(SelectCourses));
            }

            var activeEnrollmentsList = await _context.StudentCourseEnrollments
                .Include(e => e.CourseSchedule)
                    .ThenInclude(cs => cs!.Course)
                .Where(e => e.StudentInfoId == studentProfile.Id && e.CourseSchedule != null)
                .ToListAsync();

            foreach (var activeEnrollment in activeEnrollmentsList)
            {
                var existingScheduleItem = activeEnrollment.CourseSchedule!;

                bool isSameDayOfWeek = string.Equals(
                    existingScheduleItem.DayOfWeek.ToString(),
                    targetSchedule.DayOfWeek.ToString(),
                    StringComparison.OrdinalIgnoreCase);

                if (isSameDayOfWeek)
                {
                    bool hasTimeOverlap = targetSchedule.StartTime < existingScheduleItem.EndTime &&
                                          targetSchedule.EndTime > existingScheduleItem.StartTime;

                    if (hasTimeOverlap)
                    {
                        string conflictedCourseCode = existingScheduleItem.Course?.CourseCode ?? "another course";
                        string formattedStartTime = DateTime.Today.Add(existingScheduleItem.StartTime).ToString("hh:mm tt");
                        string formattedEndTime = DateTime.Today.Add(existingScheduleItem.EndTime).ToString("hh:mm tt");

                        TempData["ErrorMessage"] = $"Time Conflict Warning: This slot conflicts with {conflictedCourseCode} on {existingScheduleItem.DayOfWeek} ({formattedStartTime} - {formattedEndTime}).";

                        return RedirectToAction(nameof(SelectCourses));
                    }
                }
            }

            var newEnrollmentRecord = new StudentCourseEnrollment
            {
                StudentInfoId = studentProfile.Id,
                CourseScheduleId = scheduleId,
                EnrollmentDate = DateTime.Now
            };

            _context.StudentCourseEnrollments.Add(newEnrollmentRecord);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Successfully enrolled in the course slot!";
            return RedirectToAction(nameof(SelectCourses));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> AdminRemoveCourseFromRoutine(int enrollmentId)
        {
            var targetEnrollment = await _context.StudentCourseEnrollments.FindAsync(enrollmentId);
            if (targetEnrollment != null)
            {
                _context.StudentCourseEnrollments.Remove(targetEnrollment);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "Course removed from routine.";
            }

            return RedirectToAction(nameof(MyRoutine));
        }
    }
}