#nullable enable
using Student_Portal_1._0.Models;

namespace Student_Portal_1._0.Models.ViewModels
{
    public class CourseSelectionViewModel
    {
        public string? StudentName { get; set; }
        public List<Course> Courses { get; set; } = new();
        public List<int> EnrolledScheduleIds { get; set; } = new();
        public List<CourseDTO> AvailableCourses { get; set; } = new();
    }

    public class CourseDTO //DTO = Data Transfer Object
    {
        public int CourseId { get; set; }
        public string CourseCode { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public int Credits { get; set; }
        public List<ScheduleOptionDTO> ScheduleOptions { get; set; } = new();
    }

    public class ScheduleOptionDTO
    {
        public int ScheduleId { get; set; }
        public string Section { get; set; } = string.Empty;
        public DayOfWeek DayOfWeek { get; set; }
        public TimeSpan StartTime { get; set; }
        public TimeSpan EndTime { get; set; }
        public string RoomNo { get; set; } = string.Empty;
        public int Capacity { get; set; }
        public int EnrolledCount { get; set; }
        public bool IsFull => EnrolledCount >= Capacity;
    }
}