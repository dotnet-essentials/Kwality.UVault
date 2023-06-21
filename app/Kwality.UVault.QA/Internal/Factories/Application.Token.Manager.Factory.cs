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
namespace Kwality.UVault.QA.Internal.Factories;

using JetBrains.Annotations;

using Kwality.UVault.Extensions;
using Kwality.UVault.M2M.Extensions;
using Kwality.UVault.M2M.Managers;
using Kwality.UVault.M2M.Models;
using Kwality.UVault.M2M.Options;
using Kwality.UVault.M2M.Stores.Abstractions;

using Microsoft.Extensions.DependencyInjection;

internal sealed class ApplicationTokenManagerFactory
{
    private readonly IServiceCollection serviceCollection;

    public ApplicationTokenManagerFactory()
    {
        this.serviceCollection = new ServiceCollection();
    }

    public ApplicationTokenManager<TToken, TModel, TKey> Create<TToken, TModel, TKey>()
        where TToken : TokenModel
        where TModel : ApplicationModel<TKey>
        where TKey : IEqualityComparer<TKey>
    {
        this.serviceCollection.AddUVault(static (_, options) => options.UseApplicationTokenManagement<TToken, TModel, TKey>(null));
        this.serviceCollection.AddSingleton<IApplicationTokenStoreStaticTokenGenerator<Model, TModel, TKey>, ApplicationStoreStaticTokenGenerator<TModel, TKey>>();

        return this.serviceCollection.BuildServiceProvider()
                   .GetRequiredService<ApplicationTokenManager<TToken, TModel, TKey>>();
    }

    public ApplicationTokenManager<TToken, TModel, TKey> Create<TToken, TModel, TKey>(
        Action<ApplicationTokenManagementOptions<TToken, TModel, TKey>>? action)
        where TToken : TokenModel
        where TModel : ApplicationModel<TKey>
        where TKey : IEqualityComparer<TKey>
    {
        this.serviceCollection.AddUVault(
            (_, options) =>
                options.UseApplicationTokenManagement(action));

        return this.serviceCollection.BuildServiceProvider()
                   .GetRequiredService<ApplicationTokenManager<TToken, TModel, TKey>>();
    }

    internal sealed class ApplicationStoreStaticTokenGenerator<TModel, TKey> : IApplicationTokenStoreStaticTokenGenerator<Model, TModel, TKey>
        where TModel : ApplicationModel<TKey>
        where TKey : IEqualityComparer<TKey>
    {
        public Task<Model> GenerateToken(TModel application, string audience, string grantType)
        {
            return Task.FromResult(
                new Model(
                    Guid.NewGuid()
                        .ToString()));
        }
    }

#pragma warning disable CA1812 // "Avoid uninstantiated internal classes".
    [UsedImplicitly]
    internal sealed class Model : TokenModel
#pragma warning restore CA1812
    {
        public Model(string token)
        {
            this.Token = token;
        }

        public string Token { get; }
    }
}
