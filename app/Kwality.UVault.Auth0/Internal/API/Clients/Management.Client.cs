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
namespace Kwality.UVault.Auth0.Internal.API.Clients;

using global::System.Net.Http.Json;

using Kwality.UVault.Auth0.API.Models;
using Kwality.UVault.Auth0.Configuration;
using Kwality.UVault.Auth0.Exceptions;
using Kwality.UVault.Auth0.M2M.Configuration;
using Kwality.UVault.System.Abstractions;

internal sealed class ManagementClient
{
    private readonly IDateTimeProvider dateTimeProvider;
    private readonly HttpClient httpClient;
    private ApiManagementToken? lastRequestedManagementToken;

    public ManagementClient(HttpClient httpClient, IDateTimeProvider dateTimeProvider)
    {
        this.httpClient = httpClient;
        this.dateTimeProvider = dateTimeProvider;
    }

    public async Task<string> GetTokenAsync(ApiConfiguration configuration)
    {
        if (this.lastRequestedManagementToken is { AccessToken: not null, } &&
            !this.lastRequestedManagementToken.IsExpired(this.dateTimeProvider))
        {
            return this.lastRequestedManagementToken.AccessToken;
        }

        var data = new[]
        {
            new KeyValuePair<string, string>("grant_type", "client_credentials"),
            new KeyValuePair<string, string>("client_id", configuration.ClientId),
            new KeyValuePair<string, string>("client_secret", configuration.ClientSecret),
            new KeyValuePair<string, string>("audience", configuration.Audience),
        };

        using var formData = new FormUrlEncodedContent(data);

        HttpResponseMessage result = await this.GetOAuthTokenAsync(configuration.TokenEndpoint, formData)
                                               .ConfigureAwait(false);

        try
        {
            await EnsureHttpStatusCodeIsOkAsync(result, "Failed to retrieve an Auth0 `User Management` token.")
                .ConfigureAwait(false);

            this.lastRequestedManagementToken = await result.Content.ReadFromJsonAsync<ApiManagementToken>()
                                                            .ConfigureAwait(false);

            return string.IsNullOrEmpty(this.lastRequestedManagementToken?.AccessToken)
                ? throw new ManagementApiException("The `API Management Token / Access Token` token is `null`.")
                : this.lastRequestedManagementToken.AccessToken;
        }
        catch (Exception ex) when (ex is not ManagementApiException)
        {
            throw new ManagementApiException(
                "Failed to retrieve an Auth0 `User Management` token. Reason: Invalid HTTP response.", ex);
        }
    }

    public async Task<ApiManagementToken> GetM2MTokenAsync(
        M2MConfiguration configuration,
        string clientId,
        string clientSecret,
        string audience,
        string grantType)
    {
        var data = new[]
        {
            new KeyValuePair<string, string>("grant_type", grantType),
            new KeyValuePair<string, string>("client_id", clientId),
            new KeyValuePair<string, string>("client_secret", clientSecret),
            new KeyValuePair<string, string>("audience", audience),
        };

        using var formData = new FormUrlEncodedContent(data);

        HttpResponseMessage result = await this.GetOAuthTokenAsync(configuration.TokenEndpoint, formData)
                                               .ConfigureAwait(false);

        try
        {
            await EnsureHttpStatusCodeIsOkAsync(result, "Failed to retrieve an Auth0 `M2M` token.")
                .ConfigureAwait(false);

            ApiManagementToken? apiToken = await result.Content.ReadFromJsonAsync<ApiManagementToken>()
                                                       .ConfigureAwait(false);

            if (apiToken == null)
            {
                throw new ManagementApiException("The `M2M Token` token is `null`.");
            }

            return apiToken;
        }
        catch (Exception ex) when (ex is not ManagementApiException)
        {
            throw new ManagementApiException(
                "Failed to retrieve an Auth0 `M2M` token. Reason: Invalid HTTP response.", ex);
        }
    }

    private async Task<HttpResponseMessage> GetOAuthTokenAsync(Uri tokenEndpoint, HttpContent postData)
    {
        try
        {
            return await this.httpClient.PostAsync(tokenEndpoint, postData)
                             .ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            throw new ManagementApiException("Failed to retrieve an Auth0 `User Management` token.", ex);
        }
    }

    private static async Task EnsureHttpStatusCodeIsOkAsync(HttpResponseMessage result, string exceptionMessage)
    {
        if (!result.IsSuccessStatusCode)
        {
            string responseString = await result.Content.ReadAsStringAsync()
                                                .ConfigureAwait(false);

            if (string.IsNullOrEmpty(responseString))
            {
                throw new ManagementApiException($"{exceptionMessage} HTTP {(int)result.StatusCode}.");
            }

            throw new ManagementApiException($"{exceptionMessage} HTTP {(int)result.StatusCode}: `{responseString}`.");
        }
    }
}
