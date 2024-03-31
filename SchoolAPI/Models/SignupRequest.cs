using System.ComponentModel.DataAnnotations;

namespace SchoolAPI.Models
{
    public class SignupRequest
    {
        [Required]
        public string UserName { get; set; }

        [Required]
        public string Password { get; set; }

        [Required]
        [RegularExpression(@"^(Teacher|Student)$")]
        public string Role { get; set; }


    }
}
