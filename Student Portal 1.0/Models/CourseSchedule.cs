using System.ComponentModel.DataAnnotations;

namespace Student_Portal_1._0.Models
{
    public class CourseSchedule
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int CourseId { get; set; }
        public Course? Course { get; set; }

        [Required]
        [StringLength(50)]
        [Display(Name = "Section")]
        public string Section { get; set; } = string.Empty;

        [Required]
        [Display(Name = "Day of Week")]
        public DayOfWeek DayOfWeek { get; set; }

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
        [Display(Name = "Room Number")]
        public string RoomNo { get; set; } = string.Empty;

        [Required]
        public int Capacity { get; set; } = 40;

        public ICollection<StudentCourseEnrollment>? Enrollments { get; set; }
    }
}