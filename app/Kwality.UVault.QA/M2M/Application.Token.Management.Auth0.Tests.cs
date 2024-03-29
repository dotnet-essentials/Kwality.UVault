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
namespace Kwality.UVault.QA.M2M;

using FluentAssertions;

using global::Auth0.ManagementApi.Models;

using global::System.Diagnostics.CodeAnalysis;

using JetBrains.Annotations;

using Kwality.UVault.Auth0.Configuration;
using Kwality.UVault.Auth0.Keys;
using Kwality.UVault.Auth0.M2M.Configuration;
using Kwality.UVault.Auth0.M2M.Extensions;
using Kwality.UVault.Auth0.M2M.Mapping.Abstractions;
using Kwality.UVault.Auth0.M2M.Models;
using Kwality.UVault.Auth0.Models;
using Kwality.UVault.Exceptions;
using Kwality.UVault.M2M.Managers;
using Kwality.UVault.QA.Internal.Factories;
using Kwality.UVault.QA.Internal.System;
using Kwality.UVault.QA.Internal.Xunit.Traits;

using Xunit;

// ReSharper disable once MemberCanBeFileLocal
[Collection("Auth0")]
public sealed class ApplicationTokenManagementAuth0Tests
{
    [M2MTokenManagement]
    [Fact(DisplayName = "Get access token (for an application with permissions) succeeds.")]
    internal async Task GetToken_ApplicationWithPermission_Succeeds()
    {
        // ARRANGE.
        ApiConfiguration apiConfiguration = GetApiConfiguration();
        M2MConfiguration configuration = GetM2MConfiguration();
        ApplicationManager<Model, StringKey> applicationManager = new ApplicationManagerFactory().Create<Model, StringKey>(options => options.UseAuth0Store<Model, ModelMapper>(apiConfiguration));
        ApplicationTokenManager<TokenModel> applicationTokenManager = new ApplicationTokenManagerFactory().Create<TokenModel, ApplicationModel, StringKey>(options => options.UseAuth0Store<TokenModel, TokenModelMapper>(configuration));

        // To ensure that we don't Auth0's "Rate Limit", we wait for 2 seconds before executing this test.
        Thread.Sleep(TimeSpan.FromSeconds(2));

        Model application = await applicationManager.GetByKeyAsync(new StringKey(Environment.ReadString("AUTH0_CLIENT_ID")))
                                                    .ConfigureAwait(false);

        // ACT.
        // To ensure that we don't Auth0's "Rate Limit", we wait for 2 seconds before executing this test.
        Thread.Sleep(TimeSpan.FromSeconds(2));

        TokenModel result = await applicationTokenManager.GetAccessTokenAsync(application.Key.ToString(), application.ClientSecret ?? string.Empty, Environment.ReadString("AUTH0_AUDIENCE"), "client_credentials")
                                                         .ConfigureAwait(false);

        // ASSERT.
        result.Token.Should()
              .NotBeNullOrWhiteSpace();

        result.Scope.Should()
              .NotBeNullOrWhiteSpace();

        result.TokenType.Should()
              .Be("Bearer");

        result.ExpiresIn.Should()
              .Be(86400);
    }

    [M2MTokenManagement]
    [Fact(DisplayName = "Get access token (for an application without permissions) succeeds.")]
    internal async Task GetToken_ApplicationWithoutPermission_Fails()
    {
        // ARRANGE.
        ApiConfiguration apiConfiguration = GetApiConfiguration();
        M2MConfiguration configuration = GetM2MConfiguration();
        ApplicationManager<Model, StringKey> applicationManager = new ApplicationManagerFactory().Create<Model, StringKey>(options => options.UseAuth0Store<Model, ModelMapper>(apiConfiguration));
        ApplicationTokenManager<TokenModel> applicationTokenManager = new ApplicationTokenManagerFactory().Create<TokenModel, ApplicationModel, StringKey>(options => options.UseAuth0Store<TokenModel, TokenModelMapper>(configuration));

        // To ensure that we don't Auth0's "Rate Limit", we wait for 2 seconds before executing this test.
        Thread.Sleep(TimeSpan.FromSeconds(2));

        Model application = await applicationManager.GetByKeyAsync(new StringKey(Environment.ReadString("AUTH0_TEST_APPLICATION_1_CLIENT_ID")))
                                                    .ConfigureAwait(false);

        // ACT.
        // To ensure that we don't Auth0's "Rate Limit", we wait for 2 seconds before executing this test.
        Thread.Sleep(TimeSpan.FromSeconds(2));
        Func<Task<TokenModel>> act = () => applicationTokenManager.GetAccessTokenAsync(application.Key.ToString(), application.ClientSecret ?? string.Empty, Environment.ReadString("AUTH0_AUDIENCE"), "client_credentials");

        // ASSERT.
        await act.Should()
                 .ThrowAsync<ReadException>()
                 .WithMessage("Failed to retrieve an access token.")
                 .ConfigureAwait(false);
    }

