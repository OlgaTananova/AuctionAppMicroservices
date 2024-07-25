using Duende.IdentityServer.Models;

namespace IdentityService;

// Static configuration class for IdentityServer settings
public static class Config
{
    public static IEnumerable<IdentityResource> IdentityResources =>

        // Identity resources available in the system
        new IdentityResource[]
        {
            new IdentityResources.OpenId(), // OpenID Connect identity resource
            new IdentityResources.Profile(),  // Profile identity resource for user profile information
        };

    // API scopes available in the system
    public static IEnumerable<ApiScope> ApiScopes =>

        // API scope for auction application with full access
        new ApiScope[]
        {
            new ApiScope("auctionApp", "Auction app full access"),
        };

    public static IEnumerable<Client> Clients =>
        new Client[]
        {
           // Example of a machine-to-machine (m2m) client using client credentials flow
            //new Client
            //{
            //    ClientId = "m2m.client",
            //    ClientName = "Client Credentials Client",

            //    AllowedGrantTypes = GrantTypes.ClientCredentials,
            //    ClientSecrets = { new Secret("511536EF-F270-4058-80CA-1C89C192F69A".Sha256()) },

            //    AllowedScopes = { "scope1" }
            //},

             // Client configuration for Postman

            new Client
            {
                ClientId = "postman",
                ClientName = "Postman",
                AllowedScopes = {"openid", "profile", "auctionApp"},
                RedirectUris = {"https://www.getpostman.com/oath2/callback"},
                ClientSecrets = new []{new Secret("NotASecret".Sha256())},
                AllowedGrantTypes = {GrantType.ResourceOwnerPassword}
            },

            // Client configuration for a Next.js application

            new Client
            {
                ClientId = "nextApp",
                ClientName = "nextApp",
                ClientSecrets = { new Secret("secret".Sha256())},
                AllowedGrantTypes = GrantTypes.CodeAndClientCredentials,
                RequirePkce = false,
                RedirectUris = {"http://localhost:3000/api/auth/callback/id-server"},
                AllowOfflineAccess = true,
                AllowedScopes = {"openid", "profile", "auctionApp"},
                AccessTokenLifetime = 3600*24*30,
                AlwaysIncludeUserClaimsInIdToken = true
            }
          
        };
}
