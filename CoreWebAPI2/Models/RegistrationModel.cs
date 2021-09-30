using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace CoreWebAPI2.Models
{
    public class RegistrationModel
    {
        [MaxLength(100, ErrorMessage = "First name should be up to 100 characters only!")]
        public string FirstName { get; set; }
        [MaxLength(100, ErrorMessage = "Last name should be up to 100 characters only!")]
        public string LastName { get; set; }
        [Required]
        public string UserName { get; set; }
        [Required]
        [EmailAddress(ErrorMessage = "Invalid email address format")]
        public string Email { get; set; }
        [Required]
        public string Password { get; set; }
    }
}
