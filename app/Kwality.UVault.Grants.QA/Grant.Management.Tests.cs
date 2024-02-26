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
namespace Kwality.UVault.Grants.QA;

using System.Diagnostics.CodeAnalysis;

using AutoFixture.Xunit2;

using FluentAssertions;

using JetBrains.Annotations;

using Kwality.UVault.Core.Exceptions;
using Kwality.UVault.Core.Extensions;
using Kwality.UVault.Core.Helpers;
using Kwality.UVault.Core.Keys;
using Kwality.UVault.Core.Models;
using Kwality.UVault.Grants.Extensions;
using Kwality.UVault.Grants.Managers;
using Kwality.UVault.Grants.Models;
using Kwality.UVault.Grants.Operations.Filters.Abstractions;
using Kwality.UVault.Grants.Operations.Mappers.Abstractions;
using Kwality.UVault.Grants.QA.Factories;
using Kwality.UVault.Grants.Stores.Abstractions;
using Kwality.UVault.QA.Common.Xunit.Traits;

using Microsoft.Extensions.DependencyInjection;

using Xunit;

[SuppressMessage("ReSharper", "MemberCanBeFileLocal")]
public sealed class GrantManagementTests
{
    [AutoData]
    [GrantManagement]
    [Theory(DisplayName = "When the store is configured as a `Singleton` one, it behaves as such.")]
    internal void UseStoreAsSingleton_RegisterStoreAsSingleton(ServiceCollection services)
    {
        // ARRANGE.
        services.AddUVault(static (_, options) =>
            options.UseGrantManagement<Model, IntKey>(static options =>
                options.UseStore<Store>(ServiceLifetime.Singleton)));

        // ASSERT.
        services.Should()
                .ContainSingle(static descriptor => descriptor.ServiceType == typeof(IGrantStore<Model, IntKey>) &&
                                                    descriptor.Lifetime == ServiceLifetime.Singleton &&
                                                    descriptor.ImplementationType == typeof(Store));
    }

    [AutoData]
    [GrantManagement]
    [Theory(DisplayName = "When the store is configured as a `Scoped` one, it behaves as such.")]
    internal void UseStoreAsScoped_RegisterStoreAsScoped(ServiceCollection services)
    {
        // ARRANGE.
        services.AddUVault(static (_, options) =>
            options.UseGrantManagement<Model, IntKey>(static options =>
                options.UseStore<Store>(ServiceLifetime.Scoped)));

        // ASSERT.
        services.Should()
                .ContainSingle(static descriptor => descriptor.ServiceType == typeof(IGrantStore<Model, IntKey>) &&
                                                    descriptor.Lifetime == ServiceLifetime.Scoped &&
                                                    descriptor.ImplementationType == typeof(Store));
    }

    [AutoData]
    [GrantManagement]
    [Theory(DisplayName = "When the store is configured as a `Transient` one, it behaves as such.")]
    internal void UseStoreAsTransient_RegisterStoreAsTransient(ServiceCollection services)
    {
        // ARRANGE.
        services.AddUVault(static (_, options) =>
            options.UseGrantManagement<Model, IntKey>(static options =>
                options.UseStore<Store>(ServiceLifetime.Transient)));

        // ASSERT.
        services.Should()
                .ContainSingle(static descriptor => descriptor.ServiceType == typeof(IGrantStore<Model, IntKey>) &&
                                                    descriptor.Lifetime == ServiceLifetime.Transient &&
                                                    descriptor.ImplementationType == typeof(Store));
    }

