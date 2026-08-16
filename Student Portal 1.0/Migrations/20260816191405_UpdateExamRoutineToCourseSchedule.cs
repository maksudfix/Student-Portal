using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Student_Portal_1._0.Migrations
{
    /// <inheritdoc />
    public partial class UpdateExamRoutineToCourseSchedule : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ExamRoutines_Courses_CourseId",
                table: "ExamRoutines");

            migrationBuilder.AlterColumn<int>(
                name: "CourseId",
                table: "ExamRoutines",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddColumn<int>(
                name: "CourseScheduleId",
                table: "ExamRoutines",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_ExamRoutines_CourseScheduleId",
                table: "ExamRoutines",
                column: "CourseScheduleId");

            migrationBuilder.AddForeignKey(
                name: "FK_ExamRoutines_CourseSchedules_CourseScheduleId",
                table: "ExamRoutines",
                column: "CourseScheduleId",
                principalTable: "CourseSchedules",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ExamRoutines_Courses_CourseId",
                table: "ExamRoutines",
                column: "CourseId",
                principalTable: "Courses",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ExamRoutines_CourseSchedules_CourseScheduleId",
                table: "ExamRoutines");

            migrationBuilder.DropForeignKey(
                name: "FK_ExamRoutines_Courses_CourseId",
                table: "ExamRoutines");

            migrationBuilder.DropIndex(
                name: "IX_ExamRoutines_CourseScheduleId",
                table: "ExamRoutines");

            migrationBuilder.DropColumn(
                name: "CourseScheduleId",
                table: "ExamRoutines");

            migrationBuilder.AlterColumn<int>(
                name: "CourseId",
                table: "ExamRoutines",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_ExamRoutines_Courses_CourseId",
                table: "ExamRoutines",
                column: "CourseId",
                principalTable: "Courses",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
