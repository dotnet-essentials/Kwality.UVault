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
namespace Kwality.UVault.QA.APIs;

using AutoFixture.Xunit2;

using FluentAssertions;

using JetBrains.Annotations;

using Kwality.UVault.APIs.Extensions;
using Kwality.UVault.APIs.Managers;
using Kwality.UVault.APIs.Models;
using Kwality.UVault.APIs.Operations.Mappers.Abstractions;
using Kwality.UVault.APIs.Stores.Abstractions;
using Kwality.UVault.Exceptions;
using Kwality.UVault.Extensions;
using Kwality.UVault.Keys;
using Kwality.UVault.QA.Internal.Factories;
using Kwality.UVault.QA.Internal.Xunit.Traits;

using Microsoft.Extensions.DependencyInjection;

using Xunit;

// ReSharper disable once MemberCanBeFileLocal
public sealed class ApiManagementTests
{
    [AutoData]
    [ApiManagement]
    [Theory(DisplayName = "When the store is configured as a `Singleton` one, it behaves as such.")]
    internal void UseStoreAsSingleton_RegisterStoreAsSingleton(ServiceCollection services)
    {
        // ARRANGE.
        services.AddUVault(static (_, options) =>
            options.UseApiManagement<Model, IntKey>(
                static options => options.UseStore<Store>(ServiceLifetime.Singleton)));

        // ASSERT.
        services.Should()
                .ContainSingle(static descriptor => descriptor.ServiceType == typeof(IApiStore<Model, IntKey>) &&
                                                    descriptor.Lifetime == ServiceLifetime.Singleton &&
                                                    descriptor.ImplementationType == typeof(Store));
    }

    [AutoData]
    [ApiManagement]
    [Theory(DisplayName = "When the store is configured as a `Scoped` one, it behaves as such.")]
    internal void UseStoreAsScoped_RegisterStoreAsScoped(ServiceCollection services)
    {
        // ARRANGE.
        services.AddUVault(static (_, options) =>
            options.UseApiManagement<Model, IntKey>(static options => options.UseStore<Store>(ServiceLifetime.Scoped)));

        // ASSERT.
        services.Should()
                .ContainSingle(static descriptor => descriptor.ServiceType == typeof(IApiStore<Model, IntKey>) &&
                                                    descriptor.Lifetime == ServiceLifetime.Scoped &&
                                                    descriptor.ImplementationType == typeof(Store));
    }

    [AutoData]
    [ApiManagement]
    [Theory(DisplayName = "When the store is configured as a `Transient` one, it behaves as such.")]
    internal void UseStoreAsTransient_RegisterStoreAsTransient(ServiceCollection services)
    {
        // ARRANGE.
        services.AddUVault(static (_, options) =>
            options.UseApiManagement<Model, IntKey>(
                static options => options.UseStore<Store>(ServiceLifetime.Transient)));

        // ASSERT.
        services.Should()
                .ContainSingle(static descriptor => descriptor.ServiceType == typeof(IApiStore<Model, IntKey>) &&
                                                    descriptor.Lifetime == ServiceLifetime.Transient &&
                                                    descriptor.ImplementationType == typeof(Store));
    }

    [AutoData]
    [ApiManagement]
    [Theory(DisplayName = "Get by key raises an exception when the key is NOT found.")]
    internal async Task GetByKey_UnknownKey_RaisesException(IntKey key)
    {
        // ARRANGE.
        ApiManager<Model, IntKey> manager
            = new ApiManagerFactory().Create<Model, IntKey>(static options => options.UseStore<Store>());

        // ACT.
        Func<Task<Model>> act = () => manager.GetByKeyAsync(key);

        // ASSERT.
        await act.Should()
                 .ThrowAsync<ReadException>()
                 .WithMessage($"Custom: Failed to read API: `{key}`. Not found.")
                 .ConfigureAwait(false);
    }

