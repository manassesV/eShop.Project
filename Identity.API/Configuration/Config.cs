using Duende.IdentityServer;
using Duende.IdentityServer.Models;
using Microsoft.Extensions.Configuration;
using System.Collections.Generic;

namespace Identity.API.Configuration
{
    public class Config
    {
        // ApiResources define the apis in your system
        public static IEnumerable<ApiResource> GetApis(){

            return new List<ApiResource>()
            {
                new ApiResource("orders", "Orders Services"),
                new ApiResource("basket", "Basket Services"),
                new ApiResource("webhooks", "Webhooks registration Services"),
            };
        }

        // ApiScope is used to protect the API 
        //The effect is the same as that of API resources in IdentityServer 3.x
        public static IEnumerable<ApiScope> apiScopes() {

            return new List<ApiScope>()
            {
                new ApiScope("orders", "Orders Services"),
                new ApiScope("basket", "Basket Services"),
                new ApiScope("webhooks", "Webhooks registration Services"),
            };
        }

        public static IEnumerable<IdentityResource> GetResources()
        {
            return new List<IdentityResource>()
            {
                new IdentityResources.OpenId(),
                new IdentityResources.Profile()
            };
        }

        // client want to access resources (aka scopes)
        public static IEnumerable<Client> GetClients(IConfiguration configuration)
        {
            return new List<Client>()
            {
                new Client
                {
                    ClientId = "maui",
                    ClientName = "eShop",
                    AllowedGrantTypes = GrantTypes.Code,
                    ClientSecrets =
                    {
                        new Secret("secret".Sha256())
                    },
                    RedirectUris = { configuration["MauiCallback"]},
                    RequireConsent = false,
                    RequirePkce = true,
                    PostLogoutRedirectUris =  { $"{configuration["MauiCallback"]}/Account/Redirecting" },
                    AllowedScopes = new List<string>()
                    {
                        IdentityServerConstants.StandardScopes.OpenId,
                        IdentityServerConstants.StandardScopes.Profile,
                        IdentityServerConstants.StandardScopes.OfflineAccess,
                        "orders",
                        "basket",
                        "mobileshoppingagg",
                        "webhooks"
                    },
                    AllowOfflineAccess = true,
                    AllowAccessTokensViaBrowser = true,
                    AlwaysIncludeUserClaimsInIdToken = true,
                    AccessTokenLifetime = 60*60*2,
                    IdentityTokenLifetime = 60*60*2,
                },
                new Client
                {
                    ClientId = "webapp",
                    ClientName = "WebApp Client",
                    ClientSecrets = new List<Secret>
                    {
                        new Secret("secret".Sha256())
                    },
                    ClientUri = $"{configuration["WebAppClient"]}",
                    AllowedGrantTypes = GrantTypes.Code,
                    AllowAccessTokensViaBrowser= false,
                    RequireConsent = false,
                    AllowOfflineAccess = true,
                    AlwaysIncludeUserClaimsInIdToken = true,
                    RequirePkce = false,
                    RedirectUris = new List<string>
                    {
                                                   $"{configuration["WebAppClient"]}/signin-oidc",
                     $"{configuration["WebAppClient"]}/signin-oidc"

                    },
                    PostLogoutRedirectUris = {
                        $"{configuration["WebAppClient"]}/signout-callback-oidc"

                    },
                    AllowedScopes = new List<string>()
                    {
                        IdentityServerConstants.StandardScopes.OpenId,
                        IdentityServerConstants.StandardScopes.Profile,
                        IdentityServerConstants.StandardScopes.OfflineAccess,
                        "orders",
                        "basket",
                        "webshoppingagg",
                        "webhooks"

                    },
                    AccessTokenLifetime = 60*60*2,
                    IdentityTokenLifetime = 60*60*2

                },
                new Client
                {
                    ClientId = "webhooksclient",
                    ClientName = "Webhooks Client",
                    ClientSecrets = new List <Secret>
                    {
                        new Secret("secret".Sha256())
                    },
                    ClientUri =  $"{configuration["WebhooksWebClient"]}",
                    AllowedGrantTypes = GrantTypes.Code,
                    AllowAccessTokensViaBrowser = false,
                    RequireConsent = false,
                    AllowOfflineAccess = true,
                    AlwaysIncludeUserClaimsInIdToken= true,
                    RedirectUris = new List<string>
                    {
                       $"{configuration["WebhooksWebClient"]}/signin-oidc"
                    },
                    PostLogoutRedirectUris = new List<string>
                    {
                       $"{configuration["WebhooksWebClient"]}/signout-callback-oidc"
                    },
                    AllowedScopes = new List<string>()
                    {
                        IdentityServerConstants.StandardScopes.OpenId,
                        IdentityServerConstants.StandardScopes.Profile,
                        IdentityServerConstants.StandardScopes.OfflineAccess,
                        "webhooks"
                    },
                    AccessTokenLifetime = 60*60*2,
                    IdentityTokenLifetime = 60*60*2
                },
                new Client
                {
                    ClientId = "basketswaggerui",
                    ClientName = "Basket Swagger UI",
                    AllowedGrantTypes = GrantTypes.Implicit,
                    AllowAccessTokensViaBrowser = true,
                    RedirectUris = { $"{configuration["BasketApiClient"]}/swagger/oauth2-redirect.html" },
                    PostLogoutRedirectUris = { $"{configuration["BasketApiClient"]}/swagger/" },
                    AllowedScopes =
                    {
                        "basket"
                    }

                },
                new Client
                {
                    ClientId = "orderingswaggerui",
                    ClientName = "Ordering Swagger UI",
                    AllowedGrantTypes = GrantTypes.Implicit,
                    AllowAccessTokensViaBrowser = true,
                    RedirectUris = { $"{configuration["OrderingApiClient"]}/swagger/oauth2-redirect.html" },
                    PostLogoutRedirectUris =  { $"{configuration["OrderingApiClient"]}/swagger/" },
                    AllowedScopes =
                    {
                        "orders"
                    }
                },
                new Client
                {
                    ClientId = "webhooksswaggerui",
                    ClientName = "WebHook Service Swagger Ui",
                    AllowAccessTokensViaBrowser = true,
                    RedirectUris = { $"{configuration["WebhooksApiClient"]}/swagger/oauth2-redirect.html" },
                    PostLogoutRedirectUris =  { $"{configuration["WebhooksApiClient"]}/swagger/" },
                    AllowedScopes =
                    {
                        "webhooks"
                    }
                }
            };
        }
    }
}
