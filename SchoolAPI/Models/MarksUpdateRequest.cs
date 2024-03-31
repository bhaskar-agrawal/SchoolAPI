using System.ComponentModel.DataAnnotations;

namespace SchoolAPI.Models
{
    public class MarksUpdateRequest
    {
        [Required]
        public string StudentName { get; set; }

        [Required]
        [RegularExpression(@"^(Maths|Science|English|Hindi|SocialScience)$")]
        public string SubjectName { get; set; }

        [Required]
        [Range(0, 100)]
        public int Marks { get; set; }
    }
}
