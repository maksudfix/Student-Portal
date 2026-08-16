using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Student_Portal_1._0.Data;
using Student_Portal_1._0.Models;

namespace Student_Portal_1._0.Controllers
{
    [Authorize(Roles = "Admin")]
    public class CourseController : Controller
    {
        private readonly AppDbContext _context;

        public CourseController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var coursesList = await _context.Courses
                .AsNoTracking()
                .Include(c => c.Schedules!)
                    .ThenInclude(s => s.Enrollments)
                .Include(c => c.ExamRoutines)
                .ToListAsync();

            return View(coursesList);
        }

        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("CourseCode,Title,Credits,Department")] Course course)
        {
            if (ModelState.IsValid)
            {
                _context.Courses.Add(course);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "Course created successfully!";
                return RedirectToAction(nameof(Index));
            }

            return View(course);
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var targetCourse = await _context.Courses.FindAsync(id);
            if (targetCourse == null)
            {
                return NotFound();
            }

            return View("EditCourse", targetCourse);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,CourseCode,Title,Credits,Department")] Course course)
        {
            if (id != course.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(course);
                    await _context.SaveChangesAsync();

                    TempData["SuccessMessage"] = "Course updated successfully!";
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!await CourseExistsAsync(course.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
            }

            return View("EditCourse", course);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var targetCourse = await _context.Courses.FindAsync(id);
            if (targetCourse != null)
            {
                _context.Courses.Remove(targetCourse);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "Course deleted successfully!";
            }

            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> AddSchedule(int courseId)
        {
            var targetCourse = await _context.Courses.FindAsync(courseId);
            if (targetCourse == null)
            {
                return NotFound();
            }

            ViewBag.Course = targetCourse;
            return View("EditSchedule", new CourseSchedule { CourseId = courseId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddSchedule([Bind("Id,CourseId,Section,DayOfWeek,StartTime,EndTime,RoomNo,Capacity")] CourseSchedule schedule)
        {
            ModelState.Remove(nameof(CourseSchedule.Course));
            ModelState.Remove(nameof(CourseSchedule.Enrollments));

            if (schedule.StartTime >= schedule.EndTime)
            {
                ModelState.AddModelError("EndTime", "End time must be after start time.");
            }

            if (ModelState.IsValid)
            {
                _context.CourseSchedules.Add(schedule);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "Course section and schedule added successfully!";
                return RedirectToAction(nameof(Index));
            }

            ViewBag.Course = await _context.Courses.FindAsync(schedule.CourseId);
            return View("EditSchedule", schedule);
        }

        [HttpGet]
        public async Task<IActionResult> EditSchedule(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var targetSchedule = await _context.CourseSchedules
                .Include(s => s.Course)
                .FirstOrDefaultAsync(s => s.Id == id);

            if (targetSchedule == null)
            {
                return NotFound();
            }

            ViewBag.Course = targetSchedule.Course;
            return View("EditSchedule", targetSchedule);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditSchedule(int id, [Bind("Id,CourseId,Section,DayOfWeek,StartTime,EndTime,RoomNo,Capacity")] CourseSchedule schedule)
        {
            if (id != schedule.Id)
            {
                return NotFound();
            }

            ModelState.Remove(nameof(CourseSchedule.Course));
            ModelState.Remove(nameof(CourseSchedule.Enrollments));

            if (schedule.StartTime >= schedule.EndTime)
            {
                ModelState.AddModelError("EndTime", "End time must be after start time.");
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(schedule);
                    await _context.SaveChangesAsync();

                    TempData["SuccessMessage"] = "Schedule slot updated successfully!";
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!await ScheduleExistsAsync(schedule.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
            }

            ViewBag.Course = await _context.Courses.FindAsync(schedule.CourseId);
            return View("EditSchedule", schedule);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteSchedule(int id)
        {
            var targetSchedule = await _context.CourseSchedules
                .Include(s => s.Enrollments)
                .FirstOrDefaultAsync(s => s.Id == id);

            if (targetSchedule != null)
            {
                if (targetSchedule.Enrollments != null && targetSchedule.Enrollments.Any())
                {
                    _context.StudentCourseEnrollments.RemoveRange(targetSchedule.Enrollments);
                }

                _context.CourseSchedules.Remove(targetSchedule);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "Schedule slot removed successfully!";
            }

            return RedirectToAction(nameof(Index));
        }

        private async Task<bool> CourseExistsAsync(int id)
        {
            return await _context.Courses.AnyAsync(e => e.Id == id);
        }

        private async Task<bool> ScheduleExistsAsync(int id)
        {
            return await _context.CourseSchedules.AnyAsync(e => e.Id == id);
        }
    }
}