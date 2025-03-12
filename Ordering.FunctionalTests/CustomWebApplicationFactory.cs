using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.VisualStudio.TestPlatform.TestHost;

namespace Ordering.FunctionalTests
{
    public class CustomWebApplicationFactory<TProgram>: WebApplicationFactory<TProgram>
        where TProgram: class
    {

        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.ConfigureServices((services) =>
            {


            });
        }
    }
}
