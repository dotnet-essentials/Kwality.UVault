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
namespace Kwality.UVault.Core.Auth0.API.Clients;

using global::System.Net.Http.Json;

using Kwality.UVault.Core.Auth0.Configuration;
using Kwality.UVault.Core.Auth0.Exceptions;
using Kwality.UVault.Core.Auth0.Models;
using Kwality.UVault.Core.System.Abstractions;

public sealed class ManagementClient(HttpClient httpClient, IDateTimeProvider dateTimeProvider)
{
    private ApiManagementToken? lastRequestedManagementToken;

    public async Task<string> GetTokenAsync(ApiConfiguration apiConfiguration)
    {
        if (this.lastRequestedManagementToken is { AccessToken: not null } &&
            !this.lastRequestedManagementToken.IsExpired(dateTimeProvider))
        {
            return this.lastRequestedManagementToken.AccessToken;
        }

        this.lastRequestedManagementToken = await this.GetTokenAsync(apiConfiguration.TokenEndpoint,
                                                          "client_credentials", apiConfiguration.ClientId,
                                                          apiConfiguration.ClientSecret, apiConfiguration.Audience)
                                                      .ConfigureAwait(false);

        return string.IsNullOrEmpty(this.lastRequestedManagementToken?.AccessToken)
            ? throw new ManagementApiException("The `API Management Token / Access Token` is `null`.")
            : this.lastRequestedManagementToken.AccessToken;
    }

    public async Task<ApiManagementToken> GetM2MTokenAsync(
        Uri tokenEndpoint, string grantType, string clientId, string clientSecret, string audience)
    {
        ApiManagementToken? token = await this.GetTokenAsync(tokenEndpoint, grantType, clientId, clientSecret, audience)
                                              .ConfigureAwait(false);

        if (token == null)
        {
            throw new ManagementApiException("The Auth0 access token is `null`.");
        }

        return token;
    }

    private async Task<ApiManagementToken?> GetTokenAsync(
        Uri tokenEndpoint, string grantType, string clientId, string clientSecret, string audience)
    {
        var data = new[]
        {
            new KeyValuePair<string, string>("grant_type", grantType),
            new KeyValuePair<string, string>("client_id", clientId),
            new KeyValuePair<string, string>("client_secret", clientSecret),
            new KeyValuePair<string, string>("audience", audience),
        };

        using var formData = new FormUrlEncodedContent(data);

        HttpResponseMessage result = await this.GetOAuthTokenAsync(tokenEndpoint, formData)
                                               .ConfigureAwait(false);

        try
        {
            await EnsureHttpStatusCodeIsOkAsync(result, "Failed to retrieve an Auth0 token.")
                .ConfigureAwait(false);

            return await result.Content.ReadFromJsonAsync<ApiManagementToken>()
                               .ConfigureAwait(false);
        }
        catch (Exception ex) when (ex is not ManagementApiException)
        {
            throw new ManagementApiException("Failed to retrieve an Auth0 token. Reason: Invalid HTTP response.", ex);
        }
    }

    private async Task<HttpResponseMessage> GetOAuthTokenAsync(Uri tokenEndpoint, HttpContent postData)
    {
        try
        {
            return await httpClient.PostAsync(tokenEndpoint, postData)
                                   .ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            throw new ManagementApiException("Failed to retrieve an Auth0 token.", ex);
        }
    }

    private static async Task EnsureHttpStatusCodeIsOkAsync(HttpResponseMessage result, string exceptionMessage)
    {
        if (!result.IsSuccessStatusCode)
        {
            string responseString = await result.Content.ReadAsStringAsync()
                                                .ConfigureAwait(false);

            var errorApiException = new TokenRequestException(result.StatusCode, responseString);

            throw new ManagementApiException(exceptionMessage, errorApiException);
        }
    }
}
