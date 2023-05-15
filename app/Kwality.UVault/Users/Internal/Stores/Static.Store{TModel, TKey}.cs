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
namespace Kwality.UVault.Users.Internal.Stores;

using Kwality.UVault.Users.Exceptions;
using Kwality.UVault.Users.Models;
using Kwality.UVault.Users.Operations.Mappers.Abstractions;
using Kwality.UVault.Users.Stores.Abstractions;

internal sealed class StaticStore<TModel, TKey> : IUserStore<TModel, TKey>
    where TModel : UserModel<TKey>
    where TKey : IEqualityComparer<TKey>
{
    private readonly IList<TModel> userCollection = new List<TModel>();

    public Task<TModel> GetByKeyAsync(TKey key)
    {
        TModel? user = this.userCollection.FirstOrDefault(x => x.Key.Equals(key));

        if (user == null)
        {
            throw new UserNotFoundException($"User with key `{key}` NOT found.");
        }

        return Task.FromResult(user);
    }

    public Task<IEnumerable<TModel>> GetByEmailAsync(string email)
    {
        return Task.FromResult(this.userCollection.Where(x => x.Email.Equals(email, StringComparison.Ordinal)));
    }

    public Task<TKey> CreateAsync(TModel model, IUserOperationMapper operationMapper)
    {
        if (!this.userCollection.Any(u => u.Key.Equals(model.Key)))
        {
            this.userCollection.Add(operationMapper.Create<TModel, TModel>(model));

            return Task.FromResult(model.Key);
        }

        throw new UserExistsException($"Another user with the same key `{model.Key}` already exists.");
    }

    public async Task UpdateAsync(TKey key, TModel model, IUserOperationMapper operationMapper)
    {
        TModel? user = this.userCollection.FirstOrDefault(u => u.Key.Equals(key));

        if (user == null)
        {
            throw new UserNotFoundException($"User with key `{model.Key}` NOT found.");
        }

        this.userCollection.Remove(user);

        await this.CreateAsync(model, operationMapper)
                  .ConfigureAwait(false);
    }

    public Task DeleteByKeyAsync(TKey key)
    {
        TModel? user = this.userCollection.FirstOrDefault(x => x.Key.Equals(key));

        if (user == null)
        {
            throw new UserNotFoundException($"User with key `{key}` NOT found.");
        }

        this.userCollection.Remove(user);

        return Task.CompletedTask;
    }
}
