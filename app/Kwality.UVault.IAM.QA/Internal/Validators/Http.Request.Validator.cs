// =====================================================================================================================
// = LICENSE:       Copyright (c) 2023 Kevin De Coninck
// =
// =                Permission is hereby granted, free of charge, to any person
// =                obtaining a copy of this software and associated documentation
// =                files (the "Software"), to deal in the Software without
// =                restriction, including without limitation the rights to use,
// =                copy, modify, merge, publish, distribute, sublicense, and/or sell
// =                copies of the Software, and to permit persons to whom the
// =                Software is furnished to do so, subject to the following
// =                conditions:
// =
// =                The above copyright notice and this permission notice shall be
// =                included in all copies or substantial portions of the Software.
// =
// =                THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// =                EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES
// =                OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// =                NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT
// =                HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY,
// =                WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING
// =                FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR
// =                OTHER DEALINGS IN THE SOFTWARE.
// =====================================================================================================================
namespace Kwality.UVault.IAM.QA.Internal.Validators;

using System.Net;

using FluentAssertions;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestPlatform.TestHost;

internal sealed class HttpRequestValidator
{
    public Action<IServiceCollection>? ConfigureServices { get; init; }
    public Action<IApplicationBuilder>? ConfigureApp { get; init; }
    public Action<IEndpointRouteBuilder>? ConfigureRoutes { get; init; }
    public HttpStatusCode ExpectedHttpStatusCode { get; init; }
    public string? Jwt { get; init; }

    public async Task SendHttpRequestAsync(string endpoint)
    {
        using var testServer = new TestServer(new WebHostBuilder().UseStartup<Program>()
                                                                  .ConfigureServices(services =>
                                                                  {
                                                                      // Required services.
                                                                      services.AddRouting();

                                                                      // Optional services.
                                                                      this.ConfigureServices?.Invoke(services);
                                                                  })
                                                                  .Configure(app =>
                                                                  {
                                                                      // Required middleware.
                                                                      app.UseRouting();

                                                                      // Optional middleware.
                                                                      this.ConfigureApp?.Invoke(app);

                                                                      // Map HTTP routes.
                                                                      app.UseEndpoints(endpoints =>
                                                                          this.ConfigureRoutes?.Invoke(endpoints));
                                                                  }));

        HttpClient httpClient = testServer.CreateClient();

        if (this.Jwt != null)
        {
            httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {this.Jwt}");
        }

        // ACT.
        HttpResponseMessage result = await httpClient.GetAsync(new Uri(endpoint))
                                                     .ConfigureAwait(false);

        // ASSERT.
        result.Should()
              .HaveStatusCode(this.ExpectedHttpStatusCode);
    }
}
