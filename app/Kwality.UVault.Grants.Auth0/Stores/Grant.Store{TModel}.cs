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
namespace Kwality.UVault.Grants.Auth0.Stores;

using global::Auth0.ManagementApi;
using global::Auth0.ManagementApi.Models;
using global::Auth0.ManagementApi.Paging;

using JetBrains.Annotations;

using Kwality.UVault.Core.Auth0.API.Clients;
using Kwality.UVault.Core.Auth0.Configuration;
using Kwality.UVault.Core.Exceptions;
using Kwality.UVault.Core.Models;
using Kwality.UVault.Grants.Auth0.Keys;
using Kwality.UVault.Grants.Auth0.Mapping.Abstractions;
using Kwality.UVault.Grants.Auth0.Models;
using Kwality.UVault.Grants.Operations.Filters.Abstractions;
using Kwality.UVault.Grants.Operations.Mappers.Abstractions;
using Kwality.UVault.Grants.Stores.Abstractions;

[UsedImplicitly]
internal sealed class GrantStore<TModel>(
    ManagementClient managementClient,
    ApiConfiguration apiConfiguration,
    IModelMapper<TModel> modelMapper) : IGrantStore<TModel, StringKey>
    where TModel : GrantModel
{
    public async Task<PagedResultSet<TModel>> GetAllAsync(int pageIndex, int pageSize, IGrantFilter? filter)
    {
        using ManagementApiClient apiClient = await this.CreateManagementApiClientAsync()
                                                        .ConfigureAwait(false);

        try
        {
            GetClientGrantsRequest request
                = filter == null ? new GetClientGrantsRequest() : filter.Create<GetClientGrantsRequest>();

            IPagedList<ClientGrant>? clientGrants = await apiClient
                                                          .ClientGrants.GetAllAsync(request,
                                                              new PaginationInfo(pageIndex, pageSize, true))
                                                          .ConfigureAwait(false);

            IList<TModel> models = clientGrants.Select(modelMapper.Map)
                                               .ToList();

            return new PagedResultSet<TModel>(models, clientGrants.Paging.Total > (pageIndex + 1) * pageSize);
        }
        catch (Exception ex)
        {
            throw new ReadException("Failed to read client grants.", ex);
        }
    }

    // Stryker disable once all
    public async Task<StringKey> CreateAsync(TModel model, IGrantOperationMapper mapper)
    {
        using ManagementApiClient apiClient = await this.CreateManagementApiClientAsync()
                                                        .ConfigureAwait(false);

        try
        {
            ClientGrant clientGrant = await apiClient
                                            .ClientGrants.CreateAsync(
                                                mapper.Create<TModel, ClientGrantCreateRequest>(model))
                                            .ConfigureAwait(false);

            return new StringKey(clientGrant.Id);
        }
        catch (Exception ex)
        {
            throw new CreateException("Failed to create client grant.", ex);
        }
    }

    // Stryker disable once all
    public async Task UpdateAsync(StringKey key, TModel model, IGrantOperationMapper mapper)
    {
        using ManagementApiClient apiClient = await this.CreateManagementApiClientAsync()
                                                        .ConfigureAwait(false);

        try
        {
            await apiClient.ClientGrants.UpdateAsync(key.Value, mapper.Create<TModel, ClientGrantUpdateRequest>(model))
                           .ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            throw new UpdateException($"Failed to update client grant: `{key}`.", ex);
        }
    }

    // Stryker disable once all
    public async Task DeleteByKeyAsync(StringKey key)
    {
        using ManagementApiClient apiClient = await this.CreateManagementApiClientAsync()
                                                        .ConfigureAwait(false);

        try
        {
            await apiClient.ClientGrants.DeleteAsync(key.Value)
                           .ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            throw new UpdateException($"Failed to delete client grant: `{key}`.", ex);
        }
    }

    private async Task<ManagementApiClient> CreateManagementApiClientAsync()
    {
        string managementApiToken = await managementClient.GetTokenAsync(apiConfiguration)
                                                          .ConfigureAwait(false);

        return new ManagementApiClient(managementApiToken, apiConfiguration.TokenEndpoint.Authority);
    }
}
