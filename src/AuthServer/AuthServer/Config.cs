using System.Collections.Generic;
using System.Linq;
using IdentityServer4.Models;

namespace AuthServer
{
    public class Config
    {
        public static IEnumerable<IdentityResource> GetIdentityResources()
        {
            return new List<IdentityResource>
            {
                //new IdentityResources.OpenId(),
                //new IdentityResources.Email(),
                //new IdentityResources.Profile(),
            };
        }

        public static IEnumerable<ApiResource> GetApiResources()
        {
            return new List<ApiResource>
            {
                new ApiResource
                {
                    Name = "resourceapi",
                    DisplayName = "Resource API",
                    ApiSecrets = new List<Secret>
                    {
                        new Secret("tada".Sha256())
                    },
                    Scopes = {
                        new Scope("openid"),
                        new Scope("profile"),
                        new Scope("email"),
                        new Scope("api.read"),
                    }
                }
            };
        }

        public static IEnumerable<Client> GetClients()
        {
            return new[]
            {
                new Client {
                    RequireConsent = false,
                    ClientId = "CarfacPlusClient",
                    ClientSecrets = new HashSet<Secret>
                    {
                        new Secret("tada".Sha256())
                    },
                    ClientName = "Angular SPA",
                    //AllowedGrantTypes = new List<ICollection<string>>
                    //{
                    //    GrantTypes.Implicit,
                    //    GrantTypes.Code,
                    //}.SelectMany(g => g).ToList(),
                    AllowedGrantTypes = GrantTypes.ClientCredentials,
                    AllowedScopes = { "openid", "profile", "email", "api.read" },
                    RedirectUris = {"https://localhost:44344/signin-my-auth-server"},
                    PostLogoutRedirectUris = {"http://localhost:4200/"},
                    AllowedCorsOrigins = {"https://localhost:44344", "https://localhost:44348"},
                    AllowAccessTokensViaBrowser = true,
                    AccessTokenLifetime = 3600
                }
            };
        }
    }
}
