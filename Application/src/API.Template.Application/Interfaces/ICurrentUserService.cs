using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace API.Template.Identity
{
    public interface ICurrentUserService
    {
        bool IsAuthenticated { get; }
        Guid? UserId { get; }
    }
}
