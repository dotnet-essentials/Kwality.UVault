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

using global::System.Diagnostics.CodeAnalysis;

using JetBrains.Annotations;

using Kwality.UVault.Auth0.API.Models;
using Kwality.UVault.Auth0.Keys;
using Kwality.UVault.Auth0.M2M.Configuration;
using Kwality.UVault.Auth0.M2M.Extensions;
using Kwality.UVault.Auth0.M2M.Mapping.Abstractions;
using Kwality.UVault.Auth0.M2M.Models;
using Kwality.UVault.M2M.Managers;
using Kwality.UVault.M2M.Models;
using Kwality.UVault.QA.Internal.Factories;
using Kwality.UVault.QA.Internal.System;
using Kwality.UVault.QA.Internal.Xunit.Traits;

using Xunit;

public sealed class ApplicationTokenManagementAuth0Tests
{
    [M2MTokenManagement]
    [Fact(DisplayName = "Get access token succeeds.")]
    internal async Task GetToken_Succeeds()
    {
        // ARRANGE.
        M2MConfiguration configuration = GetM2MConfiguration();

        ApplicationTokenManager<AccessTokenModel, ApplicationModel, StringKey> manager
            = new ApplicationTokenManagerFactory().Create<AccessTokenModel, ApplicationModel, StringKey>(
                options =>
                    options.UseAuth0Store<AccessTokenModel, ApplicationModel, TokenModelMapper>(configuration));

        ApplicationModel appModel = GetApplicationModel();
        string audience = GetAudience();
        const string grantType = "client_credentials";

        // ACT.
        AccessTokenModel result = await manager.GetAccessTokenAsync(appModel, audience, grantType)
                                               .ConfigureAwait(false);

        // ASSERT.
        result.Token.Should()
              .NotBeNullOrWhiteSpace();

        result.TokenType.Should()
              .NotBeNullOrWhiteSpace();

        result.ExpiresIn.Should()
              .Be(86400);
    }

#pragma warning disable CA1812 // "Avoid uninstantiated internal classes".
    [UsedImplicitly]
    internal sealed class AccessTokenModel : TokenModel
#pragma warning restore CA1812
    {
        public AccessTokenModel(string? token, string tokenType, int expiresIn)
        {
            this.Token = token;
            this.TokenType = tokenType;
            this.ExpiresIn = expiresIn;
        }

        public string? Token { get; }
        public string TokenType { get; }
        public int ExpiresIn { get; }
    }

    [SuppressMessage("ReSharper", "ClassNeverInstantiated.Local")]
    private sealed class TokenModelMapper : IModelTokenMapper<AccessTokenModel>
    {
        public AccessTokenModel Map(ApiManagementToken token)
        {
            return new AccessTokenModel(token.AccessToken, token.TokenType, token.ExpiresIn);
        }
    }

    private static ApplicationModel GetApplicationModel()
    {
        string clientId = Environment.ReadString("AUTH0_CLIENT_ID");
        string clientSecret = Environment.ReadString("AUTH0_CLIENT_SECRET");

        return new ApplicationModel(new StringKey(clientId))
        {
            Name = clientId,
            ClientSecret = clientSecret,
        };
    }

    private static string GetAudience()
    {
        return Environment.ReadString("AUTH0_AUDIENCE");
    }

    private static M2MConfiguration GetM2MConfiguration()
    {
        return new M2MConfiguration(new Uri(Environment.ReadString("AUTH0_TOKEN_ENDPOINT")));
    }
}
