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
namespace Kwality.UVault.M2M.QA;

using AutoFixture;
using AutoFixture.Kernel;
using AutoFixture.Xunit2;

using FluentAssertions;

using JetBrains.Annotations;

using Kwality.UVault.Core.Exceptions;
using Kwality.UVault.Core.Extensions;
using Kwality.UVault.Core.Helpers;
using Kwality.UVault.Core.Keys;
using Kwality.UVault.Core.Models;
using Kwality.UVault.M2M.Extensions;
using Kwality.UVault.M2M.Managers;
using Kwality.UVault.M2M.Models;
using Kwality.UVault.M2M.Operations.Filters.Abstractions;
using Kwality.UVault.M2M.Operations.Mappers.Abstractions;
using Kwality.UVault.M2M.QA.Internal.Factories;
using Kwality.UVault.M2M.Stores.Abstractions;
using Kwality.UVault.QA.Common.Xunit.Traits;

using Microsoft.Extensions.DependencyInjection;

using Xunit;

public sealed class ApplicationManagementTests
{
    [AutoDomainData]
    [M2MManagement]
    [Theory(DisplayName = "When a custom manager is configured, it's registered.")]
    internal void UseManager_RegistersManager(IServiceCollection services)
    {
        // ARRANGE.
        services.AddUVault(static options => options.UseApplicationManagement<Model, IntKey>(static options =>
        {
            options.UseManager<Manager<Model, IntKey>>();
        }));

        // ASSERT.
        services.Should()
                .ContainSingle(static descriptor => descriptor.ServiceType == typeof(Manager<Model, IntKey>) &&
                                                    descriptor.Lifetime == ServiceLifetime.Scoped);
    }

    [AutoDomainData]
    [M2MManagement]
    [Theory(DisplayName = "When a custom manager (with a custom store) is configured, it's registered.")]
    internal void UseManagerStore_RegistersManager(IServiceCollection services)
    {
        // ARRANGE.
        services.AddUVault(static options => options.UseApplicationManagement<Model, IntKey>(static options =>
        {
            options.UseManager<ManagerStore<Model, IntKey>>();
            options.UseStore<Store<Model, IntKey>>();
        }));

        // ASSERT.
        services.Should()
                .ContainSingle(static descriptor => descriptor.ServiceType == typeof(ManagerStore<Model, IntKey>) &&
                                                    descriptor.Lifetime == ServiceLifetime.Scoped);

        services.Should()
                .ContainSingle(static descriptor =>
                    descriptor.ServiceType == typeof(IApplicationStore<Model, IntKey>) &&
                    descriptor.Lifetime == ServiceLifetime.Scoped &&
                    descriptor.ImplementationType == typeof(Store<Model, IntKey>));
    }

    [AutoDomainData]
    [M2MManagement]
    [Theory(DisplayName = "When a custom manager is configured, it can be resolved.")]
    internal void ResolveManager_RaisesNoException(IServiceCollection services)
    {
        // ARRANGE.
        services.AddUVault(static options => options.UseApplicationManagement<Model, IntKey>(static options =>
        {
            options.UseManager<Manager<Model, IntKey>>();
        }));

        // ACT.
        Func<Manager<Model, IntKey>> act = () => services.BuildServiceProvider()
                                                         .GetRequiredService<Manager<Model, IntKey>>();

        // ASSERT.
        act.Should()
           .NotThrow();
    }

    [AutoDomainData]
    [M2MManagement]
    [Theory(DisplayName = "When a custom manager is configured, it can be resolved.")]
    internal void ResolveManagerStore_RaisesNoException(IServiceCollection services)
    {
        // ARRANGE.
        services.AddUVault(static options => options.UseApplicationManagement<Model, IntKey>(static options =>
        {
            options.UseManager<ManagerStore<Model, IntKey>>();
            options.UseStore<Store<Model, IntKey>>();
        }));

        // ACT.
        Func<ManagerStore<Model, IntKey>> act = () => services.BuildServiceProvider()
                                                              .GetRequiredService<ManagerStore<Model, IntKey>>();

        // ASSERT.
        act.Should()
           .NotThrow();
    }

