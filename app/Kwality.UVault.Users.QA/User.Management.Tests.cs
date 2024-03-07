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
namespace Kwality.UVault.Users.QA;

using AutoFixture;
using AutoFixture.Kernel;
using AutoFixture.Xunit2;

using FluentAssertions;

using JetBrains.Annotations;

using Kwality.UVault.Core.Exceptions;
using Kwality.UVault.Core.Extensions;
using Kwality.UVault.Core.Helpers;
using Kwality.UVault.Core.Keys;
using Kwality.UVault.QA.Common.Xunit.Traits;
using Kwality.UVault.Users.Extensions;
using Kwality.UVault.Users.Managers;
using Kwality.UVault.Users.Models;
using Kwality.UVault.Users.Operations.Mappers.Abstractions;
using Kwality.UVault.Users.QA.Factories;
using Kwality.UVault.Users.Stores.Abstractions;

using Microsoft.Extensions.DependencyInjection;

using Xunit;

public sealed class UserManagementTests
{
    [AutoData]
    [ApiManagement]
    [Theory(DisplayName = "When the store is configured as a `Singleton` one, it behaves as such.")]
    internal void UseStoreAsSingleton_RegisterStoreAsSingleton(IServiceCollection services)
    {
        // ARRANGE.
        services.AddUVault(static (_, options) =>
            options.UseUserManagement<Model, IntKey>(static options =>
                options.UseStore<Store>(ServiceLifetime.Singleton)));

        // ASSERT.
        services.Should()
                .ContainSingle(static descriptor => descriptor.ServiceType == typeof(IUserStore<Model, IntKey>) &&
                                                    descriptor.Lifetime == ServiceLifetime.Singleton &&
                                                    descriptor.ImplementationType == typeof(Store));
    }

    [AutoData]
    [ApiManagement]
    [Theory(DisplayName = "When the store is configured as a `Scoped` one, it behaves as such.")]
    internal void UseStoreAsScoped_RegisterStoreAsScoped(IServiceCollection services)
    {
        // ARRANGE.
        services.AddUVault(static (_, options) =>
            options.UseUserManagement<Model, IntKey>(static options =>
                options.UseStore<Store>(ServiceLifetime.Scoped)));

        // ASSERT.
        services.Should()
                .ContainSingle(static descriptor => descriptor.ServiceType == typeof(IUserStore<Model, IntKey>) &&
                                                    descriptor.Lifetime == ServiceLifetime.Scoped &&
                                                    descriptor.ImplementationType == typeof(Store));
    }

    [AutoData]
    [ApiManagement]
    [Theory(DisplayName = "When the store is configured as a `Transient` one, it behaves as such.")]
    internal void UseStoreAsTransient_RegisterStoreAsTransient(IServiceCollection services)
    {
        // ARRANGE.
        services.AddUVault(static (_, options) =>
            options.UseUserManagement<Model, IntKey>(static options =>
                options.UseStore<Store>(ServiceLifetime.Transient)));

        // ASSERT.
        services.Should()
                .ContainSingle(static descriptor => descriptor.ServiceType == typeof(IUserStore<Model, IntKey>) &&
                                                    descriptor.Lifetime == ServiceLifetime.Transient &&
                                                    descriptor.ImplementationType == typeof(Store));
    }

    [AutoData]
    [UserManagement]
    [Theory(DisplayName = "Get by key raises an exception when the key is NOT found.")]
    internal async Task GetByKey_UnknownKey_RaisesException(IntKey key)
    {
        // ARRANGE.
        UserManager<Model, IntKey> manager
            = new UserManagerFactory().Create<Model, IntKey>(static options => options.UseStore<Store>());

        // ACT.
        Func<Task<Model>> act = () => manager.GetByKeyAsync(key);

        // ASSERT.
        await act.Should()
                 .ThrowAsync<ReadException>()
                 .WithMessage($"Custom: Failed to read user: `{key}`. Not found.")
                 .ConfigureAwait(true);
    }

    [AutoData]
    [UserManagement]
    [Theory(DisplayName = "Get by email returns NO users when NO users are found.")]
    internal async Task GetByEmail_UnknownEmail_ReturnsEmptyCollection(Model model)
    {
        // ARRANGE.
        UserManager<Model, IntKey> manager
            = new UserManagerFactory().Create<Model, IntKey>(static options => options.UseStore<Store>());

        await manager.CreateAsync(model, new CreateOperationMapper())
                     .ConfigureAwait(true);

        // ACT.
        IEnumerable<Model> result = await manager.GetByEmailAsync("email")
                                                 .ConfigureAwait(true);

        // ASSERT.
        result.Should()
              .BeEmpty();
    }

