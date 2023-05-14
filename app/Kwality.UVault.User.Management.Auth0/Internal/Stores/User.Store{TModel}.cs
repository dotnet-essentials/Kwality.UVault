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
namespace Kwality.UVault.User.Management.Auth0.Internal.Stores;

using global::Auth0.ManagementApi;
using global::Auth0.ManagementApi.Models;

using JetBrains.Annotations;

using Kwality.UVault.User.Management.Auth0.Configuration;
using Kwality.UVault.User.Management.Auth0.Internal.API.Clients;
using Kwality.UVault.User.Management.Auth0.Keys;
using Kwality.UVault.User.Management.Auth0.Mapping.Abstractions;
using Kwality.UVault.User.Management.Auth0.Models;
using Kwality.UVault.User.Management.Exceptions;
using Kwality.UVault.User.Management.Operations.Mappers.Abstractions;
using Kwality.UVault.User.Management.Stores.Abstractions;

[UsedImplicitly]
internal sealed class UserStore<TModel> : IUserStore<TModel, StringKey>
    where TModel : UserModel
{
    private readonly ApiConfiguration apiConfiguration;
    private readonly ManagementClient managementClient;
    private readonly IModelMapper<TModel> modelMapper;

    public UserStore(
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
            User? user = await apiClient.Users.GetAsync(key.Value)
                                        .ConfigureAwait(false);

            return this.modelMapper.Map(user);
        }
        catch (Exception ex)
        {
            throw new UserNotFoundException($"User with key `{key}` NOT found.", ex);
        }
    }

    // Stryker disable once all
    public async Task<IEnumerable<TModel>> GetByEmailAsync(string email)
    {
        using ManagementApiClient apiClient = await this.CreateManagementApiClientAsync()
                                                        .ConfigureAwait(false);

        IList<User>? users = await apiClient.Users.GetUsersByEmailAsync(email)
                                            .ConfigureAwait(false);

        return users.Select(user => this.modelMapper.Map(user));
    }

    // Stryker disable once all
    public async Task<StringKey> CreateAsync(TModel model, IUserOperationMapper operationMapper)
    {
        using ManagementApiClient apiClient = await this.CreateManagementApiClientAsync()
                                                        .ConfigureAwait(false);

        try
        {
            User user = await apiClient.Users.CreateAsync(operationMapper.Create<TModel, UserCreateRequest>(model))
                                       .ConfigureAwait(false);

            return new StringKey(user.UserId);
        }
        catch (Exception ex)
        {
            throw new UserCreationException("An error occured during the creation of the user.", ex);
        }
    }

    // Stryker disable once all
    public async Task DeleteByKeyAsync(StringKey key)
    {
        using ManagementApiClient apiClient = await this.CreateManagementApiClientAsync()
                                                        .ConfigureAwait(false);

        await apiClient.Users.DeleteAsync(key.Value)
                       .ConfigureAwait(false);
    }

    // Stryker disable once all
    public async Task UpdateAsync(StringKey key, TModel model, IUserOperationMapper operationMapper)
    {
        using ManagementApiClient apiClient = await this.CreateManagementApiClientAsync()
                                                        .ConfigureAwait(false);

        try
        {
            await apiClient.Users.UpdateAsync(key.Value, operationMapper.Create<TModel, UserUpdateRequest>(model))
                           .ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            throw new UserNotFoundException($"User with key `{key.Value}` NOT found.", ex);
        }
    }

    private async Task<ManagementApiClient> CreateManagementApiClientAsync()
    {
        string managementApiToken = await this.managementClient.GetTokenAsync(this.apiConfiguration)
                                              .ConfigureAwait(false);

        return new ManagementApiClient(managementApiToken, this.apiConfiguration.TokenEndpoint.Authority);
    }
}