    [AutoDomainData]
    [M2MManagement]
    [Theory(DisplayName = "When the store is configured as a `Singleton` one, it behaves as such.")]
    internal void UseStoreAsSingleton_RegisterStoreAsSingleton(IServiceCollection services)
    {
        // ARRANGE.
        services.AddUVault(static options =>
            options.UseApplicationManagement<Model, IntKey>(static options =>
                options.UseStore<Store>(ServiceLifetime.Singleton)));

        // ASSERT.
        services.Should()
                .ContainSingle(static descriptor =>
                    descriptor.ServiceType == typeof(IApplicationStore<Model, IntKey>) &&
                    descriptor.Lifetime == ServiceLifetime.Singleton && descriptor.ImplementationType == typeof(Store));
    }

    [AutoDomainData]
    [M2MManagement]
    [Theory(DisplayName = "When the store is configured as a `Scoped` one, it behaves as such.")]
    internal void UseStoreAsScoped_RegisterStoreAsScoped(IServiceCollection services)
    {
        // ARRANGE.
        services.AddUVault(static options =>
            options.UseApplicationManagement<Model, IntKey>(static options =>
                options.UseStore<Store>(ServiceLifetime.Scoped)));

        // ASSERT.
        services.Should()
                .ContainSingle(static descriptor =>
                    descriptor.ServiceType == typeof(IApplicationStore<Model, IntKey>) &&
                    descriptor.Lifetime == ServiceLifetime.Scoped && descriptor.ImplementationType == typeof(Store));
    }

    [AutoDomainData]
    [M2MManagement]
    [Theory(DisplayName = "When the store is configured as a `Transient` one, it behaves as such.")]
    internal void UseStoreAsTransient_RegisterStoreAsTransient(IServiceCollection services)
    {
        // ARRANGE.
        services.AddUVault(static options =>
            options.UseApplicationManagement<Model, IntKey>(static options =>
                options.UseStore<Store>(ServiceLifetime.Transient)));

        // ASSERT.
        services.Should()
                .ContainSingle(static descriptor =>
                    descriptor.ServiceType == typeof(IApplicationStore<Model, IntKey>) &&
                    descriptor.Lifetime == ServiceLifetime.Transient && descriptor.ImplementationType == typeof(Store));
    }

    [AutoData]
    [M2MManagement]
    [Theory(DisplayName = "Get all (pageIndex: 0, all data showed) succeeds.")]
    internal async Task GetAll_FirstPageWhenAllDataShowed_Succeeds(Model model)
    {
        // ARRANGE.
        ApplicationManager<Model, IntKey> manager
            = new ApplicationManagerFactory().Create<Model, IntKey>(static options => options.UseStore<Store>());

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
              .BeEquivalentTo(model,
                  static options => options.Excluding(static application => application.ClientSecret));
    }

