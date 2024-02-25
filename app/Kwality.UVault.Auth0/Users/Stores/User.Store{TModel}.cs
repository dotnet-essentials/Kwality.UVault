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
namespace Kwality.UVault.Auth0.Users.Stores;

using global::Auth0.ManagementApi;
using global::Auth0.ManagementApi.Models;

using JetBrains.Annotations;

using Kwality.UVault.Auth0.Configuration;
using Kwality.UVault.Auth0.Internal.API.Clients;
using Kwality.UVault.Auth0.Keys;
using Kwality.UVault.Auth0.Users.Mapping.Abstractions;
using Kwality.UVault.Auth0.Users.Models;
using Kwality.UVault.Exceptions;
using Kwality.UVault.Users.Operations.Mappers.Abstractions;
using Kwality.UVault.Users.Stores.Abstractions;

[UsedImplicitly]
internal sealed class UserStore<TModel>(
    ManagementClient managementClient,
    ApiConfiguration apiConfiguration,
    IModelMapper<TModel> modelMapper) : IUserStore<TModel, StringKey>
    where TModel : UserModel
{
    // Stryker disable once all
    public async Task<TModel> GetByKeyAsync(StringKey key)
    {
        using ManagementApiClient apiClient = await this.CreateManagementApiClientAsync()
                                                        .ConfigureAwait(false);

        try
        {
            User? user = await apiClient.Users.GetAsync(key.Value)
                                        .ConfigureAwait(false);

            return modelMapper.Map(user);
        }
        catch (Exception ex)
        {
            throw new ReadException($"Failed to read user: `{key}`.", ex);
        }
    }

    // Stryker disable once all
    public async Task<IEnumerable<TModel>> GetByEmailAsync(string email)
    {
        using ManagementApiClient apiClient = await this.CreateManagementApiClientAsync()
                                                        .ConfigureAwait(false);

        IList<User>? users = await apiClient.Users.GetUsersByEmailAsync(email)
                                            .ConfigureAwait(false);

        return users.Select(user => modelMapper.Map(user));
    }

    // Stryker disable once all
    public async Task<StringKey> CreateAsync(TModel model, IUserOperationMapper mapper)
    {
        using ManagementApiClient apiClient = await this.CreateManagementApiClientAsync()
                                                        .ConfigureAwait(false);

        try
        {
            User user = await apiClient.Users.CreateAsync(mapper.Create<TModel, UserCreateRequest>(model))
                                       .ConfigureAwait(false);

            return new StringKey(user.UserId);
        }
        catch (Exception ex)
        {
            throw new CreateException("Failed to create user.", ex);
        }
    }

    // Stryker disable once all
    public async Task UpdateAsync(StringKey key, TModel model, IUserOperationMapper mapper)
    {
        using ManagementApiClient apiClient = await this.CreateManagementApiClientAsync()
                                                        .ConfigureAwait(false);

        try
        {
            await apiClient.Users.UpdateAsync(key.Value, mapper.Create<TModel, UserUpdateRequest>(model))
                           .ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            throw new UpdateException($"Failed to update user: `{key}`.", ex);
        }
    }

    // Stryker disable once all
    public async Task DeleteByKeyAsync(StringKey key)
    {
        using ManagementApiClient apiClient = await this.CreateManagementApiClientAsync()
                                                        .ConfigureAwait(false);

        try
        {
            await apiClient.Users.DeleteAsync(key.Value)
                           .ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            throw new UpdateException($"Failed to delete user: `{key}`.", ex);
        }
    }

    private async Task<ManagementApiClient> CreateManagementApiClientAsync()
    {
        string managementApiToken = await managementClient.GetTokenAsync(apiConfiguration)
                                                          .ConfigureAwait(false);

        return new ManagementApiClient(managementApiToken, apiConfiguration.TokenEndpoint.Authority);
    }
}