    [M2MTokenManagement]
    [Theory(DisplayName = "Get access token is failed when client secret is null.")]
    [InlineData(null, "clientSecret", "audience", "grantType")]
    [InlineData("clientId", null, "audience", "grantType")]
    [InlineData("clientId", "clientSecret", null, "grantType")]
    [InlineData("clientId", "clientSecret", "audience", null)]
    internal async Task GetToken_InvalidArguments_Fails(string clientId, string clientSecret, string audience, string grantType)
    {
        // ARRANGE.
        M2MConfiguration configuration = GetM2MConfiguration();
        ApplicationTokenManager<TokenModel> applicationTokenManager = new ApplicationTokenManagerFactory().Create<TokenModel, ApplicationModel, StringKey>(options => options.UseAuth0Store<TokenModel, TokenModelMapper>(configuration));

        // ACT.
        Func<Task<TokenModel>> act = async () => await applicationTokenManager.GetAccessTokenAsync(clientId, clientSecret, audience, grantType)
                                                                              .ConfigureAwait(false);

        // ASSERT.
        await act.Should()
                 .ThrowAsync<ReadException>()
                 .WithMessage("Failed to retrieve an access token.")
                 .WithInnerException(typeof(ArgumentNullException))
                 .ConfigureAwait(false);
    }

    private static ApiConfiguration GetApiConfiguration()
    {
        return new ApiConfiguration(new Uri(Environment.ReadString("AUTH0_TOKEN_ENDPOINT")), Environment.ReadString("AUTH0_CLIENT_ID"), Environment.ReadString("AUTH0_CLIENT_SECRET"), Environment.ReadString("AUTH0_AUDIENCE"));
    }

    private static M2MConfiguration GetM2MConfiguration()
    {
        return new M2MConfiguration(new Uri(Environment.ReadString("AUTH0_TOKEN_ENDPOINT")));
    }

    private sealed class Model : ApplicationModel
    {
        public Model(StringKey name)
            : base(name)
        {
            this.Name = name.Value;
        }
    }

#pragma warning disable CA1812 // "Avoid uninstantiated internal classes".
    [UsedImplicitly]
    private sealed class ModelMapper : IModelMapper<Model>
#pragma warning restore CA1812
    {
        public Model Map(Client client)
        {
            return new Model(new StringKey(client.ClientId))
            {
                Name = client.Name,
                ClientSecret = client.ClientSecret,
            };
        }
    }

#pragma warning disable CA1812 // "Avoid uninstantiated internal classes".
    [UsedImplicitly]
    internal sealed class TokenModel : UVault.M2M.Models.TokenModel
#pragma warning restore CA1812
    {
        public TokenModel()
        {
        }

        public TokenModel(string? token, string tokenType, int expiresIn, string scope)
        {
            this.Token = token;
            this.TokenType = tokenType;
            this.ExpiresIn = expiresIn;
            this.Scope = scope;
        }
    }

    [SuppressMessage("ReSharper", "ClassNeverInstantiated.Local")]
    private sealed class TokenModelMapper : IModelTokenMapper<TokenModel>
    {
        public TokenModel Map(ApiManagementToken token)
        {
            return new TokenModel(token.AccessToken, token.TokenType, token.ExpiresIn, token.Scope);
        }
    }
}
