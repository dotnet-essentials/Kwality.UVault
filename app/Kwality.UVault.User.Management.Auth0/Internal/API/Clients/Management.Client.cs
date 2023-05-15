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
namespace Kwality.UVault.User.Management.Auth0.Internal.API.Clients;

using System.Net.Http.Json;

using Kwality.UVault.User.Management.Auth0.Configuration;
using Kwality.UVault.User.Management.Auth0.Exceptions;
using Kwality.UVault.User.Management.Auth0.Internal.API.Models;

internal sealed class ManagementClient
{
    private readonly HttpClient httpClient;

    public ManagementClient(HttpClient httpClient)
    {
        this.httpClient = httpClient;
    }

    public async Task<string> GetTokenAsync(ApiConfiguration apiConfiguration)
    {
        var data = new[]
        {
            new KeyValuePair<string, string>("grant_type", "client_credentials"),
            new KeyValuePair<string, string>("client_id", apiConfiguration.ClientId),
            new KeyValuePair<string, string>("client_secret", apiConfiguration.ClientSecret),
            new KeyValuePair<string, string>("audience", apiConfiguration.Audience),
        };

        using var formData = new FormUrlEncodedContent(data);

        HttpResponseMessage result = await this.GetOAuthTokenAsync(apiConfiguration.TokenEndpoint, formData)
                                               .ConfigureAwait(false);

        try
        {
            await EnsureHttpStatusCodeIsOkAsync(result, "Failed to retrieve an Auth0 `User Management` token.")
                .ConfigureAwait(false);

            ApiManagementToken? managementToken = await result.Content.ReadFromJsonAsync<ApiManagementToken>()
                                                              .ConfigureAwait(false);

            return string.IsNullOrEmpty(managementToken?.AccessToken)
                ? throw new ManagementApiException("The `API Management Token / Access Token` token is `null`.")
                : managementToken.AccessToken;
        }
        catch (Exception ex) when (ex is not ManagementApiException)
        {
            throw new ManagementApiException(
                "Failed to retrieve an Auth0 `User Management` token. Reason: Invalid HTTP response.", ex);
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