    [AutoData]
    [UserManagement]
    [Theory(DisplayName = "Get by email returns the matches.")]
    internal async Task GetByEmail_SingleMatch_ReturnsMatches(List<Model> models)
    {
        // ARRANGE.
        UserManager<Model, IntKey> manager
            = new UserManagerFactory().Create<Model, IntKey>(static options => options.UseStore<Store>());

        foreach (Model model in models)
            await manager.CreateAsync(model, new CreateOperationMapper())
                         .ConfigureAwait(true);

        // ACT.
        Model expected = models.Skip(1)
                               .First();

        IEnumerable<Model> result = await manager.GetByEmailAsync(expected.Email)
                                                 .ConfigureAwait(true);

        // ASSERT.
        result.Should()
              .BeEquivalentTo(new[] { expected });
    }

    [FixedEmail]
    [UserManagement]
    [Theory(DisplayName = "Get by email returns the matches.")]
    internal async Task GetByEmail_MultipleMatches_ReturnsMatches(List<Model> models)
    {
        // ARRANGE.
        UserManager<Model, IntKey> manager
            = new UserManagerFactory().Create<Model, IntKey>(static options => options.UseStore<Store>());

        foreach (Model model in models)
            await manager.CreateAsync(model, new CreateOperationMapper())
                         .ConfigureAwait(true);

        // ACT.
        Model expected = models.Skip(1)
                               .First();

        IEnumerable<Model> result = await manager.GetByEmailAsync(expected.Email)
                                                 .ConfigureAwait(true);

        // ASSERT.
        result.Should()
              .BeEquivalentTo(models);
    }

    [AutoData]
    [UserManagement]
    [Theory(DisplayName = "Create succeeds.")]
    internal async Task Create_Succeeds(Model model)
    {
        // ARRANGE.
        UserManager<Model, IntKey> manager
            = new UserManagerFactory().Create<Model, IntKey>(static options => options.UseStore<Store>());

        // ACT.
        IntKey key = await manager.CreateAsync(model, new CreateOperationMapper())
                                  .ConfigureAwait(true);

        // ASSERT.
        (await manager.GetByKeyAsync(key)
                      .ConfigureAwait(true)).Should()
                                            .BeEquivalentTo(model);
    }

    [AutoData]
    [UserManagement]
    [Theory(DisplayName = "Create raises an exception when another user with the same key already exist.")]
    internal async Task Create_KeyExists_RaisesException(Model model)
    {
        // ARRANGE.
        UserManager<Model, IntKey> manager
            = new UserManagerFactory().Create<Model, IntKey>(static options => options.UseStore<Store>());

        await manager.CreateAsync(model, new CreateOperationMapper())
                     .ConfigureAwait(true);

        // ACT.
        Func<Task<IntKey>> act = () => manager.CreateAsync(model, new CreateOperationMapper());

        // ASSERT.
        await act.Should()
                 .ThrowAsync<CreateException>()
                 .WithMessage($"Custom: Failed to create user: `{model.Key}`. Duplicate key.")
                 .ConfigureAwait(true);
    }

    [AutoData]
    [UserManagement]
    [Theory(DisplayName = "Update succeeds.")]
    internal async Task Update_Succeeds(Model model)
    {
        // ARRANGE.
        UserManager<Model, IntKey> manager
            = new UserManagerFactory().Create<Model, IntKey>(static options => options.UseStore<Store>());

        IntKey key = await manager.CreateAsync(model, new CreateOperationMapper())
                                  .ConfigureAwait(true);

        // ACT.
        model.Email = "kwality.uvault@github.com";

        await manager.UpdateAsync(key, model, new UpdateOperationMapper())
                     .ConfigureAwait(true);

        // ASSERT.
        (await manager.GetByKeyAsync(key)
                      .ConfigureAwait(true)).Should()
                                            .BeEquivalentTo(model);
    }

    [AutoData]
    [UserManagement]
    [Theory(DisplayName = "Update raises an exception when the key is not found.")]
    internal async Task Update_UnknownKey_RaisesException(IntKey key, Model model)
    {
        // ARRANGE.
        UserManager<Model, IntKey> manager
            = new UserManagerFactory().Create<Model, IntKey>(static options => options.UseStore<Store>());

        // ACT.
        Func<Task> act = () => manager.UpdateAsync(key, model, new UpdateOperationMapper());

        // ASSERT.
        await act.Should()
                 .ThrowAsync<UpdateException>()
                 .WithMessage($"Custom: Failed to update user: `{key}`. Not found.")
                 .ConfigureAwait(true);
    }

