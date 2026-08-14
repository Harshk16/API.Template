using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace API.Template.Application.Modules.Users.Dtos
{
    public class UserCreateDto
    {
        public required string FirstName { get; set; }
        public required string LastName { get; set; }
        public required string FullName { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        public string Password { get; set; }
        public string ConfirmPassword { get; set; }

        public UserCreateDto(string firstName, string lastName, string email, string phone, string password, string confirmPassword)
        {
            FirstName = firstName;
            LastName = lastName;
            Email = email;
            Phone = phone;
            Password = password;
            ConfirmPassword = confirmPassword;
        }

    }
}
