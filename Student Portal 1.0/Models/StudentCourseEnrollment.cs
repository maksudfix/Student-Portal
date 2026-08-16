using System.ComponentModel.DataAnnotations;

namespace Student_Portal_1._0.Models
{
    public class StudentCourseEnrollment
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int StudentInfoId { get; set; }
        public StudentInfo? StudentInfo { get; set; }

        [Required]
        public int CourseScheduleId { get; set; }
        public CourseSchedule? CourseSchedule { get; set; }

        [DataType(DataType.DateTime)]
        public DateTime EnrollmentDate { get; set; } = DateTime.Now;
    }
}