    [AutoData]
    [M2MManagement]
    [Theory(DisplayName = "Get all (pageIndex: 1, all data showed) succeeds.")]
    internal async Task GetAll_SecondPageWhenAllDataShowed_Succeeds(Model model)
    {
        // ARRANGE.
        ApplicationManager<Model, IntKey> manager
            = new ApplicationManagerFactory().Create<Model, IntKey>(static options => options.UseStore<Store>());

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
    [M2MManagement]
    [Theory(DisplayName = "Get all (pageIndex: 0, all data NOT showed) succeeds.")]
    internal async Task GetAll_FirstPageWhenNotAllDataShowed_Succeeds(Model modelOne, Model modelTwo)
    {
        // ARRANGE.
        ApplicationManager<Model, IntKey> manager
            = new ApplicationManagerFactory().Create<Model, IntKey>(static options => options.UseStore<Store>());

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
              .BeEquivalentTo(modelOne,
                  static options => options.Excluding(static application => application.ClientSecret));
    }

    [AutoData]
    [M2MManagement]
    [Theory(DisplayName = "Get all (pageIndex: 1, pageSize: Less than the total amount) succeeds.")]
    internal async Task GetAll_SecondPageWithLessElementsThanTotal_Succeeds(Model modelOne, Model modelTwo)
    {
        // ARRANGE.
        ApplicationManager<Model, IntKey> manager
            = new ApplicationManagerFactory().Create<Model, IntKey>(static options => options.UseStore<Store>());

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
              .BeEquivalentTo(modelTwo,
                  static options => options.Excluding(static application => application.ClientSecret));
    }

    [AutoData]
    [M2MManagement]
    [Theory(DisplayName = "Get all with filter succeeds.")]
    internal async Task GetAll_WithFilter_Succeeds(Model modelOne, Model modelTwo)
    {
        // ARRANGE.
        ApplicationManager<Model, IntKey> manager
            = new ApplicationManagerFactory().Create<Model, IntKey>(static options => options.UseStore<Store>());

        await manager.CreateAsync(modelOne, new CreateOperationMapper())
                     .ConfigureAwait(true);

        await manager.CreateAsync(modelTwo, new CreateOperationMapper())
                     .ConfigureAwait(true);

        PagedResultSet<Model> result = await manager
                                             .GetAllAsync(0, 10, new OperationFilter(modelTwo.Name ?? string.Empty))
                                             .ConfigureAwait(true);

        // ASSERT.
        result.ResultSet.Count()
              .Should()
              .Be(1);

        result.ResultSet.First()
              .Should()
              .BeEquivalentTo(modelTwo,
                  static options => options.Excluding(static application => application.ClientSecret));
    }

    [AutoData]
    [M2MManagement]
    [Theory(DisplayName = "Get by key raises an exception when the key is NOT found.")]
    internal async Task GetByKey_UnknownKey_RaisesException(IntKey key)
    {
        // ARRANGE.
        ApplicationManager<Model, IntKey> manager
            = new ApplicationManagerFactory().Create<Model, IntKey>(static options => options.UseStore<Store>());

        // ACT.
        Func<Task<Model>> act = () => manager.GetByKeyAsync(key);

        // ASSERT.
        await act.Should()
                 .ThrowAsync<ReadException>()
                 .WithMessage($"Custom: Failed to read application: `{key}`. Not found.")
                 .ConfigureAwait(true);
    }

    [AutoData]
    [M2MManagement]
    [Theory(DisplayName = "Create succeeds.")]
    internal async Task Create_Succeeds(Model model)
    {
        // ARRANGE.
        ApplicationManager<Model, IntKey> manager
            = new ApplicationManagerFactory().Create<Model, IntKey>(static options => options.UseStore<Store>());

        // ACT.
        IntKey key = await manager.CreateAsync(model, new CreateOperationMapper())
                                  .ConfigureAwait(true);

        // ASSERT.
        (await manager.GetByKeyAsync(key)
                      .ConfigureAwait(true)).Should()
                                            .BeEquivalentTo(model);
    }

    [AutoData]
    [M2MManagement]
    [Theory(DisplayName = "Update succeeds.")]
    internal async Task Update_Succeeds(Model model)
    {
        // ARRANGE.
        ApplicationManager<Model, IntKey> manager
            = new ApplicationManagerFactory().Create<Model, IntKey>(static options => options.UseStore<Store>());

        IntKey key = await manager.CreateAsync(model, new CreateOperationMapper())
                                  .ConfigureAwait(true);

        // ACT.
        model.Name = "UVault (Sample application)";

        await manager.UpdateAsync(key, model, new UpdateOperationMapper())
                     .ConfigureAwait(true);

        // ASSERT.
        (await manager.GetByKeyAsync(key)
                      .ConfigureAwait(true)).Should()
                                            .BeEquivalentTo(model);
    }

    [AutoData]
    [M2MManagement]
    [Theory(DisplayName = "Update raises an exception when the key is not found.")]
    internal async Task Update_UnknownKey_RaisesException(IntKey key, Model model)
    {
        // ARRANGE.
        ApplicationManager<Model, IntKey> manager
            = new ApplicationManagerFactory().Create<Model, IntKey>(static options => options.UseStore<Store>());

        // ACT.
        Func<Task> act = () => manager.UpdateAsync(key, model, new UpdateOperationMapper());

        // ASSERT.
        await act.Should()
                 .ThrowAsync<UpdateException>()
                 .WithMessage($"Custom: Failed to update application: `{key}`. Not found.")
                 .ConfigureAwait(true);
    }

    [AutoData]
    [M2MManagement]
    [Theory(DisplayName = "Delete succeeds.")]
    internal async Task Delete_Succeeds(Model model)
    {
        // ARRANGE.
        ApplicationManager<Model, IntKey> manager
            = new ApplicationManagerFactory().Create<Model, IntKey>(static options => options.UseStore<Store>());

        IntKey key = await manager.CreateAsync(model, new CreateOperationMapper())
                                  .ConfigureAwait(true);

        // ACT.
        await manager.DeleteByKeyAsync(key)
                     .ConfigureAwait(true);

        // ASSERT.
        Func<Task<Model>> act = () => manager.GetByKeyAsync(key);

        await act.Should()
                 .ThrowAsync<ReadException>()
                 .WithMessage($"Custom: Failed to read application: `{key}`. Not found.")
                 .ConfigureAwait(true);
    }

    [AutoData]
    [M2MManagement]
    [Theory(DisplayName = "Delete succeeds when the key is not found.")]
    internal async Task Delete_UnknownKey_Succeeds(IntKey key)
    {
        // ARRANGE.
        ApplicationManager<Model, IntKey> manager
            = new ApplicationManagerFactory().Create<Model, IntKey>(static options => options.UseStore<Store>());

        // ACT.
        Func<Task> act = () => manager.DeleteByKeyAsync(key);

        // ASSERT.
        await act.Should()
                 .NotThrowAsync()
                 .ConfigureAwait(true);
    }

    [AutoData]
    [M2MManagement]
    [Theory(DisplayName = "Rotate client secret raises an exception when the key is NOT found.")]
    internal async Task RotateClientSecret_UnknownKey_RaisesException(IntKey key)
    {
        // ARRANGE.
        ApplicationManager<Model, IntKey> manager
            = new ApplicationManagerFactory().Create<Model, IntKey>(static options => options.UseStore<Store>());

        // ACT.
        Func<Task<Model>> act = () => manager.RotateClientSecretAsync(key);

        // ASSERT.
        await act.Should()
                 .ThrowAsync<UpdateException>()
                 .WithMessage($"Custom: Failed to update application: `{key}`. Not found.")
                 .ConfigureAwait(true);
    }

    [AutoData]
    [M2MManagement]
    [Theory(DisplayName = "Rotate client secret succeeds.")]
    internal async Task RotateClientSecret_Succeeds(Model model)
    {
        // ARRANGE.
        ApplicationManager<Model, IntKey> manager
            = new ApplicationManagerFactory().Create<Model, IntKey>(static options => options.UseStore<Store>());

        IntKey key = await manager.CreateAsync(model, new CreateOperationMapper())
                                  .ConfigureAwait(true);

        string? initialClientSecret = model.ClientSecret;

        // ACT.
        await manager.RotateClientSecretAsync(key)
                     .ConfigureAwait(true);

        // ASSERT.
        Model application = await manager.GetByKeyAsync(key)
                                         .ConfigureAwait(true);

        initialClientSecret.Should()
                           .NotMatch(application.ClientSecret);
    }

#pragma warning disable CA1812
    private sealed class Store<TModel, TKey> : IApplicationStore<TModel, TKey>
#pragma warning restore CA1812
        where TModel : ApplicationModel<TKey>
        where TKey : IEqualityComparer<TKey>
    {
        public Task<PagedResultSet<TModel>> GetAllAsync(int pageIndex, int pageSize, IApplicationFilter? filter)
        {
            throw new NotImplementedException();
        }

        public Task<TModel> GetByKeyAsync(TKey key)
        {
            throw new NotImplementedException();
        }

        public Task<TKey> CreateAsync(TModel model, IApplicationOperationMapper mapper)
        {
            throw new NotImplementedException();
        }

        public Task UpdateAsync(TKey key, TModel model, IApplicationOperationMapper mapper)
        {
            throw new NotImplementedException();
        }

        public Task DeleteByKeyAsync(TKey key)
        {
            throw new NotImplementedException();
        }

        public Task<TModel> RotateClientSecretAsync(TKey key)
        {
            throw new NotImplementedException();
        }
    }

#pragma warning disable CA1812
    private sealed class Manager<TModel, TKey>(IApplicationStore<TModel, TKey> store)
#pragma warning restore CA1812
        : ApplicationManager<TModel, TKey>(store)
        where TModel : ApplicationModel<TKey>
        where TKey : IEqualityComparer<TKey>;

#pragma warning disable CA1812
    private sealed class ManagerStore<TModel, TKey> : ApplicationManager<TModel, TKey>
#pragma warning restore CA1812
        where TModel : ApplicationModel<TKey>
        where TKey : IEqualityComparer<TKey>
    {
        public ManagerStore(IApplicationStore<TModel, TKey> store)
            : base(store)
        {
            if (store is not Store<TModel, TKey>)
            {
                throw new InvalidOperationException("The provided store isn't valid for this manager.");
            }
        }
    }

#pragma warning disable CA1812
    internal sealed class Model : ApplicationModel<IntKey>
#pragma warning restore CA1812
    {
        public Model(IntKey key, string name)
            : base(key)
        {
            this.Name = name;
        }
    }

    private sealed class OperationFilter(string name) : IApplicationFilter
    {
        public TDestination Create<TDestination>()
            where TDestination : class
        {
            if (typeof(TDestination) != typeof(Func<KeyValuePair<IntKey, Model>, bool>))
            {
                throw new ReadException(
                    $"Invalid {nameof(IApplicationFilter)}: Destination is NOT `{typeof(Func<KeyValuePair<IntKey, Model>, bool>).Name}`.");
            }

            return ((Func<KeyValuePair<IntKey, Model>, bool>)Filter)
                .UnsafeAs<Func<KeyValuePair<IntKey, Model>, bool>, TDestination>();

            // The filter which is filters out data in the store.
            bool Filter(KeyValuePair<IntKey, Model> kvp)
            {
                return kvp.Value.Name == name;
            }
        }
    }

    private sealed class CreateOperationMapper : IApplicationOperationMapper
    {
        public TDestination Create<TSource, TDestination>(TSource source)
            where TDestination : class
        {
            if (typeof(TDestination) != typeof(TSource))
            {
                throw new CreateException(
                    $"Invalid {nameof(IApplicationOperationMapper)}: Destination is NOT `{nameof(TSource)}`.");
            }

            return source.UnsafeAs<TSource, TDestination>();
        }
    }

    private sealed class UpdateOperationMapper : IApplicationOperationMapper
    {
        public TDestination Create<TSource, TDestination>(TSource source)
            where TDestination : class
        {
            if (typeof(TDestination) != typeof(TSource))
            {
                throw new UpdateException(
                    $"Invalid {nameof(IApplicationOperationMapper)}: Destination is NOT `{nameof(TSource)}`.");
            }

            return source.UnsafeAs<TSource, TDestination>();
        }
    }

    [UsedImplicitly]
#pragma warning disable CA1812
    internal sealed class Store : IApplicationStore<Model, IntKey>
#pragma warning restore CA1812
    {
        private readonly Dictionary<IntKey, Model> collection = new();

        public Task<PagedResultSet<Model>> GetAllAsync(int pageIndex, int pageSize, IApplicationFilter? filter)
        {
            IQueryable<KeyValuePair<IntKey, Model>> dataSet = this.collection.AsQueryable();

            if (filter != null)
            {
                dataSet = dataSet.AsEnumerable()
                                 .Where(filter.Create<Func<KeyValuePair<IntKey, Model>, bool>>())
                                 .AsQueryable();
            }

            IEnumerable<Model> applications = dataSet.Skip(pageIndex * pageSize)
                                                     .Take(pageSize)
                                                     .Select(static kvp => kvp.Value);

            var result = new PagedResultSet<Model>(applications, this.collection.Count > (pageIndex + 1) * pageSize);

            return Task.FromResult(result);
        }

        public Task<Model> GetByKeyAsync(IntKey key)
        {
            if (!this.collection.TryGetValue(key, out Model? value))
            {
                throw new ReadException($"Custom: Failed to read application: `{key}`. Not found.");
            }

            return Task.FromResult(value);
        }

        public Task<IntKey> CreateAsync(Model model, IApplicationOperationMapper mapper)
        {
            this.collection.Add(model.Key, mapper.Create<Model, Model>(model));

            return Task.FromResult(model.Key);
        }

        public async Task UpdateAsync(IntKey key, Model model, IApplicationOperationMapper mapper)
        {
            if (!this.collection.ContainsKey(key))
            {
                throw new UpdateException($"Custom: Failed to update application: `{key}`. Not found.");
            }

            this.collection.Remove(key);

            await this.CreateAsync(model, mapper)
                      .ConfigureAwait(false);
        }

        public Task DeleteByKeyAsync(IntKey key)
        {
            this.collection.Remove(key);

            return Task.CompletedTask;
        }

        public Task<Model> RotateClientSecretAsync(IntKey key)
        {
            if (this.collection.TryGetValue(key, out Model? model))
            {
                model.ClientSecret = Guid.NewGuid()
                                         .ToString();

                this.collection[key] = model;

                return Task.FromResult(model);
            }

            throw new UpdateException($"Custom: Failed to update application: `{key}`. Not found.");
        }
    }

    [AttributeUsage(AttributeTargets.Method)]
    private sealed class AutoDomainDataAttribute() : AutoDataAttribute(static () =>
    {
        var fixture = new Fixture();
        fixture.Customizations.Add(new TypeRelay(typeof(IServiceCollection), typeof(ServiceCollection)));

        return fixture;
    });
}
