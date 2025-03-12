using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;

namespace eShop.ServiceDefaults
{
    public static class HttpClientExtensions
    {

        private class HttpClientAuthorizationDelegatingHandler: DelegatingHandler
        {
            private readonly IHttpContextAccessor _httpContextAccessor;

            public HttpClientAuthorizationDelegatingHandler(IHttpContextAccessor httpContextAccessor)
            {
                _httpContextAccessor = httpContextAccessor;
            }

            public HttpClientAuthorizationDelegatingHandler(IHttpContextAccessor httpContextAccessor, HttpMessageHandler innerHandler):base(innerHandler) 
            {
                _httpContextAccessor = httpContextAccessor;
            }

            protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
            {
                if (_httpContextAccessor.HttpContext is HttpContext context)
                {
                    var accessToken = await context.GetTokenAsync("access_token");

                    if (accessToken is not null)
                    {
                        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
                    }
                }

                return await base.SendAsync(request, cancellationToken);
            }

        }
    }
}
