using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel.DataAnnotations;

namespace Student_Portal_1._0.Models
{
    public class ExamRoutine
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int CourseScheduleId { get; set; }

        [ValidateNever]
        public CourseSchedule? CourseSchedule { get; set; }

        [Required]
        [DataType(DataType.Date)]
        [Display(Name = "Exam Date")]
        public DateTime ExamDate { get; set; }

        [Required]
        [DataType(DataType.Time)]
        [Display(Name = "Start Time")]
        public TimeSpan StartTime { get; set; }

        [Required]
        [DataType(DataType.Time)]
        [Display(Name = "End Time")]
        public TimeSpan EndTime { get; set; }

        [Required]
        [StringLength(50)]
        [Display(Name = "Room / Building")]
        public string RoomNo { get; set; } = string.Empty;
    }
}