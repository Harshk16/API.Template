using API.Template.Application.Modules.Users.Dtos;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace API.Template.Application.Modules.Users.Queries
{
    public class GetUserQuery : IRequest<UserDto>
    {
    }

    public class GetUserQueryHandler : IRequestHandler<GetUserQuery, UserDto>
    {
        public Task<UserDto> Handle(GetUserQuery request, CancellationToken cancellationToken)
        {
            return Task.FromResult(new UserDto{ Id = Guid.NewGuid(), Name = "Test", LastName = "Test", Email = "Test@test.com", 
            PhoneNumber = "+91 123456780"});
        }
    }
}
