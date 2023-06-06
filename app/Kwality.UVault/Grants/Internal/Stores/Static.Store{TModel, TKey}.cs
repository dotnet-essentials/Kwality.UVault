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
namespace Kwality.UVault.Grants.Internal.Stores;

using Kwality.UVault.Exceptions;
using Kwality.UVault.Grants.Models;
using Kwality.UVault.Grants.Operations.Filters.Abstractions;
using Kwality.UVault.Grants.Operations.Mappers.Abstractions;
using Kwality.UVault.Grants.Stores.Abstractions;
using Kwality.UVault.Models;

internal sealed class StaticStore<TModel, TKey> : IGrantStore<TModel, TKey>
    where TModel : GrantModel<TKey>
    where TKey : IEqualityComparer<TKey>
{
    private readonly IList<TModel> collection = new List<TModel>();

    public Task<PagedResultSet<TModel>> GetAllAsync(int pageIndex, int pageSize, IGrantFilter? filter)
    {
        IQueryable<TModel> dataSet = this.collection.AsQueryable();

        if (filter != null)
        {
            dataSet = dataSet.AsEnumerable()
                             .Where(filter.Create<Func<TModel, bool>>())
                             .AsQueryable();
        }

        IEnumerable<TModel> grants = dataSet.Skip(pageIndex * pageSize)
                                            .Take(pageSize);

        var result = new PagedResultSet<TModel>(grants, this.collection.Count > (pageIndex + 1) * pageSize);

        return Task.FromResult(result);
    }

    public Task<TKey> CreateAsync(TModel model, IGrantOperationMapper mapper)
    {
        this.collection.Add(mapper.Create<TModel, TModel>(model));

        return Task.FromResult(model.Key);
    }

    public async Task UpdateAsync(TKey key, TModel model, IGrantOperationMapper mapper)
    {
        TModel? grant = this.collection.FirstOrDefault(u => u.Key.Equals(key));

        if (grant == null)
        {
            throw new UpdateException($"Failed to update grant: `{key}`. Not found.");
        }

        this.collection.Remove(grant);

        await this.CreateAsync(model, mapper)
                  .ConfigureAwait(false);
    }

    public Task DeleteByKeyAsync(TKey key)
    {
        TModel? grant = this.collection.FirstOrDefault(x => x.Key.Equals(key));

        if (grant != null)
        {
            this.collection.Remove(grant);
        }

        return Task.CompletedTask;
    }
}