    [AutoData]
    [UserManagement]
    [Theory(DisplayName = "Delete succeeds.")]
    internal async Task Delete_Succeeds(Model model)
    {
        // ARRANGE.
        UserManager<Model, IntKey> manager
            = new UserManagerFactory().Create<Model, IntKey>(static options => options.UseStore<Store>());

        IntKey key = await manager.CreateAsync(model, new CreateOperationMapper())
                                  .ConfigureAwait(true);

        // ACT.
        await manager.DeleteByKeyAsync(key)
                     .ConfigureAwait(true);

        // ASSERT.
        Func<Task<Model>> act = () => manager.GetByKeyAsync(key);

        await act.Should()
                 .ThrowAsync<ReadException>()
                 .WithMessage($"Custom: Failed to read user: `{key}`. Not found.")
                 .ConfigureAwait(true);
    }

    [AutoData]
    [UserManagement]
    [Theory(DisplayName = "Delete succeeds when the key is not found.")]
    internal async Task Delete_UnknownKey_Succeeds(IntKey key)
    {
        // ARRANGE.
        UserManager<Model, IntKey> manager
            = new UserManagerFactory().Create<Model, IntKey>(static options => options.UseStore<Store>());

        // ACT.
        Func<Task> act = () => manager.DeleteByKeyAsync(key);

        // ASSERT.
        await act.Should()
                 .NotThrowAsync()
                 .ConfigureAwait(true);
    }

    [UsedImplicitly]
    internal sealed class Model(IntKey key, string email) : UserModel<IntKey>(key, email);

    private sealed class CreateOperationMapper : IUserOperationMapper
    {
        public TDestination Create<TSource, TDestination>(TSource source)
            where TDestination : class
        {
            if (typeof(TDestination) != typeof(TSource))
            {
                throw new CreateException(
                    $"Invalid {nameof(IUserOperationMapper)}: Destination is NOT `{nameof(TSource)}`.");
            }

            return source.UnsafeAs<TSource, TDestination>();
        }
    }

    private sealed class UpdateOperationMapper : IUserOperationMapper
    {
        public TDestination Create<TSource, TDestination>(TSource source)
            where TDestination : class
        {
            if (typeof(TDestination) != typeof(TSource))
            {
                throw new UpdateException(
                    $"Invalid {nameof(IUserOperationMapper)}: Destination is NOT `{nameof(TSource)}`.");
            }

            return source.UnsafeAs<TSource, TDestination>();
        }
    }

    [UsedImplicitly]
#pragma warning disable CA1812
    internal sealed class Store : IUserStore<Model, IntKey>
#pragma warning restore CA1812
    {
        private readonly Dictionary<IntKey, Model> collection = new();

        public Task<Model> GetByKeyAsync(IntKey key)
        {
            if (!this.collection.TryGetValue(key, out Model? value))
            {
                throw new ReadException($"Custom: Failed to read user: `{key}`. Not found.");
            }

            return Task.FromResult(value);
        }

        public Task<IEnumerable<Model>> GetByEmailAsync(string email)
        {
            return Task.FromResult(this
                                   .collection.Where(user => user.Value.Email.Equals(email, StringComparison.Ordinal))
                                   .Select(static user => user.Value));
        }

        public Task<IntKey> CreateAsync(Model model, IUserOperationMapper mapper)
        {
            if (!this.collection.ContainsKey(model.Key))
            {
                this.collection.Add(model.Key, mapper.Create<Model, Model>(model));

                return Task.FromResult(model.Key);
            }

            throw new CreateException($"Custom: Failed to create user: `{model.Key}`. Duplicate key.");
        }

        public async Task UpdateAsync(IntKey key, Model model, IUserOperationMapper mapper)
        {
            if (!this.collection.ContainsKey(key))
            {
                throw new UpdateException($"Custom: Failed to update user: `{key}`. Not found.");
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
    }

    [AttributeUsage(AttributeTargets.Method)]
    private sealed class FixedEmailAttribute() : AutoDataAttribute(static () =>
    {
        var fixture = new Fixture();
        var email = $"{fixture.Create<string>()}@acme.com";
        fixture.Customizations.Add(new FixedEmailSpecimenBuilder(email));

        return fixture;
    })
    {
        private sealed class FixedEmailSpecimenBuilder(string email) : ISpecimenBuilder
        {
            public object Create(object request, ISpecimenContext context)
            {
                if (request is Type type && type == typeof(Model))
                {
                    return new Model(context.Create<IntKey>(), email);
                }

                return new NoSpecimen();
            }
        }
    }
}
