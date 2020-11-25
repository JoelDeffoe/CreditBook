using System.ComponentModel.DataAnnotations;

namespace CreditBook.Models
{
    public class AutRequest
    {
        [Required]
        public string Username { get; set; }

        [Required]
        public string Password { get; set; }
    }
}
