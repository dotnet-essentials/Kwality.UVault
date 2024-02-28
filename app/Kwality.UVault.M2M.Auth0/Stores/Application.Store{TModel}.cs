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
namespace Kwality.UVault.M2M.Auth0.Stores;

using global::Auth0.ManagementApi;
using global::Auth0.ManagementApi.Models;
using global::Auth0.ManagementApi.Paging;

using JetBrains.Annotations;

using Kwality.UVault.Core.Auth0.API.Clients;
using Kwality.UVault.Core.Auth0.Configuration;
using Kwality.UVault.Core.Auth0.Keys;
using Kwality.UVault.Core.Exceptions;
using Kwality.UVault.Core.Models;
using Kwality.UVault.M2M.Auth0.Mapping.Abstractions;
using Kwality.UVault.M2M.Auth0.Models;
using Kwality.UVault.M2M.Operations.Filters.Abstractions;
using Kwality.UVault.M2M.Operations.Mappers.Abstractions;
using Kwality.UVault.M2M.Stores.Abstractions;

[UsedImplicitly]
internal sealed class ApplicationStore<TModel>(
    ManagementClient managementClient,
    ApiConfiguration apiConfiguration,
    IModelMapper<TModel> modelMapper) : IApplicationStore<TModel, StringKey>
    where TModel : ApplicationModel
{
    public async Task<PagedResultSet<TModel>> GetAllAsync(int pageIndex, int pageSize, IApplicationFilter? filter)
    {
        using ManagementApiClient apiClient = await this.CreateManagementApiClientAsync()
                                                        .ConfigureAwait(false);

        try
        {
            GetClientsRequest request = filter == null ? new GetClientsRequest() : filter.Create<GetClientsRequest>();

            IPagedList<Client>? clients = await apiClient
                                                .Clients.GetAllAsync(request,
                                                    new PaginationInfo(pageIndex, pageSize, true))
                                                .ConfigureAwait(false);

            IList<TModel> models = clients.Select(modelMapper.Map)
                                          .ToList();

            return new PagedResultSet<TModel>(models, clients.Paging.Total > (pageIndex + 1) * pageSize);
        }
        catch (Exception ex)
        {
            throw new ReadException("Failed to read applications.", ex);
        }
    }

    // Stryker disable once all
    public async Task<TModel> GetByKeyAsync(StringKey key)
    {
        using ManagementApiClient apiClient = await this.CreateManagementApiClientAsync()
                                                        .ConfigureAwait(false);

        try
        {
            Client? client = await apiClient.Clients.GetAsync(key.Value)
                                            .ConfigureAwait(false);

            return modelMapper.Map(client);
        }
        catch (Exception ex)
        {
            throw new ReadException($"Failed to read application: `{key}`.", ex);
        }
    }

    // Stryker disable once all
    public async Task<StringKey> CreateAsync(TModel model, IApplicationOperationMapper mapper)
    {
        using ManagementApiClient apiClient = await this.CreateManagementApiClientAsync()
                                                        .ConfigureAwait(false);

        try
        {
            Client client = await apiClient.Clients.CreateAsync(mapper.Create<TModel, ClientCreateRequest>(model))
                                           .ConfigureAwait(false);

            return new StringKey(client.ClientId);
        }
        catch (Exception ex)
        {
            throw new CreateException("Failed to create application.", ex);
        }
    }

    // Stryker disable once all
    public async Task UpdateAsync(StringKey key, TModel model, IApplicationOperationMapper mapper)
    {
        using ManagementApiClient apiClient = await this.CreateManagementApiClientAsync()
                                                        .ConfigureAwait(false);

        try
        {
            await apiClient.Clients.UpdateAsync(key.Value, mapper.Create<TModel, ClientUpdateRequest>(model))
                           .ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            throw new UpdateException($"Failed to update application: `{key}`.", ex);
        }
    }

    // Stryker disable once all
    public async Task DeleteByKeyAsync(StringKey key)
    {
        using ManagementApiClient apiClient = await this.CreateManagementApiClientAsync()
                                                        .ConfigureAwait(false);

        try
        {
            await apiClient.Clients.DeleteAsync(key.Value)
                           .ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            throw new UpdateException($"Failed to delete application: `{key}`.", ex);
        }
    }

    public async Task<TModel> RotateClientSecretAsync(StringKey key)
    {
        using ManagementApiClient apiClient = await this.CreateManagementApiClientAsync()
                                                        .ConfigureAwait(false);

        try
        {
            Client? client = await apiClient.Clients.RotateClientSecret(key.Value)
                                            .ConfigureAwait(false);

            return modelMapper.Map(client);
        }
        catch (Exception ex)
        {
            throw new UpdateException($"Failed to update application: `{key}`.", ex);
        }
    }

    private async Task<ManagementApiClient> CreateManagementApiClientAsync()
    {
        string managementApiToken = await managementClient.GetTokenAsync(apiConfiguration)
                                                          .ConfigureAwait(false);

        return new ManagementApiClient(managementApiToken, apiConfiguration.TokenEndpoint.Authority);
    }
}