    [AutoData]
    [GrantManagement]
    [Theory(DisplayName = "Get all (pageIndex: 0, all data showed) succeeds.")]
    internal async Task GetAll_FirstPageWhenAllDataShowed_Succeeds(Model model)
    {
        // ARRANGE.
        GrantManager<Model, IntKey> manager
            = new GrantManagerFactory().Create<Model, IntKey>(static options => options.UseStore<Store>());

        await manager.CreateAsync(model, new CreateOperationMapper())
                     .ConfigureAwait(true);

        // ACT.
        PagedResultSet<Model> result = await manager.GetAllAsync(0, 10)
                                                    .ConfigureAwait(true);

        // ASSERT.
        result.HasNextPage.Should()
              .BeFalse();

        result.ResultSet.Count()
              .Should()
              .Be(1);

        result.ResultSet.Take(1)
              .First()
              .Should()
              .BeEquivalentTo(model);
    }

    [AutoData]
    [GrantManagement]
    [Theory(DisplayName = "Get all (pageIndex: 1, all data showed) succeeds.")]
    internal async Task GetAll_SecondPageWhenAllDataShowed_Succeeds(Model model)
    {
        // ARRANGE.
        GrantManager<Model, IntKey> manager
            = new GrantManagerFactory().Create<Model, IntKey>(static options => options.UseStore<Store>());

        await manager.CreateAsync(model, new CreateOperationMapper())
                     .ConfigureAwait(true);

        // ACT.
        PagedResultSet<Model> result = await manager.GetAllAsync(1, 10)
                                                    .ConfigureAwait(true);

        // ASSERT.
        result.HasNextPage.Should()
              .BeFalse();

        result.ResultSet.Count()
              .Should()
              .Be(0);
    }

    [AutoData]
    [GrantManagement]
    [Theory(DisplayName = "Get all (pageIndex: 0, all data NOT showed) succeeds.")]
    internal async Task GetAll_FirstPageWhenNotAllDataShowed_Succeeds(Model modelOne, Model modelTwo)
    {
        // ARRANGE.
        GrantManager<Model, IntKey> manager
            = new GrantManagerFactory().Create<Model, IntKey>(static options => options.UseStore<Store>());

        await manager.CreateAsync(modelOne, new CreateOperationMapper())
                     .ConfigureAwait(true);

        await manager.CreateAsync(modelTwo, new CreateOperationMapper())
                     .ConfigureAwait(true);

        // ACT.
        PagedResultSet<Model> result = await manager.GetAllAsync(0, 1)
                                                    .ConfigureAwait(true);

        // ASSERT.
        result.HasNextPage.Should()
              .BeTrue();

        result.ResultSet.Count()
              .Should()
              .Be(1);

        result.ResultSet.Take(1)
              .First()
              .Should()
              .BeEquivalentTo(modelOne);
    }

    [AutoData]
    [GrantManagement]
    [Theory(DisplayName = "Get all (pageIndex: 1, pageSize: Less than the total amount) succeeds.")]
    internal async Task GetAll_SecondPageWithLessElementsThanTotal_Succeeds(Model modelOne, Model modelTwo)
    {
        // ARRANGE.
        GrantManager<Model, IntKey> manager
            = new GrantManagerFactory().Create<Model, IntKey>(static options => options.UseStore<Store>());

        await manager.CreateAsync(modelOne, new CreateOperationMapper())
                     .ConfigureAwait(true);

        await manager.CreateAsync(modelTwo, new CreateOperationMapper())
                     .ConfigureAwait(true);

        // ACT.
        PagedResultSet<Model> result = await manager.GetAllAsync(1, 1)
                                                    .ConfigureAwait(true);

        // ASSERT.
        result.HasNextPage.Should()
              .BeFalse();

        result.ResultSet.Count()
              .Should()
              .Be(1);

        result.ResultSet.Take(1)
              .First()
              .Should()
              .BeEquivalentTo(modelTwo);
    }