    [AutoData]
    [ApiManagement]
    [Theory(DisplayName = "Create succeeds.")]
    internal async Task Create_Succeeds(Model model)
    {
        // ARRANGE.
        ApiManager<Model, IntKey> manager
            = new ApiManagerFactory().Create<Model, IntKey>(static options => options.UseStore<Store>());

        IntKey key = await manager.CreateAsync(model, new CreateOperationMapper())
                                  .ConfigureAwait(false);

        // ASSERT.
        (await manager.GetByKeyAsync(key)
                      .ConfigureAwait(false)).Should()
                                             .BeEquivalentTo(model);
    }

    [AutoData]
    [ApiManagement]
    [Theory(DisplayName = "Delete succeeds.")]
    internal async Task Delete_Succeeds(Model model)
    {
        // ARRANGE.
        ApiManager<Model, IntKey> manager
            = new ApiManagerFactory().Create<Model, IntKey>(static options => options.UseStore<Store>());

        IntKey key = await manager.CreateAsync(model, new CreateOperationMapper())
                                  .ConfigureAwait(false);

        // ACT.
        await manager.DeleteByKeyAsync(key)
                     .ConfigureAwait(false);

        // ASSERT.
        Func<Task<Model>> act = () => manager.GetByKeyAsync(key);

        await act.Should()
                 .ThrowAsync<ReadException>()
                 .WithMessage($"Custom: Failed to read API: `{key}`. Not found.")
                 .ConfigureAwait(false);
    }

    [AutoData]
    [ApiManagement]
    [Theory(DisplayName = "Delete succeeds when the key is not found.")]
    internal async Task Delete_UnknownKey_Succeeds(IntKey key)
    {
        // ARRANGE.
        ApiManager<Model, IntKey> manager
            = new ApiManagerFactory().Create<Model, IntKey>(static options => options.UseStore<Store>());

        // ACT.
        Func<Task> act = () => manager.DeleteByKeyAsync(key);

        // ASSERT.
        await act.Should()
                 .NotThrowAsync()
                 .ConfigureAwait(false);
    }

#pragma warning disable CA1812 // "Avoid uninstantiated internal classes".
    [UsedImplicitly]
    internal sealed class Model : ApiModel<IntKey>
#pragma warning restore CA1812
    {
        public Model(IntKey name)
            : base(name)
        {
        }
    }

    private sealed class CreateOperationMapper : IApiOperationMapper
    {
        public TDestination Create<TSource, TDestination>(TSource source)
            where TDestination : class
        {
            if (typeof(TDestination) != typeof(TSource))
            {
                throw new CreateException(
                    $"Invalid {nameof(IApiOperationMapper)}: Destination is NOT `{nameof(TSource)}`.");
            }

            // ReSharper disable once NullableWarningSuppressionIsUsed - Known to be safe. See previous statement.
            return (source as TDestination)!;
        }
    }

#pragma warning disable CA1812 // "Avoid uninstantiated internal classes".
    [UsedImplicitly]
    internal sealed class Store : IApiStore<Model, IntKey>
#pragma warning restore CA1812
    {
        private readonly IDictionary<IntKey, Model> collection = new Dictionary<IntKey, Model>();

        public Task<Model> GetByKeyAsync(IntKey key)
        {
            if (!this.collection.ContainsKey(key))
            {
                throw new ReadException($"Custom: Failed to read API: `{key}`. Not found.");
            }

            return Task.FromResult(this.collection[key]);
        }

        public Task<IntKey> CreateAsync(Model model, IApiOperationMapper mapper)
        {
            this.collection.Add(model.Key, mapper.Create<Model, Model>(model));

            return Task.FromResult(model.Key);
        }

        public Task DeleteByKeyAsync(IntKey key)
        {
            if (this.collection.ContainsKey(key))
            {
                this.collection.Remove(key);
            }

            return Task.CompletedTask;
        }
    }
}
