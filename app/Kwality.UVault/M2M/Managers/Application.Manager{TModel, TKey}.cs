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
namespace Kwality.UVault.M2M.Managers;

using JetBrains.Annotations;

using Kwality.UVault.M2M.Models;
using Kwality.UVault.M2M.Operations.Mappers.Abstractions;
using Kwality.UVault.M2M.Stores.Abstractions;
using Kwality.UVault.Models;

[PublicAPI]
public sealed class ApplicationManager<TModel, TKey>
    where TModel : ApplicationModel<TKey>
    where TKey : IEqualityComparer<TKey>
{
    private readonly IApplicationStore<TModel, TKey> store;

    public ApplicationManager(IApplicationStore<TModel, TKey> store)
    {
        this.store = store;
    }

    public Task<PagedResultSet<TModel>> GetAllAsync(int pageIndex, int pageSize)
    {
        return this.store.GetAllAsync(pageIndex, pageSize);
    }

    // Stryker disable once all
    public Task<TModel> GetByKeyAsync(TKey key)
    {
        return this.store.GetByKeyAsync(key);
    }

    // Stryker disable once all
    public Task<TKey> CreateAsync(TModel model, IApplicationOperationMapper mapper)
    {
        return this.store.CreateAsync(model, mapper);
    }

    // Stryker disable once all
    public Task UpdateAsync(TKey key, TModel model, IApplicationOperationMapper mapper)
    {
        return this.store.UpdateAsync(key, model, mapper);
    }

    // Stryker disable once all
    public Task DeleteByKeyAsync(TKey key)
    {
        return this.store.DeleteByKeyAsync(key);
    }

    // Stryker disable once all
    public Task<TModel> RotateClientSecretAsync(TKey key)
    {
        return this.store.RotateClientSecretAsync(key);
    }
}
