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
namespace Kwality.UVault.Grants.Managers;

using JetBrains.Annotations;

using Kwality.UVault.Grants.Models;
using Kwality.UVault.Grants.Operations.Filters.Abstractions;
using Kwality.UVault.Grants.Operations.Mappers.Abstractions;
using Kwality.UVault.Grants.Stores.Abstractions;
using Kwality.UVault.Models;

[PublicAPI]
public sealed class GrantManager<TModel, TKey>
    where TModel : GrantModel<TKey>
    where TKey : IEqualityComparer<TKey>
{
    private readonly IGrantStore<TModel, TKey> store;

    public GrantManager(IGrantStore<TModel, TKey> store)
    {
        this.store = store;
    }

    public Task<PagedResultSet<TModel>> GetAllAsync(int pageIndex, int pageSize)
    {
        return this.GetAllAsync(pageIndex, pageSize, null);
    }

    public Task<PagedResultSet<TModel>> GetAllAsync(int pageIndex, int pageSize, IGrantFilter? filter)
    {
        return this.store.GetAllAsync(pageIndex, pageSize, filter);
    }

    // Stryker disable once all
    public Task<TKey> CreateAsync(TModel model, IGrantOperationMapper mapper)
    {
        return this.store.CreateAsync(model, mapper);
    }

    // Stryker disable once all
    public Task UpdateAsync(TKey key, TModel model, IGrantOperationMapper mapper)
    {
        return this.store.UpdateAsync(key, model, mapper);
    }

    // Stryker disable once all
    public Task DeleteByKeyAsync(TKey key)
    {
        return this.store.DeleteByKeyAsync(key);
    }
}
