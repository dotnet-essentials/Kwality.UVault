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
namespace Kwality.UVault.M2M.Internal.Stores;

using Kwality.UVault.Core.Exceptions;
using Kwality.UVault.Core.Models;
using Kwality.UVault.M2M.Models;
using Kwality.UVault.M2M.Operations.Filters.Abstractions;
using Kwality.UVault.M2M.Operations.Mappers.Abstractions;
using Kwality.UVault.M2M.Stores.Abstractions;

#pragma warning disable CA1812
internal sealed class StaticStore<TModel, TKey> : IApplicationStore<TModel, TKey>
#pragma warning disable CA1812
    where TModel : ApplicationModel<TKey>
    where TKey : IEqualityComparer<TKey>
{
    private readonly List<TModel> collection = [];

    public Task<PagedResultSet<TModel>> GetAllAsync(int pageIndex, int pageSize, IApplicationFilter? filter)
    {
        IQueryable<TModel> dataSet = this.collection.AsQueryable();

        if (filter != null)
        {
            dataSet = dataSet.AsEnumerable()
                             .Where(filter.Create<Func<TModel, bool>>())
                             .AsQueryable();
        }

        IEnumerable<TModel> applications = dataSet.Skip(pageIndex * pageSize)
                                                  .Take(pageSize);

        var result = new PagedResultSet<TModel>(applications, this.collection.Count > (pageIndex + 1) * pageSize);

        return Task.FromResult(result);
    }

    public Task<TModel> GetByKeyAsync(TKey key)
    {
        TModel? application = this.collection.FirstOrDefault(x => x.Key.Equals(key));

        if (application == null)
        {
            throw new ReadException($"Failed to read application: `{key}`. Not found.");
        }

        return Task.FromResult(application);
    }

    public Task<TKey> CreateAsync(TModel model, IApplicationOperationMapper mapper)
    {
        this.collection.Add(mapper.Create<TModel, TModel>(model));

        return Task.FromResult(model.Key);
    }

    public async Task UpdateAsync(TKey key, TModel model, IApplicationOperationMapper mapper)
    {
        TModel? application = this.collection.FirstOrDefault(u => u.Key.Equals(key));

        if (application == null)
        {
            throw new UpdateException($"Failed to update application: `{key}`. Not found.");
        }

        this.collection.Remove(application);

        await this.CreateAsync(model, mapper)
                  .ConfigureAwait(false);
    }

    public Task DeleteByKeyAsync(TKey key)
    {
        TModel? application = this.collection.FirstOrDefault(x => x.Key.Equals(key));

        if (application != null)
        {
            this.collection.Remove(application);
        }

        return Task.CompletedTask;
    }

    public Task<TModel> RotateClientSecretAsync(TKey key)
    {
        TModel? application = this.collection.FirstOrDefault(u => u.Key.Equals(key));

        if (application != null)
        {
            application.ClientSecret = Guid.NewGuid()
                                           .ToString();

            return Task.FromResult(application);
        }

        throw new UpdateException($"Failed to update application: `{key}`. Not found.");
    }
}
