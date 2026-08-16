using System;
using System.Collections.Generic;

namespace Student_Portal_1._0.Models.ViewModels
{
    public class ExamRoutineViewModel
    {
        public int StudentId { get; set; }
        public string StudentName { get; set; } = string.Empty;
        public string Department { get; set; } = string.Empty;
        public List<ExamScheduleItem> ExamSchedules { get; set; } = new();
    }

    public class ExamScheduleItem
    {
        public int ExamRoutineId { get; set; }
        public string CourseCode { get; set; } = string.Empty;
        public string CourseTitle { get; set; } = string.Empty;
        public string Section { get; set; } = string.Empty;
        public DateTime ExamDate { get; set; }
        public TimeSpan StartTime { get; set; }
        public TimeSpan EndTime { get; set; }
        public string RoomNo { get; set; } = string.Empty;
        public string FormattedDate => ExamDate.ToString("dd MMM yyyy (dddd)");
        public string TimeSlotFormatted =>
            $"{DateTime.Today.Add(StartTime):hh:mm tt} - {DateTime.Today.Add(EndTime):hh:mm tt}";
    }
}