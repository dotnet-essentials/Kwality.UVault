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
namespace Kwality.UVault.Auth0.M2M.Stores;

using JetBrains.Annotations;

using Kwality.UVault.Auth0.Internal.API.Clients;
using Kwality.UVault.Auth0.M2M.Configuration;
using Kwality.UVault.Auth0.M2M.Mapping.Abstractions;
using Kwality.UVault.Auth0.Models;
using Kwality.UVault.Exceptions;
using Kwality.UVault.M2M.Models;
using Kwality.UVault.M2M.Stores.Abstractions;

[UsedImplicitly]
internal sealed class ApplicationTokenStore<TToken>(
    ManagementClient managementClient,
    M2MConfiguration m2MConfiguration,
    IModelTokenMapper<TToken> modelMapper) : IApplicationTokenStore<TToken>
    where TToken : TokenModel
{
    public async Task<TToken> GetAccessTokenAsync(
        string clientId, string clientSecret, string audience, string grantType)
    {
        try
        {
            ArgumentNullException.ThrowIfNull(clientId);
            ArgumentNullException.ThrowIfNull(clientSecret);
            ArgumentNullException.ThrowIfNull(audience);
            ArgumentNullException.ThrowIfNull(grantType);

            ApiManagementToken managementApiToken = await managementClient.GetM2MTokenAsync(
                                                                  m2MConfiguration.TokenEndpoint, grantType,
                                                                  clientId, clientSecret, audience)
                                                              .ConfigureAwait(false);

            return modelMapper.Map(managementApiToken);
        }
        catch (Exception ex)
        {
            throw new ReadException("Failed to retrieve an access token.", ex);
        }
    }
}
