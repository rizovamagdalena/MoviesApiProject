using System.ComponentModel.DataAnnotations;

namespace MoviesAPI.Models.System
{
    public class User
    {
        public int Id { get; set; }

        [Required]
        [Display(Name = "Username")]
        public string Username { get; set; }

        [Required]
        [Display(Name = "Password")]
        public string Password { get; set; }

        [Required]
        [Display(Name = "First Name")]
        public string Name { get; set; }

        [Required]
        [Display(Name = "Phone Number")]
        public string Phone { get; set; }

        public bool IsActive { get; set; }
        [Required]
        public string Role { get; set; }
    }

    public class CreateUser
    {
        public string Name { get; set; }
        public string Phone { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
    }

    public class UserProfile
    {
        public string Name { get; set; }
        public string Phone { get; set; }
        public string Username { get; set; }
    }
}
