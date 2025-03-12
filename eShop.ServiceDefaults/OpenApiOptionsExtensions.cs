﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Asp.Versioning;
using Asp.Versioning.ApiExplorer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Primitives;
using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;

namespace eShop.ServiceDefaults
{
    public static class OpenApiOptionsExtensions
    {
        public static OpenApiOptions ApplyApiVersionInfo(this OpenApiOptions options, string title, string description)
        {
            options.AddDocumentTransformer((document, context, cancellationToken) =>
            {
                var versionDescriptionProvider = context.ApplicationServices.GetService<IApiVersionDescriptionProvider>();
                var apiDescription = versionDescriptionProvider?.ApiVersionDescriptions
                .SingleOrDefault(description => description.GroupName == context.DocumentName);

                if(apiDescription == null)
                {
                    return Task.CompletedTask;
                }

                document.Info.Version = apiDescription.ApiVersion.ToString();
                document.Info.Title = title;
                document.Info.Description = BuildDescription(apiDescription, description);
                return Task.CompletedTask;

            });

            return options;
        }

        private static string BuildDescription(ApiVersionDescription api, string description)
        {
            var text = new StringBuilder(description);

            if (api.IsDeprecated)
            {
                if (text.Length > 0) {

                    if (text[^1] != '-')
                    {
                        text.Append('.');
                    }

                    text.Append(" ");
                }
                text.Append("This API version has been deprecated.");
            }


            if (api.SunsetPolicy is { } policy)
            {
                if (policy.Date is { } when)
                {
                    if (text.Length > 0)
                    {
                        text.Append(' ');
                    }

                    text.Append("The API will be sunset on")
                        .Append(when.Date.ToShortDateString())
                    .Append('.');
                }

                if (policy.HasLinks)
                {
                    text.AppendLine();

                    var rendered = false;


                    foreach (var link in policy.Links.Where(l => l.Type == "text/html"))
                    {
                        if (!rendered)
                        {
                            text.AppendLine("<h4>Links</h4><ul>");
                            rendered = true;
                        }

                        text.Append("<li><a href=\"");
                        text.AppendLine(link.LinkTarget.OriginalString);
                        text.Append("\">");

                        text.Append(
                            StringSegment.IsNullOrEmpty(link.Title)
                            ? link.LinkTarget.OriginalString
                            : link.Title.ToString());
                        text.Append("</a></li>");

                    }

                    if (rendered)
                    {
                        text.AppendLine("</ul>");
                    }
                }
            }

            return text.ToString();
        }

        public static OpenApiOptions ApplySecuritySchemeDefinitions(this OpenApiOptions options)
        {
            options.AddDocumentTransformer<SecuritySchemeDefinitionsTransformer>();

            return options;
        }

        public static OpenApiOptions ApplyAuthorizationChecks(this OpenApiOptions options, string[] scopes)
        {
            options.AddOperationTransformer((operation, context, cancellationToken) =>
            {
                var metadata = context.Description.ActionDescriptor.EndpointMetadata;

                if (!metadata.OfType<IAuthorizeData>().Any())
                {
                    return Task.CompletedTask;
                }

                operation.Responses.TryAdd("401", new OpenApiResponse { Description = "Unauthorized" });
                operation.Responses.TryAdd("403", new OpenApiResponse { Description = "Forbidden" });

                var oAuthScheme = new OpenApiSecurityScheme
                {
                    Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "oauth2" }
                };

                operation.Security = new List<OpenApiSecurityRequirement>
                {
                   new()
                   {
                       [oAuthScheme] = scopes
                   }
                }; 

                return Task.CompletedTask;
            });

            return options;
        }

        public static OpenApiOptions ApplyOperationDeprecatedStatus(this OpenApiOptions options)
        {
            options.AddOperationTransformer((operation, context, cancellationToken) =>
            {
                var apiDescription = context.Description;
                operation.Deprecated |= apiDescription.IsDeprecated();
                return Task.CompletedTask;
            });

            return options;
        }

        public static OpenApiOptions ApplyApiVersionDescription(this OpenApiOptions options)
        {
            options.AddOperationTransformer((operation, context, cancellationToken) =>
            {
                //Find parameter named "api-version" and add a description to it
                var apiVersionParameter = operation.Parameters.FirstOrDefault( p => p.Name == "api-version");
                if (apiVersionParameter is not null)
                {
                    apiVersionParameter.Description = "The API version, in the format 'major.minor'.";
                    switch (context.DocumentName)
                    {
                        case "v1":
                            apiVersionParameter.Schema.Example = new OpenApiString("1.0");
                            break;
                        case "v2":
                            apiVersionParameter.Schema.Example = new OpenApiString("2.0");
                            break;
                        default:
                            break;
                    }
                }

                return Task.CompletedTask;
            });
            return options;
        }

        // This extension method adds a schema transformer that sets "nullable" to false for all optional properties.
        public static OpenApiOptions ApplySchemaNullableFalse(this OpenApiOptions options)
        {
            options.AddSchemaTransformer((schema, context, cancellationToken) =>
            {
                if(schema.Properties is not null)
                {
                    foreach (var property in schema.Properties)
                    {
                        if(schema.Required.Contains(property.Key) != true)
                        {
                            property.Value.Nullable = false;
                        }
                    }
                }

                return Task.CompletedTask;
            });

            return options;
        }
        private class SecuritySchemeDefinitionsTransformer(IConfiguration configuration) : IOpenApiDocumentTransformer
        {
            public Task TransformAsync(OpenApiDocument document, OpenApiDocumentTransformerContext context, CancellationToken cancellationToken)
            {
                var identitySection = configuration.GetSection("Identity");
                if (!identitySection.Exists())
                {
                    return Task.CompletedTask;
                }

                var identityUrlExternal = identitySection.GetRequiredValue("Url");
                var scopes = identitySection.GetRequiredSection("Scopes").GetChildren()
                    .ToDictionary(p => p.Key, p => p.Value);

                var securityScheme = new OpenApiSecurityScheme
                {
                    Type = SecuritySchemeType.OAuth2,
                    Flows = new OpenApiOAuthFlows()
                    {
                        // TODO: Change this to use Authorization Code flow with PKCE
                        Implicit = new OpenApiOAuthFlow()
                        {
                            AuthorizationUrl = new Uri($"{identityUrlExternal}/connect/authorize"),
                            TokenUrl = new Uri($"{identityUrlExternal}/connect/token"),
                            Scopes = scopes,
                        }
                    }
                };

                document.Components ??= new();
                document.Components.SecuritySchemes.Add("oauth2", securityScheme);
                return Task.CompletedTask;
            }
        }
    }
}
