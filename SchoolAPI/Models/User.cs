using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace SchoolAPI.Models
{
    public class User
    {
        public string UserName { get; set; }

        public string Password { get; set; }

        public string Role { get; set; }

    }
}
