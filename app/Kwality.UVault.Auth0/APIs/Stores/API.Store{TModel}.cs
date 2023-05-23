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
namespace Kwality.UVault.Auth0.APIs.Stores;

using global::Auth0.ManagementApi;
using global::Auth0.ManagementApi.Models;

using JetBrains.Annotations;

using Kwality.UVault.APIs.Operations.Mappers.Abstractions;
using Kwality.UVault.APIs.Stores.Abstractions;
using Kwality.UVault.Auth0.APIs.Mapping.Abstractions;
using Kwality.UVault.Auth0.APIs.Models;
using Kwality.UVault.Auth0.Configuration;
using Kwality.UVault.Auth0.Internal.API.Clients;
using Kwality.UVault.Auth0.Keys;
using Kwality.UVault.Exceptions;

[UsedImplicitly]
internal sealed class ApiStore<TModel> : IApiStore<TModel, StringKey>
    where TModel : ApiModel
{
    private readonly ApiConfiguration apiConfiguration;
    private readonly ManagementClient managementClient;
    private readonly IModelMapper<TModel> modelMapper;

    public ApiStore(
        ManagementClient managementClient, ApiConfiguration apiConfiguration, IModelMapper<TModel> modelMapper)
    {
        this.managementClient = managementClient;
        this.apiConfiguration = apiConfiguration;
        this.modelMapper = modelMapper;
    }

    // Stryker disable once all
    public async Task<TModel> GetByKeyAsync(StringKey key)
    {
        using ManagementApiClient apiClient = await this.CreateManagementApiClientAsync()
                                                        .ConfigureAwait(false);

        try
        {
            ResourceServer? resourceServer = await apiClient.ResourceServers.GetAsync(key.Value)
                                                            .ConfigureAwait(false);

            return this.modelMapper.Map(resourceServer);
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
        string managementApiToken = await this.managementClient.GetTokenAsync(this.apiConfiguration)
                                              .ConfigureAwait(false);

        return new ManagementApiClient(managementApiToken, this.apiConfiguration.TokenEndpoint.Authority);
    }
}