    [AutoData]
    [GrantManagement]
    [Theory(DisplayName = "Get all with filter succeeds.")]
    internal async Task GetAll_WithFilter_Succeeds(Model modelOne, Model modelTwo)
    {
        // ARRANGE.
        GrantManager<Model, IntKey> manager
            = new GrantManagerFactory().Create<Model, IntKey>(static options => options.UseStore<Store>());

        await manager.CreateAsync(modelOne, new CreateOperationMapper())
                     .ConfigureAwait(true);

        await manager.CreateAsync(modelTwo, new CreateOperationMapper())
                     .ConfigureAwait(true);

        PagedResultSet<Model> result = await manager.GetAllAsync(0, 10, new OperationFilter(modelTwo.Scopes))
                                                    .ConfigureAwait(true);

        // ASSERT.
        result.ResultSet.Count()
              .Should()
              .Be(1);

        (await manager.GetAllAsync(0, 100)
                      .ConfigureAwait(true)).ResultSet.Should()
                                            .ContainEquivalentOf(modelTwo);
    }

    [AutoData]
    [GrantManagement]
    [Theory(DisplayName = "Create succeeds.")]
    internal async Task Create_Succeeds(Model model)
    {
        // ARRANGE.
        GrantManager<Model, IntKey> manager
            = new GrantManagerFactory().Create<Model, IntKey>(static options => options.UseStore<Store>());

        // ACT.
        await manager.CreateAsync(model, new CreateOperationMapper())
                     .ConfigureAwait(true);

        // ASSERT.
        (await manager.GetAllAsync(0, 100)
                      .ConfigureAwait(true)).ResultSet.Should()
                                            .ContainEquivalentOf(model);
    }

    [AutoData]
    [GrantManagement]
    [Theory(DisplayName = "Update succeeds.")]
    internal async Task Update_Succeeds(Model model)
    {
        // ARRANGE.
        GrantManager<Model, IntKey> manager
            = new GrantManagerFactory().Create<Model, IntKey>(static options => options.UseStore<Store>());

        IntKey key = await manager.CreateAsync(model, new CreateOperationMapper())
                                  .ConfigureAwait(true);

        // ACT.
        model.Scopes = new[] { "newScope", "newScope2" };

        await manager.UpdateAsync(key, model, new UpdateOperationMapper())
                     .ConfigureAwait(true);

        // ASSERT.
        (await manager.GetAllAsync(0, 100)
                      .ConfigureAwait(true)).ResultSet.Should()
                                            .ContainEquivalentOf(model);
    }

    [AutoData]
    [GrantManagement]
    [Theory(DisplayName = "Update raises an exception when the key is not found.")]
    internal async Task Update_UnknownKey_RaisesException(IntKey key, Model model)
    {
        // ARRANGE.
        GrantManager<Model, IntKey> manager
            = new GrantManagerFactory().Create<Model, IntKey>(static options => options.UseStore<Store>());

        // ACT.
        Func<Task> act = () => manager.UpdateAsync(key, model, new UpdateOperationMapper());

        // ASSERT.
        await act.Should()
                 .ThrowAsync<UpdateException>()
                 .WithMessage($"Custom: Failed to update client grant: `{key}`. Not found.")
                 .ConfigureAwait(true);
    }

    [AutoData]
    [GrantManagement]
    [Theory(DisplayName = "Delete succeeds.")]
    internal async Task Delete_Succeeds(Model model)
    {
        // ARRANGE.
        GrantManager<Model, IntKey> manager
            = new GrantManagerFactory().Create<Model, IntKey>(static options => options.UseStore<Store>());

        IntKey key = await manager.CreateAsync(model, new CreateOperationMapper())
                                  .ConfigureAwait(true);

        // ACT.
        await manager.DeleteByKeyAsync(key)
                     .ConfigureAwait(true);

        // ASSERT.
        (await manager.GetAllAsync(0, 100)
                      .ConfigureAwait(true)).ResultSet.Should()
                                            .BeEmpty();
    }

    [AutoData]
    [GrantManagement]
    [Theory(DisplayName = "Delete succeeds when the key is not found.")]
    internal async Task Delete_UnknownKey_Succeeds(IntKey key)
    {
        // ARRANGE.
        GrantManager<Model, IntKey> manager
            = new GrantManagerFactory().Create<Model, IntKey>(static options => options.UseStore<Store>());

        // ACT.
        Func<Task> act = () => manager.DeleteByKeyAsync(key);

        // ASSERT.
        await act.Should()
                 .NotThrowAsync()
                 .ConfigureAwait(true);
    }

