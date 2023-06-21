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
using Kwality.UVault.M2M.Stores.Abstractions;

[PublicAPI]
#pragma warning disable CA1005
public sealed class ApplicationTokenManager<TToken, TModel, TKey>
#pragma warning restore CA1005
    where TToken : TokenModel
    where TModel : ApplicationModel<TKey>
    where TKey : IEqualityComparer<TKey>
{
    private readonly IApplicationTokenStore<TToken, TModel, TKey> store;

    public ApplicationTokenManager(IApplicationTokenStore<TToken, TModel, TKey> store)
    {
        this.store = store;
    }

    public Task<TToken> GetAccessTokenAsync(TModel application, string audience, string grantType)
    {
        return this.store.GetAccessTokenAsync(application, audience, grantType);
    }
}
