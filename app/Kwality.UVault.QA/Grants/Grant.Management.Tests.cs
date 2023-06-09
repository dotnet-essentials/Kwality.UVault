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
namespace Kwality.UVault.QA.Grants;

using AutoFixture.Xunit2;

using FluentAssertions;

using JetBrains.Annotations;

using Kwality.UVault.Exceptions;
using Kwality.UVault.Extensions;
using Kwality.UVault.Grants.Extensions;
using Kwality.UVault.Grants.Managers;
using Kwality.UVault.Grants.Models;
using Kwality.UVault.Grants.Operations.Filters.Abstractions;
using Kwality.UVault.Grants.Operations.Mappers.Abstractions;
using Kwality.UVault.Grants.Stores.Abstractions;
using Kwality.UVault.Keys;
using Kwality.UVault.Models;
using Kwality.UVault.QA.Internal.Factories;
using Kwality.UVault.QA.Internal.Xunit.Traits;

using Microsoft.Extensions.DependencyInjection;

using Xunit;

// ReSharper disable once MemberCanBeFileLocal
public sealed class GrantManagementTests
{
    [AutoData]
    [GrantManagement]
    [Theory(DisplayName = "When the store is configured as a `Singleton` one, it behaves as such.")]
    internal void UseStoreAsSingleton_RegisterStoreAsSingleton(ServiceCollection services)
    {
        // ARRANGE.
        services.AddUVault(static (_, options) => options.UseGrantManagement<Model, IntKey>(static options => options.UseStore<Store>(ServiceLifetime.Singleton)));

        // ASSERT.
        services.Should()
                .ContainSingle(static descriptor => descriptor.ServiceType == typeof(IGrantStore<Model, IntKey>) && descriptor.Lifetime == ServiceLifetime.Singleton && descriptor.ImplementationType == typeof(Store));
    }

    [AutoData]
    [GrantManagement]
    [Theory(DisplayName = "When the store is configured as a `Scoped` one, it behaves as such.")]
    internal void UseStoreAsScoped_RegisterStoreAsScoped(ServiceCollection services)
    {
        // ARRANGE.
        services.AddUVault(static (_, options) => options.UseGrantManagement<Model, IntKey>(static options => options.UseStore<Store>(ServiceLifetime.Scoped)));

        // ASSERT.
        services.Should()
                .ContainSingle(static descriptor => descriptor.ServiceType == typeof(IGrantStore<Model, IntKey>) && descriptor.Lifetime == ServiceLifetime.Scoped && descriptor.ImplementationType == typeof(Store));
    }

    [AutoData]
    [GrantManagement]
    [Theory(DisplayName = "When the store is configured as a `Transient` one, it behaves as such.")]
    internal void UseStoreAsTransient_RegisterStoreAsTransient(ServiceCollection services)
    {
        // ARRANGE.
        services.AddUVault(static (_, options) => options.UseGrantManagement<Model, IntKey>(static options => options.UseStore<Store>(ServiceLifetime.Transient)));

        // ASSERT.
        services.Should()
                .ContainSingle(static descriptor => descriptor.ServiceType == typeof(IGrantStore<Model, IntKey>) && descriptor.Lifetime == ServiceLifetime.Transient && descriptor.ImplementationType == typeof(Store));
    }

