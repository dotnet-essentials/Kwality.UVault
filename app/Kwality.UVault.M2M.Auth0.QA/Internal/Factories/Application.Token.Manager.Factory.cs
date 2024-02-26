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
namespace Kwality.UVault.M2M.Auth0.QA.Internal.Factories;

using Kwality.UVault.Core.Extensions;
using Kwality.UVault.M2M.Extensions;
using Kwality.UVault.M2M.Managers;
using Kwality.UVault.M2M.Models;
using Kwality.UVault.M2M.Options;

using Microsoft.Extensions.DependencyInjection;

internal sealed class ApplicationTokenManagerFactory
{
    private readonly IServiceCollection serviceCollection = new ServiceCollection();

    public ApplicationTokenManager<TToken> Create<TToken, TModel, TKey>()
        where TToken : TokenModel, new()
        where TModel : ApplicationModel<TKey>
        where TKey : IEqualityComparer<TKey>
    {
        this.serviceCollection.AddUVault(static (_, options) =>
            options.UseApplicationTokenManagement<TToken, TModel, TKey>(null));

        return this.serviceCollection.BuildServiceProvider()
                   .GetRequiredService<ApplicationTokenManager<TToken>>();
    }

    public ApplicationTokenManager<TToken> Create<TToken, TModel, TKey>(
        Action<ApplicationTokenManagementOptions<TToken>>? applicationTokenManagementOptions)
        where TToken : TokenModel, new()
        where TModel : ApplicationModel<TKey>
        where TKey : IEqualityComparer<TKey>
    {
        this.serviceCollection.AddUVault((_, options) =>
            options.UseApplicationTokenManagement<TToken, TModel, TKey>(applicationTokenManagementOptions));

        return this.serviceCollection.BuildServiceProvider()
                   .GetRequiredService<ApplicationTokenManager<TToken>>();
    }
}