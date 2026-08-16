using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Student_Portal_1._0.Models;

namespace Student_Portal_1._0.Data
{
    public class AppDbContext : IdentityDbContext<ApplicationUser>
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        public DbSet<StudentInfo> studentInfos { get; set; }
        public DbSet<Course> Courses { get; set; }
        public DbSet<CourseSchedule> CourseSchedules { get; set; }
        public DbSet<StudentCourseEnrollment> StudentCourseEnrollments { get; set; }
        public DbSet<ExamRoutine> ExamRoutines { get; set; }

        protected override void OnModelCreating(ModelBuilder modelbuilder)
        {
            base.OnModelCreating(modelbuilder);

            modelbuilder.Entity<StudentInfo>()
                .HasIndex(s => s.StudentId)
                .IsUnique();

            modelbuilder.Entity<ApplicationUser>()
                .HasOne(u => u.StudentInfo)
                .WithOne(s => s.ApplicationUser)
                .HasForeignKey<StudentInfo>(s => s.ApplicationUserId)
                .OnDelete(DeleteBehavior.SetNull);

            modelbuilder.Entity<StudentInfo>().HasData(
                new StudentInfo
                {
                    Id = 1,
                    StudentId = 22103265,
                    Name = "Maksud Mubin",
                    Department = "CSE",
                    Semester = "Spring 2022",
                    Email = "maksudmubin1@gmail.com",
                    Phone = "01700000000",
                    Gender = "Male",
                    Address = "Dhaka, Bangladesh",
                    Image = "Images1.jpg"
                },
                new StudentInfo
                {
                    Id = 2,
                    StudentId = 22103266,
                    Name = "Latif",
                    Department = "CSE",
                    Semester = "Spring 2022",
                    Email = "latif71@gmail.com",
                    Phone = "01800000000",
                    Gender = "Male",
                    Address = "Gazipur, Bangladesh",
                    Image = "Images2.jpg"
                },
                new StudentInfo
                {
                    Id = 3,
                    StudentId = 22103267,
                    Name = "Abdullah",
                    Department = "CSE",
                    Semester = "Spring 2022",
                    Email = "abd12@gmail.com",
                    Phone = "01700000000",
                    Gender = "Male",
                    Address = "Khulna, Bangladesh",
                    Image = "Images3.jpg"
                }
            );

            modelbuilder.Entity<CourseSchedule>()
                .HasOne(cs => cs.Course)
                .WithMany(c => c.Schedules)
                .HasForeignKey(cs => cs.CourseId)
                .OnDelete(DeleteBehavior.Cascade);

            modelbuilder.Entity<StudentCourseEnrollment>()
                .HasOne(sce => sce.StudentInfo)
                .WithMany()
                .HasForeignKey(sce => sce.StudentInfoId)
                .OnDelete(DeleteBehavior.Cascade);

            modelbuilder.Entity<StudentCourseEnrollment>()
                .HasOne(sce => sce.CourseSchedule)
                .WithMany(cs => cs.Enrollments)
                .HasForeignKey(sce => sce.CourseScheduleId)
                .OnDelete(DeleteBehavior.Cascade);

            modelbuilder.Entity<ExamRoutine>()
                  .HasOne(er => er.CourseSchedule)
                  .WithMany()
                  .HasForeignKey(er => er.CourseScheduleId)
                  .OnDelete(DeleteBehavior.Cascade);
        }
    }
}