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
namespace Kwality.UVault.QA.Internal.Auth0.Factories;

using global::System.Diagnostics.CodeAnalysis;
using global::System.Net.Http.Json;

using Kwality.UVault.QA.Internal.Auth0.Models;
using Kwality.UVault.QA.Internal.System;

[ExcludeFromCodeCoverage]
internal static class Auth0AuthenticationFactory
{
    public static async Task<string> RequestAsync()
    {
        string endpoint = Environment.ReadString("AUTH0_TOKEN_ENDPOINT");
        string userName = Environment.ReadString("AUTH0_USERNAME");
        string password = Environment.ReadString("AUTH0_PASSWORD");
        string audience = Environment.ReadString("AUTH0_AUDIENCE");
        string clientId = Environment.ReadString("AUTH0_CLIENT_ID");
        string clientSecret = Environment.ReadString("AUTH0_CLIENT_SECRET");
        using var httpClient = new HttpClient();

        var data = new[]
        {
            new KeyValuePair<string, string>("grant_type", "password"),
            new KeyValuePair<string, string>("username", userName),
            new KeyValuePair<string, string>("password", password),
            new KeyValuePair<string, string>("audience", audience),
            new KeyValuePair<string, string>("client_id", clientId),
            new KeyValuePair<string, string>("client_secret", clientSecret),
        };

        using var formUrlEncodedContent = new FormUrlEncodedContent(data);

        HttpResponseMessage responseMessage = await httpClient.PostAsync(new Uri(endpoint), formUrlEncodedContent)
                                                              .ConfigureAwait(false);

        if (!responseMessage.IsSuccessStatusCode)
        {
            string response = await responseMessage.Content.ReadAsStringAsync()
                                                   .ConfigureAwait(false);

            throw new NotSupportedException($"Failed to retrieve an Auth0 access token. HTTP Response: {response}");
        }

        Auth0AuthenticationResponse? responseModel = await responseMessage
                                                           .Content.ReadFromJsonAsync<Auth0AuthenticationResponse>()
                                                           .ConfigureAwait(false);

        return responseModel?.AccessToken ?? string.Empty;
    }
}
