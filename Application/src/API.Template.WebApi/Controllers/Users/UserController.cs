using API.Template.Application.Modules.Users.Queries;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace API.Template.WebApi.Controllers.Users
{
    public class UserController : BaseController
    {
        public UserController(IMediator mediator) : base(mediator)
        {
        }

        [HttpGet()]
        public async Task<IActionResult> Get()
        {
            var user = await Mediator.Send(new GetUserQuery());
            return Ok(user);
        }
    }
}
