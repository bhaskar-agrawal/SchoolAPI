using Microsoft.EntityFrameworkCore;

namespace SchoolAPI.Models
{
    public class SubjectEntity
    {
        public string UserName { get; set; }

        public string SubjectName { get; set; }

        public int Marks { get; set; }
    }
}
