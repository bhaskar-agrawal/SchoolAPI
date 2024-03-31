namespace SchoolAPI.Models
{
    public class Subject
    {
        public string Name { get; set; }

        public int Marks { get; set; }

        public Subject(string name, int marks)
        {
            Name = name;
            Marks = marks;
        } 
    }
}
