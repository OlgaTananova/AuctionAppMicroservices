using Duende.IdentityServer.Models;
using Duende.IdentityServer.Services;
using IdentityModel;
using IdentityService.Models;
using Microsoft.AspNetCore.Identity;
using System.Security.Claims;

namespace IdentityService.Services
{
    // Custom implementation of the IProfileService for IdentityServer
    public class CustomProfileService : IProfileService
    {
        private readonly UserManager<ApplicationUser> _userManager;

        // Constructor to inject UserManager for managing application users
        public CustomProfileService(UserManager<ApplicationUser> userManager)
        {
            _userManager = userManager;
        }

        // Method to get profile data for the user
        public async Task  GetProfileDataAsync(ProfileDataRequestContext context)
        {
            // Retrieve the application user based on the context subject (user identifier)
            ApplicationUser user = await _userManager.GetUserAsync(context.Subject);

            // Get existing claims for the user
            IList<Claim> existingClaims = await _userManager.GetClaimsAsync(user);

            // Create a list of custom claims to add to the profile data
            List<Claim> claims = new List<Claim>
            {
                new Claim("username", user.UserName)
            };

            // Add the custom claims to the issued claims
            context.IssuedClaims.AddRange(claims);

            // Add the existing "name" claim to the issued claims if it exists
            context.IssuedClaims.Add(existingClaims.FirstOrDefault(x => x.Type == JwtClaimTypes.Name));
        }

        // Method to check if the user is active
        public Task IsActiveAsync(IsActiveContext context)
        {
            return Task.CompletedTask;
        }
    }
}
