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

using Kwality.UVault.Core.Exceptions;
using Kwality.UVault.Users.Models;
using Kwality.UVault.Users.Operations.Mappers.Abstractions;
using Kwality.UVault.Users.Stores.Abstractions;

internal sealed class StaticStore<TModel, TKey> : IUserStore<TModel, TKey>
    where TModel : UserModel<TKey>
    where TKey : IEqualityComparer<TKey>
{
    private readonly List<TModel> collection = [];

    public Task<TModel> GetByKeyAsync(TKey key)
    {
        TModel? user = this.collection.FirstOrDefault(x => x.Key.Equals(key));

        if (user == null)
        {
            throw new ReadException($"Failed to read user: `{key}`. Not found.");
        }

        return Task.FromResult(user);
    }

    public Task<IEnumerable<TModel>> GetByEmailAsync(string email)
    {
        return Task.FromResult(this.collection.Where(x => x.Email.Equals(email, StringComparison.Ordinal)));
    }

    public Task<TKey> CreateAsync(TModel model, IUserOperationMapper mapper)
    {
        if (!this.collection.Any(u => u.Key.Equals(model.Key)))
        {
            this.collection.Add(mapper.Create<TModel, TModel>(model));

            return Task.FromResult(model.Key);
        }

        throw new CreateException($"Failed to create user: `{model.Key}`. Duplicate key.");
    }

    public async Task UpdateAsync(TKey key, TModel model, IUserOperationMapper mapper)
    {
        TModel? user = this.collection.FirstOrDefault(u => u.Key.Equals(key));

        if (user == null)
        {
            throw new UpdateException($"Failed to update user: `{key}`. Not found.");
        }

        this.collection.Remove(user);

        await this.CreateAsync(model, mapper)
                  .ConfigureAwait(false);
    }

    public Task DeleteByKeyAsync(TKey key)
    {
        TModel? user = this.collection.FirstOrDefault(x => x.Key.Equals(key));

        if (user != null)
        {
            this.collection.Remove(user);
        }

        return Task.CompletedTask;
    }
}
