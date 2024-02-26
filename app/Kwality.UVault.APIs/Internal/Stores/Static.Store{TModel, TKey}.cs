﻿// =====================================================================================================================
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
namespace Kwality.UVault.APIs.Internal.Stores;

using Kwality.UVault.APIs.Models;
using Kwality.UVault.APIs.Operations.Mappers.Abstractions;
using Kwality.UVault.APIs.Stores.Abstractions;
using Kwality.UVault.Core.Exceptions;

internal sealed class StaticStore<TModel, TKey> : IApiStore<TModel, TKey>
    where TModel : ApiModel<TKey>
    where TKey : IEqualityComparer<TKey>
{
    private readonly List<TModel> collection = [];

    public Task<TModel> GetByKeyAsync(TKey key)
    {
        TModel? api = this.collection.FirstOrDefault(x => x.Key.Equals(key));

        if (api == null)
        {
            throw new ReadException($"Failed to read API: `{key}`. Not found.");
        }

        return Task.FromResult(api);
    }

    public Task<TKey> CreateAsync(TModel model, IApiOperationMapper mapper)
    {
        this.collection.Add(mapper.Create<TModel, TModel>(model));

        return Task.FromResult(model.Key);
    }

    public Task DeleteByKeyAsync(TKey key)
    {
        TModel? api = this.collection.FirstOrDefault(x => x.Key.Equals(key));

        if (api != null)
        {
            this.collection.Remove(api);
        }

        return Task.CompletedTask;
    }
}
