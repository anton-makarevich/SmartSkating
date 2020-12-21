using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components.Authorization;

namespace SmartSkating.Dashboard.Blazor.Services
{
    public class AuthorizationProvider:AuthenticationStateProvider
    {
        public override Task<AuthenticationState> GetAuthenticationStateAsync()
        {
            var claims = new[] {new Claim(ClaimTypes.Name, "Me")};
            var identity = new ClaimsIdentity(claims, "Server authentication");

            return Task.FromResult( new AuthenticationState(new ClaimsPrincipal(identity)));
        }
    }
}
