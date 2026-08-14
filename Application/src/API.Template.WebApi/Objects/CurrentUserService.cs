using API.Template.Identity;
using API.Template.Identity.Extensions;

namespace API.Template.WebApi.Objects
{
    internal sealed class CurrentUserService : ICurrentUserService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private bool isParsed;
        private bool isAuthenticated;
        private Guid id;

        public CurrentUserService(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
            isParsed = false;
        }

        public bool IsAuthenticated
        {
            get
            {
                ParseIdentity();
                return isAuthenticated;
            }
        }

        public Guid? UserId
        {
            get
            {
                ParseIdentity();
                return id;
            }
        }

        private void ParseIdentity()
        {
            if (!isParsed)
            {
                var user = _httpContextAccessor?.HttpContext?.User;   // ← fixed

                if (user == null)
                    return;

                isAuthenticated = user.Identity?.IsAuthenticated ?? false;

                if (isAuthenticated)
                {
                    id = user.Claims.GetId() ?? Guid.Empty;
                }

                isParsed = true;
            }
        }
    }
}
