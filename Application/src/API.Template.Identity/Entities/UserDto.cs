using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace API.Template.Identity.Entities
{
    /// <summary>
    /// Plain DTO — safe to pass around Application/Domain. Never exposes
    /// PasswordHash, SecurityStamp, or any other Identity-internal field.
    /// </summary>
    //public sealed record UserDto(
    //    Guid Id,
    //    string Email,
    //    string FirstName,
    //    string LastName,
    //    bool IsActive);
}
