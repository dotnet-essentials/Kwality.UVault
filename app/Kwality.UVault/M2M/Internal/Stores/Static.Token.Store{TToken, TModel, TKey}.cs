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

using Kwality.UVault.M2M.Models;
using Kwality.UVault.M2M.Stores.Abstractions;

internal sealed class StaticTokenStore<TToken, TModel, TKey> : IApplicationTokenStore<TToken, TModel, TKey>
    where TToken : TokenModel
    where TModel : ApplicationModel<TKey>
    where TKey : IEqualityComparer<TKey>
{
    private readonly IDictionary<TKey, TToken> tokenStore = new Dictionary<TKey, TToken>();
    private readonly IApplicationTokenStoreStaticTokenGenerator<TToken, TModel, TKey> tokenGenerator;

    public StaticTokenStore(IApplicationTokenStoreStaticTokenGenerator<TToken, TModel, TKey> tokenGenerator)
    {
        this.tokenGenerator = tokenGenerator;
    }

    public async Task<TToken> GetAccessTokenAsync(TModel application, string audience, string grantType)
    {
        if (this.tokenGenerator == null)
        {
            throw new ArgumentException($"The {typeof(IApplicationTokenStoreStaticTokenGenerator<TToken, TModel, TKey>).FullName} service should not be null");
        }

        if (!this.tokenStore.ContainsKey(application.Key))
        {
            TToken value = await this.tokenGenerator.GenerateToken(application, audience, grantType)
                                     .ConfigureAwait(false);

            this.tokenStore.Add(application.Key, value);
        }

        return this.tokenStore[application.Key];
    }
}
