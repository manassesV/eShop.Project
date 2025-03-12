﻿using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.JsonWebTokens;

namespace eShop.ServiceDefaults
{
    public static class AuthenticationExtensions
    {
       
        public static IServiceCollection AddDefaultAuthentication(this IHostApplicationBuilder builder)
        {
            var services = builder.Services;
            var configuration = builder.Configuration;

            var identitySection = configuration.GetSection("Identity");

            if (!identitySection.Exists())
            {
                return services;
            }


            // prevent from mapping "sub" claim to nameidentifier.
            JsonWebTokenHandler.DefaultInboundClaimTypeMap.Remove("sub");


            services.AddAuthentication().AddJwtBearer(options =>
            {
                var identityUrl = identitySection.GetRequiredValue("Url");
                var audience = identitySection.GetRequiredValue("Audience");


                options.Authority = identityUrl;
                options.RequireHttpsMetadata = false;
                options.Audience = audience;

#if DEBUG

                options.TokenValidationParameters.ValidIssuers = [identityUrl, "https://10.0.2.2:5243"];
#else
                options.TokenValidationParameters.ValidIssuers = [identityUrl];
#endif
                options.TokenValidationParameters.ValidateAudience  = true;
            });

            services.AddAuthorization();

            return services;
        }

    }
}
