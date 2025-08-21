using System.ComponentModel.DataAnnotations;

namespace MoviesAPI.Models.System
{
    public class RegisterRequest
    {
        public string Name { get; set; }
        public string Phone { get; set; }
        public string Username { get; set; }

        [DataType(DataType.Password)]
        public string Password { get; set; }
        public bool isActive { get; set; }
    }
}
