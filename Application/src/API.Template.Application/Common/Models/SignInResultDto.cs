using API.Template.Application.Interfaces.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace API.Template.Application.Common.Models
{
    public enum SignInStatus
    {
        Succeeded,
        InvalidCredentials,
        LockedOut,
        NotAllowed   // account inactive, email not confirmed, etc.
    }

    public sealed record SignInResultDto(SignInStatus Status, UserDto? User);
}
