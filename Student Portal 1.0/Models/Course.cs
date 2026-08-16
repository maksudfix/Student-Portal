using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Student_Portal_1._0.Models
{
    public class Course
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(20)]
        [Display(Name = "Course Code")]
        public string CourseCode { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        [Display(Name = "Course Title")]
        public string Title { get; set; } = string.Empty;

        [Required]
        [Range(1, 6, ErrorMessage = "Credits must be between 1 and 6")]
        [Column(TypeName = "decimal(3, 1)")]
        public decimal Credits { get; set; }

        [Required]
        [StringLength(100)]
        public string Department { get; set; } = string.Empty;

        public ICollection<CourseSchedule> Schedules { get; set; } = new List<CourseSchedule>();
        public ICollection<ExamRoutine> ExamRoutines { get; set; } = new List<ExamRoutine>();
    }
}