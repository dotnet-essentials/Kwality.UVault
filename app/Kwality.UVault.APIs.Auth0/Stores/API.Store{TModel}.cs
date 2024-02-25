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
namespace Kwality.UVault.APIs.Auth0.Stores;

using global::Auth0.ManagementApi;
using global::Auth0.ManagementApi.Models;

using JetBrains.Annotations;

using Kwality.UVault.APIs.Auth0.Keys;
using Kwality.UVault.APIs.Auth0.Mapping.Abstractions;
using Kwality.UVault.APIs.Auth0.Models;
using Kwality.UVault.APIs.Operations.Mappers.Abstractions;
using Kwality.UVault.APIs.Stores.Abstractions;
using Kwality.UVault.Core.Auth0.API.Clients;
using Kwality.UVault.Core.Auth0.Configuration;
using Kwality.UVault.Core.Exceptions;

[UsedImplicitly]
internal sealed class ApiStore<TModel>(
    ManagementClient client,
    ApiConfiguration configuration,
    IModelMapper<TModel> modelMapper) : IApiStore<TModel, StringKey>
    where TModel : ApiModel
{
    // Stryker disable once all
    public async Task<TModel> GetByKeyAsync(StringKey key)
    {
        using ManagementApiClient apiClient = await this.CreateManagementApiClientAsync()
                                                        .ConfigureAwait(false);

        try
        {
            ResourceServer? resourceServer = await apiClient.ResourceServers.GetAsync(key.Value)
                                                            .ConfigureAwait(false);

            return modelMapper.Map(resourceServer);
        }
        catch (Exception ex)
        {
            throw new ReadException($"Failed to read API: `{key}`.", ex);
        }
    }

    // Stryker disable once all
    public async Task<StringKey> CreateAsync(TModel model, IApiOperationMapper mapper)
    {
        using ManagementApiClient apiClient = await this.CreateManagementApiClientAsync()
                                                        .ConfigureAwait(false);

        try
        {
            ResourceServer resourceServer = await apiClient
                                                  .ResourceServers.CreateAsync(
                                                      mapper.Create<TModel, ResourceServerCreateRequest>(model))
                                                  .ConfigureAwait(false);

            return new StringKey(resourceServer.Id);
        }
        catch (Exception ex)
        {
            throw new CreateException("Failed to create API.", ex);
        }
    }

    // Stryker disable once all
    public async Task DeleteByKeyAsync(StringKey key)
    {
        using ManagementApiClient apiClient = await this.CreateManagementApiClientAsync()
                                                        .ConfigureAwait(false);

        try
        {
            await apiClient.ResourceServers.DeleteAsync(key.Value)
                           .ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            throw new UpdateException($"Failed to delete API: `{key}`.", ex);
        }
    }

    private async Task<ManagementApiClient> CreateManagementApiClientAsync()
    {
        string managementApiToken = await client.GetTokenAsync(configuration)
                                                .ConfigureAwait(false);

        return new ManagementApiClient(managementApiToken, configuration.TokenEndpoint.Authority);
    }
}