    [AutoData]
    [GrantManagement]
    [Theory(DisplayName = "Get all (pageIndex: 0, all data showed) succeeds.")]
    internal async Task GetAll_FirstPageWhenAllDataShowed_Succeeds(Model model)
    {
        // ARRANGE.
        GrantManager<Model, IntKey> manager = new GrantManagerFactory().Create<Model, IntKey>(static options => options.UseStore<Store>());

        await manager.CreateAsync(model, new CreateOperationMapper())
                     .ConfigureAwait(false);

        // ACT.
        PagedResultSet<Model> result = await manager.GetAllAsync(0, 10)
                                                    .ConfigureAwait(false);

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
        GrantManager<Model, IntKey> manager = new GrantManagerFactory().Create<Model, IntKey>(static options => options.UseStore<Store>());

        await manager.CreateAsync(model, new CreateOperationMapper())
                     .ConfigureAwait(false);

        // ACT.
        PagedResultSet<Model> result = await manager.GetAllAsync(1, 10)
                                                    .ConfigureAwait(false);

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
        GrantManager<Model, IntKey> manager = new GrantManagerFactory().Create<Model, IntKey>(static options => options.UseStore<Store>());

        await manager.CreateAsync(modelOne, new CreateOperationMapper())
                     .ConfigureAwait(false);

        await manager.CreateAsync(modelTwo, new CreateOperationMapper())
                     .ConfigureAwait(false);

        // ACT.
        PagedResultSet<Model> result = await manager.GetAllAsync(0, 1)
                                                    .ConfigureAwait(false);

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
        GrantManager<Model, IntKey> manager = new GrantManagerFactory().Create<Model, IntKey>(static options => options.UseStore<Store>());

        await manager.CreateAsync(modelOne, new CreateOperationMapper())
                     .ConfigureAwait(false);

        await manager.CreateAsync(modelTwo, new CreateOperationMapper())
                     .ConfigureAwait(false);

        // ACT.
        PagedResultSet<Model> result = await manager.GetAllAsync(1, 1)
                                                    .ConfigureAwait(false);

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
        GrantManager<Model, IntKey> manager = new GrantManagerFactory().Create<Model, IntKey>(static options => options.UseStore<Store>());

        await manager.CreateAsync(modelOne, new CreateOperationMapper())
                     .ConfigureAwait(false);

        await manager.CreateAsync(modelTwo, new CreateOperationMapper())
                     .ConfigureAwait(false);

        PagedResultSet<Model> result = await manager.GetAllAsync(0, 10, new OperationFilter(modelTwo.Scopes))
                                                    .ConfigureAwait(false);

        // ASSERT.
        result.ResultSet.Count()
              .Should()
              .Be(1);

        (await manager.GetAllAsync(0, 100)
                      .ConfigureAwait(false)).ResultSet.Should()
                                             .ContainEquivalentOf(modelTwo);
    }

    [AutoData]
    [GrantManagement]
    [Theory(DisplayName = "Create succeeds.")]
    internal async Task Create_Succeeds(Model model)
    {
        // ARRANGE.
        GrantManager<Model, IntKey> manager = new GrantManagerFactory().Create<Model, IntKey>(static options => options.UseStore<Store>());

        // ACT.
        await manager.CreateAsync(model, new CreateOperationMapper())
                     .ConfigureAwait(false);

        // ASSERT.
        (await manager.GetAllAsync(0, 100)
                      .ConfigureAwait(false)).ResultSet.Should()
                                             .ContainEquivalentOf(model);
    }

    [AutoData]
    [GrantManagement]
    [Theory(DisplayName = "Update succeeds.")]
    internal async Task Update_Succeeds(Model model)
    {
        // ARRANGE.
        GrantManager<Model, IntKey> manager = new GrantManagerFactory().Create<Model, IntKey>(static options => options.UseStore<Store>());

        IntKey key = await manager.CreateAsync(model, new CreateOperationMapper())
                                  .ConfigureAwait(false);

        // ACT.
        model.Scopes = new[] { "newScope", "newScope2", };

        await manager.UpdateAsync(key, model, new UpdateOperationMapper())
                     .ConfigureAwait(false);

        // ASSERT.
        (await manager.GetAllAsync(0, 100)
                      .ConfigureAwait(false)).ResultSet.Should()
                                             .ContainEquivalentOf(model);
    }

    [AutoData]
    [GrantManagement]
    [Theory(DisplayName = "Update raises an exception when the key is not found.")]
    internal async Task Update_UnknownKey_RaisesException(IntKey key, Model model)
    {
        // ARRANGE.
        GrantManager<Model, IntKey> manager = new GrantManagerFactory().Create<Model, IntKey>(static options => options.UseStore<Store>());

        // ACT.
        Func<Task> act = () => manager.UpdateAsync(key, model, new UpdateOperationMapper());

        // ASSERT.
        await act.Should()
                 .ThrowAsync<UpdateException>()
                 .WithMessage($"Custom: Failed to update client grant: `{key}`. Not found.")
                 .ConfigureAwait(false);
    }