    [UsedImplicitly]
    internal sealed class Model(IntKey key, IEnumerable<string> scopes) : GrantModel<IntKey>(key)
    {
        public IEnumerable<string> Scopes { get; set; } = scopes;
    }

    private sealed class OperationFilter(IEnumerable<string> scopes) : IGrantFilter
    {
        public TDestination Create<TDestination>()
            where TDestination : class
        {
            if (typeof(TDestination) != typeof(Func<KeyValuePair<IntKey, Model>, bool>))
            {
                throw new ReadException(
                    $"Invalid {nameof(IGrantFilter)}: Destination is NOT `{typeof(Func<KeyValuePair<IntKey, Model>, bool>).Name}`.");
            }

            return ((Func<KeyValuePair<IntKey, Model>, bool>)Filter)
                .UnsafeAs<Func<KeyValuePair<IntKey, Model>, bool>, TDestination>();

            // The filter which is filters out data in the store.
            bool Filter(KeyValuePair<IntKey, Model> kvp)
            {
                return Equals(kvp.Value.Scopes, scopes);
            }
        }
    }

    private sealed class CreateOperationMapper : IGrantOperationMapper
    {
        public TDestination Create<TSource, TDestination>(TSource source)
            where TDestination : class
        {
            if (typeof(TDestination) != typeof(TSource))
            {
                throw new CreateException(
                    $"Invalid {nameof(IGrantOperationMapper)}: Destination is NOT `{nameof(TSource)}`.");
            }

            return source.UnsafeAs<TSource, TDestination>();
        }
    }

    private sealed class UpdateOperationMapper : IGrantOperationMapper
    {
        public TDestination Create<TSource, TDestination>(TSource source)
            where TDestination : class
        {
            if (typeof(TDestination) != typeof(TSource))
            {
                throw new UpdateException(
                    $"Invalid {nameof(IGrantOperationMapper)}: Destination is NOT `{nameof(TSource)}`.");
            }

            return source.UnsafeAs<TSource, TDestination>();
        }
    }

    [UsedImplicitly]
    internal sealed class Store : IGrantStore<Model, IntKey>
    {
        private readonly Dictionary<IntKey, Model> collection = new();

        public Task<PagedResultSet<Model>> GetAllAsync(int pageIndex, int pageSize, IGrantFilter? filter)
        {
            IQueryable<KeyValuePair<IntKey, Model>> dataSet = this.collection.AsQueryable();

            if (filter != null)
            {
                dataSet = dataSet.AsEnumerable()
                                 .Where(filter.Create<Func<KeyValuePair<IntKey, Model>, bool>>())
                                 .AsQueryable();
            }

            IEnumerable<Model> grants = dataSet.Skip(pageIndex * pageSize)
                                               .Take(pageSize)
                                               .Select(static kvp => kvp.Value);

            var result = new PagedResultSet<Model>(grants, this.collection.Count > (pageIndex + 1) * pageSize);

            return Task.FromResult(result);
        }

        public Task<IntKey> CreateAsync(Model model, IGrantOperationMapper mapper)
        {
            this.collection.Add(model.Key, mapper.Create<Model, Model>(model));

            return Task.FromResult(model.Key);
        }

        public async Task UpdateAsync(IntKey key, Model model, IGrantOperationMapper mapper)
        {
            if (!this.collection.ContainsKey(key))
            {
                throw new UpdateException($"Custom: Failed to update client grant: `{key}`. Not found.");
            }

            this.collection.Remove(key);

            await this.CreateAsync(model, mapper)
                      .ConfigureAwait(true);
        }

        public Task DeleteByKeyAsync(IntKey key)
        {
            this.collection.Remove(key);

            return Task.CompletedTask;
        }
    }
}
