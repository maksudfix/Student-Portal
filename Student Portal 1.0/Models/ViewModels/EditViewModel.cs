namespace Student_Portal_1._0.Models.ViewModels
{
    public class EditViewModel
    {
        public StudentInfo StudentInfo { get; set; } = new();
        public IFromFile? ImageFile { get; set; }
    }
}
