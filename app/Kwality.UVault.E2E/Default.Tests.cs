namespace Kwality.UVault.E2E;

using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Net.Http.Json;

using Kwality.UVault.Core.Extensions;
using Kwality.UVault.Core.Keys;
using Kwality.UVault.IAM.Extensions;
using Kwality.UVault.QA.Common.Xunit.Traits;
using Kwality.UVault.Users.Extensions;
using Kwality.UVault.Users.Managers;
using Kwality.UVault.Users.Models;
using Kwality.UVault.Users.Operations.Mappers;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestPlatform.TestHost;

using Xunit;

[E2E]
[Auth0]
[Collection("Auth0")]
[SuppressMessage("ReSharper", "MemberCanBeFileLocal")]
public sealed class DefaultTests
{
    private static IWebHostBuilder CreateWebHostBuilder()
    {
        return new WebHostBuilder().UseStartup<Program>()
                                   .ConfigureServices(static services =>
                                   {
                                       services.AddRouting();

                                       services.AddUVault(static options =>
                                       {
                                           options.UseIAM(static options => { options.UseDefault("", ""); });
                                           options.UseUserManagement<UserModel, StringKey>(ServiceLifetime.Singleton);
                                       });
                                   })
                                   .Configure(static app =>
                                   {
                                       app.UseUVault(static options => options.UseIAM());
                                       app.UseRouting();

                                       app.UseEndpoints(static builder =>
                                       {
                                           builder.MapPost("/api/v1/users/", static (
                                               UserManager<UserModel, StringKey> userManager, UserModel model,
                                               HttpContext context) =>
                                           {
                                               userManager.CreateAsync(model, new UserCreateOperationMapper());
                                               context.Response.StatusCode = (int)HttpStatusCode.OK;

                                               return Task.CompletedTask;
                                           });
                                       });
                                   });
    }

    /// <summary>
    ///     An E2E test which performs the following:
    ///     - Create a new user.
    ///     - Create the same user again.
    /// </summary>
    [Fact]
    public async Task RunAsync()
    {
        // ARRANGE.
        using var server = new TestServer(CreateWebHostBuilder());
        using HttpClient httpClient = server.CreateClient();

        // ACT.
        if (await CreateUserAsync(httpClient)
                .ConfigureAwait(true) != HttpStatusCode.OK)
        {
            throw new InvalidOperationException("Test failure: Was NOT able to create a user.");
        }

        if (await CreateUserAsync(httpClient)
                .ConfigureAwait(true) != HttpStatusCode.OK)
        {
            throw new InvalidOperationException("Test failure: Was NOT able to create a user.");
        }
    }

    private static async Task<HttpStatusCode> CreateUserAsync(HttpClient httpClient)
    {
        var userModel = new UserModel(new StringKey("UVAULT_E2E_USER"), "uvault@e2e.com", "Kwality", "UVault");
        using var json = JsonContent.Create(userModel);

        return (await httpClient.PostAsync(new Uri("/api/v1/users", UriKind.Relative), json)
                                .ConfigureAwait(true)).StatusCode;
    }

    private sealed class UserModel(StringKey key, string email, string firstName, string lastName)
        : UserModel<StringKey>(key, email)
    {
        public string FirstName { get; set; } = firstName;
        public string LastName { get; set; } = lastName;
    }
}
