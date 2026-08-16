using System;
using System.Collections.Generic;

namespace Student_Portal_1._0.Models.ViewModels
{
    public class StudentRoutineOverviewViewModel
    {
        public int StudentInfoId { get; set; }
        public int StudentId { get; set; }
        public string StudentName { get; set; } = string.Empty;
        public string Department { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public int TotalEnrolledClasses { get; set; }
    }

    public class StudentClassRoutineViewModel
    {
        public int StudentInfoId { get; set; }
        public int StudentId { get; set; }
        public string StudentName { get; set; } = string.Empty;
        public string Department { get; set; } = string.Empty;
        public List<ClassRoutineItemViewModel> RoutineItems { get; set; } = new();
    }

    public class ClassRoutineItemViewModel
    {
        public int EnrollmentId { get; set; }
        public int CourseScheduleId { get; set; }
        public string CourseCode { get; set; } = string.Empty;
        public string CourseTitle { get; set; } = string.Empty;
        public string Section { get; set; } = string.Empty; // Added Section property
        public string DayOfWeek { get; set; } = string.Empty;
        public TimeSpan StartTime { get; set; }
        public TimeSpan EndTime { get; set; }
        public string RoomNo { get; set; } = string.Empty;
    }
}