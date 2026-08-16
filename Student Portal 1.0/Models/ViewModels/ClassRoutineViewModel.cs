using System;
using System.Collections.Generic;

namespace Student_Portal_1._0.Models.ViewModels
{
    public class ClassRoutineViewModel
    {
        public int StudentId { get; set; }
        public string StudentName { get; set; } = string.Empty;
        public string Department { get; set; } = string.Empty;
        public string Semester { get; set; } = string.Empty;
        public Dictionary<DayOfWeek, List<RoutineSlot>> Timetable { get; set; } = new()
        {
            { DayOfWeek.Sunday, new List<RoutineSlot>() },
            { DayOfWeek.Monday, new List<RoutineSlot>() },
            { DayOfWeek.Tuesday, new List<RoutineSlot>() },
            { DayOfWeek.Wednesday, new List<RoutineSlot>() },
            { DayOfWeek.Thursday, new List<RoutineSlot>() }
        };
    }

    public class RoutineSlot
    {
        public int EnrollmentId { get; set; }
        public int CourseScheduleId { get; set; }
        public string CourseCode { get; set; } = string.Empty;
        public string CourseTitle { get; set; } = string.Empty;
        public string Section { get; set; } = string.Empty;
        public TimeSpan StartTime { get; set; }
        public TimeSpan EndTime { get; set; }
        public string RoomNo { get; set; } = string.Empty;
        public string TimeSlotFormatted => $"{DateTime.Today.Add(StartTime):hh:mm tt} - {DateTime.Today.Add(EndTime):hh:mm tt}";
    }
}