    [AutoData]
    [GrantManagement]
    [Theory(DisplayName = "Delete succeeds.")]
    internal async Task Delete_Succeeds(Model model)
    {
        // ARRANGE.
        GrantManager<Model, IntKey> manager = new GrantManagerFactory().Create<Model, IntKey>(static options => options.UseStore<Store>());

        IntKey key = await manager.CreateAsync(model, new CreateOperationMapper())
                                  .ConfigureAwait(false);

        // ACT.
        await manager.DeleteByKeyAsync(key)
                     .ConfigureAwait(false);

        // ASSERT.
        (await manager.GetAllAsync(0, 100)
                      .ConfigureAwait(false)).ResultSet.Should()
                                             .BeEmpty();
    }

    [AutoData]
    [GrantManagement]
    [Theory(DisplayName = "Delete succeeds when the key is not found.")]
    internal async Task Delete_UnknownKey_Succeeds(IntKey key)
    {
        // ARRANGE.
        GrantManager<Model, IntKey> manager = new GrantManagerFactory().Create<Model, IntKey>(static options => options.UseStore<Store>());

        // ACT.
        Func<Task> act = () => manager.DeleteByKeyAsync(key);

        // ASSERT.
        await act.Should()
                 .NotThrowAsync()
                 .ConfigureAwait(false);
    }

#pragma warning disable CA1812 // "Avoid uninstantiated internal classes".
    [UsedImplicitly]
    internal sealed class Model : GrantModel<IntKey>
#pragma warning restore CA1812
    {
        public Model(IntKey key, IEnumerable<string> scopes)
            : base(key)
        {
            this.Scopes = scopes;
        }

        public IEnumerable<string> Scopes { get; set; }
    }

    private sealed class OperationFilter : IGrantFilter
    {
        private readonly IEnumerable<string> scopes;

        public OperationFilter(IEnumerable<string> scopes)
        {
            this.scopes = scopes;
        }

        public TDestination Create<TDestination>()
            where TDestination : class
        {
            if (typeof(TDestination) != typeof(Func<KeyValuePair<IntKey, Model>, bool>))
            {
                throw new ReadException($"Invalid {nameof(IGrantFilter)}: Destination is NOT `{typeof(Func<KeyValuePair<IntKey, Model>, bool>).Name}`.");
            }

            // ReSharper disable once NullableWarningSuppressionIsUsed - Known to be safe. See previous statement.
            return ((Func<KeyValuePair<IntKey, Model>, bool>)Filter as TDestination)!;

            // The filter which is filters out data in the store.
            bool Filter(KeyValuePair<IntKey, Model> kvp)
            {
                return Equals(kvp.Value.Scopes, this.scopes);
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
                throw new CreateException($"Invalid {nameof(IGrantOperationMapper)}: Destination is NOT `{nameof(TSource)}`.");
            }

            // ReSharper disable once NullableWarningSuppressionIsUsed - Known to be safe. See previous statement.
            return (source as TDestination)!;
        }
    }

    private sealed class UpdateOperationMapper : IGrantOperationMapper
    {
        public TDestination Create<TSource, TDestination>(TSource source)
            where TDestination : class
        {
            if (typeof(TDestination) != typeof(TSource))
            {
                throw new UpdateException($"Invalid {nameof(IGrantOperationMapper)}: Destination is NOT `{nameof(TSource)}`.");
            }

            // ReSharper disable once NullableWarningSuppressionIsUsed - Known to be safe. See previous statement.
            return (source as TDestination)!;
        }
    }

#pragma warning disable CA1812 // "Avoid uninstantiated internal classes".
    [UsedImplicitly]
    internal sealed class Store : IGrantStore<Model, IntKey>
#pragma warning restore CA1812
    {
        private readonly IDictionary<IntKey, Model> collection = new Dictionary<IntKey, Model>();

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
                      .ConfigureAwait(false);
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
